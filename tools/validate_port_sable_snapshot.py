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
        "mara_delivery_choice": (1, 2),
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

    mission_director = read("Assets/ProjectTideline/Scripts/Missions/MissionDirector.cs")
    check("NarrativeStateLedger.Instance" in mission_director and "HasFlag(condition.storyFlagId)" in mission_director,
          "MissionDirector is missing persistent story-flag branch evaluation")
    check("usesRelationship == usesStoryFlag" in mission_director,
          "Mission branch condition must require exactly one condition source")

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
