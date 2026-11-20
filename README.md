# Project Tideline — PORT SABLE: NO FAVOURS LEFT

An original open-world crime drama in production. The quality ambition is cinematic character work, a memorable criminal world, authored missions, a dense coastal city and strong mobile presentation. GTA V, GTA VI and Red Dead Redemption 2 are reference bars for craft and ambition only; Tideline does not reuse their characters, stories, maps, dialogue, music or assets.

## Current campaign canon

The campaign follows three playable leads tied together by a failed waterfront settlement eight years earlier:

- **Mara Kade** — driver, mechanic and co-owner of Kade Motor Works.
- **Rafi Sayegh** — nightlife promoter and operator of Lowlight.
- **Elias Ward** — former mediator for the underworld institution called the House.

The city is **Port Sable**, with seven planned districts, five major factions, seven named supporting characters, three lead prologues, eighteen core missions and three endings. The story's shared-history event is Tideglass; its present-day inciting incident is Harbour Exchange.

Read the complete canon in [PORT_SABLE_STORY_CANON.md](PORT_SABLE_STORY_CANON.md), the [visual direction / mobile rendering contract](docs/PORT_SABLE_VISUAL_TARGET.md), and the [three-lead prefab and control-handoff specification](docs/PORT_SABLE_THREE_LEAD_SYSTEM.md).

## Campaign data tools

The source includes:

- `CampaignCanon.cs`: machine-checkable leads, supporting cast, factions, districts, mission goals, gameplay verbs, character complications, player choices and persistent consequences.
- `CampaignCanonAudit.cs`: Unity Editor validation for missing/duplicate IDs and incomplete campaign structure.
- `CampaignMissionOutline.cs`: separate narrative-outline data type, intentionally not a playable `MissionDefinition`.
- `CampaignOutlineAssetBuilder.cs`: Unity Editor command to create/update 21 editable campaign outline assets without deleting stale records.

Unity menu commands:
- **Project Tideline → 6 — Audit Campaign Canon**
- **Project Tideline → 7 — Generate Campaign Outline Assets**

Generated outlines are design records, not playable missions. Do not attach them to `MissionDirector`.

## Recovered technical foundation

The recovered v0.4.9 bundle identifies source checkpoint `ba72e8d7ebec35f2158e318dab8afad75bf518b0`, Unity `6000.3.28f1`, URP 17.3, the Android target and a CC0 model installer. The original bundle's `v0.4.9` annotated tag peels to the same commit as `main`.

The old Ari Solis / Ghost Fleet Old Quay scenario is retained only as a legacy technical testbed for movement, camera, vehicles, traffic, UI, imported assets and performance checks. It is **not** the current campaign and is not a completed PORT SABLE mission.

## Current implementation truth

**Implemented in source:** recovered player/movement/vehicle/NPC/dialogue/save/mission systems, CC0 asset installer and audits, campaign registry, campaign audit and outline-asset generator.

**Not yet implemented or verified:** the Mara prologue in Unity, Rafi/Elias playable profiles, real character switching, new campaign dialogue playback, faction simulation, the full seven-district world, campaign mission triggers/objectives, endings, cinematic scene pipeline, Unity compilation, Play Mode, Android APK launch and Vivo performance/thermal testing.

The campaign registry and story bible are content/design groundwork, not a claim of playable content. The lead handoff code also requires three original hero prefabs and scene wiring before it can run.

## Source and asset recovery status

The connected GitHub repository currently contains the narrative canon and selected campaign source/tool files, not the complete recovered Unity project tree. The full source bundle remains the authoritative recovery package until the entire Unity source tree, project settings, packages and required assets are imported to GitHub.

The pinned 48 GLB LOD variants are not embedded here. The Unity Editor installer downloads and verifies the CC0 assets by byte length and SHA-256 when run.

## Validation

From the full recovered Unity source folder:

```bash
python tools/validate_static.py
```

The static validator checks source structure, project inventory, legacy testbed wiring and new campaign-registry invariants. It does **not** compile C#, import GLBs, run Unity, generate scenes, build an APK or test a physical device.

## Target device and quality bar

The target phone is Vivo T2x 5G (Dimensity 6020 / Mali-G57 MC2). The visual goal is an authored mobile experience with strong environment composition, credible lighting and materials, clear character silhouettes, disciplined LOD/streaming and measured frame pacing. Literal GTA VI / RDR2 parity is not a realistic mobile promise. Quality must be demonstrated with actual scene captures, device profiling and blind playtests—not declared in a story document.
