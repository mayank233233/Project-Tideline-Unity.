# Project Tideline documentation

- [PORT SABLE: NO FAVOURS LEFT — full story canon](../PORT_SABLE_STORY_CANON.md): setting, criminal factions, playable leads, supporting cast, Tideglass history, relationship engine, 3 prologues, 18 core missions and 3 endings.
- [README](../README.md): current implementation truth, setup/validation status and repository limitations.
- [RECOVERY](../RECOVERY.md): recovered v0.4.9 checkpoint and source-publication constraints.

## Campaign source

- `Assets/ProjectTideline/Scripts/Story/CampaignCanon.cs`: canonical machine-checkable mission registry.
- `Assets/ProjectTideline/Scripts/Story/CampaignMissionOutline.cs`: narrative outline ScriptableObject, deliberately separate from playable mission definitions.
- `Assets/Editor/ProjectTideline/CampaignCanonAudit.cs`: campaign metadata integrity check.
- `Assets/Editor/ProjectTideline/CampaignOutlineAssetBuilder.cs`: creates or updates the 21 editable outline assets without deleting unexpected stale assets.

## Non-negotiable implementation rule

The v0.4.9 Old Quay Ari Solis scene is a legacy technical testbed only. Do not present it as the PORT SABLE campaign. The Mara prologue, Rafi/Elias playable identity and switching, new dialogue playback, mission triggers/objectives, faction simulation, city districts, and endings still require implementation and Unity/device verification.
