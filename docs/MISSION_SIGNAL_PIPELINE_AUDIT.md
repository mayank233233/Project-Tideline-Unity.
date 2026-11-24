# Project Tideline — mission signal pipeline audit

**Audit target:** selected PORT SABLE source snapshot on 20 November 2026.  
**Audit type:** source-level event-ordering and signal-producer review.  
**Unity compilation and Play Mode:** not run.

## Result

Two runtime ordering defects were found and corrected:

1. A dialogue choice could publish its completion signal before the relationship and story-flag consequences had been stored.
2. A listener such as `PublicAttentionState` could publish a nested mission signal while another signal was still being dispatched. Since the director advanced immediately, nested or remaining events could be observed by the next mission.

The fix makes choice outcomes atomic from the mission system's perspective and defers objective advancement until the signal queue is drained.

## 1. Dialogue choice state ordering

Before the fix, `DialogueController.SelectChoice()` published `choice.signalId` first, then changed the relationship, then stored the story flag.

`MissionSignalBus.Raised` is synchronous. `MissionDirector.OnSignal()` can therefore complete an objective and evaluate the next mission's branch while the choice's flag or relationship is still old.

A simple reorder was not sufficient: `NPCRelationshipLedger.ChangeValue()` and `NarrativeStateLedger.SetFlag()` also publish generic mission signals. One of those could have completed the objective before the other state value was written.

### Correction

Both ledger methods now accept an optional `publishMissionSignal` argument, defaulting to `true` to preserve existing callers.

For a dialogue choice, the controller:
1. Applies the relationship delta with generic mission-signal publication suppressed.
2. Stores the story flag with generic mission-signal publication suppressed.
3. Publishes the authored `choice.signalId` once, after both mutations.
4. When a choice has no explicit signal ID, publishes the corresponding generic ledger signals only after the state updates.

The normal `RelationshipChanged` and `FlagChanged` ledger events are still raised. The suppression applies only to the mission signal bus.

The runtime director also validates every objective before starting or restoring a mission. It rejects null objectives, blank/duplicate objective IDs, blank descriptions, blank completion signals and counts below one. This mirrors the Editor audit and prevents a malformed asset from silently leaving a live campaign stuck.

## 2. Nested signal dispatch and mission transitions

`PublicAttentionState` listens for signals including `vehicle.taken`, `vehicle.collision`, `confrontation.resolved` and `confrontation.failed`. A threshold crossing can publish a second signal such as `world.attention.band.noticed` while the original signal is still being dispatched.

Previously, `MissionSignalBus.Publish()` invoked the event immediately and recursively. `MissionDirector.OnSignal()` also advanced the objective immediately. A nested event, or a remaining listener's work from the outer event, could therefore be processed after the director had already switched to the next objective or mission.

### Correction

`MissionSignalBus` now uses a FIFO queue:
- Every published signal is enqueued.
- A signal published by a listener is queued instead of recursively dispatched.
- All queued signals are delivered before `DispatchCompleted` is raised.
- Signals published during the completion callback are queued and drained in a subsequent pass.

`MissionDirector` now:
- Marks an objective completion as pending when its required count is reached.
- Ignores additional signals while that completion is pending.
- Advances the objective only from `DispatchCompleted`, after the current queue has drained.
- Clears the pending state when a mission is started or progress is restored.
- Subscribes to the bus's internal abort notification. If the 4,096-event cap discards a batch while an objective is completion-pending, the director clears the pending state and rolls the count back to at most `requiredCount - 1`. The objective must receive a fresh matching signal after the signal loop is fixed; a later unrelated batch cannot silently advance it.

This keeps one signal-dispatch batch attached to the objective that was active when the batch began. Branch evaluation occurs after nested state changes have been applied. The abort notification itself is exception-isolated and does not raise `DispatchCompleted`.

## 3. Current source-level signal producers

The inspected selected-snapshot sources currently include these explicit signal paths:

| Producer | Signal(s) | Trigger |
|---|---|---|
| `DialogueController` | authored `choice.signalId` | selected dialogue choice |
| `NPCRelationshipLedger` | `relationship.changed.<characterId>` | relationship mutation; suppressible for an atomic dialogue choice |
| `NarrativeStateLedger` | `story.flag.changed` | flag mutation/clear; suppressible for an atomic dialogue choice |
| `VehicleDriveController` | `vehicle.collision` | collision handler |
| `VehicleSeatInteractable` | `vehicle.taken`, `vehicle.entered`, `vehicle.exited` | successful vehicle interaction/transition |
| `CinematicConfrontationController` | `confrontation.resolved`, `confrontation.withdrawn`, `confrontation.failed` | confrontation outcome |
| `PublicAttentionState` | `world.attention.band.calm`, `world.attention.band.noticed`, `world.attention.band.escalated` | attention changes across a configured band boundary |

The signal list is a runtime source inventory, not proof that any campaign mission consumes those signals. One additional interface gap was found: `IInteractable.SignalId` is stored by `InteractableBase`, but `ProximityInteractor` does not read or publish it. The property must not be treated as a working generic mission signal.

See the separate [Mission Signal Producer Inventory](MISSION_SIGNAL_PRODUCER_INVENTORY.md) for emission conditions, repeatability, interaction wiring, per-mission gaps and save/restore behaviour.

## 4. Remaining content and wiring gap

The campaign registry still contains 21 narrative metadata records, not playable `MissionDefinition` assets. The selected source snapshot contains zero PORT SABLE `MissionDefinition` assets, and the four prologue outcome flags are not yet consumed by PORT SABLE mission branch assets.

Consequently, no source-level audit can yet prove that all authored campaign objectives have valid, reachable signal producers. Before creating the mission assets, each objective needs a specific signal contract and a concrete in-world producer. Objectives should not be populated with signal strings merely to make the asset validator pass.

The next implementation gate is:
1. Define a signal contract for each planned objective.
2. Implement or identify the gameplay component that emits each signal.
3. Add mission assets with real objectives, ordered branch conditions and explicit fallbacks.
4. Extend the Editor audit to validate every objective's completion signal against registered producers and generated dialogue assets.
5. Run Unity compilation and Play Mode tests, including nested-signal transitions and branch choices from fresh and restored saves.

## 5. Verification

The latest verified GitHub Actions run is [45344726457](https://github.com/mayank233233/Project-Tideline-Unity./actions/runs/45344726457), on commit `217e95e`: **PASS — 148 checks, 33 C# files scanned**. The legacy mission sequence validator reports **SKIP** because the full recovered `OldQuayVerticalSliceBuilder.cs` is absent from this selected snapshot. The static validator checks repository structure and selected source contracts; it does not execute Unity code.

The legacy Old Quay sequence validator still reports `SKIP` on the selected GitHub snapshot because the full recovered `OldQuayVerticalSliceBuilder.cs` is not present there. The local recovered-tree pass from the previous audit remains a separate result.

No Unity compilation, Editor asset generation, Play Mode test, APK build or physical-device test is claimed.
