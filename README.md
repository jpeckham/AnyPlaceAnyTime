# AlwaysSpecial

Place Wi-Fi cameras, MS2000 markers and signal jammers without packing them.

Version 0.1.0 targets SPT 5.0 / EFT 1.3.0.0.48484, Unity 6 and BepInXDX IL2CPP. Standalone SPT only: patches are disabled when Fika is detected.

## Use and configuration

Open **F12 → AlwaysSpecial → Special items** on James's installation. The Configuration Manager hotkey is configurable.

- **Camera**
- **MS2000 Marker**
- **Signal Jammer**

Each switch defaults to enabled. Disable one to require and consume real devices of that type as usual. Other switches remain independent. Settings persist in `BepInEx/config/com.jpeckham.alwaysspecial.cfg`; edit offline if preferred.

Approach an active quest's valid placement spot and use the usual PLACE/Plant interaction. Native quest filtering, placement movement/timer and marker preview/animation remain in use. Marker protection duration still applies. Destroying the marker cancels completion and closes its countdown.

Enabled devices are virtual and offered before real copies. They occupy no inventory or special slots, add no carried weight and are never inserted into profile inventory. Carried real copies remain untouched when a virtual device is used. Ordinary hand-in items and unrelated special equipment are excluded.

Changes apply to new placements without restarting. Already-started placement/protection timers finish normally. All three devices' resources load with the raid so an initially disabled device can be enabled later.

## Build and install

Requires .NET 10 SDK and the target installation's generated interop assemblies:

```powershell
dotnet build AlwaysSpecial.csproj -c Release
# Override the default local 48484 installation:
dotnet build AlwaysSpecial.csproj -c Release -p:SPTPath='C:\path\to\SPT'
```

Close the client and copy `Build/BepInEx/plugins/AlwaysSpecial` into the game folder. No server mod is required. Builds stage locally without automatically deploying or packaging game assemblies.

Source: [jpeckham/AlwaysSpecial](https://github.com/jpeckham/AlwaysSpecial), branch `spt-5.0-eft-1.3.0.0.48484`. Linked in [SPT-BE](https://github.com/jpeckham/SPT-BE).

## Implementation

The plugin supplies transient native Items only while the local raid player's placement interaction queries inventory. The native game still matches quest conditions and builds its interactions. Prefab discovery includes the devices' resources, including use/animation prefabs.

The camera completion hook records normal placement progress without removing the virtual item. Marker/jammer setup creates the normal world object, observes its destruction and waits the quest's protection duration before invoking normal local placement progress. No fabricated inventory-removal request is sent to the server. Death or leaving the raid cancels pending protection work.

Configuration uses standard BepInEx boolean entries. Routine Update work handles settings refresh or pending timers; no global inventory scan runs every frame. The implementation is version-specific, including the generated camera completion callback.

## Verification and limitations

See [tests/README.md](tests/README.md) for commands, coverage and cleanup. Production/test builds pass with zero warnings/errors. Managed tests cover all eight toggle combinations and timer lifecycle. Native checks cover item construction, native placement predicates, real-item exclusion, native inventory interception, configuration combinations, prefab inclusion and installed hook targets.

**Full raid behavior still needs player validation:** prompt generation end-to-end, quick-use/preview, interruption/destruction, repeated placements and quest progress after extraction/restart. Startup and synthetic native checks do not prove these outcomes. The harness does not complete quests or reset profiles.

To uninstall, close the client and remove `BepInEx/plugins/AlwaysSpecial`. Config can be retained. Virtual devices require no inventory cleanup; earned quest progress follows normal save behavior.
