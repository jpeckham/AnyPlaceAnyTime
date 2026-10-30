# AlwaysSpecial

An SPT mod idea by James Peckham: **cameras, markers and jammers are always available to place, even if you did not bring one into the raid.**

Status: concept repository. No working plugin or installable release yet.

## Intended behavior

At a valid quest placement location, allow the player to place the required camera, marker or jammer without having packed that device.

Proposed defaults for implementation: preserve the normal placement interaction, timer, valid locations and quest requirements. Unlimited availability should cover multiple placements in the same raid. These defaults and the treatment of devices the player actually carries remain to be finalized before implementation.

Initial target: local SPT 5.0 bleeding edge / EFT 1.3.0.0.48484 (Unity 6, BepInXDX IL2CPP). Version branch: `spt-5.0-eft-1.3.0.0.48484`. This is a standalone mod repository, linked under `mods/AlwaysSpecial` in [SPT-BE](https://github.com/jpeckham/SPT-BE).

## Implementation research

Determine how the current client checks device availability, starts/cancels placement and submits item consumption to the server. Choose between virtual availability and supplying temporary real devices only after inspecting those paths. Check both native client behavior and server inventory validation; allowing a prompt alone is insufficient.

Identify the exact supported item templates. Keep ordinary quest item hand-ins and unrelated special equipment outside the initial scope. No implementation approach or extra dependency has been selected yet.

## Verification to save here

When implemented, keep mod-specific test source and instructions in this repository. Cover missing devices, repeated placements, interrupted placement, carried devices, normal quest progress, server/client inventory agreement and save/reconnect behavior. Document which checks require a real raid.

Local BE process orchestration belongs in [SPT-BE's scripts directory](https://github.com/jpeckham/SPT-BE/tree/spt-5.0-eft-1.3.0.0.48484/scripts). Do not commit game assemblies, installed binaries, profiles or logs here.
