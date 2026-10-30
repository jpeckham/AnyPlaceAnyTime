# All placement items implementation plan

User confirmed 0.1.0 works and requested every other placement item except task-item-storage items. Preserve the existing independent toggles.

1. Generate a versioned catalog from LeaveItemAtLocation/PlaceBeacon targets in the installed quest database. Require explicit QuestItem=false; reject task storage items and unknown definitions. Current data: 98 ordinary, 22 protected.
2. Keep original three config keys; add one toggle per additional template, all enabled. Duplicate display names include an ID suffix.
3. Supply only targets relevant to the current placement zone; retain native availability rules and real-item ordering. Recheck native template QuestItem at creation and completion interception; never virtualize protected items even if definitions change.
4. Preload resources only for original devices and beacon-type additions, not every rifle/food/armor target. Ordinary placements use the existing generic timer callback.
5. Add managed exclusion/selection tests first, native creation/config and protected-item tests, then build/install 0.2.0. Preserve config/profiles and production 0.1.0 for rollback. Save tools/tests here, local process logs/backups in parent ignored storage.
6. Review, document, commit/push child and parent. Report synthetic coverage separately from manual raid validation.

No server changes, direct quest completion, automatic hand-ins, or task-storage item grants. Versioned stock catalog is intentional; regenerate/revalidate when game or quest-mod definitions change.

Completed: renamed GitHub repository, source namespace/assembly/project, local submodule and installed plugin to AnyPlaceAnyTime. Kept compatibility GUID/config filename com.jpeckham.alwaysspecial so existing toggle values survive. Old plugin was backed up outside BepInEx/plugins and removed before testing the new one.

Verification: 584 managed checks; native construction/predicates for 98 templates; rejection and original-consumption pass-through for 22 task-storage templates; synthetic native QuestBook and real zone-handler lookup with toolset/document/repeater; extra toggle, original toggle combinations, inventory isolation, prefab resources and patch targets. Native startup caught quoted config-key names; full forbidden-character sanitization fixed it. Both builds passed with zero warnings/errors. Independent review found no remaining blockers. New item types still need player raid acceptance.

Tools/harnesses are saved here. Parent local evidence/backup: .local-build-attempt/any-place-any-time. Verified both real profiles' PMC/scav inventory/quests unchanged and original three config values preserved; 95 new entries present. Temporary harness removed and test processes stopped.
