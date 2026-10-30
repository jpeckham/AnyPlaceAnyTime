using BepInEx.Configuration;
namespace AlwaysSpecial;
internal static class Settings
{
    internal static ConfigEntry<bool> Camera=null!,Marker=null!,Jammer=null!;
    internal static void Bind(ConfigFile config)
    {
        const string section="Special items";
        Camera=config.Bind(section,"Camera",true,"Allow Wi-Fi camera placement without bringing a camera. Disable to require a real camera. Applies to new placements.");
        Marker=config.Bind(section,"MS2000 Marker",true,"Allow MS2000 marker placement without bringing a marker. Disable to require a real marker. Existing protection timers finish normally.");
        Jammer=config.Bind(section,"Signal Jammer",true,"Allow signal jammer placement without bringing a jammer. Disable to require a real jammer. Applies to new placements.");
    }
    internal static bool Enabled(string template) => PlacementPolicy.IsEnabled(template,Camera.Value,Marker.Value,Jammer.Value);
}
