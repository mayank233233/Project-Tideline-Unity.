# Project Tideline — mission signal producer and objective contract audit

**Audit target:** PORT SABLE source snapshot on 24 November 2026 (updated after the interaction/dialogue implementation pass).  
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
| TWO MINUTES TO CLOSE | Workshop interaction and precision driving | **Not authored.** No workshop-job completion or authored route/race result signal is defined. Dialogue choices and generic vehicle events are only possible building blocks. |
| LAST SONG, FIRST LIE | Social navigation and live-event management | **Not authored.** No venue booking, performer-slot resolution or event-state producer is present. |
| THE OLD TERMS | Negotiation and conflicting-account analysis | **Not authored.** Dialogue, relationship and story-flag primitives exist, but no mission objective maps to them and no playable dialogue entry point is wired in this snapshot. |
| THE CUT | Three-perspective playable set piece | **Not authored.** No staged sequence/checkpoint/lead-coordination completion contract exists. |
| NO SAFE HOUSE | Hub setup and relationship interaction | **Not authored.** No hub upgrade, resource-spend or support-contact availability producer is present. |
| RUNNING TAB | Compact open-world job and negotiation | **Not authored.** No job lifecycle, payment/favour outcome or job-completion producer is present. |
| THE LONG LOOP | Authored route race and vehicle mastery | **Not authored.** Vehicle collision exists, but race start, checkpoint, finish, result and winner signals do not. A collision is not a race result. |
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

## 2. Existing signal producer inventory

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

### Generic interaction signal metadata removed

The unused `IInteractable.SignalId` property and `InteractableBase`'s default `interaction.generic` field have been removed. `ProximityInteractor` does not publish a generic signal merely because an interaction button was pressed.

This is deliberate: the old `void Interact(GameObject actor)` contract cannot report whether an action actually succeeded. Automatically publishing a generic event would count invalid, rejected or incomplete interactions as objective progress. Concrete interaction components must publish their own specific signals only after their success conditions pass.

## 3. Interaction and scene-wiring evidence

| System | What source proves | What remains unproven |
|---|---|---|
| Vehicle seats | `ProximityInteractor` finds `IInteractable` components and calls `VehicleSeatInteractable.Interact`; the seat component emits its three specific signals on successful transitions. | No tracked scene/prefab proves a configured seat, vehicle controller, driver seat, exit point, camera rig or NPC takeover setup. |
| Vehicle collision | `VehicleDriveController` contains a Unity collision callback and publishes `vehicle.collision` when its conditions pass. | No tracked vehicle prefab/scene proves Rigidbody, colliders, wheel bindings or player-driven collision setup. |
| Dialogue | `ProximityInteractor` dispatches focused `IInteractable` instances; the new `DialogueInteractable` calls `DialogueController.Begin(dialogueGraph)` using Inspector-assigned references. The controller rejects missing/invalid start nodes and overlapping conversations. | The code path now exists, but no tracked NPC prefab or scene assigns the graph/controller references. The prologue builder still explicitly says its generated graphs are not assigned to scene NPCs or mission triggers, and generated graph assets are not included in this source snapshot. |
| Confrontation | `ProximityInteractor` dispatches `CinematicConfrontationInteractable`, which calls `CinematicConfrontationController.Begin(actor, opponentAnimator)`; `Finish` emits the result after the sequence. | The source-level path now exists, but no tracked scene/prefab assigns the controller or opponent Animator. This is not verified playable scene wiring. |
| Relationships and story flags | Both ledgers store and serialize their state; dialogue choices apply consequences before publishing their explicit signal. | No scene/bootstrap asset proves that the persistent ledger instances and dialogue UI are configured in a playable PORT SABLE scene. |
| Public attention | A persistent component listens to upstream mission signals and emits band-transition signals. | No tracked scene proves that the component exists or that a playable objective can cause the required transition. |

## 4. Save/restore contract

The current save path has the following semantics:

- `SessionSaveCoordinator` stores `missionIndex`, `objectiveIndex` and `objectiveCount` in schema-v3 `SaveSnapshot`.
- `MissionDirector.RestoreProgress` restores an unfinished objective's partial count, clamped to the range zero through `requiredCount - 1`. It deliberately restores the objective as incomplete.
- Relationship and narrative-flag JSON are imported silently. The import paths do not republish `relationship.changed.*` or `story.flag.changed`.
- `PublicAttentionState.Restore` restores the value and raises its local UI/state event, but does not publish a mission signal.
- The signal queue itself is transient and is not serialized. No historic vehicle, dialogue or confrontation event is replayed after loading.

**Design consequence:** a MissionDefinition objective must not depend on a one-time event that happened before the active objective or that is expected to replay during load. If a later branch depends on an earlier choice, evaluate the persisted story flag or relationship value when resolving the branch. If an objective needs to observe a newly selected choice, its dialogue choice must emit the exact objective signal while that objective is active, and the objective progress must be saved after the resulting dispatch/advance has completed.

The current save model does not have a separate “completion pending” field. A normal objective completion is advanced synchronously after the signal batch drains; the active objective count is intended to be below its threshold at save time. A save invoked from inside a progress callback during dispatch is an edge case that still needs a focused Play Mode test.

## 5. Signal-batch abort recovery

The 4,096-signal runaway-loop guard originally cleared the queue without raising `DispatchCompleted`. If an objective reached its threshold earlier in that same batch, `MissionDirector` could remain completion-pending until another batch arrived; the first later signal could then be ignored by the old objective and lost to the next one.

The current source now raises an internal `DispatchAborted` notification after clearing the queue. `MissionDirector` clears its pending flag, rolls the active objective count back to at most `requiredCount - 1`, and logs that the authored action must be repeated. This is intentionally fail-closed: a signal batch that did not drain cannot complete an objective. The queue-abort path is statically checked, but it has not been exercised in Unity.

## 6. Required implementation gates

1. Decide whether generic interactions should publish an authored signal. If yes, implement it once at the interaction boundary with explicit duplicate/failed-interaction semantics; otherwise remove or clearly deprecate the unused `SignalId` property.
2. Attach `DialogueInteractable` to each prologue NPC in a real scene/prefab and assign its DialogueController and generated graph references; then test each prologue end-to-end.
3. Attach and configure `CinematicConfrontationInteractable` on the intended opponent in a real scene/prefab, assign its controller and Animator, and test success, timeout/failure, withdrawal and disable cleanup.
4. Implement the missing gameplay domains required by the campaign: race checkpoints/results, venue event state, job lifecycle, hub upgrades/resources, recruit selection, faction standing, score stages and pursuit/set-piece stages.
5. For every objective, record: stable objective ID, exact signal ID, producer class/method, world interaction path, required count, whether it can repeat, whether it can be missed, and how partial progress survives save/restore.
6. Extend the Unity Editor audit to resolve each objective signal against an explicit producer declaration and generated dialogue data. A source registry alone must not claim scene wiring.
7. Generate MissionDefinition assets only after those contracts and producers exist. Then run Editor audits, Unity compilation, Play Mode objective completion, and fresh/restored-save test cases.

## 8. Follow-up source changes

- Removed the unused generic `IInteractable.SignalId` API rather than emitting false-positive completion events for failed interactions.
- Added `DialogueInteractable`, which uses the existing proximity-interaction route to open a configured dialogue graph.
- Added `CinematicConfrontationInteractable`, which uses the same route to start a configured non-graphic confrontation sequence; its controller still owns outcome signal emission.
- Changed `DialogueController.Begin` to return success/failure, reject overlapping conversations and reject graphs whose start node is missing. Disabling the controller now closes the active graph and releases the gameplay input lock.
- These are reusable source components only. No PORT SABLE scene or NPC prefab is included or claimed wired.

## 7. Verification boundary

This is a source-level audit. GitHub's static validator checks selected code patterns and repository consistency; it does not execute Unity, invoke collision callbacks, prove that a component exists in a scene, generate ScriptableObjects, or validate Android performance. No Unity compile, Editor menu run, Play Mode test, APK build or device test is claimed.
