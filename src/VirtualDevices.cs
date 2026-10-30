using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
namespace AnyPlaceAnyTime;
internal static class VirtualDevices
{
    private static readonly Dictionary<string,Item> available = new();
    private static readonly HashSet<string> virtualIds = new();
    private static IntPtr factoryPointer;
    [ThreadStatic] internal static Inventory? InventoryScope;
    [ThreadStatic] internal static string[]? TargetScope;
    internal static bool IsVirtual(Item? item) => item!=null && !item.QuestItem && virtualIds.Contains(item.Id) && PlacementPolicy.IsSupported(item.StringTemplateId);
    internal static bool CanSupply(string template)
    {
        return IsOrdinaryTemplate(template) && Settings.Enabled(template);
    }
    internal static bool IsOrdinaryTemplate(string template)
    {
        if(!Singleton<ItemFactory>.Instantiated) return false;
        var definitions=Singleton<ItemFactory>.Instance.ItemTemplates;
        return PlacementPolicy.IsSupported(template) && definitions.TryGetValue(new MongoID(template),out var definition)
            && PlacementPolicy.CanSupply(template,definition?.QuestItem,true);
    }
    internal static Item Get(string template)
    {
        var factory=Singleton<ItemFactory>.Instance;
        if(!PlacementPolicy.IsSupported(template) || !factory.ItemTemplates.TryGetValue(new MongoID(template),out var definition) || definition==null || definition.QuestItem)
            throw new InvalidOperationException("AnyPlaceAnyTime refuses task-storage or unknown template: "+template);
        if(factoryPointer!=factory.Pointer) { available.Clear();virtualIds.Clear();factoryPointer=factory.Pointer; }
        if(!available.TryGetValue(template,out var item)) {
            item=factory.CreateItem(Guid.NewGuid().ToString("N")[..24],template,null);
            available.Add(template,item);virtualIds.Add(item.Id);
        }
        return item;
    }
    internal static void Retire(Item item) => available.Remove(item.StringTemplateId);
    internal static void Clear() { available.Clear();virtualIds.Clear();InventoryScope=null;TargetScope=null; }
    internal static Il2CppSystem.Collections.Generic.IEnumerable<Item> Include(Inventory inventory,Il2CppSystem.Collections.Generic.IEnumerable<Item> original)
    {
        if(InventoryScope==null || InventoryScope.Pointer!=inventory.Pointer) return original;
        var items=new Il2CppSystem.Collections.Generic.List<Item>();
        foreach(var template in TargetScope ?? PlacementPolicy.Templates) if(CanSupply(template)) items.Add(Get(template));
        foreach(var item in original) items.Add(item);
        return items.Cast<Il2CppSystem.Collections.Generic.IEnumerable<Item>>();
    }
}
