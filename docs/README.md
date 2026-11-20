# Project Tideline documentation

- [PORT SABLE: NO FAVOURS LEFT — full story canon](../PORT_SABLE_STORY_CANON.md): setting, criminal factions, playable leads, supporting cast, Tideglass history, relationship engine, 3 prologues, 18 core missions and 3 endings.
- [README](../README.md): current implementation truth, setup/validation status and repository limitations.
- [RECOVERY](../RECOVERY.md): recovered v0.4.9 checkpoint and source-publication constraints.

- [PORT SABLE visual target](PORT_SABLE_VISUAL_TARGET.md): district art direction, character/vehicle standards, lighting, weather and mobile rendering budgets.
- [Three-lead system](PORT_SABLE_THREE_LEAD_SYSTEM.md): required hero prefabs, component contracts, scene wiring, switch locks and device test plan.

## Campaign source

- `Assets/ProjectTideline/Scripts/Story/CampaignCanon.cs`: canonical machine-checkable mission registry.
- `Assets/ProjectTideline/Scripts/Story/CampaignMissionOutline.cs`: narrative outline ScriptableObject, deliberately separate from playable mission definitions.
- `Assets/Editor/ProjectTideline/CampaignCanonAudit.cs`: campaign metadata integrity check.
- `Assets/Editor/ProjectTideline/CampaignOutlineAssetBuilder.cs`: creates or updates the 21 editable outline assets without deleting unexpected stale assets.
- `Assets/Editor/ProjectTideline/PlayableLeadPrefabAudit.cs`: requires distinct animated hero prefabs with three LODs and locomotion/interaction clips.
- `Assets/ProjectTideline/Scripts/Player/PlayableLeadSwitcher.cs` and `Assets/ProjectTideline/Scripts/UI/PlayableLeadSwitcherUI.cs`: control transfer and mobile switching UI.

## Non-negotiable implementation rule

The v0.4.9 Old Quay Ari Solis scene is a legacy technical testbed only. Do not present it as the PORT SABLE campaign. The Mara prologue, Rafi/Elias playable identity and switching, new dialogue playback, mission triggers/objectives, faction simulation, city districts, and endings still require implementation and Unity/device verification.

- [Opening campaign design](PORT_SABLE_OPENING_CAMPAIGN.md): separate lead prologues, THE CUT sequence, choice/consequence matrix and narrative QA criteria.
- [Criminal ecosystem](PORT_SABLE_CRIMINAL_ECOSYSTEM.md): six factions, rival crew Nika Vale, law-pressure institution and side-story arcs.
