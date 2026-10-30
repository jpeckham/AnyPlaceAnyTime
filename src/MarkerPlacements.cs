using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
namespace AnyPlaceAnyTime;
internal static class MarkerPlacements
{
    private sealed record Pending(Player Player,Item Item,string Zone);
    private static readonly Dictionary<string,Pending> pending = new();
    private static readonly PlacementTimers timers = new();
    private static readonly List<KeyValuePair<string,Pending>> snapshot = new();
    private static GameWorld? world;
    private static readonly Action<IKillable> destroyedHandler=OnDestroyed;
    internal static void Start(Player player,Item item,string zone,Vector3 position,Quaternion rotation,float duration)
    {
        var current=Singleton<GameWorld>.Instance;
        if(current==null || current.MainPlayer?.Pointer!=player.Pointer) throw new InvalidOperationException("No local raid player");
        if(world?.Pointer!=current.Pointer) { Clear();world=current;world.OnLootItemDestroyed+=destroyedHandler; }
        if(!timers.Start(item.Id,Time.time,duration)) throw new InvalidOperationException("Invalid or duplicate marker timer");
        try {
            current.SetupItem(item,player.Cast<IPlayer>(),position,rotation);
            pending.Add(item.Id,new Pending(player,item,zone));
            VirtualDevices.Retire(item);
        } catch { timers.Cancel(item.Id);throw; }
    }
    private static void OnDestroyed(IKillable loot)
    {
        var dropped=loot.TryCast<EFT.Interactive.LootItem>();
        if(dropped==null) return;
        var id=dropped.ItemId;
        if(pending.Remove(id)) {
            timers.Cancel(id);
            if(MonoBehaviourSingleton<EFT.UI.GameUI>.Instantiated)
                MonoBehaviourSingleton<EFT.UI.GameUI>.Instance.BattleUiPanelExtraction.Close();
            GamePlayerOwner.MyPlayer?.UpdateInteractionCast();
        }
    }
    internal static void Tick()
    {
        if(world==null) return;
        if(Singleton<GameWorld>.Instance?.Pointer!=world.Pointer || world.MainPlayer==null || world.MainPlayer.Destroyed) { Clear();return; }
        if(pending.Count==0) return;
        snapshot.Clear();snapshot.AddRange(pending);
        foreach(var pair in snapshot) {
            var placement=pair.Value;
            try {
                if(!placement.Player.HealthController.IsAlive) { pending.Remove(pair.Key);timers.Cancel(pair.Key);world.DestroyLoot(pair.Key);continue; }
                if(!timers.TryComplete(pair.Key,Time.time)) continue;
                pending.Remove(pair.Key);
                world.DestroyLoot(pair.Key);
                placement.Player.PlantItemLocalOnly(placement.Item,placement.Zone);
                placement.Player.UpdateInteractionCast();
            } catch(Exception error) {
                pending.Remove(pair.Key);timers.Cancel(pair.Key);
                Plugin.Instance.Log.LogError("Virtual marker cancelled after runtime error: "+error);
            }
        }
        snapshot.Clear();
    }
    internal static void Clear()
    {
        if(world!=null) {
            world.OnLootItemDestroyed-=destroyedHandler;
            foreach(var id in pending.Keys) world.DestroyLoot(id);
            if(pending.Count>0 && MonoBehaviourSingleton<EFT.UI.GameUI>.Instantiated)
                MonoBehaviourSingleton<EFT.UI.GameUI>.Instance.BattleUiPanelExtraction.Close();
        }
        pending.Clear();snapshot.Clear();timers.Clear();world=null;
    }
}
