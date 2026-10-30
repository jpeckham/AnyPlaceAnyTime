using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Quests;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
namespace AnyPlaceAnyTime;
[HarmonyPatch]
internal static class AvailabilityScopePatch
{
    internal readonly record struct Scope(Inventory? Inventory,string[]? Targets);
    static MethodBase TargetMethod() => AccessTools.Method(typeof(InteractionContextHelper),nameof(InteractionContextHelper.GetAvailableActions),[typeof(GamePlayerOwner),typeof(PlaceItemTrigger)]);
    static void Prefix(GamePlayerOwner __0,PlaceItemTrigger __1,ref Scope __state)
    {
        __state=new(VirtualDevices.InventoryScope,VirtualDevices.TargetScope);
        VirtualDevices.InventoryScope=null;
        VirtualDevices.TargetScope=null;
        var player=__0?.Player;
        var world=Singleton<GameWorld>.Instance;
        if(player!=null && player.IsYourPlayer && world!=null && world.MainPlayer?.Pointer==player.Pointer && world.TryCast<HideoutGameWorld>()==null && __1!=null) {
            VirtualDevices.TargetScope=TargetsForZone(player.QuestController.Quests,__1.Id);
            VirtualDevices.InventoryScope=player.Profile.Inventory;
        }
    }
    internal static string[] TargetsForZone(QuestBook quests,string zoneId)
    {
            var targets=new HashSet<string>();
            foreach(var handler in quests.GetConditionHandlersByZone<ConditionZone>(zoneId)) {
                var condition=handler.Condition;
                if(condition==null || handler.CurrentValue>=condition.value) continue;
                if(condition.TryCast<ConditionLeaveItemAtLocation>()==null && condition.TryCast<ConditionPlaceBeacon>()==null) continue;
                var zone=condition.Cast<ConditionZone>();
                if(zone.target!=null) foreach(var template in zone.target) if(VirtualDevices.CanSupply(template)) targets.Add(template);
            }
            return targets.ToArray();
    }
    static Exception? Finalizer(Exception? __exception,Scope __state)
    {
        VirtualDevices.InventoryScope=__state.Inventory;
        VirtualDevices.TargetScope=__state.Targets;
        return __exception;
    }
}
[HarmonyPatch(typeof(Inventory),nameof(Inventory.GetPlayerItems))]
internal static class InventoryQueryPatch
{
    static void Postfix(Inventory __instance,ref Il2CppSystem.Collections.Generic.IEnumerable<Item> __result) => __result=VirtualDevices.Include(__instance,__result);
}
[HarmonyPatch(typeof(Profile),nameof(Profile.GetAllPrefabPaths))]
internal static class ResourcePatch
{
    internal static void Postfix(ref Il2CppSystem.Collections.Generic.IEnumerable<ResourceKey> __result)
    {
        if(!Singleton<ItemFactory>.Instantiated) return;
        var resources=new Il2CppSystem.Collections.Generic.List<ResourceKey>();
        var paths=new HashSet<string>();
        foreach(var resource in __result) if(paths.Add(resource.path)) resources.Add(resource);
        foreach(var entry in PlacementCatalog.Entries.Where(entry=>entry.NeedsPrefab && VirtualDevices.IsOrdinaryTemplate(entry.Id)))
            foreach(var resource in VirtualDevices.Get(entry.Id).Template.AllResources)
                if(resource!=null && !string.IsNullOrEmpty(resource.path) && paths.Add(resource.path)) resources.Add(resource);
        __result=resources.Cast<Il2CppSystem.Collections.Generic.IEnumerable<ResourceKey>>();
    }
}
[HarmonyPatch(typeof(InteractionContextHelper.__c__DisplayClass8_0),"_GetAvailableActions_b__2")]
internal static class CameraCompletionPatch
{
    internal static bool Prefix(InteractionContextHelper.__c__DisplayClass8_0 __instance,bool __0)
    {
        if(!VirtualDevices.IsVirtual(__instance.resultItem)) return true;
        var owner=__instance.owner;
        owner.Player.PlantItemNetwork(__instance.resultItem.StringTemplateId,__instance.itemTrigger.Id,__0);
        owner.CloseObjectivesPanel();
        if(__0) owner.ClearInteractionState();
        return false;
    }
}
[HarmonyPatch(typeof(Player.PlayerInventoryController),nameof(Player.PlayerInventoryController.SetupItem))]
internal static class MarkerSetupPatch
{
    static bool Prefix(Player.PlayerInventoryController __instance,Item __0,string __1,Vector3 __2,Quaternion __3,float __4,Callback __5)
    {
        if(!VirtualDevices.IsVirtual(__0)) return true;
        try {
            MarkerPlacements.Start(__instance.Player,__0,__1,__2,__3,__4);
            __5?.Invoke(SuccessfulResult.New);
        } catch(Exception error) {
            Plugin.Instance.Log.LogError("Virtual marker setup failed: "+error);
            __5?.Invoke(new FailedResult(error.Message,0).Cast<IResult>());
        }
        return false;
    }
}
