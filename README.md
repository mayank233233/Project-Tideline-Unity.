# Project Tideline — PORT SABLE: NO FAVOURS LEFT

An original open-world crime-action game in production. Its intended genre loop is free-roam city exploration, vehicle play, criminal jobs and heists, police-pressure escalation, rival factions, safehouses, character switching and branching story missions. The quality ambition is cinematic character work, a memorable criminal world, authored missions, a dense coastal city and strong mobile presentation. GTA V, GTA VI and Red Dead Redemption 2 are reference bars for craft and ambition only; Tideline does not reuse their characters, stories, maps, dialogue, music or assets.

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
- `MaraDeliveryHandoffController.cs` / `MaraCustomerHandoffInteractable.cs`: require the assigned driver, completed route and secured bay before opening the customer acceptance dialogue. The generated dialogue offers a courteous or brusque handoff response, updates the `saltline_customer` relationship, and emits `mara.delivery.finished` only when the player selects a response.
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
- **Project Tideline → 14 — Build Playable Old Quay Blockout** (generates a primitive Old Quay scene, Juno dialogue, a three-objective tutorial mission, interaction prompts, mission HUD, keyboard/touch controls, and adds the scene to Build Settings)

Generated outlines are design records, not playable missions. Do not attach them to `MissionDirector`.

## Genre direction: open-world crime action

PORT SABLE is intended to occupy the same broad genre and gameplay space as GTA V / GTA VI: a modern open-world crime sandbox with story missions, freely explorable districts, driving, criminal jobs, escalating police pressure, rival organisations, side activities and multiple playable protagonists. This means the delivery tutorial is only an early character/vehicle-control slice; it is not the game's central identity or its long-term mission template.

The full intended player loop and system boundaries are defined in [Open-World Crime Gameplay Pillars](docs/PORT_SABLE_OPEN_WORLD_CRIME_GAMEPLAY.md). That document is a design target, not a claim that those systems are already implemented. The current selected source snapshot still lacks the complete open-world runtime, police response, heist/job framework, traffic simulation and fully wired three-lead scene.

## Police-response prototype

The generated Old Quay scene includes a deliberately simple arcade response director. Public-attention bands dispatch one or two placeholder patrol vehicles; when attention returns to Calm, the HUD enters a visible cooldown countdown before clearing those units. The director exposes explicit None, Dispatch, Escalated and Cooldown phases for future UI and mission integration. This is not a complete wanted/search system: there is no investigation AI, arrest loop, dispatch network or production pursuit behaviour, and the generated scene still requires Unity runtime testing.

## Content rating and presentation boundaries

PORT SABLE is intended to have a mature crime-drama tone: serious criminal themes, high-stakes missions, tense pursuits, consequential character conflict and intense action. Violence must remain non-graphic in its presentation; do not add gore or gruesome injury detail. **Nudity is excluded**: no nude characters or nudity scenes. These are content boundaries, not a claim of any official age-rating certification.

## Generated Old Quay tutorial scene

Run **Project Tideline → 14 — Build Playable Old Quay Blockout** from the Unity Editor after the runtime scripts have compiled. The command creates `Assets/ProjectTideline/Scenes/PortSable_OldQuay_Blockout.unity`, generates the tutorial dialogue and mission assets, adds the scene to Build Settings, and opens it.

The eight-stage first-play sequence is:

1. **Meet Juno.** Speak to her outside Kade Motor Works. The two responses change Juno's relationship value.
2. **Check the ledger.** Travel to the harbour office and inspect the shipment ledger.
3. **Accept the delivery.** Return to the workshop and use the delivery order board. The route cannot be started before this objective.
4. **Enter the vehicle.** Get into the customer's utility car. The player switches to vehicle controls and the camera follows the vehicle.
5. **Drive the route.** Pass three ordered checkpoints from Old Quay toward Saltline. The second and third gates offset across the road to form a short S-bend; the route HUD reports checkpoint progress and calls the next gate left, right or ahead. A vehicle-only speedometer shows current km/h and changes to parking-speed guidance during the final bay objective. Only the assigned player-driven vehicle can advance them.
6. **Park at the customer bay.** Stop inside the marked bay. The arrival checkpoint only accepts the assigned occupied vehicle when its speed is at or below 3 km/h.
7. **Hand over the vehicle.** Exit the car and speak to the waiting customer. Choose a courteous or brusque response; the choice changes the customer relationship and completes the handoff. The conversation is unavailable until the assigned driver has finished the route and secured the vehicle in the bay.
8. **Debrief with Juno.** Return to Kade Motor Works and choose a response in her objective-specific closing conversation.

### First criminal job — The First Score

After Juno's debrief, the campaign advances into a separate four-stage criminal job:

1. **Accept the score.** Meet the Saltline contact and accept the sealed-case recovery job.
2. **Recover the case.** Interact with the marked case. This raises public attention and triggers the staged police response.
3. **Get back to the car.** The vehicle seat has a separate mission gate, so the car becomes available for this objective without opening it during earlier tutorial stages.
4. **Reach the safehouse.** Drive the designated occupied vehicle into the marked safehouse zone. The arrival signal resolves the score's attention state; the patrol response clears after its short Calm grace period.

The score is a compact first crime-action loop, not a full heist framework. The generated Old Quay blockout now extends into a southern market lane with the Breakwater Cafe and Quay Tool & Tackle storefronts, a covered pier market, marked loading bays, benches and public bins. These are still modular greybox props and facades, not final art. Four additional waypoint-driven pedestrians now circulate along the new storefront and pier-market routes, with staggered start times. Their capsule placeholders have been replaced by lightweight articulated low-poly figures with separate heads, workwear, arms, legs and a procedural walk cycle; the root collider remains responsible for crossing occupancy. They still have no NavMesh pathfinding, crowd avoidance or ambient conversation. It now advances into The Paper Trail, a three-objective follow-up that connects the recovered case to a freight clerk, duplicate manifest and Saltline handoff. The campaign then continues into The Market Ledger, a three-objective market-side job to meet a pier vendor, recover a stock tally and return it to the Breakwater Cafe owner. The vendor and cafe owner now use authored, objective-specific dialogue graphs with two response choices each; choices update named NPC relationship values and persistent story flags while completing the relevant mission objective. The characters and props remain greybox placeholders, and these choices do not yet branch later missions. The patrol cars are generated arcade prototypes that follow the player actor; they are not production police AI.

The utility car has a Rigidbody, four configured WheelColliders, front steering, rear-wheel drive, braking, wheel visuals, and enter/exit handling. While driving, W / the touch ACCEL button accelerates; S / the touch BRAKE / REV button brakes while moving forward and selects speed-limited reverse near a stop. A/D or the virtual joystick steers; Space remains the keyboard handbrake. If the driver exits before the marked bay is reached, they can re-enter and finish parking; entry closes once bay arrival advances the mission to handoff. The scene also includes a mission HUD, proximity prompts, two placeholder sidewalk pedestrians, a signal-gated pedestrian crossing route, and six NPC traffic cars using WheelColliders, world-space waypoints, sampled obstacle checks, emissive lamps and route-boundary recycling. One cross-street route now curves into the main road; the southern four-way junction has timed vehicle phases, all-red clearance, a protected pedestrian walk phase, visible pedestrian/vehicle signal lenses, directional stop-line checks and occupancy-based yielding while a pedestrian is in the marked crossing. A conservative single-car reservation prevents competing NPC cars from entering the junction simultaneously; it is not a full multi-lane queue or priority system. Pedestrians still do not use NavMesh pathfinding or crowd avoidance, and this remains a small traffic prototype, not a city traffic simulation.

This remains a primitive greybox. Juno, the customer, the pedestrians, vehicle and player are placeholders; there is no finished character rig, vehicle audio, save flow, cinematic presentation, full city simulation or Android performance validation. The menu command still needs to be run in Unity. It now generates both the eight-stage delivery tutorial, the four-stage first score, and the three-stage Paper Trail follow-up, including the case-recovery attention event, patrol response and safehouse vehicle-arrival gate. Static CI does not execute the command or prove that the generated scene compiles or plays.

## Recovered technical foundation

The recovered v0.4.9 bundle identifies source checkpoint `ba72e8d7ebec35f2158e318dab8afad75bf518b0`, Unity `6000.3.28f1`, URP 17.3, the Android target and a CC0 model installer. The original bundle's `v0.4.9` annotated tag peels to the same commit as `main`.

The old Ari Solis / Ghost Fleet Old Quay scenario is retained only as a legacy technical testbed for movement, camera, vehicles, traffic, UI, imported assets and performance checks. It is **not** the current campaign and is not a completed PORT SABLE mission.

## Current implementation truth

**Present in this selected source snapshot:** the campaign registry and audits, four prologue dialogue graph definitions across three leads, dialogue/confrontation proximity-interaction components, a vehicle-bound ordered route controller, Mara's three-stage workshop/delivery/choice objective source, a Mara MissionDefinition asset generator, dialogue-choice state ledger, mission branch evaluator/audit, three-lead handoff/save source, selected player/vehicle/NPC systems, an event-driven `PolicePressureHUD`, a staged `PoliceResponseDirector` with generated arcade patrol vehicles, and the four-stage `old_quay_first_score` mission generator. Vehicle takeover, collision, confrontation and case-recovery signals can raise attention; Noticed dispatches one patrol and Escalated dispatches a second. Patrol vehicles steer, accelerate and flash their lightbars while following the player actor. Quiet-time decay reduces attention; the score's safehouse arrival resolves it, and units clear after a Calm grace period. Same-action multi-signal outcomes are batched; route completion is signalled only after its batch drains. These files have not yet been compiled together in Unity.

**Generated by the new blockout builder (Editor command 14; not yet executed in Unity):** a cohesive Old Quay greybox slice with walkable streets, harbour edge, warehouse silhouettes, container stacks, streetlamps, a CharacterController player, keyboard and on-screen touch controls, and a third-person camera. Its first visual pass uses warm coastal key lighting, cool ambient harbour fill, restrained distance fog, one shadowed directional sun, two unshadowed point-light accents, road-facing industrial facades and lettering, rain-dark road repairs, container ribs, storm drains and quay bollards. The mission includes Juno dialogue with two persistent relationship outcomes, objective-gated harbour/report interactions, an event-driven HUD, an eight-objective delivery tutorial with a three-gate S-bend route, live parking-bay alignment guidance, an authored customer acceptance dialogue with two persistent relationship outcomes, vehicle handoff and Juno debrief. The scene and its ScriptableObject assets are generated locally when the menu command is run; they are not yet committed as serialized assets. It remains low-poly greybox art: there are no final hero character or vehicle models, authored texture sets, final environment meshes, production animation or finished cinematic lighting. Touch HUD wiring is included but has not been exercised on an Android device.

**Not yet implemented or verified:** the generated Old Quay scene and tutorial assets have not yet been executed/inspected in Unity; visual appearance and mobile frame time of the new lighting/facade pass have not been checked in Play Mode or on-device; any tracked final PORT SABLE NPC/player/vehicle prefab wiring; the generated MissionDefinition asset for Mara (the Editor generator is present but has not been run); Rafi/Elias playable profiles; real character switching; competitive race timing/rival outcomes; the venue/job/hub/faction/score gameplay systems; the remaining 20 executable campaign mission definitions; endings; cinematic scene pipeline; Unity compilation; Play Mode; Android APK launch; and Vivo performance/thermal testing.

The campaign registry and story bible are content/design groundwork, not a claim of playable content. The lead handoff code also requires three original hero prefabs and scene wiring before it can run.

## Source and asset recovery status

The connected GitHub repository contains a selected campaign/source snapshot plus the recovered Unity version, package manifest, runtime assembly definition and ignore rules. It does **not** yet contain the complete recovered Unity source tree, all project settings, all scenes/prefabs, stable .meta GUIDs or required art assets. The uploaded bundle remains the authoritative v0.4.9 recovery package until that complete tree is imported.

The pinned 48 GLB LOD variants are not embedded here. The Unity Editor installer downloads and verifies the CC0 assets by byte length and SHA-256 when run.

## Validation

For the selected GitHub snapshot:

```bash
python tools/validate_port_sable_snapshot.py
```

The GitHub Action runs this check on every push and pull request. Latest verified baseline before the four-car traffic increment: **PASS — 522 checks, 58 C# files scanned** ([run 45376917486](https://github.com/mayank233233/Project-Tideline-Unity./actions/runs/45376917486)). The four-car increment is awaiting its own CI result. The separate legacy mission sequence check reports **SKIP** because the full recovered `OldQuayVerticalSliceBuilder.cs` is not present in this selected snapshot. This validates repository consistency only; it does **not** compile C#, import GLBs, run Unity, generate scenes, build an APK or test a physical device.

The recovered v0.4.9 working tree also passed its original `python tools/validate_static.py` before these new campaign files were added. That original validator is in the complete recovery bundle, not yet mirrored into this selected GitHub snapshot.

## Target device and quality bar

The target phone is Vivo T2x 5G (Dimensity 6020 / Mali-G57 MC2). The visual goal is an authored mobile experience with strong environment composition, credible lighting and materials, clear character silhouettes, disciplined LOD/streaming and measured frame pacing. Literal GTA VI / RDR2 parity is not a realistic mobile promise. Quality must be demonstrated with actual scene captures, device profiling and blind playtests—not declared in a story document.

### Market Ledger — consequence-driven follow-up

The fourth Old Quay mission now evaluates the vendor and cafe-owner choices after its final dialogue choice has been applied. The positive route requires both promise/context story flags and both character relationship values to meet their thresholds. When all four conditions match, the campaign starts **A Place at the Table**: collect the supplies the market has reserved for Kade Motor Works. If any condition is missing or below threshold, the fallback is **No More Favors**: collect a replacement order from an independent quay supplier.

Both routes are separate executable MissionDefinition assets with objective-gated world interactions. The branch is evaluated by the existing MissionBranchEvaluator; it is not a dialogue-only cosmetic difference. The narrative and relationship ledgers expose JSON export/import and the schema-v3 session save coordinator can capture both. The generated Old Quay blockout currently creates the ledgers but does not wire a SessionSaveCoordinator, so these choices survive mission transitions in that running scene but are not yet proven to survive an app restart in this scene. The builder creates both follow-up assets and places both route interactions in the scene; mission-ID gates ensure only the active route can be used.

This remains a short two-way consequence test, not a full branching campaign. It has not been verified in Unity Play Mode in this environment.
