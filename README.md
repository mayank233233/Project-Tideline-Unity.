# Project Tideline — PORT SABLE: NO FAVOURS LEFT

An original open-world crime drama in production. The quality ambition is cinematic character work, a memorable criminal world, authored missions, a dense coastal city and strong mobile presentation. GTA V, GTA VI and Red Dead Redemption 2 are reference bars for craft and ambition only; Tideline does not reuse their characters, stories, maps, dialogue, music or assets.

## Current campaign canon

The campaign follows three playable leads tied together by a failed waterfront settlement eight years earlier:

- **Mara Kade** — driver, mechanic and co-owner of Kade Motor Works.
- **Rafi Sayegh** — nightlife promoter and operator of Lowlight.
- **Elias Ward** — former mediator for the underworld institution called the House.

The city is **Port Sable**, with seven planned districts, six major factions, eight named supporting characters, one investigative institution, three lead prologues, eighteen core missions and three endings. The story's shared-history event is Tideglass; its present-day inciting incident is Harbour Exchange.

Read the [prologue dialogue graph and scene wiring reference](docs/PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md), the [full story canon](PORT_SABLE_STORY_CANON.md), [visual direction and mobile rendering contract](docs/PORT_SABLE_VISUAL_TARGET.md), [three-lead integration specification](docs/PORT_SABLE_THREE_LEAD_SYSTEM.md), [criminal ecosystem and rival-crew brief](docs/PORT_SABLE_CRIMINAL_ECOSYSTEM.md), [opening campaign mission design](docs/PORT_SABLE_OPENING_CAMPAIGN.md), [narrative-state/save contract](docs/PORT_SABLE_NARRATIVE_STATE.md), the [signature score design](docs/PORT_SABLE_SIGNATURE_SCORE_ALL_THE_WAY_DOWN.md), and the [mission asset/branch audit](docs/MISSION_ASSET_BRANCH_AUDIT.md), the [mission signal pipeline audit](docs/MISSION_SIGNAL_PIPELINE_AUDIT.md), and the [mission signal producer inventory](docs/MISSION_SIGNAL_PRODUCER_INVENTORY.md).

## Campaign data tools

The source includes:

- `CampaignCanon.cs`: machine-checkable leads, supporting cast, factions, districts, mission goals, gameplay verbs, character complications, player choices and persistent consequences.
- `CampaignCanonAudit.cs`: Unity Editor validation for missing/duplicate IDs, incomplete campaign structure and the separate institution registry.
- `MissionAssetAudit.cs`: scans serialized MissionDefinition assets, objective signals against actual source/data producers, branches, fallbacks, ending flags, unconsumed story flags and legacy builder filename wiring. A source match is not proof of scene/prefab wiring.
- `MissionBranchEvaluator.cs`: shared branch-condition evaluator; missing relationship/story values are not coerced to zero.
- `PortSablePrologueDialogueBuilder.cs`: creates four dialogue graphs across three lead prologues; Mara's Juno/work-order opening and Nico post-delivery conversation are separate graphs. See the [exact graph/node/choice reference](docs/PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md).
- `PortSablePrologueDialogueAudit.cs`: checks graph IDs, node links, reachability and required saved choice flags.
- `PlayableLeadPrefabAudit.cs`: blocks missing or reused hero meshes, invalid animation rigs, missing locomotion clips and inadequate LOD setup.
- `NarrativeStateLedger.cs`, `DialogueController.cs` and `MissionDirector.cs`: persist dialogue choice flags and evaluate later mission branches.
- `DialogueInteractable.cs` and `CinematicConfrontationInteractable.cs`: open authored dialogue and the non-graphic confrontation sequence through the existing proximity-interaction path; their scene references still need to be assigned.
- `RouteRaceStartInteractable.cs`, `RouteRaceController.cs` and `RouteRaceCheckpoint.cs`: ordered player-driven route checkpoints with start/checkpoint/finish/cancel signals. This prototype has no rival timing, placement result or outcome-choice layer.
- `MissionSignalBus.cs`: FIFO nested dispatch, exception-isolated listeners, batch publication for multiple outcomes from one action, and runaway-batch abort recovery. The unused generic `IInteractable.SignalId` API has been removed.
- `MaraWorkshopOrderController.cs` / `MaraWorkshopTaskInteractable.cs`: three distinct final vehicle checks, locked until Juno's dialogue persists `mara_work_order_accepted = 1`; the mission advances only after all three tasks complete.
- `MaraDeliveryHandoffController.cs` / `MaraCustomerHandoffInteractable.cs`: emit `mara.delivery.finished` only after the assigned route has finished and the customer explicitly accepts the vehicle.
- `MaraDeliveryStartInteractable.cs` / `MaraPostDeliveryDialogueInteractable.cs`: gate route start until the workshop order is complete and gate Nico's graph until customer handoff is complete.
- `MaraPrologueMissionBuilder.cs`: generates one three-objective `two_minutes_to_close` MissionDefinition after both Mara dialogue graphs exist.
- `MaraPrologueSceneWiringAudit.cs`: checks actual loaded-scene references for the player, input, dialogue UI/graphs, three workshop stations, vehicle seat, ordered delivery checkpoints, customer handoff and mission objective signals. It reports missing wiring; it does not create a fake scene. The exact graph/Inspector contract is in [PORT SABLE Prologue Dialogue Graphs](docs/PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md).
- `SaveSnapshot.cs` / `SessionSaveCoordinator.cs`: schema v3 saves the active lead, all lead positions and narrative flags, while reading v1/v2 legacy saves.
- `CampaignMissionOutline.cs`: separate narrative-outline data type, intentionally not a playable `MissionDefinition`.
- `CampaignOutlineAssetBuilder.cs`: Unity Editor command to create/update 21 editable campaign outline assets without deleting stale records.

Unity menu commands:
- **Project Tideline → 6 — Audit Campaign Canon**
- **Project Tideline → 7 — Generate Campaign Outline Assets**
- **Project Tideline → 8 — Audit Bespoke Lead Prefabs**
- **Project Tideline → 9 — Generate PORT SABLE Prologue Dialogue**
- **Project Tideline → 10 — Audit PORT SABLE Prologue Dialogue**
- **Project Tideline → 11 — Audit Mission Assets and Branches**
- **Project Tideline → 12 — Generate Mara Prologue Mission** (run after dialogue generation; creates one Mara MissionDefinition asset)
- **Project Tideline → 13 — Audit Mara Prologue Scene Wiring** (run with the Mara scene loaded; checks serialized scene references without modifying the scene)

Generated outlines are design records, not playable missions. Do not attach them to `MissionDirector`.

## Recovered technical foundation

The recovered v0.4.9 bundle identifies source checkpoint `ba72e8d7ebec35f2158e318dab8afad75bf518b0`, Unity `6000.3.28f1`, URP 17.3, the Android target and a CC0 model installer. The original bundle's `v0.4.9` annotated tag peels to the same commit as `main`.

The old Ari Solis / Ghost Fleet Old Quay scenario is retained only as a legacy technical testbed for movement, camera, vehicles, traffic, UI, imported assets and performance checks. It is **not** the current campaign and is not a completed PORT SABLE mission.

## Current implementation truth

**Present in this selected source snapshot:** the campaign registry and audits, four prologue dialogue graph definitions across three leads, dialogue/confrontation proximity-interaction components, a vehicle-bound ordered route controller, Mara's three-stage workshop/delivery/choice objective source, a Mara MissionDefinition asset generator, dialogue-choice state ledger, mission branch evaluator/audit, three-lead handoff/save source, and selected player/vehicle/NPC systems. Same-action multi-signal outcomes are batched; route completion is signalled only after its batch drains. These files have not yet been compiled together in Unity.

**Not yet implemented or verified:** any tracked PORT SABLE scene or NPC/player/vehicle prefab wiring; the generated MissionDefinition asset for Mara (the Editor generator is present but has not been run); Rafi/Elias playable profiles; real character switching; competitive race timing/rival outcomes; the venue/job/hub/faction/score gameplay systems; the remaining 20 executable campaign mission definitions; endings; cinematic scene pipeline; Unity compilation; Play Mode; Android APK launch; and Vivo performance/thermal testing.

The campaign registry and story bible are content/design groundwork, not a claim of playable content. The lead handoff code also requires three original hero prefabs and scene wiring before it can run.

## Source and asset recovery status

The connected GitHub repository contains a selected campaign/source snapshot plus the recovered Unity version, package manifest, runtime assembly definition and ignore rules. It does **not** yet contain the complete recovered Unity source tree, all project settings, all scenes/prefabs, stable .meta GUIDs or required art assets. The uploaded bundle remains the authoritative v0.4.9 recovery package until that complete tree is imported.

The pinned 48 GLB LOD variants are not embedded here. The Unity Editor installer downloads and verifies the CC0 assets by byte length and SHA-256 when run.

## Validation

For the selected GitHub snapshot:

```bash
python tools/validate_port_sable_snapshot.py
```

The GitHub Action runs this check on every push and pull request. Latest verified run for the current Mara scene-audit and graph-documentation changes: **PASS — 235 checks, 46 C# files scanned** ([run 45351133879](https://github.com/mayank233233/Project-Tideline-Unity./actions/runs/45351133879)). The separate legacy mission sequence check reports **SKIP** because the full recovered `OldQuayVerticalSliceBuilder.cs` is not present in this selected snapshot. This validates repository consistency only; it does **not** compile C#, import GLBs, run Unity, generate scenes, build an APK or test a physical device.

The recovered v0.4.9 working tree also passed its original `python tools/validate_static.py` before these new campaign files were added. That original validator is in the complete recovery bundle, not yet mirrored into this selected GitHub snapshot.

## Target device and quality bar

The target phone is Vivo T2x 5G (Dimensity 6020 / Mali-G57 MC2). The visual goal is an authored mobile experience with strong environment composition, credible lighting and materials, clear character silhouettes, disciplined LOD/streaming and measured frame pacing. Literal GTA VI / RDR2 parity is not a realistic mobile promise. Quality must be demonstrated with actual scene captures, device profiling and blind playtests—not declared in a story document.
