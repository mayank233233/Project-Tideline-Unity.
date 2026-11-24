# PORT SABLE — Prologue Dialogue Graph and Scene Wiring Reference

This document records the exact graph IDs, authored node flow, choice signals, persistent outcomes and required scene references in the current source. It is a wiring contract, not evidence that a scene has already been assembled or played.

## Graph inventory

**Project Tideline → 9 — Generate PORT SABLE Prologue Dialogue** creates or updates four `DialogueGraph` assets for three lead prologues:

| Lead | Graph ID | Asset filename | Intended entry point |
|---|---|---|---|
| Mara Kade | `prologue.mara.two_minutes_to_close` | `Dialogue_prologue.mara.two_minutes_to_close.asset` | Juno at Kade Motor Works; work-order conversation |
| Mara Kade | `prologue.mara.nico_challenge` | `Dialogue_prologue.mara.nico_challenge.asset` | Nico at the customer delivery endpoint, after the handoff |
| Rafi Sayegh | `prologue.rafi.last_song_first_lie` | `Dialogue_prologue.rafi.last_song_first_lie.asset` | Mina at Lowlight soundcheck |
| Elias Ward | `prologue.elias.the_old_terms` | `Dialogue_prologue.elias.the_old_terms.asset` | Operator in the settlement room |

The four assets are intentionally not three: Mara has two separate conversations, while Rafi and Elias each have one. The generator creates ScriptableObjects only. It does not create NPC prefabs, a playable scene, UI, cameras, mission assets or scene references.

## Mara — opening and work-order graph

Graph: `prologue.mara.two_minutes_to_close`

Start node: `garage_close`

| Node | Speaker | Choice | Next node | Signal | Persistent outcome |
|---|---|---|---|---|---|
| `garage_close` | JUNO | The customer's car comes first. | `work_order` | `prologue.mara.work_first` | `juno_kade +2` |
| `garage_close` | JUNO | What is that notice? | `account_notice` | `prologue.mara.notice_raised` | `juno_kade -1` |
| `work_order` | JUNO | I'll finish all three checks before the delivery. | End | `prologue.mara.work_order_accepted` | `juno_kade +1` |
| `account_notice` | JUNO | You should have told me. | `notice_close` | `prologue.mara.challenge_juno` | `juno_kade -2` |
| `account_notice` | JUNO | Then we talk after the drive. | `notice_close` | `prologue.mara.defer_argument` | `juno_kade +1` |
| `notice_close` | MARA | Get the vehicle ready for its final checks. | End | `prologue.mara.work_order_accepted` | `juno_kade +0` |

Both conversation routes can reach the work-order handoff signal. The graph's authored choice is not itself a mission objective. The first executable MissionDefinition objective listens for `mara.workshop.order.completed`, emitted only after all three unique workshop stations have been inspected.

## Mara — Nico's post-delivery graph

Graph: `prologue.mara.nico_challenge`

Start node: `nico_at_delivery`

| Node | Choice | Next node | Signal | Persistent outcome |
|---|---|---|---|---|
| `nico_at_delivery` | There is one way to find out. | `mara_accepts_challenge` | `prologue.mara.accept_challenge` | `nico_okoye +2`; `mara_race_choice = 1` |
| `nico_at_delivery` | The customer paid for a delivery, not a race. | `mara_declines_challenge` | `prologue.mara.decline_challenge` | `nico_okoye +1`; `mara_race_choice = 2` |
| `mara_accepts_challenge` | The car is delivered. Let's close the shop. | End | `prologue.mara.complete` | `juno_kade +1` |
| `mara_declines_challenge` | The car is delivered. Let's close the shop. | End | `prologue.mara.complete` | `juno_kade +1` |

The challenge is a narrative choice after the customer delivery, not a playable competitive race. Both branches keep the customer-first delivery outcome; the saved flag records Mara's response for later campaign content.

## Rafi — LAST SONG, FIRST LIE

Graph: `prologue.rafi.last_song_first_lie`

Start node: `lowlight_soundcheck`

| Node | Choice | Next node | Signal | Persistent outcome |
|---|---|---|---|---|
| `lowlight_soundcheck` | Keep the first promise. | `honour_first_slot` | `prologue.rafi.honour_first_slot` | `mina_sato +3`; `rafi_event_choice = 1` |
| `lowlight_soundcheck` | Keep the headline event intact. | `protect_headline` | `prologue.rafi.protect_headline` | `mina_sato -2`; `rafi_event_choice = 2` |
| `honour_first_slot` | Tell the performers yourself. | `mina_reply` | `prologue.rafi.own_promise` | `mina_sato +2`; `rafi_staff_responsibility = 1` |
| `protect_headline` | I'll take it. | `mina_reply` | `prologue.rafi.take_responsibility` | `mina_sato +1`; `rafi_staff_responsibility = 1` |
| `protect_headline` | Mina, tell them. | `mina_reply` | `prologue.rafi.shift_burden` | `mina_sato -3`; `rafi_staff_responsibility = 2` |
| `mina_reply` | I know. I'll make the call. | `house_invitation` | `prologue.rafi.staff_commitment` | `mina_sato +2` |
| `house_invitation` | End the sound check. | End | `prologue.rafi.complete` | `mina_sato +1` |

## Elias — THE OLD TERMS

Graph: `prologue.elias.the_old_terms`

Start node: `settlement_room`

| Node | Choice | Next node | Signal | Persistent outcome |
|---|---|---|---|---|
| `settlement_room` | Tell me what you were promised. | `hear_operator` | `prologue.elias.listen_first` | `silas_rook +0` |
| `settlement_room` | The terms are not as simple as they look. | `explain_terms` | `prologue.elias.explain_terms` | `silas_rook +1` |
| `hear_operator` | Continue the meeting. | `silas_call` | `prologue.elias.listened` | `anika_bose +1` |
| `explain_terms` | Continue the meeting. | `silas_call` | `prologue.elias.acknowledged_cost` | `silas_rook -1` |
| `silas_call` | I'll be there. I'm not promising to like what I hear. | `accept_invitation` | `prologue.elias.accept_invitation` | `silas_rook +2`; `elias_settlement_choice = 1` |
| `silas_call` | I'll attend. The next page doesn't get my name just because you brought the pen. | `refuse_signature` | `prologue.elias.refuse_signature` | `silas_rook -2`; `elias_settlement_choice = 2` |
| `accept_invitation` | Leave the meeting room. | End | `prologue.elias.complete` | `silas_rook +1` |
| `refuse_signature` | Leave the meeting room. | End | `prologue.elias.complete` | `silas_rook +1` |

## Required scene wiring — Mara's first playable slice

The source snapshot currently contains no tracked PORT SABLE scene or actor/NPC/vehicle prefabs. The following is the explicit Inspector wiring contract for the first Mara slice.

### Shared scene services

| Scene object | Required component | Required references / conditions |
|---|---|---|
| Player input root | `PlayerInputRouter` | Exactly one active singleton in the scene |
| Mara hero root | `CharacterController`, `ThirdPersonMotor`, `ProximityInteractor`, `CharacterAnimationDriver` | A valid player camera/view origin; proximity interactor must target Mara's actor root and interaction layer |
| Dialogue UI root | `DialogueController` | Assign `CanvasGroup panel`, speaker and line `Text`, matching choice `Button[]` and label `Text[]`, and a close button |
| Narrative services | `NarrativeStateLedger`, `NPCRelationshipLedger` | One active instance of each; dialogue choices update these ledgers before publishing their authored signals |
| Mission service | `MissionDirector` | Campaign array includes generated `two_minutes_to_close`; auto-start only when this is the intended first mission |

### Juno, workshop and delivery

| Scene object | Component | Inspector wiring |
|---|---|---|
| Juno NPC | `DialogueInteractable` | Shared `DialogueController`; graph `prologue.mara.two_minutes_to_close` |
| Workshop order root | `MaraWorkshopOrderController` | Assign scene `MissionDirector`; task IDs exactly `steering_check`, `door_latch_check`, `engine_rattle_check` |
| Steering station | `MaraWorkshopTaskInteractable` | Shared order controller; task ID `steering_check` |
| Door-latch station | `MaraWorkshopTaskInteractable` | Shared order controller; task ID `door_latch_check` |
| Engine-rattle station | `MaraWorkshopTaskInteractable` | Shared order controller; task ID `engine_rattle_check` |
| Customer delivery vehicle | `VehicleDriveController` | This exact component instance is assigned as the route's `playerVehicle`; the player's vehicle-seat interaction must call `SetDriven(true)` when occupied |
| Delivery route root | `RouteRaceController` | Assign the designated player vehicle and checkpoint references in ascending index order |
| Each route trigger | `RouteRaceCheckpoint` + trigger `Collider` | Assign the same route controller; indices must be contiguous from zero; each collider must have `isTrigger = true` |
| Delivery-start marker | `MaraDeliveryStartInteractable` | Assign the shared workshop order and route; it refuses to start before the order is complete |
| Customer NPC at destination | `MaraCustomerHandoffInteractable` | Assign the handoff controller; this interaction, not the final checkpoint, emits `mara.delivery.finished` |
| Handoff controller | `MaraDeliveryHandoffController` | Assign the same route and scene `MissionDirector`; objective index restoration re-opens the correct gate after load |

### Nico and mission completion

| Scene object | Component | Inspector wiring |
|---|---|---|
| Nico NPC | `MaraPostDeliveryDialogueInteractable` | Assign the same handoff controller, shared dialogue controller and graph `prologue.mara.nico_challenge` |
| MissionDirector campaign entry | `MissionDefinition` | Ordered signals: `mara.workshop.order.completed`, `mara.delivery.finished`, `prologue.mara.complete` |

The gates are intentionally staged: the route-start interaction requires the workshop objective to have been completed; the route itself requires its designated driven vehicle to cross every checkpoint in order; the customer handoff requires `RouteRaceController.HasFinished`; Nico's conversation requires the handoff to have advanced the MissionDirector to objective index 2.

### Editor audit and generation order

1. Run **Project Tideline → 9 — Generate PORT SABLE Prologue Dialogue**.
2. Run **Project Tideline → 10 — Audit PORT SABLE Prologue Dialogue**.
3. Run **Project Tideline → 12 — Generate Mara Prologue Mission**.
4. Create/open the Mara scene and assign the references listed above.
5. Run **Project Tideline → 13 — Audit Mara Prologue Scene Wiring**.
6. Run **Project Tideline → 11 — Audit Mission Assets and Branches**.

The scene-wiring audit inspects loaded scene instances and their serialized references. It reports missing services, duplicate singletons, missing task stations, invalid checkpoint arrays, incorrect dialogue graph bindings and mission objective signal mismatches. It does not create placeholder geometry or pretend an unconfigured scene is playable.

## Verification boundary

The dialogue builder and graph audit are source code. The graph assets are generated only when the Unity Editor menu is run. The scene-wiring audit has been authored but has not been executed in Unity. No PORT SABLE scene/prefab wiring, C# compilation, Play Mode, save/load gameplay, Android build or physical-device test is claimed by this document.
