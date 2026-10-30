using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
namespace AlwaysSpecial;
internal static class VirtualDevices
{
    private static readonly Dictionary<string,Item> available = new();
    private static readonly HashSet<string> virtualIds = new();
    private static IntPtr factoryPointer;
    [ThreadStatic] internal static Inventory? InventoryScope;
    internal static bool IsVirtual(Item? item) => item!=null && virtualIds.Contains(item.Id) && PlacementPolicy.IsSupported(item.StringTemplateId);
    internal static Item Get(string template)
    {
        var factory=Singleton<ItemFactory>.Instance;
        if(factoryPointer!=factory.Pointer) { available.Clear();virtualIds.Clear();factoryPointer=factory.Pointer; }
        if(!available.TryGetValue(template,out var item)) {
            item=factory.CreateItem(Guid.NewGuid().ToString("N")[..24],template,null);
            available.Add(template,item);virtualIds.Add(item.Id);
        }
        return item;
    }
    internal static void Retire(Item item) => available.Remove(item.StringTemplateId);
    internal static void Clear() { available.Clear();virtualIds.Clear();InventoryScope=null; }
    internal static Il2CppSystem.Collections.Generic.IEnumerable<Item> Include(Inventory inventory,Il2CppSystem.Collections.Generic.IEnumerable<Item> original)
    {
        if(InventoryScope==null || InventoryScope.Pointer!=inventory.Pointer) return original;
        var items=new Il2CppSystem.Collections.Generic.List<Item>();
        foreach(var template in PlacementPolicy.Templates) if(Settings.Enabled(template)) items.Add(Get(template));
        foreach(var item in original) items.Add(item);
        return items.Cast<Il2CppSystem.Collections.Generic.IEnumerable<Item>>();
    }
}
