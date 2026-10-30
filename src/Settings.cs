using BepInEx.Configuration;
namespace AnyPlaceAnyTime;
internal static class Settings
{
    internal static ConfigEntry<bool> Camera=null!,Marker=null!,Jammer=null!;
    internal static readonly Dictionary<string,ConfigEntry<bool>> Additional = new();
    internal static void Bind(ConfigFile config)
    {
        const string section="Special items";
        Camera=config.Bind(section,"Camera",true,"Allow Wi-Fi camera placement without bringing a camera. Disable to require a real camera. Applies to new placements.");
        Marker=config.Bind(section,"MS2000 Marker",true,"Allow MS2000 marker placement without bringing a marker. Disable to require a real marker. Existing protection timers finish normally.");
        Jammer=config.Bind(section,"Signal Jammer",true,"Allow signal jammer placement without bringing a jammer. Disable to require a real jammer. Applies to new placements.");
        Additional.Clear();
        foreach(var entry in PlacementCatalog.Entries) {
            if(Array.IndexOf(PlacementPolicy.Templates,entry.Id)>=0) continue;
            var key=PlacementPolicy.ConfigKey(entry);
            Additional.Add(entry.Id,config.Bind("Other placement items",key,true,
                "Allow quest placement of "+entry.Name+" without bringing it. Disable to require and consume real items. Task-storage items are always excluded. Template: "+entry.Id));
        }
    }
    internal static bool Enabled(string template) => Additional.TryGetValue(template,out var setting) ? setting.Value : PlacementPolicy.IsEnabled(template,Camera.Value,Marker.Value,Jammer.Value);
}
