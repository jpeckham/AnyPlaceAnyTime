# Repeatable verification

From the mod repo root, using .NET 10 SDK and the matching 48484 interop/runtime assemblies:

```powershell
dotnet run --project tests/PolicyTests.csproj -c Release
dotnet build AlwaysSpecial.csproj -c Release
dotnet build tests/RuntimeChecks.csproj -c Release
```

Build commands accept `-p:SPTPath='<game>'`. Expected managed result: **47 checks pass**. Covers the three-item allowlist, all eight toggle combinations, unrelated-item exclusion, duplicate setup, early/normal completion, single credit, destruction cancellation, raid isolation and invalid timer input. Initial policy and toggle tests were observed failing before implementation.

## Native client checks

With the client closed, install production files, then temporarily copy **only** `tests/bin/Release/net10.0/AlwaysSpecial.RuntimeChecks.dll` into `BepInEx/plugins/AlwaysSpecial`.

Start matching server/client with an appropriate test account. Parent SPT-BE `scripts/README.md` documents its start/stop helpers, process identity checks and log capture. Expect a PASS line in BepInEx/LogOutput.log covering native devices, native placement predicates, real-item exclusion, scoped native inventory queries, all eight config combinations, inventory isolation, prefab inclusion and five patched targets.

Inventory fixtures are native identity-only objects with no containers; the harness calls native GetPlayerItems using the equipment/quest-item flags. It never inserts items into a real inventory. Config tests temporarily turn off auto-save and restore original values in finally. The first native attempt exposed the test's use of Item.Parent, which throws for unattached items; CurrentAddress is the correct property and is now used.

Stop the client, preserve the log locally and remove AlwaysSpecial.RuntimeChecks.dll. Stop the test server. Do not package test DLLs, logs or profile backups. These tests do not enter a raid, complete quests or purchase items. Normal startup may update profile timestamps.

## Manual acceptance checks — pending

Use a disposable account with applicable active quests; back it up with the server stopped.

1. With none packed, verify each enabled device produces its normal interaction at an eligible location.
2. Disable one in F12: no free action for that device. With a real copy, verify normal availability/consumption while disabled.
3. Enable it with a real copy packed: complete placement and confirm the real copy remains.
4. Interrupt camera placement; verify no credit and successful retry.
5. For marker-style placement, check preview, hand animation, full protection duration and cancellation/UI closure if destroyed.
6. Repeat across objectives, switch one toggle without affecting the others, and toggle after an action has started.
7. Extract/restart: verify saved quest progress and real inventory. Die/leave during protection: no delayed credit in a later raid.

The synthetic harness does not prove the native GetAvailableActions call path end-to-end or quick-use acceptance of virtual markers. These remain explicit raid checks.

## Saved inspection tool

`tools/Inspect` uses Mono.Cecil to read locally owned assemblies:

```powershell
dotnet run --project tools/Inspect -- '<interop>/Assembly-CSharp.dll' '^EFT.InteractionContextHelper' 'GetAvailableActions'
dotnet run --project tools/Inspect -- '<managed-assembly>' '<type-regex>' '<method-regex>' --il
dotnet run --project tools/Inspect -- '<managed-assembly>' '.' '<called-member-regex>' --callers
```

Generated interop wrappers do not contain native game implementations. Earlier 4.1 managed IL informed semantic research; it is not proof of current behavior. Keep extracted output/assemblies out of source control. Local evidence is under parent `.local-build-attempt/always-special`.
