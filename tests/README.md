# Repeatable verification

From the mod repo root, using .NET 10 SDK and the matching 48484 interop/runtime assemblies:

```powershell
dotnet run --project tests/PolicyTests.csproj -c Release
dotnet build AnyPlaceAnyTime.csproj -c Release
dotnet build tests/RuntimeChecks.csproj -c Release
```

Build commands accept `-p:SPTPath='<game>'`. Expected managed result: **584 checks pass**. Covers all 98 ordinary catalog entries, 22 task-storage exclusions, native-flag override policy, config-key validity, the original eight toggle combinations and timer lifecycle. Tests were observed failing before expanded support was implemented.

## Native client checks

With the client closed, install production files, then temporarily copy **only** `tests/bin/Release/net10.0/AnyPlaceAnyTime.RuntimeChecks.dll` into `BepInEx/plugins/AnyPlaceAnyTime`.

Start matching server/client with an appropriate test account. Parent SPT-BE `scripts/README.md` documents its start/stop helpers, process identity checks and log capture. Expect a PASS line for 98 native items/predicates, 22 protected task items rejected, native zone discovery/filtering, an extra-item toggle, original toggle combinations, inventory isolation, beacon resources and five patched targets.

Inventory fixtures are native identity-only objects with no containers; the harness calls native GetPlayerItems using the equipment/quest-item flags. It never inserts items into a real inventory. Config tests temporarily turn off auto-save and restore original values in finally. The first native attempt exposed the test's use of Item.Parent, which throws for unattached items; CurrentAddress is the correct property and is now used.

`ZoneFixture.cs` creates a separate native QuestBook with actual LeaveItemAtLocation and PlaceBeacon conditions for a toolset, protected quest document and radio repeater. The production TargetsForZone method calls native GetConditionHandlersByZone and returns only the two ordinary targets; an unrelated zone returns nothing and disabling the toolset removes it. No fixture is attached to a real profile. Each protected item also exercises the completion prefix directly, which must return true so the original require/consume path runs.

The expanded menu's first startup caught quoted item names invalid for BepInEx keys. ConfigKey now sanitizes all forbidden characters; managed catalog tests and successful plugin startup verify the fix.

Stop the client, preserve the log locally and remove AnyPlaceAnyTime.RuntimeChecks.dll. Stop the test server. Do not package test DLLs, logs or profile backups. These tests do not enter a raid, complete quests or purchase items. Normal startup may update profile timestamps.

## Manual acceptance checks — pending

Use a disposable account with applicable active quests; back it up with the server stopped.

James confirmed the original camera/marker/jammer version works. Prioritize toolset/food/weapon placement, radio-repeater protection and a genuine task-storage item for the expanded release.

1. With none packed, verify each enabled device produces its normal interaction at an eligible location.
2. Disable one in F12: no free action for that device. With a real copy, verify normal availability/consumption while disabled.
3. Enable it with a real copy packed: complete placement and confirm the real copy remains.
4. Interrupt camera placement; verify no credit and successful retry.
5. For marker-style placement, check preview, hand animation, full protection duration and cancellation/UI closure if destroyed.
6. Repeat across objectives, switch one toggle without affecting the others, and toggle after an action has started.
7. Extract/restart: verify saved quest progress and real inventory. Die/leave during protection: no delayed credit in a later raid.
8. At a task-storage objective, verify no free action without the actual document/case/sample; bring it and confirm it is consumed normally.

The synthetic harness does not prove the native GetAvailableActions call path end-to-end or quick-use acceptance of virtual markers. These remain explicit raid checks.

## Catalog regeneration

```powershell
python tools/generate_catalog.py --database '<game>/SPT_Runtime/SPT_Data/database'
```

The generator reads only installed quest/item/localization data and writes the reviewable source catalog. It includes LeaveItemAtLocation/PlaceBeacon targets only with explicit QuestItem=false, excludes missing/true flags, and records protected IDs for tests. Review the diff and rerun all checks after a game/quest-definition update. Keep source metadata inspection, catalog generation and tests in this repo; local process orchestration remains in SPT-BE.

## Saved inspection tool

`tools/Inspect` uses Mono.Cecil to read locally owned assemblies:

```powershell
dotnet run --project tools/Inspect -- '<interop>/Assembly-CSharp.dll' '^EFT.InteractionContextHelper' 'GetAvailableActions'
dotnet run --project tools/Inspect -- '<managed-assembly>' '<type-regex>' '<method-regex>' --il
dotnet run --project tools/Inspect -- '<managed-assembly>' '.' '<called-member-regex>' --callers
```

Generated interop wrappers do not contain native game implementations. Earlier 4.1 managed IL informed semantic research; it is not proof of current behavior. Keep extracted output/assemblies out of source control. Local evidence is under parent `.local-build-attempt/always-special`.
