using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("AnyPlaceAnyTime.RuntimeChecks")]
namespace AnyPlaceAnyTime;
[BepInPlugin("com.jpeckham.alwaysspecial", "AnyPlaceAnyTime", "0.2.0")]
[BepInDependency("com.sptushonka.core", "5.0.0")]
public sealed class Plugin : BasePlugin
{
    internal static Plugin Instance = null!;
    private Harmony? harmony;
    private MarkerPump? pump;
    private static volatile bool refreshInteractions;
    public override void Load()
    {
        Instance=this;
        Settings.Bind(Config);
        Config.SettingChanged+=OnSettingChanged;
        if(IL2CPPChainloader.Instance.Plugins.ContainsKey("com.fika.core")) {
            Log.LogError("AnyPlaceAnyTime supports standalone SPT only; patches not applied.");return;
        }
        harmony=new Harmony("com.jpeckham.alwaysspecial");
        try { harmony.PatchAll(typeof(Plugin).Assembly);pump=AddComponent<MarkerPump>(); }
        catch { harmony.UnpatchSelf(); throw; }
        Log.LogInfo($"AnyPlaceAnyTime ready: {PlacementCatalog.Entries.Length} placement templates with independent toggles; task-storage items excluded.");
    }
    public override bool Unload()
    {
        Config.SettingChanged-=OnSettingChanged;
        harmony?.UnpatchSelf(); MarkerPlacements.Clear(); VirtualDevices.Clear();
        if(pump!=null) UnityEngine.Object.Destroy(pump);
        return true;
    }
    private void OnSettingChanged(object? sender,BepInEx.Configuration.SettingChangedEventArgs args) => refreshInteractions=true;
    internal static void Tick()
    {
        if(refreshInteractions) {
            refreshInteractions=false;
            var player=Comfort.Common.Singleton<EFT.GameWorld>.Instance?.MainPlayer;
            if(player!=null && !player.Destroyed) player.OnPlaceItemTriggerChanged(player.PlaceItemZone);
        }
        MarkerPlacements.Tick();
    }
}
public sealed class MarkerPump : MonoBehaviour
{
    public MarkerPump(IntPtr pointer):base(pointer){}
    public void Update() => Plugin.Tick();
}
