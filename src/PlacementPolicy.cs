namespace AlwaysSpecial;
internal static class PlacementPolicy
{
    public static readonly string[] Templates = ["5991b51486f77447b112d44f", "5b4391a586f7745321235ab2", "5ac78a9b86f7741cca0bbd8d"];
    public static bool IsSupported(string? template) => template != null && Array.IndexOf(Templates, template) >= 0;
    public static bool IsEnabled(string? template,bool camera,bool marker,bool jammer) => template switch {
        "5b4391a586f7745321235ab2" => camera,
        "5991b51486f77447b112d44f" => marker,
        "5ac78a9b86f7741cca0bbd8d" => jammer,
        _ => false
    };
}
internal sealed class PlacementTimers
{
    private readonly Dictionary<string,float> deadlines = new();
    public bool Start(string id,float now,float duration) => float.IsFinite(now) && float.IsFinite(duration) && duration >= 0 && deadlines.TryAdd(id,now+duration);
    public bool TryComplete(string id,float now)
    {
        if(!deadlines.TryGetValue(id,out var deadline) || now < deadline) return false;
        deadlines.Remove(id);
        return true;
    }
    public void Cancel(string id) => deadlines.Remove(id);
    public void Clear() => deadlines.Clear();
}
