# Project Tideline — mission signal producer and objective contract audit

**Audit target:** PORT SABLE source snapshot on 24 November 2026 (updated after dialogue, confrontation and route-race interaction work).  
**Scope:** campaign registry, runtime signal publishers, interaction entry points, dialogue builders, mission save/restore code and tracked Unity content.  
**Verification type:** source inspection plus repository static checks. Unity compilation, Editor generation, scene execution and Play Mode were not run.

## Executive result

| Measure | Result |
|---|---:|
| PORT SABLE narrative mission records | 21 |
| PORT SABLE MissionDefinition assets in this snapshot | 0 |
| Missions with authored objective IDs, completion signals and required counts | 0 / 21 |
| Missions with verified objective-to-producer wiring | 0 / 21 |
| Tracked Unity scenes, prefabs or serialized content assets in this snapshot | 0 |

The first number is the campaign design registry. It is not playable content. The current repository snapshot has no serialized mission assets, scenes or prefabs from which to prove mission setup or scene references. The source contains reusable signal-producing code, but none of the 21 campaign missions has an objective-level contract assigning one of those signals to an objective.

**Do not create MissionDefinition assets by choosing convenient existing signal strings.** A contract must start with a concrete in-world action, define its completion semantics and repeatability, identify a reachable producer, and specify what happens after save/restore.

## 1. Mission-by-mission contract status

The gameplay verbs below are copied from `CampaignCanon`. The final column describes the missing contract work; it does not invent objectives or signal names.

| Mission | Campaign gameplay verb | Objective contract status |
|---|---|---|
| TWO MINUTES TO CLOSE | Workshop interaction and precision driving | **Objective template and producers now exist in source.** `MaraPrologueMissionBuilder` generates three ordered objectives: complete three final workshop checks (`mara.workshop.order.completed`), finish the assigned customer delivery route (`mara.delivery.finished`), and resolve Nico's post-delivery choice (`prologue.mara.complete`). The Juno opening and Nico follow-up are separate dialogue graphs. No `.asset` has been generated in Unity and no scene/prefab wiring exists in the repository, so it is not yet a playable mission. |
| LAST SONG, FIRST LIE | Social navigation and live-event management | **Not authored.** No venue booking, performer-slot resolution or event-state producer is present. |
| THE OLD TERMS | Negotiation and conflicting-account analysis | **Not authored.** Dialogue, relationship and story-flag primitives exist, but no mission objective maps to them and no playable dialogue entry point is wired in this snapshot. |
| THE CUT | Three-perspective playable set piece | **Not authored.** No staged sequence/checkpoint/lead-coordination completion contract exists. |
| NO SAFE HOUSE | Hub setup and relationship interaction | **Not authored.** No hub upgrade, resource-spend or support-contact availability producer is present. |
| RUNNING TAB | Compact open-world job and negotiation | **Not authored.** No job lifecycle, payment/favour outcome or job-completion producer is present. |
| THE LONG LOOP | Authored route race and vehicle mastery | **Mission contract not authored.** A reusable route-race prototype now emits `race.long_loop.started`, `.checkpoint`, `.finished` and `.cancelled`; it accepts only sequential checkpoints crossed by a player-driven vehicle. No PORT SABLE race scene, rival/result comparison, winner choice or MissionDefinition exists yet. A route finish is not a competitive race result. |
| THE GUEST LIST | Live social navigation and event timing | **Not authored.** No guest-list, access, timed-event or venue-stage producer is present. |
| DRY DOCK | Harbour traversal and multi-party negotiation | **Not authored.** General vehicle events exist, but no harbour-route objective or property-dispute outcome producer is defined. |
| THE OFFER | Three-perspective negotiation and choice | **Not authored.** Authored dialogue choices can emit signals, but no mission-specific choices, graph assignment or objective contract exists. |
| A BETTER NUMBER | Crew selection and score preparation | **Not authored.** No crew roster, recruit-selection or preparation-complete producer is present. |
| ALL THE WAY DOWN | Three-lead auction score, event navigation, authored vehicle set piece and relationship choice | **Not authored.** Generic vehicle and dialogue signals do not cover auction state, score stages, crew support or the authored set-piece outcomes. |
| FAMILY RATE | Environmental storytelling and character-led exploration | **Not authored.** Dialogue and relationship primitives exist; no clue/discovery interaction producer is currently emitted by the generic interaction path. |
| OLD ACCOUNTS | Dialogue with persistent memory and return-to-place exploration | **Not authored.** Relationship/flag persistence exists, but there is no mission-specific memory, revisit or conversation-complete contract. |
| A FRIENDLY FACE | Social crisis management and public event navigation | **Not authored.** No staff-trust repair, event-state or venue resolution producer is present. |
| NO NEUTRAL GROUND | Parallel conversations and faction-state decision | **Not authored.** No faction-standing ledger or faction-decision signal producer is present. Relationship values are not a substitute for faction state. |
| THE WRONG SIDE OF THE WATER | Cinematic pursuit and relationship decision | **Not authored.** A confrontation outcome component exists, but no pursuit objective producer or verified call path into that component is present. |
| LAST CALL | Playable hub drama and mission-preparation choice | **Not authored.** No hub-state, preparation or support-availability producer is present. |
| THE AUCTION | Three-lead parallel set piece and event navigation | **Not authored.** No event-stage, parallel-lead coordination or ally-protection objective producer is present. |
| THE LAST COLLECTION | Branching multi-lead score with crew support | **Not authored.** No score-stage, crew-support or collection-completion producer is present. |
| NO CLEAN EXIT | Final multi-lead confrontation and authored epilogue | **Not authored.** Generic confrontation outcomes exist in code, but their call path is not established and no finale MissionDefinition/branch assets exist. |

## 2. Mara prologue — first objective contract

The Mara prologue is the first campaign slice with a concrete source-level objective sequence.

| Order | Objective | Completion signal | Producer |
|---:|---|---|---|
| 1 | Inspect steering, driver's door latch and engine rattle | `mara.workshop.order.completed` | `MaraWorkshopOrderController`, after all three distinct task IDs have been completed |
| 2 | Deliver the customer's vehicle through the configured Old Quay-to-Saltline route | `mara.delivery.finished` | `MaraDeliveryHandoffController`, after the assigned `RouteRaceController` has completely finished and the player hands the vehicle to the customer |
| 3 | Resolve Nico's challenge and close the shop | `prologue.mara.complete` | Final choice in `prologue.mara.nico_challenge` |

The workshop task IDs are `steering_check`, `door_latch_check` and `engine_rattle_check`. Each is one-shot per controller instance; duplicate interactions do not count twice. The final task batches `mara.workshop.task.completed` and `mara.workshop.order.completed` atomically. If that batch aborts, the last task is rolled back and can be repeated.

The route controller now requires a specific `VehicleDriveController` reference, so a different driven vehicle cannot satisfy this route. On final checkpoint, it publishes the generic route checkpoint/finish signals and marks `HasFinished` only after `DispatchCompleted`. The player must exit the vehicle and use `MaraCustomerHandoffInteractable`; `MaraDeliveryHandoffController` then publishes `mara.delivery.finished` once. If the route signal batch aborts, the final checkpoint is re-armed. If the handoff batch aborts, the handoff can be repeated.

The dialogue builder now creates two Mara graphs: the Juno/work-order conversation and a separate Nico follow-up intended for the delivery endpoint. The post-delivery choice writes `mara_race_choice=1` or `2`, and the final choice publishes `prologue.mara.complete`.

**Generation order in Unity:**
1. Run **Project Tideline → 9 — Generate PORT SABLE Prologue Dialogue**.
2. Run **Project Tideline → 12 — Generate Mara Prologue Mission**.
3. Run **Project Tideline → 10 — Audit PORT SABLE Prologue Dialogue** and **Project Tideline → 11 — Audit Mission Assets and Branches**.

This creates dialogue and mission ScriptableObjects only. A playable scene still needs a Mara hero prefab, customer vehicle prefab, PlayerInputRouter, ProximityInteractor, DialogueController/UI, a Juno `DialogueInteractable`, three workshop task stations sharing one `MaraWorkshopOrderController`, an assigned route controller with sequential trigger checkpoints and its specific player vehicle, a `MaraDeliveryStartInteractable` (blocked until all workshop tasks are complete), a `MaraCustomerHandoffInteractable` with `MaraDeliveryHandoffController` (blocked until the route is complete), and a `MaraPostDeliveryDialogueInteractable` (blocked until customer handoff is complete). The workshop and handoff controllers also need the scene's `MissionDirector` assigned so their gates can restore from the saved objective index. None of those scene/prefab instances is tracked in the current repository snapshot.
Mara's workshop tasks are now gated by Juno's persistent `mara_work_order_accepted = 1` choice flag. The flag is set by both branches that reach the work-order handoff, and the workshop controller reopens the gate from the restored mission objective index after the order has already been completed.
The exact graph IDs, node transitions, saved choice values and complete Inspector reference matrix are maintained in [PORT SABLE Prologue Dialogue Graphs](PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md). The loaded-scene audit also checks that the interaction services are active, the proximity interactor has actor/view references, the dialogue UI arrays are paired and assigned, the dialogue/interaction targets have non-trigger colliders, the designated vehicle has exactly one matching seat interaction, the route checkpoints are ordered, and Juno's workshop and customer-handoff controllers reference the same MissionDirector. The audit is authored but not yet executed in Unity; static CI verifies its source contract only. The Editor command **Project Tideline → 13 — Audit Mara Prologue Scene Wiring** checks a loaded scene against that contract; it has not yet been run because no Mara scene is tracked in this snapshot.

## 3. Existing signal producer inventory

These are signals the current source can emit. Their existence does not establish that they are used by a PORT SABLE mission.

| Signal | Producer | Emission condition | Repeatability / caveat |
|---|---|---|---|
| `vehicle.entered` | `VehicleSeatInteractable.EnterVehicle` | Successful player entry into a configured vehicle seat | Repeats on each successful entry. |
| `vehicle.exited` | `VehicleSeatInteractable.Exit` | Successful exit from the occupied vehicle | Repeats on each exit. `OnDisable` also calls `Exit` for an occupant, so disabling the component can emit this signal as cleanup rather than as an intentional player action. |
| `vehicle.taken` | `VehicleSeatInteractable.TakeoverSequence` | Successful entry after taking over an NPC-occupied vehicle | Only emitted for that takeover path, after `vehicle.entered`. It is not emitted for an unoccupied vehicle. |
| `vehicle.collision` | `VehicleDriveController.OnCollisionEnter` | Player-driven collision with relative-velocity squared at least 64 | Rate-limited to one event per 2.5 seconds. It is not a reliable substitute for race progress or a specific set-piece checkpoint. |
| `confrontation.resolved` | `CinematicConfrontationController.Finish` | The configured response sequence succeeds | One signal per finished sequence. The class exposes `Begin`, but it is not an `IInteractable`; no committed scene/caller wiring is available in this snapshot. |
| `confrontation.withdrawn` | `CinematicConfrontationController.Finish` | The player withdraws, or the active component is disabled | One signal per finish. Disabling an active controller also records a withdrawal, so this signal is not exclusively a deliberate player choice. |
| `confrontation.failed` | `CinematicConfrontationController.Finish` | The response sequence fails or times out | One signal per finished sequence. No mission uses it yet. |
| `world.attention.band.calm`, `.noticed`, `.escalated` | `PublicAttentionState.SetAttention` | Attention changes across a configured band boundary | Emits only on a band transition, not on every attention change. Initialisation and save restore do not emit a band signal. A later transition back into a band can emit it again. |
| `relationship.changed.<characterId>` | `NPCRelationshipLedger.ChangeValue` | Non-zero relationship mutation with mission-signal publication enabled | Emits once per mutation, can repeat. `DialogueController` suppresses this generic signal when a choice has its own authored `signalId`. |
| `story.flag.changed` | `NarrativeStateLedger.SetFlag` / `ClearFlag` | Flag set or successful clear with mission-signal publication enabled | A set emits even if the stored value is unchanged; clearing emits only when a flag existed. Explicit dialogue-choice signals suppress this generic signal for that choice. |
| Authored `choice.signalId` | `DialogueController.SelectChoice` | A dialogue choice is selected | The graph authors the signal ID and amount. The prologue builder defines choice signals, but its own documentation says generated graphs are not assigned to scene NPCs or mission triggers. No PORT SABLE mission currently consumes these signals. |
| `mara.workshop.task.completed` | `MaraWorkshopOrderController.TryCompleteTask` | A distinct configured workshop station is inspected | One per required station. Duplicate task IDs are rejected. The last task batches this event with order completion. |
| `mara.workshop.order.completed` | `MaraWorkshopOrderController.TryCompleteTask` | All three configured Mara vehicle checks are complete | One per controller instance. If the signal batch aborts, the last task is rolled back and can be repeated. |
| `mara.delivery.finished` | `MaraDeliveryHandoffController.TryCompleteHandoff` | The assigned route has fully finished and the player interacts with the customer after exiting the vehicle | One per handoff. If the handoff signal batch aborts, the controller clears its pending/completed state so the player can repeat the handoff. |
| `race.long_loop.started` | `RouteRaceController.BeginRace` | A configured route with at least one valid ordered trigger starts through `RouteRaceStartInteractable` | Once per start. Invalid route configuration or an already-active event is rejected. |
| `race.long_loop.checkpoint` | `RouteRaceController.TryRegisterCheckpoint` | The player-driven vehicle enters the exact next configured checkpoint | Once per checkpoint per run. Skipped, repeated, out-of-order and NPC-driven vehicle entries are ignored. |
| `race.long_loop.finished` | `RouteRaceController.TryRegisterCheckpoint` | The final configured checkpoint is accepted | Once per successful run. This means the route was completed; it does not encode time, rank, rival outcome or player choice. |
| `race.long_loop.cancelled` | `RouteRaceController.CancelRace` | An active route is explicitly cancelled by a caller | Once per cancellation. No UI cancellation control or scene caller is currently wired. |
| `race.long_loop.started` | `RouteRaceController.BeginRace` | A configured route with at least one valid ordered trigger starts through `RouteRaceStartInteractable` | Once per start. Invalid route configuration or an already-active event is rejected. |
| `race.long_loop.checkpoint` | `RouteRaceController.TryRegisterCheckpoint` | The player-driven vehicle enters the exact next configured checkpoint | Once per checkpoint per run. Skipped, repeated, out-of-order and NPC-driven vehicle entries are ignored. |
| `race.long_loop.finished` | `RouteRaceController.TryRegisterCheckpoint` | The final configured checkpoint is accepted | Once per successful run. This means the route was completed; it does not encode time, rank, rival outcome or player choice. |
| `race.long_loop.cancelled` | `RouteRaceController.CancelRace` | An active route is explicitly cancelled by a caller | Once per cancellation. No UI cancellation control or scene caller is currently wired. |

### Same-action signal batching

`MissionSignalBus.PublishBatch` now queues all valid signals from one gameplay action before dispatch starts. It is used for dialogue choices that need both generic ledger notifications, NPC vehicle takeovers (`vehicle.entered` plus `vehicle.taken`), and the final race checkpoint (`.checkpoint` plus `.finished`). This prevents the first signal from advancing the MissionDirector before the second signal belonging to the same action is delivered.

### Generic interaction signal metadata removed

The unused `IInteractable.SignalId` property and `InteractableBase`'s default `interaction.generic` field have been removed. `ProximityInteractor` does not publish a generic signal merely because an interaction button was pressed.

This is deliberate: the old `void Interact(GameObject actor)` contract cannot report whether an action actually succeeded. Automatically publishing a generic event would count invalid, rejected or incomplete interactions as objective progress. Concrete interaction components must publish their own specific signals only after their success conditions pass.

## 4. Interaction and scene-wiring evidence

| System | What source proves | What remains unproven |
|---|---|---|
| Vehicle seats | `ProximityInteractor` finds `IInteractable` components and calls `VehicleSeatInteractable.Interact`; the seat component emits its three specific signals on successful transitions. | No tracked scene/prefab proves a configured seat, vehicle controller, driver seat, exit point, camera rig or NPC takeover setup. |
| Vehicle collision | `VehicleDriveController` contains a Unity collision callback and publishes `vehicle.collision` when its conditions pass. | No tracked vehicle prefab/scene proves Rigidbody, colliders, wheel bindings or player-driven collision setup. |
| Dialogue | `ProximityInteractor` dispatches focused `IInteractable` instances; the new `DialogueInteractable` calls `DialogueController.Begin(dialogueGraph)` using Inspector-assigned references. The controller rejects missing/invalid start nodes and overlapping conversations. | The code path now exists, but no tracked NPC prefab or scene assigns the graph/controller references. The prologue builder still explicitly says its generated graphs are not assigned to scene NPCs or mission triggers, and generated graph assets are not included in this source snapshot. |
| Route race | `ProximityInteractor` dispatches `RouteRaceStartInteractable`; the route controller validates an ordered array of trigger checkpoints. Each `RouteRaceCheckpoint` accepts only a player-driven `VehicleDriveController`. | No tracked scene/prefab assigns the start interactable, route controller or checkpoint array. No rival AI, timing/placement result, or authored winner/protect-rival choice is implemented. |
| Confrontation | `ProximityInteractor` dispatches `CinematicConfrontationInteractable`, which calls `CinematicConfrontationController.Begin(actor, opponentAnimator)`; `Finish` emits the result after the sequence. | The source-level path now exists, but no tracked scene/prefab assigns the controller or opponent Animator. This is not verified playable scene wiring. |
| Relationships and story flags | Both ledgers store and serialize their state; dialogue choices apply consequences before publishing their explicit signal. | No scene/bootstrap asset proves that the persistent ledger instances and dialogue UI are configured in a playable PORT SABLE scene. |
| Public attention | A persistent component listens to upstream mission signals and emits band-transition signals. | No tracked scene proves that the component exists or that a playable objective can cause the required transition. |

## 5. Save/restore contract

The current save path has the following semantics:

- `SessionSaveCoordinator` stores `missionIndex`, `objectiveIndex` and `objectiveCount` in schema-v3 `SaveSnapshot`.
- `MissionDirector.RestoreProgress` restores an unfinished objective's partial count, clamped to the range zero through `requiredCount - 1`. It deliberately restores the objective as incomplete.
- Relationship and narrative-flag JSON are imported silently. The import paths do not republish `relationship.changed.*` or `story.flag.changed`.
- `PublicAttentionState.Restore` restores the value and raises its local UI/state event, but does not publish a mission signal.
- The signal queue itself is transient and is not serialized. No historic vehicle, dialogue or confrontation event is replayed after loading.
- The route-race controller's active flag, final checkpoint state and `HasFinished` flag are transient. Saving mid-route, or after route arrival but before handing over the vehicle, does not preserve route progress; the player must repeat the route after the scene is recreated. The mission objective advances only on the explicit customer handoff, not on route arrival.
- Mara's workshop task IDs and route checkpoint index are not serialized. A save before the three workshop checks are complete resets those checks on scene recreation; the mission objective has not advanced yet, so all three are repeated. After the work-order objective has advanced, `MaraWorkshopOrderController` reads the restored MissionDirector objective index and keeps the workshop gate open. A save during the delivery route or after route arrival but before customer handoff restarts the route from checkpoint zero. After the handoff objective advances, `MaraDeliveryHandoffController` reads the restored objective index and allows Nico's follow-up dialogue to open.

**Design consequence:** a MissionDefinition objective must not depend on a one-time event that happened before the active objective or that is expected to replay during load. If a later branch depends on an earlier choice, evaluate the persisted story flag or relationship value when resolving the branch. If an objective needs to observe a newly selected choice, its dialogue choice must emit the exact objective signal while that objective is active, and the objective progress must be saved after the resulting dispatch/advance has completed.

The current save model does not have a separate “completion pending” field. A normal objective completion is advanced synchronously after the signal batch drains; the active objective count is intended to be below its threshold at save time. A save invoked from inside a progress callback during dispatch is an edge case that still needs a focused Play Mode test.

## 6. Signal-batch abort recovery

The 4,096-signal runaway-loop guard originally cleared the queue without raising `DispatchCompleted`. If an objective reached its threshold earlier in that same batch, `MissionDirector` could remain completion-pending until another batch arrived; the first later signal could then be ignored by the old objective and lost to the next one.

The current source now raises an internal `DispatchAborted` notification after clearing the queue. `MissionDirector` clears its pending flag, rolls the active objective count back to at most `requiredCount - 1`, and logs that the authored action must be repeated. This is intentionally fail-closed: a signal batch that did not drain cannot complete an objective. The queue-abort path is statically checked, but it has not been exercised in Unity.

## 7. Required implementation gates

1. Keep mission signals owned by concrete successful interactions. The unused generic `IInteractable.SignalId` API has been removed so rejected interactions cannot generate false objective progress.
2. Attach `DialogueInteractable` to each prologue NPC in a real scene/prefab and assign its DialogueController and generated graph references; then test each prologue end-to-end.
3. Attach and configure `CinematicConfrontationInteractable` on the intended opponent in a real scene/prefab, assign its controller and Animator, and test success, timeout/failure, withdrawal and disable cleanup.
4. Extend the route-race prototype with rival timing/placement and authored win/protect-rival outcomes. Other missing domains still include venue event state, job lifecycle, hub upgrades/resources, recruit selection, faction standing, score stages and pursuit/set-piece stages.
5. For every objective, record: stable objective ID, exact signal ID, producer class/method, world interaction path, required count, whether it can repeat, whether it can be missed, and how partial progress survives save/restore.
6. Extend the Unity Editor audit to resolve each objective signal against an explicit producer declaration and generated dialogue data. A source registry alone must not claim scene wiring.
7. Generate MissionDefinition assets only after those contracts and producers exist. Then run Editor audits, Unity compilation, Play Mode objective completion, and fresh/restored-save test cases.

## 8. Follow-up source changes

- Removed the unused generic `IInteractable.SignalId` API rather than emitting false-positive completion events for failed interactions.
- Added `DialogueInteractable`, which uses the existing proximity-interaction route to open a configured dialogue graph.
- Added `CinematicConfrontationInteractable`, which uses the same route to start a configured non-graphic confrontation sequence; its controller still owns outcome signal emission.
- Added a route-race prototype: `RouteRaceStartInteractable` starts it, `RouteRaceCheckpoint` trigger components feed ordered progress, and `RouteRaceController` emits route start/checkpoint/finish/cancel signals. It requires the designated player vehicle and does not model a rival or competitive result.
- Added Mara's three-step workshop order and task interactables, a vehicle-bound route finish relay, split Juno/Nico dialogue graphs, and `MaraPrologueMissionBuilder` to generate the three-objective `two_minutes_to_close` MissionDefinition.
- Extended `MissionAssetAudit` to match each objective's `completionSignal` against literal `Publish` calls, `PublishBatch` signal entries, generated `DialogueGraph` choice signals and the supported dynamic relationship/attention/race patterns. An unmatched signal is now an audit error.
- Added seven Unity Editor regression cases: two for FIFO/nested batch dispatch and one completion callback, plus five for exact, relationship, attention-band, route-race and unknown signal resolution. They are included in the Editor audit report but have not been run in Unity.
- Changed `DialogueController.Begin` to return success/failure, reject overlapping conversations and reject graphs whose start node is missing. Disabling the controller now closes the active graph and releases the gameplay input lock.
- These are reusable source components only. No PORT SABLE scene or NPC prefab is included or claimed wired.

## 9. Verification boundary

This is a source-level audit. GitHub's static validator checks selected code patterns and repository consistency; it does not execute Unity, invoke collision callbacks, prove that a component exists in a scene, generate ScriptableObjects, or validate Android performance. No Unity compile, Editor menu run, Play Mode test, APK build or device test is claimed.
