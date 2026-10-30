# AlwaysSpecial Implementation Plan

**Goal:** Place cameras, MS2000 markers and signal jammers without packing them on local SPT 5 / EFT 48484.

**Architecture:** Client-only BepInXDX IL2CPP plugin. Supply transient native items only while the game's placement interaction queries inventory. Preserve the game's condition filtering, camera placement movement/timer, marker preview/quick-use animation and protection time. Handle virtual device consumption without inventory mutations. Actual carried devices remain untouched because virtual equivalents are offered first.

**Tech stack:** .NET 10, Harmony, generated IL2CPP assemblies; managed executable tests and temporary in-client checks. Work directly in the new dedicated version branch. User requested implementation and installation after the concept repo; proceed inline.

## Alternatives and decisions

- Supplying real items to special slots would occupy space, require replenishment and affect saves; rejected.
- Replacing all placement UI/timing would duplicate game rules; rejected.
- Scoped virtual items keep native availability/interaction handling. Camera completion bypasses only virtual removal; marker setup creates the normal world object, observes its destruction, waits the quest's protection duration, then invokes normal placement progress. No synthetic inventory request is sent to the server.
- Scope exactly the three verified item template IDs. Keep unrelated items and nonlocal players untouched. Standalone SPT only; no Fika support claimed.

## Tasks

- [x] Save metadata inspection tool and document current native API findings.
- [x] Write failing managed policy/lifecycle tests (allowlist, cancellation, expiry, single completion, raid isolation); implement policy.
- [x] Add plugin and narrow Harmony patches; stage a self-contained package with no game assemblies.
- [x] Build temporary native harness for item construction, scoped enumeration and resource inclusion. Run on test startup without modifying quest progress.
- [x] Review diffs, install production only, remove harness and stop owned test processes; record hashes and remaining manual raid checks.
- [x] Commit/push mod and parent submodule/documentation updates.

## Verification limits

Metadata comes from the current local interop assemblies. Read-only IL inspection of the previous 4.1 managed client helps explain semantics but is not proof of current native behavior. Real-raid cancellation, marker animation/destruction and saved quest progression require player validation. All inspection output, logs and profiles remain ignored local artifacts. Reusable tools and tests live in this repo.

## Execution record

User added independent configuration toggles during implementation. Added Camera, MS2000 Marker and Signal Jammer BepInEx entries, all true by default, visible through the existing F12 Configuration Manager. Changes refresh placement interactions and affect new placements; pending actions finish normally. Preload all three assets even while disabled to support enabling mid-raid.

47 managed tests pass, including all eight switch combinations. Native checks passed for item construction/current address, actual native placement predicates, real-item exclusion, scoped native GetPlayerItems calls with all toggle combinations, other-inventory isolation, resource inclusion and five Harmony targets. Builds: zero warnings/errors. No quest progress was changed by the harness.

Review fixes: close protection UI when a virtual marker is destroyed; clear the reusable snapshot after completion/cleanup to avoid retaining previous raid objects. Local process tools and profile backup were used; temporary harness removed afterward. Full raid acceptance remains pending and is listed in tests/README.md.
