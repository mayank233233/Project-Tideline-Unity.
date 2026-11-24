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
    for graph_id in (
        "prologue.mara.two_minutes_to_close",
        "prologue.rafi.last_song_first_lie",
        "prologue.elias.the_old_terms",
    ):
        check(graph_id in builder, f"Missing generated prologue graph ID: {graph_id}")
        check(graph_id in dialogue_audit, f"Dialogue audit does not expect graph ID: {graph_id}")

    required_flags = {
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

    mission_director = read("Assets/ProjectTideline/Scripts/Missions/MissionDirector.cs")
    mission_signal_bus = read("Assets/ProjectTideline/Scripts/Missions/MissionSignalBus.cs")
    branch_evaluator = read("Assets/ProjectTideline/Scripts/Missions/MissionBranchEvaluator.cs")
    relationship_ledger = read("Assets/ProjectTideline/Scripts/NPC/NPCRelationshipLedger.cs")
    narrative_ledger = read("Assets/ProjectTideline/Scripts/Core/NarrativeStateLedger.cs")
    dialogue_controller = read("Assets/ProjectTideline/Scripts/Dialogue/DialogueController.cs")
    dialogue_interactable = read("Assets/ProjectTideline/Scripts/Interaction/DialogueInteractable.cs")
    vehicle_seat = read("Assets/ProjectTideline/Scripts/Vehicles/VehicleSeatInteractable.cs")
    vehicle_drive = read("Assets/ProjectTideline/Scripts/Vehicles/VehicleDriveController.cs")
    confrontation = read("Assets/ProjectTideline/Scripts/Interaction/CinematicConfrontationController.cs")
    attention = read("Assets/ProjectTideline/Scripts/World/PublicAttentionState.cs")
    proximity_interactor = read("Assets/ProjectTideline/Scripts/Interaction/ProximityInteractor.cs")
    interactable = read("Assets/ProjectTideline/Scripts/Interaction/IInteractable.cs")
    check("MissionBranchEvaluator.ConditionsMatch" in mission_director,
          "MissionDirector does not delegate branch decisions to the shared evaluator")
    check("InvokeDispatchCompletedSafely();" in mission_signal_bus
          and "while (PendingSignals.Count > 0)" in mission_signal_bus
          and "if (IsDispatching)" in mission_signal_bus,
          "MissionSignalBus must drain nested signals through a queue before completing dispatch")
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
    check('MissionSignalBus.Publish("relationship.changed." + choice.relationshipCharacterId, 1);' in dialogue_controller
          and 'MissionSignalBus.Publish("story.flag.changed", 1);' in dialogue_controller,
          "Dialogue choices without an explicit signal must retain generic ledger mission signals")
    check('MissionSignalBus.Publish("vehicle.taken", 1);' in vehicle_seat
          and 'MissionSignalBus.Publish("vehicle.entered", 1);' in vehicle_seat
          and 'MissionSignalBus.Publish("vehicle.exited", 1);' in vehicle_seat,
          "Vehicle seat producer must retain its three documented transition signals")
    check('MissionSignalBus.Publish("vehicle.collision", 1);' in vehicle_drive
          and "collision.relativeVelocity.sqrMagnitude < 64f" in vehicle_drive
          and "_nextCollisionSignalAt = Time.time + 2.5f;" in vehicle_drive,
          "Vehicle collision signal must retain its speed threshold and cooldown")
    for signal in ("confrontation.resolved", "confrontation.withdrawn", "confrontation.failed"):
        check(f'MissionSignalBus.Publish("{signal}", 1);' in confrontation,
              f"Confrontation producer is missing {signal}")
    check('"world.attention.band." + nextBand.ToString().ToLowerInvariant()' in attention,
          "Public-attention producer must publish only configured band transitions")
    check("SignalId" not in interactable and "interaction.generic" not in interactable
          and "SignalId" not in proximity_interactor,
          "Unused generic interaction signal metadata must not remain in the interaction interface")
    check("public sealed class DialogueInteractable" in dialogue_interactable
          and "dialogueController.Begin(dialogueGraph);" in dialogue_interactable
          and "dialogueController == null || dialogueGraph == null" in dialogue_interactable,
          "DialogueInteractable must route a configured graph into the dialogue controller")
    check("public bool Begin(DialogueGraph graph)" in dialogue_controller
          and "graph == null || IsOpen" in dialogue_controller
          and "graph.FindNode(graph.startNodeId)" in dialogue_controller,
          "DialogueController must reject missing, malformed-start or overlapping dialogue graphs")
    check("_focused.Interact(actorRoot.gameObject);" in proximity_interactor,
          "ProximityInteractor must dispatch focused dialogue interactions through IInteractable")
    check("MissionSignalBus.Publish(choice.signalId, Mathf.Max(1, choice.signalAmount));" in dialogue_controller,
          "DialogueController must publish authored choice signals")
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
