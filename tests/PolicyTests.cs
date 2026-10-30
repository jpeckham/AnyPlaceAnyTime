using AnyPlaceAnyTime;
int passed=0;
void Check(bool condition,string name) { if(!condition) throw new Exception(name); passed++; }
Check(PlacementPolicy.IsSupported("5991b51486f77447b112d44f"),"marker");
Check(PlacementPolicy.IsSupported("5b4391a586f7745321235ab2"),"camera");
Check(PlacementPolicy.IsSupported("5ac78a9b86f7741cca0bbd8d"),"jammer");
Check(PlacementPolicy.IsSupported("590c2e1186f77425357b6124"),"toolset placement included");
Check(PlacementPolicy.IsSupported("63a0b2eabea67a6d93009e52"),"radio repeater included");
Check(PlacementPolicy.IsSupported("5448fee04bdc2dbc018b4567"),"water placement included");
Check(!PlacementPolicy.IsSupported("544fb25a4bdc2dfb738b4567"),"non-placement item excluded");
foreach(var taskItem in PlacementCatalog.ExcludedTaskItemIds) {
    Check(!PlacementPolicy.IsSupported(taskItem),"task-storage catalog exclusion");
    Check(!PlacementPolicy.CanSupply(taskItem,true,true),"task-storage item cannot be supplied");
}
foreach(var entry in PlacementCatalog.Entries) {
    Check(!PlacementPolicy.ConfigKey(entry).Any(c=>new[]{'=','\n','\t','\\','"','\'','[',']'}.Contains(c)),"valid BepInEx config key");
    Check(PlacementPolicy.CanSupply(entry.Id,false,true),"ordinary placement allowed");
    Check(!PlacementPolicy.CanSupply(entry.Id,true,true),"native task flag overrides catalog");
    Check(!PlacementPolicy.CanSupply(entry.Id,null,true),"unknown template rejected");
    Check(!PlacementPolicy.CanSupply(entry.Id,false,false),"disabled item rejected");
}
Check(!PlacementPolicy.IsSupported(null),"null excluded");
for(var mask=0;mask<8;mask++) {
    bool camera=(mask&1)!=0,marker=(mask&2)!=0,jammer=(mask&4)!=0;
    Check(PlacementPolicy.IsEnabled("5b4391a586f7745321235ab2",camera,marker,jammer)==camera,"independent camera toggle");
    Check(PlacementPolicy.IsEnabled("5991b51486f77447b112d44f",camera,marker,jammer)==marker,"independent marker toggle");
    Check(PlacementPolicy.IsEnabled("5ac78a9b86f7741cca0bbd8d",camera,marker,jammer)==jammer,"independent jammer toggle");
    Check(!PlacementPolicy.IsEnabled("590c2e1186f77425357b6124",camera,marker,jammer),"unrelated items never enabled");
}
var timers=new PlacementTimers();
Check(timers.Start("a",10,30),"start marker");
Check(!timers.Start("a",10,30),"duplicate setup rejected");
Check(!timers.TryComplete("a",39),"not early");
Check(timers.TryComplete("a",40),"normal protection duration");
Check(!timers.TryComplete("a",41),"no double credit");
timers.Start("b",10,30);timers.Cancel("b");
Check(!timers.TryComplete("b",100),"destroyed marker no credit");
timers.Start("c",10,30);timers.Clear();
Check(!timers.TryComplete("c",100),"raid exit no credit");
Check(!timers.Start("bad",10,float.NaN),"invalid duration rejected");
Check(!timers.Start("negative",10,-1),"negative duration rejected");
Check(timers.Start("next",50,0)&&timers.TryComplete("next",50),"zero duration supported");
Console.WriteLine($"PASS: {passed} placement policy/lifecycle checks");
