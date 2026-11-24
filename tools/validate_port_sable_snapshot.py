#!/usr/bin/env python3
"""Static consistency checks for the selected PORT SABLE source snapshot.

This intentionally does not attempt to compile Unity C# or prove gameplay.
It checks repository metadata, authored campaign counts/IDs, save/branch contracts,
dialogue flags, and basic C# delimiter/preprocessor structure.
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ERRORS: list[str] = []
CHECKS = 0


def check(condition: bool, message: str) -> None:
    global CHECKS
    CHECKS += 1
    if not condition:
        ERRORS.append(message)


def read(path: str) -> str:
    file_path = ROOT / path
    check(file_path.is_file(), f"Missing required file: {path}")
    if not file_path.is_file():
        return ""
    return file_path.read_text(encoding="utf-8")


def balanced_csharp(path: Path, source: str) -> None:
    """Check (), [], {} and #if/#endif while ignoring comments and string bodies."""
    stack: list[tuple[str, int]] = []
    state = "normal"
    i = 0
    line = 1
    preprocessor_stack: list[int] = []
    pairs = {")": "(", "]": "[", "}": "{"}

    while i < len(source):
        ch = source[i]
        nxt = source[i + 1] if i + 1 < len(source) else ""

        if ch == "\n":
            line += 1

        if state == "line_comment":
            if ch == "\n":
                state = "normal"
            i += 1
            continue

        if state == "block_comment":
            if ch == "*" and nxt == "/":
                state = "normal"
                i += 2
            else:
                i += 1
            continue

        if state == "string":
            if ch == "\\":
                i += 2
                continue
            if ch == '"':
                state = "normal"
            i += 1
            continue

        if state == "verbatim_string":
            if ch == '"' and nxt == '"':
                i += 2
                continue
            if ch == '"':
                state = "normal"
            i += 1
            continue

        if state == "char":
            if ch == "\\":
                i += 2
                continue
            if ch == "'":
                state = "normal"
            i += 1
            continue

        # Normal lexical state.
        if ch == "/" and nxt == "/":
            state = "line_comment"
            i += 2
            continue
        if ch == "/" and nxt == "*":
            state = "block_comment"
            i += 2
            continue

        if ch == "@" and nxt == '"':
            state = "verbatim_string"
            i += 2
            continue
        # Skip the $ or $@ prefix. Interpolated-expression braces are inside
        # the string and deliberately not treated as class/method delimiters.
        if ch == "$" and nxt == '"':
            state = "string"
            i += 2
            continue
        if ch == "$" and nxt == "@" and i + 2 < len(source) and source[i + 2] == '"':
            state = "verbatim_string"
            i += 3
            continue
        if ch == "@" and nxt == "$" and i + 2 < len(source) and source[i + 2] == '"':
            state = "verbatim_string"
            i += 3
            continue
        if ch == '"':
            state = "string"
            i += 1
            continue
        if ch == "'":
            state = "char"
            i += 1
            continue

        if ch == "#" and (i == 0 or source[source.rfind("\n", 0, i) + 1:i].strip() == ""):
            directive = re.match(r"#\s*(if|endif)\b", source[i:])
            if directive:
                if directive.group(1) == "if":
                    preprocessor_stack.append(line)
                elif not preprocessor_stack:
                    ERRORS.append(f"{path}:{line}: #endif without matching #if")
                else:
                    preprocessor_stack.pop()

        if ch in "([{":
            stack.append((ch, line))
        elif ch in ")]}":
            if not stack or stack[-1][0] != pairs[ch]:
                ERRORS.append(f"{path}:{line}: unmatched closing delimiter {ch}")
            else:
                stack.pop()
        i += 1

    for opening, opening_line in stack:
        ERRORS.append(f"{path}:{opening_line}: unclosed delimiter {opening}")
    for opening_line in preprocessor_stack:
        ERRORS.append(f"{path}:{opening_line}: #if without matching #endif")
    if state in {"block_comment", "string", "verbatim_string", "char"}:
        ERRORS.append(f"{path}: unterminated {state.replace('_', ' ')}")


def run() -> int:
    manifest_text = read("Packages/manifest.json")
    asmdef_text = read("Assets/ProjectTideline/Scripts/ProjectTideline.Runtime.asmdef")
    version_text = read("ProjectSettings/ProjectVersion.txt")

    try:
        manifest = json.loads(manifest_text)
        check("com.unity.render-pipelines.universal" in manifest["dependencies"],
              "URP package missing from Packages/manifest.json")
        check("com.unity.inputsystem" in manifest["dependencies"],
              "Input System package missing from Packages/manifest.json")
        check("com.unity.cloud.gltfast" in manifest["dependencies"],
              "glTFast package missing from Packages/manifest.json")
    except (json.JSONDecodeError, KeyError, TypeError) as exc:
        ERRORS.append(f"Invalid package manifest: {exc}")

    try:
        asmdef = json.loads(asmdef_text)
        check(asmdef.get("name") == "ProjectTideline.Runtime",
              "Runtime assembly definition has the wrong assembly name")
    except json.JSONDecodeError as exc:
        ERRORS.append(f"Invalid runtime assembly definition: {exc}")

    check("m_EditorVersion: 6000.3.28f1" in version_text,
          "Unity editor version is not pinned to recovered 6000.3.28f1")

    canon = read("Assets/ProjectTideline/Scripts/Story/CampaignCanon.cs")
    check('CampaignTitle = "PORT SABLE: NO FAVOURS LEFT"' in canon,
          "Campaign title is missing or inconsistent")
    check(len(re.findall(r"new Mission\(", canon)) == 21,
          "Campaign registry must contain 3 prologues plus 18 core missions (21 total)")
    check(len(re.findall(r'new Character\("mara_kade"|new Character\("rafi_sayegh"|new Character\("elias_ward"', canon)) == 3,
          "Exactly three playable leads must be registered")
    for faction in ("the_house", "the_cutwater_circle", "the_breakers",
                    "the_velvet_circuit", "southline_motors", "crownline_response"):
        check(f'"{faction}"' in canon, f"Missing campaign faction ID: {faction}")
    for district in ("old_quay", "southline", "the_velvet_mile", "breaker_island",
                     "crown_heights", "saltline", "glass_ward"):
        check(f'"{district}"' in canon, f"Missing district ID: {district}")
    for ending in ("the_empty_chair", "the_new_house", "clean_exit"):
        check(f'"{ending}"' in canon, f"Missing campaign ending ID: {ending}")

    builder = read("Assets/Editor/ProjectTideline/PortSablePrologueDialogueBuilder.cs")
    dialogue_audit = read("Assets/Editor/ProjectTideline/PortSablePrologueDialogueAudit.cs")
    mara_mission_builder = read("Assets/Editor/ProjectTideline/MaraPrologueMissionBuilder.cs")
    mara_workshop = read("Assets/ProjectTideline/Scripts/World/MaraWorkshopOrderController.cs")
    mara_workshop_interactable = read("Assets/ProjectTideline/Scripts/Interaction/MaraWorkshopTaskInteractable.cs")
    mara_handoff_controller = read("Assets/ProjectTideline/Scripts/World/MaraDeliveryHandoffController.cs")
    mara_customer_handoff = read("Assets/ProjectTideline/Scripts/Interaction/MaraCustomerHandoffInteractable.cs")
    mara_delivery_start = read("Assets/ProjectTideline/Scripts/Interaction/MaraDeliveryStartInteractable.cs")
    mara_nico_interactable = read("Assets/ProjectTideline/Scripts/Interaction/MaraPostDeliveryDialogueInteractable.cs")
    mara_scene_audit = read("Assets/Editor/ProjectTideline/MaraPrologueSceneWiringAudit.cs")
    prologue_graph_docs = read("docs/PORT_SABLE_PROLOGUE_DIALOGUE_GRAPHS.md")
    for graph_id in (
        "prologue.mara.two_minutes_to_close",
        "prologue.mara.nico_challenge",
        "prologue.rafi.last_song_first_lie",
        "prologue.elias.the_old_terms",
    ):
        check(graph_id in builder, f"Missing generated prologue graph ID: {graph_id}")
        check(graph_id in dialogue_audit, f"Dialogue audit does not expect graph ID: {graph_id}")

    check("BuildMaraNicoChallenge()" in builder
          and "prologue.mara.nico_challenge" in builder
          and "The steering's straight, the latch holds, and the rattle's gone." not in builder,
          "Mara's opening dialogue must hand off to workshop gameplay and use a separate post-delivery Nico graph")
    check("prologue.mara.work_order_accepted" in dialogue_audit
          and "prologue.mara.complete" in dialogue_audit,
          "Mara dialogue audit must require the workshop handoff and mission completion signals")

    required_flags = {
        "mara_work_order_accepted": (1,),
        "mara_race_choice": (1, 2),
        "rafi_event_choice": (1, 2),
        "rafi_staff_responsibility": (1, 2),
        "elias_settlement_choice": (1, 2),
    }
    for flag, values in required_flags.items():
        check(flag in builder, f"Dialogue builder does not write required choice flag {flag}")
        check(flag in dialogue_audit, f"Dialogue audit does not check required choice flag {flag}")
        for value in values:
            pattern = re.escape(f'"{flag}", {value})')
            check(re.search(pattern, builder) is not None,
                  f"Dialogue builder does not write {flag}={value}")

    required_choice_outcomes = {
        "prologue.mara.accept_challenge": ("mara_race_choice", 1),
        "prologue.mara.decline_challenge": ("mara_race_choice", 2),
        "prologue.rafi.honour_first_slot": ("rafi_event_choice", 1),
        "prologue.rafi.protect_headline": ("rafi_event_choice", 2),
        "prologue.rafi.own_promise": ("rafi_staff_responsibility", 1),
        "prologue.rafi.take_responsibility": ("rafi_staff_responsibility", 1),
        "prologue.rafi.shift_burden": ("rafi_staff_responsibility", 2),
        "prologue.elias.accept_invitation": ("elias_settlement_choice", 1),
        "prologue.elias.refuse_signature": ("elias_settlement_choice", 2),
    }
    for signal, (flag, value) in required_choice_outcomes.items():
        pattern = (
            re.escape(f'"{signal}"')
            + r'\s*,\s*"[^"]+"\s*,\s*-?\d+\s*,\s*'
            + re.escape(f'"{flag}"')
            + r'\s*,\s*' + str(value) + r'\s*\)'
        )
        check(re.search(pattern, builder) is not None,
              f"Dialogue choice {signal} must write {flag}={value}")
        check(signal in dialogue_audit,
              f"Dialogue audit must validate the specific choice {signal}")

    input_router = read("Assets/ProjectTideline/Scripts/Input/PlayerInputRouter.cs")
    character_animation_driver = read("Assets/ProjectTideline/Scripts/Animation/CharacterAnimationDriver.cs")
    proximity_interactor = read("Assets/ProjectTideline/Scripts/Interaction/ProximityInteractor.cs")
    mission_director = read("Assets/ProjectTideline/Scripts/Missions/MissionDirector.cs")
    mission_signal_bus = read("Assets/ProjectTideline/Scripts/Missions/MissionSignalBus.cs")
    branch_evaluator = read("Assets/ProjectTideline/Scripts/Missions/MissionBranchEvaluator.cs")
    relationship_ledger = read("Assets/ProjectTideline/Scripts/NPC/NPCRelationshipLedger.cs")
    narrative_ledger = read("Assets/ProjectTideline/Scripts/Core/NarrativeStateLedger.cs")
    dialogue_controller = read("Assets/ProjectTideline/Scripts/Dialogue/DialogueController.cs")
    dialogue_interactable = read("Assets/ProjectTideline/Scripts/Interaction/DialogueInteractable.cs")
    confrontation_interactable = read("Assets/ProjectTideline/Scripts/Interaction/CinematicConfrontationInteractable.cs")
    race_start_interactable = read("Assets/ProjectTideline/Scripts/Interaction/RouteRaceStartInteractable.cs")
    route_race = read("Assets/ProjectTideline/Scripts/World/RouteRaceController.cs")
    route_checkpoint = read("Assets/ProjectTideline/Scripts/World/RouteRaceCheckpoint.cs")
    vehicle_seat = read("Assets/ProjectTideline/Scripts/Vehicles/VehicleSeatInteractable.cs")
    vehicle_drive = read("Assets/ProjectTideline/Scripts/Vehicles/VehicleDriveController.cs")
    traffic_driver = read("Assets/ProjectTideline/Scripts/Vehicles/TrafficVehicleDriver.cs")
    confrontation = read("Assets/ProjectTideline/Scripts/Interaction/CinematicConfrontationController.cs")
    attention = read("Assets/ProjectTideline/Scripts/World/PublicAttentionState.cs")
    proximity_interactor = read("Assets/ProjectTideline/Scripts/Interaction/ProximityInteractor.cs")
    interactable = read("Assets/ProjectTideline/Scripts/Interaction/IInteractable.cs")
    check("MissionBranchEvaluator.ConditionsMatch" in mission_director,
          "MissionDirector does not delegate branch decisions to the shared evaluator")
    check("InvokeDispatchCompletedSafely();" in mission_signal_bus
          and "while (PendingSignals.Count > 0)" in mission_signal_bus
          and "if (IsDispatching || PendingSignals.Count == 0)" in mission_signal_bus,
          "MissionSignalBus must drain nested signals through a queue before completing dispatch")
    check("public readonly struct Signal" in mission_signal_bus
          and "public static void PublishBatch(params Signal[] signals)" in mission_signal_bus
          and "DispatchPendingSignals();" in mission_signal_bus,
          "MissionSignalBus must support atomic batches of signals from one gameplay action")
    check("handlers.GetInvocationList()" in mission_signal_bus
          and "Debug.LogException(exception)" in mission_signal_bus,
          "MissionSignalBus must isolate subscriber exceptions and continue dispatching other listeners")
    check("MaxSignalsPerBatch = 4096" in mission_signal_bus
          and "PendingSignals.Clear();" in mission_signal_bus,
          "MissionSignalBus must cap runaway nested signal loops and clear the abandoned batch")
    check("internal static event Action<int> DispatchAborted;" in mission_signal_bus
          and "InvokeDispatchAbortedSafely(dispatched);" in mission_signal_bus,
          "MissionSignalBus must notify the mission director when a signal batch is aborted")
    check("private static void InvokeDispatchAbortedSafely(int dispatched)" in mission_signal_bus
          and "((Action<int>)subscriber).Invoke(dispatched);" in mission_signal_bus,
          "MissionSignalBus must isolate exceptions raised by abort notification listeners")
    check("MissionSignalBus.DispatchCompleted += OnSignalDispatchCompleted;" in mission_director
          and "MissionSignalBus.DispatchCompleted -= OnSignalDispatchCompleted;" in mission_director,
          "MissionDirector must subscribe and unsubscribe from signal-batch completion")
    check("MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;" in mission_director
          and "MissionSignalBus.DispatchAborted -= OnSignalDispatchAborted;" in mission_director,
          "MissionDirector must subscribe and unsubscribe from aborted signal batches")
    abort_handler = mission_director.split("private void OnSignalDispatchAborted(int dispatched)", 1)[-1].split(
        "private void AdvanceObjective()", 1)[0]
    check("_objectiveCompletionPending = false;" in abort_handler
          and "_currentCount = Mathf.Min(_currentCount, required - 1);" in abort_handler
          and "PublishProgress();" in abort_handler,
          "MissionDirector must roll back a pending completion after an aborted signal batch")
    check("_objectiveCompletionPending" in mission_director
          and "objective == null || _objectiveCompletionPending ||" in mission_director,
          "MissionDirector must prevent repeated signals from advancing a completed objective twice")
    signal_handler = mission_director.split("private void OnSignal(string signalId, int amount)", 1)[-1].split(
        "private void OnSignalDispatchCompleted()", 1)[0]
    check("_objectiveCompletionPending = true;" in signal_handler
          and "AdvanceObjective()" not in signal_handler,
          "MissionDirector must defer objective advancement until the signal queue drains")
    dispatch_complete_handler = mission_director.split("private void OnSignalDispatchCompleted()", 1)[-1].split(
        "private void AdvanceObjective()", 1)[0]
    check("AdvanceObjective();" in dispatch_complete_handler,
          "MissionDirector does not advance the objective after signal dispatch completes")
    check("NarrativeStateLedger.Instance" in mission_director and "HasFlag(flag)" in mission_director,
          "MissionDirector is missing persistent story-flag branch evaluation")
    check("usesRelationship == usesStoryFlag" in branch_evaluator,
          "Mission branch condition must require exactly one condition source")
    check("if (!actual.HasValue) return false;" in branch_evaluator,
          "Missing relationship/story state must not be treated as numeric zero")
    check("public bool HasValue(string characterId)" in relationship_ledger,
          "Relationship ledger cannot distinguish absent values from explicit zero")
    check("bool publishMissionSignal = true" in relationship_ledger
          and "if (publishMissionSignal)" in relationship_ledger,
          "Relationship ledger cannot suppress mission signals during an atomic dialogue choice")
    check("bool publishMissionSignal = true" in narrative_ledger
          and "if (publishMissionSignal)" in narrative_ledger,
          "Narrative ledger cannot suppress mission signals during an atomic dialogue choice")
    relationship_write = dialogue_controller.find("choice.relationshipCharacterId, choice.relationshipDelta, false);")
    flag_write = dialogue_controller.find("choice.storyFlagId, choice.storyFlagValue, false);")
    choice_signal = dialogue_controller.find("MissionSignalBus.Publish(choice.signalId, Mathf.Max(1, choice.signalAmount));")
    check(relationship_write >= 0 and flag_write >= 0 and choice_signal >= 0
          and relationship_write < choice_signal and flag_write < choice_signal,
          "Dialogue choice must persist relationship and story-flag results before publishing its mission signal")
    check("MissionSignalBus.PublishBatch(" in dialogue_controller
          and 'new MissionSignalBus.Signal(\n                                "relationship.changed." + choice.relationshipCharacterId, 1)' in dialogue_controller
          and 'new MissionSignalBus.Signal("story.flag.changed", 1)' in dialogue_controller,
          "Dialogue choices without an authored signal must batch both generic ledger outcomes atomically")
    check('new MissionSignalBus.Signal("vehicle.entered", 1)' in vehicle_seat
          and 'new MissionSignalBus.Signal("vehicle.taken", 1)' in vehicle_seat
          and 'MissionSignalBus.Publish("vehicle.exited", 1);' in vehicle_seat
          and 'MissionSignalBus.Publish("vehicle.taken", 1);' not in vehicle_seat,
          "Vehicle takeover must batch entered/taken outcomes and exit must remain a single signal")
    check('MissionSignalBus.Publish("vehicle.collision", 1);' in vehicle_drive
          and "collision.relativeVelocity.sqrMagnitude < 64f" in vehicle_drive
          and "_nextCollisionSignalAt = Time.time + 2.5f;" in vehicle_drive,
          "Vehicle collision signal must retain its speed threshold and cooldown")
    check("Distant NPC Wheel Visual LOD" in vehicle_drive
          and "_visualLodReference = Camera.main.transform" in vehicle_drive
          and "distanceSquared <= fullRateSquared" in vehicle_drive
          and "distanceSquared <= reducedRateSquared" in vehicle_drive
          and "if (!_isDriven && _visualLodReference != null)" in vehicle_drive
          and "_nextWheelVisualUpdateTime = Time.time + interval" in vehicle_drive,
          "Vehicle wheel visual poses must remain full-rate for the player and use distance-based update intervals for distant NPC vehicles")
    check("Distant Traffic Decision LOD" in traffic_driver
          and "_decisionLodReference = Camera.main.transform" in traffic_driver
          and "ShouldUpdateTrafficDecision()" in traffic_driver
          and "reducedDecisionInterval" in traffic_driver
          and "distantDecisionInterval" in traffic_driver
          and "if (_decisionLodReference == null) return true;" in traffic_driver,
          "Traffic AI decisions must be distance-throttled only when a camera reference exists, with full-rate fallback")
    traffic_decision = traffic_driver.split("private bool ShouldUpdateTrafficDecision()", 1)[-1].split(
        "private bool IsObstacleBlocking()", 1)[0]
    check("Time.fixedTime < _nextTrafficDecisionTime" in traffic_decision
          and "_nextTrafficDecisionTime = Time.fixedTime + interval" in traffic_decision,
          "Distant traffic decision LOD must schedule bounded fixed-time decision intervals")
    check("WheelCollider physics at the normal fixed timestep" in traffic_driver,
          "Traffic decision LOD must explicitly preserve full-rate vehicle physics")
    check("obstacleCheckInterval = 0.12f" in traffic_driver
          and "private bool IsObstacleBlocking()" in traffic_driver
          and "Time.fixedTime < _nextObstacleCheckTime" in traffic_driver
          and "_cachedObstacleBlocked" in traffic_driver
          and "_obstacleCheckPhase" in traffic_driver
          and "Physics.SphereCastNonAlloc(" in traffic_driver,
          "Traffic obstacle sensing must reuse cached results between staggered, non-allocating physics samples")
    traffic_fixed_update = traffic_driver.split("private void FixedUpdate()", 1)[-1].split(
        "private bool IsObstacleBlocking()", 1)[0]
    check("IsObstacleBlocking()" in traffic_fixed_update
          and "Physics.SphereCastNonAlloc(" not in traffic_fixed_update,
          "Traffic steering must not issue an obstacle physics query on every FixedUpdate tick")
    for signal in ("confrontation.resolved", "confrontation.withdrawn", "confrontation.failed"):
        check(f'MissionSignalBus.Publish("{signal}", 1);' in confrontation,
              f"Confrontation producer is missing {signal}")
    check('"world.attention.band." + nextBand.ToString().ToLowerInvariant()' in attention,
          "Public-attention producer must publish only configured band transitions")
    check("SignalId" not in interactable and "interaction.generic" not in interactable
          and "SignalId" not in proximity_interactor,
          "Unused generic interaction signal metadata must not remain in the interaction interface")
    check("public sealed class DialogueInteractable" in dialogue_interactable
          and "dialogueController.Begin(dialogueGraph) && oneShot" in dialogue_interactable
          and "dialogueController == null || dialogueGraph == null" in dialogue_interactable,
          "DialogueInteractable must route a configured graph into the dialogue controller")
    check("public bool Begin(DialogueGraph graph)" in dialogue_controller
          and "graph == null || IsOpen" in dialogue_controller
          and "graph.FindNode(graph.startNodeId)" in dialogue_controller,
          "DialogueController must reject missing, malformed-start or overlapping dialogue graphs")
    check("HashSet<object> _gameplayBlockOwners" in input_router
          and "AcquireGameplayBlock(object owner)" in input_router
          and "ReleaseGameplayBlock(object owner)" in input_router
          and "GameplayBlocked => _legacyGameplayBlocked || _gameplayBlockOwners.Count > 0" in input_router,
          "PlayerInputRouter must track independently owned gameplay input blocks")
    check("GetDistanceUpdateInterval()" in character_animation_driver
          and "lodReference" in character_animation_driver
          and "fullRateDistance" in character_animation_driver
          and "reducedRateDistance" in character_animation_driver
          and "animationDeltaTime" in character_animation_driver,
          "CharacterAnimationDriver must reduce distant NPC parameter-update frequency while preserving near-player full-rate animation")
    check("List<MonoBehaviour> _candidateBuffer" in proximity_interactor
          and "GetComponentsInParent<MonoBehaviour>(true, _candidateBuffer)" in proximity_interactor
          and "MonoBehaviour[] candidates" not in proximity_interactor,
          "ProximityInteractor must reuse a component buffer instead of allocating a candidate array during interaction scans")
    check("_inputBlockRouter = PlayerInputRouter.Instance;" in dialogue_controller
          and "_inputBlockRouter.AcquireGameplayBlock(this);" in dialogue_controller
          and "_inputBlockRouter.ReleaseGameplayBlock(this);" in dialogue_controller
          and "SetGameplayBlocked(false)" not in dialogue_controller,
          "DialogueController must release only its own gameplay input block")
    check("_focused.Interact(actorRoot.gameObject);" in proximity_interactor,
          "ProximityInteractor must dispatch focused dialogue interactions through IInteractable")
    check("MissionSignalBus.Publish(choice.signalId, Mathf.Max(1, choice.signalAmount));" in dialogue_controller,
          "DialogueController must publish authored choice signals")
    for task_id in ("steering_check", "door_latch_check", "engine_rattle_check"):
        check(f'"{task_id}"' in mara_workshop,
              f"Mara workshop order is missing required task {task_id}")
    check("HashSet<string>(StringComparer.Ordinal)" in mara_workshop
          and "_completedTaskIds.Add(taskId)" in mara_workshop,
          "Mara workshop tasks must be unique and cannot be counted twice")
    check('HasFlag("mara_work_order_accepted")' in mara_workshop
          and 'GetFlag("mara_work_order_accepted") == 1' in mara_workshop
          and "|| !IsUnlocked || IsComplete" in mara_workshop,
          "Mara workshop inspections must require Juno's persistent work-order acceptance flag")
    check("!orderController.IsUnlocked" in mara_workshop_interactable
          and "Talk to Juno first" in mara_workshop_interactable,
          "Mara workshop stations must communicate the Juno dialogue prerequisite")
    check('new MissionSignalBus.Signal("mara.workshop.task.completed", 1)' in mara_workshop
          and 'new MissionSignalBus.Signal("mara.workshop.order.completed", 1)' in mara_workshop
          and "MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;" in mara_workshop,
          "Mara workshop final task must batch completion and roll back on signal-batch abort")
    check("orderController.TryCompleteTask(taskId, actor);" in mara_workshop_interactable,
          "Mara workshop inspection stations must route through their shared order controller")
    check('MissionSignalBus.Publish("mara.delivery.finished", 1);' in mara_handoff_controller
          and "deliveryRoute != null && deliveryRoute.HasFinished" in mara_handoff_controller
          and "deliveryBay.HasArrived" in mara_handoff_controller
          and "_handoffPending" in mara_handoff_controller,
          "Mara customer handoff must require a finished route, a stopped delivery-bay arrival and publish the mission-specific delivery signal")
    check("MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;" in mara_handoff_controller
          and "_handedOff = false;" in mara_handoff_controller
          and "MissionProgressHasPassedHandoff" in mara_handoff_controller,
          "Mara handoff must roll back on an aborted batch and restore from saved mission objective progress")
    check("handoffController.TryCompleteHandoff(actor);" in mara_customer_handoff,
          "Mara customer handoff interactable must call the one-time handoff controller")
    check("public event Action Finished;" in route_race
          and "MissionSignalBus.DispatchCompleted += OnSignalDispatchCompleted;" in route_race
          and "MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;" in route_race
          and "NotifyFinished();" in route_race,
          "Route completion relay must fire only after signal dispatch completes and recover aborted finishes")
    check("vehicle != playerVehicle" in route_race
          and "playerVehicle == null" in route_race,
          "Route race must be bound to one designated player vehicle")
    check("workshopOrder == null || !workshopOrder.IsComplete" in mara_delivery_start
          and "deliveryRoute.HasFinished" in mara_delivery_start
          and "deliveryRoute.BeginRace();" in mara_delivery_start,
          "Mara delivery start must be blocked until all workshop tasks are complete")
    check("|| deliveryRoute.HasFinished" in mara_delivery_start
          and "|| deliveryRoute.IsActive" in mara_delivery_start,
          "Mara delivery route must not start twice or while already active")
    check("handoffController == null || !handoffController.HasHandedOff" in mara_nico_interactable
          and "dialogueController.Begin(dialogueGraph);" in mara_nico_interactable,
          "Nico's follow-up dialogue must remain locked until customer handoff is complete or restored")
    check("public bool HasFinished { get; private set; }" in route_race
          and "if (_isActive || HasFinished || !HasValidRoute())" in route_race,
          "Route completion state must gate follow-up dialogue and prevent route replay")
    check("MissionProgressHasPassedWorkOrder" in mara_workshop
          and "missionDirector.CurrentObjectiveIndex >= 1" in mara_workshop,
          "Mara workshop tasks must remain completed after restoring a save at the delivery objective")
    check("[MenuItem(\"Project Tideline/13 — Audit Mara Prologue Scene Wiring\"" in mara_scene_audit
          and "Resources.FindObjectsOfTypeAll<T>()" in mara_scene_audit
          and "gameObject.scene.isLoaded" in mara_scene_audit,
          "Mara scene audit must inspect actual components in loaded Unity scenes, not prefab assets")
    for scene_contract in (
        "ValidateDialogueView(dialogue, issues)",
        "ValidateDialogueCapacity(dialogue, introGraph, nicoGraph, issues)",
        "MaxChoiceCount(introGraph)",
        "MaxChoiceCount(nicoGraph)",
        "choiceButtons and choiceLabels",
        "MaraWorkshopTaskInteractable",
        "VehicleSeatInteractable",
        "RouteRaceCheckpoint",
        "MaraCustomerHandoffInteractable",
        "MaraPostDeliveryDialogueInteractable",
        "MissionProgressHasPassedHandoff",
        "HasSolidInteractionCollider",
        "actorRoot.GetComponent<CharacterController>()",
        "actorRoot.GetComponent<ThirdPersonMotor>()",
        "isActiveAndEnabled",
        'FindProperty("autoStart")',
    ):
        check(scene_contract in mara_scene_audit or scene_contract in mara_workshop + mara_handoff_controller,
              f"Mara scene wiring audit is missing required validation: {scene_contract}")
    for graph_id in (
        "prologue.mara.two_minutes_to_close",
        "prologue.mara.nico_challenge",
        "prologue.rafi.last_song_first_lie",
        "prologue.elias.the_old_terms",
    ):
        check(graph_id in prologue_graph_docs, f"Prologue graph documentation is missing {graph_id}")
    check("four `DialogueGraph` assets" in prologue_graph_docs
          and "The four assets are intentionally not three" in prologue_graph_docs,
          "Prologue graph documentation must explain four assets across three leads")
    check("mara_work_order_accepted = 1" in prologue_graph_docs
          and "Talk to Juno first" in mara_workshop_interactable
          and 'HasFlag("mara_work_order_accepted")' in mara_workshop,
          "Graph documentation and workshop interaction must enforce the persistent Juno acceptance gate")
    blockout_builder = read("Assets/Editor/ProjectTideline/PortSableBlockoutSceneBuilder.cs")
    for token in (
        'PortSable_OldQuay_Blockout.unity',
        'Build Playable Old Quay Blockout',
        'AddComponent<PlayerInputRouter>()',
        'AddComponent<CharacterController>()',
        'AddComponent<ThirdPersonMotor>()',
        'AddComponent<ThirdPersonCameraRig>()',
        'SetTarget(player.transform',
        'SetMovementCamera(cameraObject.transform)',
        'EditorBuildSettings.scenes = scenes.ToArray()',
        'WASD / arrows or touch joystick to move',
        'This is still a primitive greybox',
    ):
        check(token in blockout_builder,
              f"Playable Old Quay blockout builder is missing: {token}")
    check('NewSceneMode.Single' in blockout_builder
          and 'EditorSceneManager.SaveScene(scene, ScenePath)' in blockout_builder,
          "Blockout builder must create and save a real Unity scene")
    check('DestroyImmediate(body.GetComponent<Collider>())' in blockout_builder,
          "Player visual placeholder must not add a duplicate collider to CharacterController")

    mobile_controls = read("Assets/ProjectTideline/Scripts/Input/MobileTouchControl.cs")
    for token in (
        "IPointerDownHandler",
        "IDragHandler",
        "IPointerUpHandler",
        "MobileTouchControlMode.MoveStick",
        "MobileTouchControlMode.LookPad",
        "MobileTouchControlMode.SprintButton",
        "MobileTouchControlMode.JumpButton",
        "MobileTouchControlMode.InteractButton",
        "PlayerInputRouter.Instance.SetMove(direction)",
        "PlayerInputRouter.Instance.AddLookDelta",
        "PlayerInputRouter.Instance.SetSprint(false)",
    ):
        check(token in mobile_controls,
              f"Mobile touch control implementation is missing: {token}")
    for token in (
        "CanvasScaler",
        "GraphicRaycaster",
        "InputSystemUIInputModule",
        "AssignDefaultActions()",
        "BuildMobileControls()",
        '"MOVE — virtual joystick"',
        '"LOOK — drag to orbit camera"',
        '"INTERACT"',
    ):
        check(token in blockout_builder,
              f"Generated scene is missing mobile HUD wiring: {token}")

    mission_dialogue_sequence = read("Assets/ProjectTideline/Scripts/Interaction/MissionDialogueSequenceInteractable.cs")
    for token in (
        "public sealed class MissionDialogueSequenceInteractable",
        "MissionDefinition mission = missionDirector.CurrentMission",
        "MissionObjective objective = missionDirector.CurrentObjective",
        "string.Equals(step.objectiveId, objective.objectiveId",
        "dialogueController.Begin(step.graph)",
    ):
        check(token in mission_dialogue_sequence,
              f"Objective-aware NPC dialogue sequence is missing: {token}")
    for token in (
        "old_quay.juno_debrief",
        "old_quay.juno_debrief_completed",
        "return_to_juno",
        "TutorialDebriefDialoguePath",
        "MissionDialogueSequenceInteractable",
        "Report back to Juno",
    ):
        check(token in blockout_builder,
              f"Opening mission debrief sequence is missing: {token}")

    pedestrian_walker = read("Assets/ProjectTideline/Scripts/World/AmbientPedestrianWalker.cs")
    for token in (
        "public sealed class AmbientPedestrianWalker",
        "public void Configure(Transform[] waypoints",
        "walkingSpeed * Time.deltaTime",
        "Quaternion.RotateTowards",
        "_targetIndex = (_targetIndex + 1) % route.Length",
    ):
        check(token in pedestrian_walker,
              f"Ambient pedestrian waypoint movement is missing: {token}")
    for token in (
        "BuildAmbientPedestrian(",
        "DOCK WORKER — east sidewalk",
        "LOCAL RESIDENT — west sidewalk",
        "AmbientPedestrianWalker",
        "new[] { -29f, -5f, 20f, 49f }",
        "new[] { 49f, 20f, -5f, -29f }",
    ):
        check(token in blockout_builder,
              f"Generated Old Quay pedestrian population is missing: {token}")
    check("SetVehicleControlMode(true)" in vehicle_seat
          and "SetVehicleControlMode(false)" in vehicle_seat,
          "Vehicle entry/exit must switch the shared input mapping")
    check("VehicleControlMode) PlayerInputRouter.Instance.SetThrottle(1f)" in mobile_controls
          and "VehicleControlMode) PlayerInputRouter.Instance.SetHandbrake(true)" in mobile_controls,
          "Touch accelerator and handbrake must map to vehicle controls when seated")
    check("_npcDriving ? _npcBrake : 1f" in vehicle_drive
          and "public void SetDriven(bool value)" in vehicle_drive,
          "Unoccupied player vehicles must hold the parking brake")

    delivery_checkpoint = read("Assets/ProjectTideline/Scripts/Vehicles/VehicleDeliveryCheckpoint.cs")
    customer_handoff = read("Assets/ProjectTideline/Scripts/Interaction/MaraCustomerHandoffInteractable.cs")
    handoff_controller = read("Assets/ProjectTideline/Scripts/World/MaraDeliveryHandoffController.cs")
    route_start_interactable = read("Assets/ProjectTideline/Scripts/Interaction/RouteRaceStartInteractable.cs")
    route_controller = read("Assets/ProjectTideline/Scripts/World/RouteRaceController.cs")
    route_checkpoint = read("Assets/ProjectTideline/Scripts/World/RouteRaceCheckpoint.cs")
    for token in (
        'DeliveryArrivedSignal = "old_quay.delivery_arrived"',
        "OnTriggerStay(Collider other)",
        "candidateBody != deliveryVehicle.GetComponent<Rigidbody>()",
        "!driverSeat.IsOccupied",
        "deliveryVehicle.SpeedKph > maximumHandoffSpeedKph",
        "requiredRoute != null && !requiredRoute.HasFinished",
        'MissionSignalBus.Publish("old_quay.delivery_arrived", 1)',
    ):
        check(token in delivery_checkpoint,
              f"Delivery bay checkpoint is missing: {token}")
    for token in (
        "CanCompleteHandoff",
        "deliveryRoute.HasFinished",
        "deliveryBay.HasArrived",
        'MissionSignalBus.Publish("mara.delivery.finished", 1)',
    ):
        check(token in handoff_controller,
              f"Customer handoff controller is missing: {token}")
    for token in (
        "handoffController.CanCompleteHandoff",
        "handoffController.TryCompleteHandoff(actor)",
        "Hand over the vehicle",
    ):
        check(token in customer_handoff,
              f"Customer handoff interaction is missing: {token}")
    for token in (
        "public bool IsOccupied => _occupant != null",
        "SetVehicleControlMode(true)",
        "SetVehicleControlMode(false)",
        "cameraRig.SetTarget(actor.transform",
    ):
        check(token in vehicle_seat,
              f"Vehicle seat/input/camera integration is missing: {token}")
    for token in (
        "WheelCollider",
        "wheelCollider.radius = 0.36f",
        "CUSTOMER DELIVERY CAR",
        "VehicleDriveController driveController = car.AddComponent<VehicleDriveController>()",
        "new Vector3(-0.84f, 0.40f, 1.18f)",
        "new Vector3(0.84f, 0.40f, -1.18f)",
        "VehicleSeatInteractable",
        "BuildCustomerDeliveryRoute(deliveryVehicle)",
        "BuildCustomerDeliveryBay(deliveryVehicle, deliverySeat, deliveryRoute)",
        "bay.Configure(deliveryVehicle, deliverySeat, deliveryRoute)",
        "handoff.Configure(deliveryRoute, missionDirector, deliveryBay)",
    ):
        check(token in blockout_builder,
              f"Generated drivable delivery sequence is missing: {token}")
    for token in (
        "race.long_loop.started",
        "vehicle.entered",
        "race.long_loop.finished",
        "old_quay.delivery_arrived",
        "mara.delivery.finished",
        "old_quay.juno_debrief_completed",
        "start_customer_delivery",
        "enter_delivery_vehicle",
        "drive_customer_delivery_route",
        "park_in_delivery_bay",
        "hand_over_customer_vehicle",
        "return_to_juno",
        "checkpointIndex",
        "TryRegisterCheckpoint",
    ):
        check(token in blockout_builder + route_controller + route_checkpoint,
              f"Generated ordered delivery mission is missing: {token}")
    for token in (
        "missionDirector.CurrentObjective",
        "requiredObjectiveId",
        "raceController.BeginRace()",
        "Complete the previous objective first",
    ):
        check(token in route_start_interactable,
              f"Delivery route start objective gate is missing: {token}")

    mission_signal_interactable = read("Assets/ProjectTideline/Scripts/Interaction/MissionSignalInteractable.cs")
    mission_status_display = read("Assets/ProjectTideline/Scripts/UI/MissionStatusDisplay.cs")
    mission_dialogue_sequence = read("Assets/ProjectTideline/Scripts/Interaction/MissionDialogueSequenceInteractable.cs")
    for token in (
        'HarbourLedgerSignal = "old_quay.harbour_ledger_inspected"',
        "requiredMissionId",
        "requiredObjectiveId",
        "!IsCurrentObjective()",
        'MissionSignalBus.Publish("old_quay.harbour_ledger_inspected", 1)',
    ):
        check(token in mission_signal_interactable,
              f"Tutorial mission signal interaction is missing: {token}")
    input_router = read("Assets/ProjectTideline/Scripts/Input/PlayerInputRouter.cs")
    check("VehicleControlModeChanged" in input_router
          and "VehicleControlModeChanged?.Invoke(value)" in input_router,
          "Vehicle entry/exit must notify the mobile HUD of control-mode changes")
    check("contextualLabel.text = inVehicle ? \"ACCEL\" : \"SPRINT\"" in mobile_controls
          and "contextualLabel.text = inVehicle ? \"BRAKE\" : \"JUMP\"" in mobile_controls
          and "VehicleControlModeChanged += OnVehicleControlModeChanged" in mobile_controls,
          "Touch pedal labels must show accelerator and brake functions while driving")
    check('binding.FindPropertyRelative("driven").boolValue = i >= 2;' in blockout_builder
          and 'binding.FindPropertyRelative("steer").boolValue = i < 2;' in blockout_builder,
          "Generated delivery vehicle must have front steering and rear-wheel drive")

    for token in ("ProgressChanged", "MissionCompleted", "objective.description", "mission.title"):
        check(token in mission_status_display,
              f"Mission objective HUD is missing event-driven display behavior: {token}")
    for token in (
        "old_quay.juno_first_contact",
        "old_quay.juno_debrief",
        "old_quay.juno_briefing_accepted",
        "old_quay.juno_debrief_completed",
        "old_quay_first_steps",
        "meet_juno",
        "inspect_harbour_ledger",
        "start_customer_delivery",
        "enter_delivery_vehicle",
        "drive_customer_delivery_route",
        "park_in_delivery_bay",
        "hand_over_customer_vehicle",
        "return_to_juno",
        "BuildTutorialGameplay(",
        "MissionSignalInteractable.HarbourLedgerSignal",
        "RouteRaceStartInteractable",
        "BuildCustomerDeliveryVehicle",
        "BuildCustomerDeliveryRoute",
        "BuildCustomerDeliveryBay",
        "BuildCustomerHandoffActor",
        "MissionDialogueSequenceInteractable",
        "MissionStatusDisplay",
        "DialogueController",
        "ProximityInteractor",
        "BuildAmbientPedestrian(",
    ):
        check(token in blockout_builder,
              f"Playable Old Quay delivery mission wiring is missing: {token}")
    for token in (
        "public sealed class MissionDialogueSequenceInteractable",
        "MissionDefinition mission = missionDirector.CurrentMission",
        "MissionObjective objective = missionDirector.CurrentObjective",
        "string.Equals(step.objectiveId, objective.objectiveId",
        "dialogueController.Begin(step.graph)",
    ):
        check(token in mission_dialogue_sequence,
              f"Objective-aware NPC dialogue sequence is missing: {token}")
    check('routeStart.Configure(deliveryRoute, missionDirector, "old_quay_first_steps"' in blockout_builder
          and '"start_customer_delivery"' in blockout_builder,
          "Delivery route must only start during its assigned active mission objective")
    check('junoInteractable.Configure(missionDirector, dialogueController, "old_quay_first_steps"' in blockout_builder
          and 'objectiveId = "meet_juno"' in blockout_builder
          and 'objectiveId = "return_to_juno"' in blockout_builder,
          "Juno opening/debrief dialogue must be selected by the current mission objective")
    check("if (isActiveAndEnabled) Subscribe()" in mission_status_display
          and "private void Unsubscribe()" in mission_status_display,
          "Mission HUD must bind safely whether configured before or after OnEnable")
    check("missionDirector.CurrentObjective" in mission_signal_interactable
          and "string.Equals(objective.objectiveId, requiredObjectiveId" in mission_signal_interactable,
          "Tutorial interactions must refuse out-of-order objective signals")
    check('FindProperty("closeButton")' not in blockout_builder,
          "Tutorial dialogue must not expose an unfinished close path that can strand the objective")

    readme = read("README.md")
    check("MaraDeliveryRouteCompletionRelay" not in readme
          and "MaraDeliveryHandoffController" in readme,
          "README must describe the real Mara handoff producer rather than a nonexistent route relay")
    for token in ("complete_customer_work_order", "mara.workshop.order.completed",
                  "deliver_customer_vehicle", "mara.delivery.finished",
                  "resolve_nico_challenge", "prologue.mara.complete"):
        check(token in mara_mission_builder,
              f"Mara mission builder is missing objective contract token: {token}")
    check("public sealed class CinematicConfrontationInteractable" in confrontation_interactable
          and "confrontationController.Begin(actor, opponentAnimator);" in confrontation_interactable
          and "opponentAnimator == null" in confrontation_interactable,
          "Cinematic confrontation must have an explicit proximity-interaction entry component")
    check("public bool Begin(GameObject actor, Animator opponentAnimator)" in confrontation,
          "CinematicConfrontationController must expose a guarded begin operation")
    check("public sealed class RouteRaceStartInteractable" in race_start_interactable
          and "raceController.BeginRace();" in race_start_interactable,
          "Route race must be startable through the normal proximity-interaction system")
    for suffix in ("started", "checkpoint", "finished", "cancelled"):
        check(f'signalPrefix + ".{suffix}"' in route_race,
              f"Route race controller must publish its {suffix} signal")
    check("checkpoint.CheckpointIndex != _nextCheckpointIndex" in route_race
          and "checkpoints[_nextCheckpointIndex] != checkpoint" in route_race
          and "!vehicle.IsDriven" in route_race,
          "Route race must reject skipped/out-of-order checkpoints and non-player-driven vehicles")
    check("checkpoint.Controller != this" in route_race
          and "checkpoint.CheckpointIndex != i" in route_race
          and "!checkpoint.HasTriggerCollider" in route_race,
          "Route race start must validate every assigned checkpoint and trigger collider")
    final_checkpoint_batch = route_race.split("if (_nextCheckpointIndex == checkpoints.Length)", 1)[-1].split(
        "return true;", 1)[0]
    check("MissionSignalBus.PublishBatch(" in final_checkpoint_batch
          and 'signalPrefix + ".checkpoint"' in final_checkpoint_batch
          and 'signalPrefix + ".finished"' in final_checkpoint_batch,
          "Final checkpoint and race-finished signals must be dispatched in one interaction batch")
    check("other.GetComponentInParent<VehicleDriveController>()" in route_checkpoint
          and "controller.TryRegisterCheckpoint(this, vehicle);" in route_checkpoint
          and "trigger != null && trigger.isTrigger" in route_checkpoint,
          "Route checkpoint triggers must report only vehicle entry to their owning route controller")
    check("Mathf.Max(1, objective.requiredCount) - 1" in mission_director,
          "Mission progress restore must clamp an active objective below its completion threshold")
    check("candidate.objectives == null || candidate.objectives.Length == 0" in mission_director,
          "MissionDirector can still auto-complete an empty placeholder mission")
    check("Refusing to auto-complete it." in mission_director,
          "Empty mission placeholder rejection is missing")
    check("HasValidObjectives(candidate)" in mission_director
          and "HasValidObjectives(mission)" in mission_director,
          "MissionDirector must validate objective data on both start and restore")
    objective_guard = mission_director.split("private bool HasValidObjectives", 1)[-1].split(
        "private void OnSignal(string signalId, int amount)", 1)[0]
    for token in ("objective == null", "objective.objectiveId", "objective.description",
                  "objective.completionSignal", "objective.requiredCount < 1",
                  "objectiveIds.Add(objective.objectiveId)"):
        check(token in objective_guard,
              f"MissionDirector runtime objective guard is missing: {token}")

    mission_audit = read("Assets/Editor/ProjectTideline/MissionAssetAudit.cs")
    for token in (
        'AssetDatabase.FindAssets("t:MissionDefinition")',
        "completionBranches",
        "fallbackNextMissionId",
        "endsCampaign",
        "storyFlagsUsedByMissions",
        "RunEvaluatorSelfTests()",
        "PORT SABLE is not playable yet",
    ):
        check(token in mission_audit, f"Mission asset audit is missing required check: {token}")
    check("MissionBranchEvaluator.ConditionsMatch" in mission_audit,
          "Editor audit is not exercising the production mission-branch evaluator")
    check("RunSignalBusSelfTests()" in mission_audit
          and "MissionSignalBus.PublishBatch(" in mission_audit
          and "observed.SequenceEqual" in mission_audit,
          "Editor audit must include FIFO and nested-signal batch regression cases")
    check("RunSignalProducerCatalogSelfTests()" in mission_audit
          and 'catalog.TryResolve("relationship.changed.mina_sato"'
          and 'catalog.TryResolve("world.attention.band.noticed"'
          and 'catalog.TryResolve("race.long_loop.finished"',
          "Editor audit must include regression cases for exact and dynamic signal producer matching")
    check("evaluatorSelfTests.Any" in mission_audit
          and "signalBusSelfTests.Any" in mission_audit
          and "producerCatalogSelfTests.Any" in mission_audit,
          "Mission audit status must fail when any Editor regression self-test fails")
    check("dispatchCompletedCount == 1" in mission_audit
          and "if (EditorApplication.isPlaying)" in mission_audit,
          "Signal bus Editor tests must verify one completion callback and skip active Play Mode")
    check("CollectSignalProducerCatalog()" in mission_audit
          and "signalProducers.TryResolve(objective.completionSignal" in mission_audit,
          "Mission asset audit must resolve every objective completion signal against a producer catalog")
    check("MissionSignalBus\\.Publish" in mission_audit
          and "MissionSignalBus\\.Signal" in mission_audit
          and "HasRelationshipPattern" in mission_audit
          and "HasAttentionBandPattern" in mission_audit
          and "RouteRacePrefix" in mission_audit,
          "Mission signal catalog must recognize literal, batched and documented dynamic producer patterns")
    check('AssetDatabase.FindAssets("t:DialogueGraph")' in mission_audit
          and "choice.signalId" in mission_audit
          and "source/data presence does not prove that a producer component is wired into a playable scene." in mission_audit,
          "Signal catalog must include generated dialogue choices and disclose the scene-wiring limitation")
    check("unconditional branch" in mission_audit,
          "Mission asset audit does not detect catch-all branches shadowing later branches")

    save = read("Assets/ProjectTideline/Scripts/Core/SaveSnapshot.cs")
    session = read("Assets/ProjectTideline/Scripts/Core/SessionSaveCoordinator.cs")
    for schema, key, field in (
        (3, "project_tideline.save.v3", "SaveKeyV3"),
        (2, "project_tideline.save.v2", "LegacySaveKeyV2"),
        (1, "project_tideline.save.v1", "LegacySaveKeyV1"),
    ):
        check(key in save, f"Save key for schema {schema} is missing")
        check(f"TryReadKey({field}, {schema}, out snapshot)" in save,
              f"Save reader does not map {field} to schema {schema}")
    check("snapshot.schemaVersion = 3" in save, "Save writer does not force schema v3")
    check("objectiveCount = missionDirector != null ? missionDirector.CurrentObjectiveCount : 0" in session,
          "Session save must persist partial objective counts")
    check("schemaVersion = 3" in session, "Session coordinator does not create v3 snapshots")
    check("CaptureLeadTransforms()" in session and "RestoreLeadTransforms(" in session,
          "Session save does not capture and restore all playable lead transforms")
    check("narrativeStateData" in session, "Session save does not persist narrative state")
    check("!leadSwitcher.CanPersistActiveLead" in session,
          "Vehicle-safe save guard is missing")

    readme = read("README.md")
    check("does **not** yet contain the complete recovered Unity source tree" in readme,
          "README must clearly disclose the selected-snapshot limitation")
    check("does **not** compile C#" in readme,
          "README must not imply the static check is a Unity compile")

    cs_files = sorted((ROOT / "Assets").rglob("*.cs"))
    check(len(cs_files) >= 20, "Too few C# files are present in the source snapshot")
    for file_path in cs_files:
        source = file_path.read_text(encoding="utf-8")
        balanced_csharp(file_path.relative_to(ROOT), source)

    if ERRORS:
        print(f"PORT SABLE STATIC AUDIT: FAIL ({len(ERRORS)} issues, {CHECKS} checks)")
        for error in ERRORS:
            print(f" - {error}")
        return 1

    print(f"PORT SABLE STATIC AUDIT: PASS ({CHECKS} checks, {len(cs_files)} C# files scanned)")
    print("This is a repository consistency check only; Unity compilation, Play Mode, APK and device tests remain separate.")
    return 0


if __name__ == "__main__":
    sys.exit(run())
