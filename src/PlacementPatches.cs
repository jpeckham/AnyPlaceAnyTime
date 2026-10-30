using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
namespace AlwaysSpecial;
[HarmonyPatch]
internal static class AvailabilityScopePatch
{
    static MethodBase TargetMethod() => AccessTools.Method(typeof(InteractionContextHelper),nameof(InteractionContextHelper.GetAvailableActions),[typeof(GamePlayerOwner),typeof(PlaceItemTrigger)]);
    static void Prefix(GamePlayerOwner __0,ref Inventory? __state)
    {
        __state=VirtualDevices.InventoryScope;
        VirtualDevices.InventoryScope=null;
        var player=__0?.Player;
        var world=Singleton<GameWorld>.Instance;
        if(player!=null && player.IsYourPlayer && world!=null && world.MainPlayer?.Pointer==player.Pointer && world.TryCast<HideoutGameWorld>()==null)
            VirtualDevices.InventoryScope=player.Profile.Inventory;
    }
    static Exception? Finalizer(Exception? __exception,Inventory? __state)
    {
        VirtualDevices.InventoryScope=__state;
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
        foreach(var template in PlacementPolicy.Templates)
            foreach(var resource in VirtualDevices.Get(template).Template.AllResources)
                if(resource!=null && !string.IsNullOrEmpty(resource.path) && paths.Add(resource.path)) resources.Add(resource);
        __result=resources.Cast<Il2CppSystem.Collections.Generic.IEnumerable<ResourceKey>>();
    }
}
[HarmonyPatch(typeof(InteractionContextHelper.__c__DisplayClass8_0),"_GetAvailableActions_b__2")]
internal static class CameraCompletionPatch
{
    static bool Prefix(InteractionContextHelper.__c__DisplayClass8_0 __instance,bool __0)
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
