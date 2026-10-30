# AnyPlaceAnyTime

Place ordinary quest-placement items without packing them: cameras, markers, jammers, radio repeaters, toolsets, food, weapons, armor, posters and more.

Version 0.2.0 (formerly AlwaysSpecial) targets SPT 5.0 / EFT 1.3.0.0.48484, Unity 6 and BepInXDX IL2CPP. Standalone SPT only: patches are disabled when Fika is detected. The installed quest catalog has 98 ordinary placement templates; 22 task-storage placement templates are explicitly excluded.

**Task-item storage stays normal:** quest documents, cases, samples and other items flagged QuestItem must still be found, carried and consumed. The plugin never supplies virtual copies or bypasses their consumption. Ordinary trader hand-ins also remain unchanged.

## Use and configuration

Open **F12 → AnyPlaceAnyTime** on James's installation. The Configuration Manager hotkey is configurable. The original **Special items** section retains:

- **Camera**
- **MS2000 Marker**
- **Signal Jammer**

**Other placement items** adds 95 individual toggles. Names include a short template ID to distinguish duplicate ammunition-pack names. Each switch defaults to enabled. Disable one to require and consume real items of that type as usual. Other switches remain independent. Task-storage items have no enable toggle.

Settings persist in `BepInEx/config/com.jpeckham.alwaysspecial.cfg`; edit offline if preferred. The legacy filename and plugin GUID are intentionally retained so the rename preserves your original settings.

Approach an active quest's valid placement spot and use the usual PLACE/Plant interaction. Native quest filtering, placement movement/timer and marker preview/animation remain in use. Marker protection duration still applies. Destroying the marker cancels completion and closes its countdown.

Enabled placement items are virtual and offered before real copies. They occupy no inventory or special slots, add no carried weight and are never inserted into profile inventory. Carried real copies remain untouched when a virtual item is used. Items are supplied only when they are targets of an active placement condition at the current zone.

Changes apply to new placements without restarting. Already-started placement/protection timers finish normally. Original device resources plus the radio repeater load with the raid so they can be enabled later. Ordinary food/weapon/armor placement does not require loading every item's model.

## Build and install

Requires .NET 10 SDK and the target installation's generated interop assemblies:

```powershell
dotnet build AnyPlaceAnyTime.csproj -c Release
# Override the default local 48484 installation:
dotnet build AnyPlaceAnyTime.csproj -c Release -p:SPTPath='C:\path\to\SPT'
```

Close the client and copy `Build/BepInEx/plugins/AnyPlaceAnyTime` into the game folder. **When upgrading from AlwaysSpecial, remove its old plugin folder first** (back it up outside BepInEx/plugins). Never load both assemblies together: they use the same compatibility GUID. Keep the existing config file. No server mod is required. Builds stage locally without automatically deploying or packaging game assemblies.

Source: [jpeckham/AnyPlaceAnyTime](https://github.com/jpeckham/AnyPlaceAnyTime), branch `spt-5.0-eft-1.3.0.0.48484`. Linked in [SPT-BE](https://github.com/jpeckham/SPT-BE).

## Implementation

The plugin discovers current-zone targets through native quest handlers and supplies transient native Items only while the local raid player's placement interaction queries inventory. The native game still matches quest conditions and builds its interactions. The versioned catalog admits only explicit QuestItem=false definitions; the native template flag is rechecked at runtime and creation/consumption gates fail closed. Prefab discovery adds only the devices requiring preview/quick-use assets.

The generic timed-placement completion hook records normal placement progress without removing the virtual item. Marker-style setup creates the normal world object, observes its destruction and waits the quest's protection duration before invoking normal local placement progress. Real task-storage items pass through to the game's original require/consume behavior. No fabricated inventory-removal request is sent to the server. Death or leaving the raid cancels pending protection work.

Configuration uses standard BepInEx boolean entries. Routine Update work handles settings refresh or pending timers; no global inventory scan runs every frame. The implementation is version-specific, including the generated camera completion callback.

## Verification and limitations

See [tests/README.md](tests/README.md) for commands, coverage and cleanup. Production/test builds pass with zero warnings/errors. Managed tests cover all catalog entries, task-item exclusions, config-key validity, the original eight toggle combinations and timer lifecycle. Native checks cover item construction/predicates, task-item consumption pass-through, native zone discovery, scoped inventory interception, configuration and prefab inclusion.

James confirmed the original three-device version works in game. **The expanded item set still needs player validation**, including ordinary-item placement and protected task-item consumption, radio-repeater preview/protection and saved progress. Synthetic checks do not prove all raid outcomes. The harness does not complete real quests or reset profiles.

The catalog is pinned to the installed 48484 quest data. Regenerate with `python tools/generate_catalog.py --database '<game>/SPT_Runtime/SPT_Data/database'` and review/test the diff when game or custom quest definitions change. Unknown templates are excluded until reviewed; this does not automatically grant every inventory item.

To uninstall, close the client and remove `BepInEx/plugins/AnyPlaceAnyTime`; ensure no old AlwaysSpecial plugin remains from an earlier installation. Config can be retained. Virtual items require no inventory cleanup; earned quest progress follows normal save behavior.
