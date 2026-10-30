using AlwaysSpecial;
int passed=0;
void Check(bool condition,string name) { if(!condition) throw new Exception(name); passed++; }
Check(PlacementPolicy.IsSupported("5991b51486f77447b112d44f"),"marker");
Check(PlacementPolicy.IsSupported("5b4391a586f7745321235ab2"),"camera");
Check(PlacementPolicy.IsSupported("5ac78a9b86f7741cca0bbd8d"),"jammer");
Check(!PlacementPolicy.IsSupported("590c2e1186f77425357b6124"),"multitool excluded");
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
