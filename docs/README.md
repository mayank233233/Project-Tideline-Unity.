# Project Tideline documentation

- [PORT SABLE: NO FAVOURS LEFT — full story canon](../PORT_SABLE_STORY_CANON.md): setting, criminal factions, playable leads, supporting cast, Tideglass history, relationship engine, 3 prologues, 18 core missions and 3 endings.
- [README](../README.md): current implementation truth, setup/validation status and repository limitations.
- [RECOVERY](../RECOVERY.md): recovered v0.4.9 checkpoint and source-publication constraints.

- [PORT SABLE visual target](PORT_SABLE_VISUAL_TARGET.md): district art direction, character/vehicle standards, lighting, weather and mobile rendering budgets.
- [Three-lead system](PORT_SABLE_THREE_LEAD_SYSTEM.md): required hero prefabs, component contracts, scene wiring, switch locks and device test plan.
- [Narrative state and save contract](PORT_SABLE_NARRATIVE_STATE.md): persistent prologue flags, mission-branch conditions, v3 saves, legacy migration and vehicle-save guard.
- [Mission asset and branch audit](MISSION_ASSET_BRANCH_AUDIT.md): recovered Old Quay filename mismatch, reachable legacy ending gates, zero PORT SABLE MissionDefinition assets and Unity verification steps.
- [Mission signal producer inventory](MISSION_SIGNAL_PRODUCER_INVENTORY.md): mission-by-mission objective-contract gaps, current signal producers, interaction wiring limits and save/restore semantics.
- [ALL THE WAY DOWN — signature score](PORT_SABLE_SIGNATURE_SCORE_ALL_THE_WAY_DOWN.md): full three-lead prep, crew selection, seven mission beats, three outcomes and test criteria.
- [Hero character art bible](PORT_SABLE_HERO_CHARACTER_BIBLE.md): unique lead identities, recurring cast designs, facial/gesture language, hero rig contract and mobile LOD starting budgets.

## Campaign source

- `Assets/ProjectTideline/Scripts/Story/CampaignCanon.cs`: canonical machine-checkable mission registry.
- `Assets/ProjectTideline/Scripts/Story/CampaignMissionOutline.cs`: narrative outline ScriptableObject, deliberately separate from playable mission definitions.
- `Assets/Editor/ProjectTideline/CampaignCanonAudit.cs`: campaign metadata integrity check.
- `Assets/Editor/ProjectTideline/CampaignOutlineAssetBuilder.cs`: creates or updates the 21 editable outline assets without deleting unexpected stale assets.
- `Assets/Editor/ProjectTideline/PlayableLeadPrefabAudit.cs`: requires distinct animated hero prefabs with three LODs and locomotion/interaction clips.
- `Assets/ProjectTideline/Scripts/Player/PlayableLeadSwitcher.cs` and `Assets/ProjectTideline/Scripts/UI/PlayableLeadSwitcherUI.cs`: control transfer and mobile switching UI.
- `Assets/ProjectTideline/Scripts/Core/NarrativeStateLedger.cs`: persistent choice flags.
- `Assets/ProjectTideline/Scripts/Interaction/DialogueInteractable.cs`: proximity-interaction entry point for Inspector-assigned dialogue graphs; scene/prefab references still need to be wired.
- `Assets/ProjectTideline/Scripts/Interaction/CinematicConfrontationInteractable.cs`: proximity-interaction entry point for the non-graphic confrontation sequence; scene/prefab references still need to be wired.
- `Assets/ProjectTideline/Scripts/Interaction/RouteRaceStartInteractable.cs`, `Assets/ProjectTideline/Scripts/World/RouteRaceController.cs` and `RouteRaceCheckpoint.cs`: ordered player-driven route checkpoints and route-finish signals; scene setup and competitive race results are not implemented.
- `Assets/ProjectTideline/Scripts/World/MaraWorkshopOrderController.cs` and `MaraWorkshopTaskInteractable.cs`: three unique vehicle inspection tasks, gated by Juno's saved `mara_work_order_accepted` flag.
- `Assets/ProjectTideline/Scripts/Interaction/MaraDeliveryStartInteractable.cs` and `MaraPostDeliveryDialogueInteractable.cs`: enforce workshop → delivery → Nico dialogue order.
- `Assets/ProjectTideline/Scripts/World/MaraDeliveryHandoffController.cs` and `MaraCustomerHandoffInteractable.cs`: emit the Mara-specific delivery completion only after the assigned route has finished and the customer accepts the vehicle.
- `Assets/Editor/ProjectTideline/MaraPrologueMissionBuilder.cs`: generates the three-objective `two_minutes_to_close` MissionDefinition after both Mara dialogue graphs exist.
- `Assets/Editor/ProjectTideline/MaraPrologueSceneWiringAudit.cs`: audits loaded-scene references for the Mara player, input, dialogue UI/graphs, workshop, route, customer handoff and mission director.
- [PORT SABLE Prologue Dialogue Graphs](PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md): four generated graph IDs, authored node/choice flow, persistent choice flags and the Mara Inspector wiring matrix; graph generation is not scene wiring.
- `docs/PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md`: exact four-graph inventory, node/choice flow, persistent flags and Inspector wiring contract.
- `Assets/ProjectTideline/Scripts/Dialogue/DialogueController.cs`: rejects malformed or overlapping graphs and releases the input lock when disabled.
- `Assets/ProjectTideline/Scripts/Missions/MissionDirector.cs`: evaluates story-flag and relationship branches.
- `Assets/Editor/ProjectTideline/PortSablePrologueDialogueBuilder.cs` / `PortSablePrologueDialogueAudit.cs`: generates and validates four dialogue graphs across the three prologues (Mara has a separate post-delivery Nico stage).
- `Assets/Editor/ProjectTideline/MissionAssetAudit.cs`: audits actual mission ScriptableObjects, builder asset-name wiring and branch/fallback conditions.
- `Assets/ProjectTideline/Scripts/Missions/MissionBranchEvaluator.cs`: deterministic condition evaluation used by MissionDirector.

## Non-negotiable implementation rule

The v0.4.9 Old Quay Ari Solis scene is a legacy technical testbed only. Do not present it as the PORT SABLE campaign. The Mara prologue, Rafi/Elias playable identity and switching, new dialogue playback, mission triggers/objectives, faction simulation, city districts, and endings still require implementation and Unity/device verification.

- [Opening campaign design](PORT_SABLE_OPENING_CAMPAIGN.md): separate lead prologues, THE CUT sequence, choice/consequence matrix and narrative QA criteria.
- [Criminal ecosystem](PORT_SABLE_CRIMINAL_ECOSYSTEM.md): six factions, rival crew Nika Vale, law-pressure institution and side-story arcs.

- [Legacy Old Quay filename patch](../patches/0001-fix-oldquay-mission-asset-load-names.patch): fixes two stale asset names in ConfigureMissionDirector.
- `../tools/validate_legacy_mission_sequence.py`: checks mission creation names against the configured campaign list when the complete recovered builder is present.
