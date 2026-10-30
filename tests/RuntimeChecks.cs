using AlwaysSpecial;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.Quests;
using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;
namespace AlwaysSpecialChecks;
[BepInPlugin("com.jpeckham.alwaysspecial.checks","AlwaysSpecial Runtime Checks","1.0.0")]
[BepInDependency("com.jpeckham.alwaysspecial","0.1.0")]
public sealed class RuntimeChecks : BasePlugin
{
    internal static RuntimeChecks Instance=null!;
    private bool ran;
    private readonly DateTime ready=DateTime.UtcNow.AddSeconds(20);
    private readonly DateTime deadline=DateTime.UtcNow.AddMinutes(3);
    public override void Load() { Instance=this;AddComponent<CheckPump>(); }
    private void Check(bool value,string message) { if(!value) throw new Exception(message); }
    internal void Tick()
    {
        if(ran || DateTime.UtcNow<ready) return;
        if(!Singleton<ItemFactory>.Instantiated && DateTime.UtcNow<deadline) return;
        ran=true;
        var config=AlwaysSpecial.Plugin.Instance.Config;
        var save=config.SaveOnConfigSet;
        var originalCamera=Settings.Camera.Value;var originalMarker=Settings.Marker.Value;var originalJammer=Settings.Jammer.Value;
        config.SaveOnConfigSet=false;
        try {
            Settings.Camera.Value=Settings.Marker.Value=Settings.Jammer.Value=true;
            Check(Singleton<ItemFactory>.Instantiated,"Item templates never initialized");
            foreach(var template in PlacementPolicy.Templates) {
                var item=VirtualDevices.Get(template);
                Check(item.StringTemplateId==template && item.CurrentAddress==null && VirtualDevices.IsVirtual(item),"Native virtual device creation");
                var actual=Singleton<ItemFactory>.Instance.CreateItem(Guid.NewGuid().ToString("N")[..24],template,null);
                Check(!VirtualDevices.IsVirtual(actual),"Real devices must not be classified virtual");
                var condition=new ConditionZone { target=new[]{template} };
                var predicate=new InteractionContextHelper.__c__DisplayClass8_1 { conditionZone=condition };
                Check(predicate.Method_Internal_Boolean_Item_PDM_0(item),"Native placement predicate rejects virtual device");
                condition.target=new[]{"590c2e1186f77425357b6124"};
                Check(!predicate.Method_Internal_Boolean_Item_PDM_0(item),"Unrelated quest must not match");
            }
            // Identity-only native instances: no constructor/profile/raid mutation.
            var inventory=new Inventory(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<Inventory>.NativeClassPtr));
            var other=new Inventory(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<Inventory>.NativeClassPtr));
            var empty=new Il2CppSystem.Collections.Generic.List<Item>().Cast<Il2CppSystem.Collections.Generic.IEnumerable<Item>>();
            Check(VirtualDevices.Include(inventory,empty).Pointer==empty.Pointer,"No devices outside placement scope");
            VirtualDevices.InventoryScope=inventory;
            Check(VirtualDevices.Include(other,empty).Pointer==empty.Pointer,"Other inventory isolation");
            var count=0;foreach(var item in inventory.GetPlayerItems((EPlayerItems)6)) { Check(VirtualDevices.IsVirtual(item),"Only virtual devices injected by native inventory hook");count++; }
            Check(count==3,"All three devices supplied to empty inventory query");
            for(int mask=0;mask<8;mask++) {
                Settings.Camera.Value=(mask&1)!=0;Settings.Marker.Value=(mask&2)!=0;Settings.Jammer.Value=(mask&4)!=0;
                var supplied=new HashSet<string>();foreach(var item in inventory.GetPlayerItems((EPlayerItems)6)) supplied.Add(item.StringTemplateId);
                foreach(var template in PlacementPolicy.Templates) Check(supplied.Contains(template)==Settings.Enabled(template),"Configuration toggle did not control native availability");
            }
            VirtualDevices.InventoryScope=null;
            var resources=new Il2CppSystem.Collections.Generic.List<ResourceKey>().Cast<Il2CppSystem.Collections.Generic.IEnumerable<ResourceKey>>();
            ResourcePatch.Postfix(ref resources);
            var paths=new HashSet<string>();foreach(var resource in resources) paths.Add(resource.path);
            foreach(var template in PlacementPolicy.Templates) Check(paths.Contains(VirtualDevices.Get(template).Prefab.path),"Missing virtual device prefab in resource load list");
            Check(Harmony.GetAllPatchedMethods().Count(m=>Harmony.GetPatchInfo(m)?.Owners.Contains("com.jpeckham.alwaysspecial")==true)==5,"Expected five native hooks");
            Log.LogInfo("PASS: three native devices, placement predicates, real-item exclusion, scoped empty-inventory availability, all eight config combinations, inventory isolation, prefab inclusion and five patched targets");
        } catch(Exception error) { Log.LogError("FAIL: "+error); }
        finally {
            VirtualDevices.InventoryScope=null;
            Settings.Camera.Value=originalCamera;Settings.Marker.Value=originalMarker;Settings.Jammer.Value=originalJammer;
            config.SaveOnConfigSet=save;
        }
    }
}
public sealed class CheckPump : MonoBehaviour
{
    public CheckPump(IntPtr pointer):base(pointer){}
    public void Update()=>RuntimeChecks.Instance.Tick();
}
