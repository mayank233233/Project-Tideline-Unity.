#!/usr/bin/env python3
"""Audit the recovered Old Quay mission asset creation/configuration contract.

Run from the full Project Tideline Unity project root. This catches filename drift
between EnsureMissionAssets() and ConfigureMissionDirector(); it does not run Unity.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BUILDER = ROOT / "Assets/Editor/ProjectTideline/OldQuayVerticalSliceBuilder.cs"


def main() -> int:
    if not BUILDER.is_file():
        print("SKIP: full recovered project source is not present; OldQuayVerticalSliceBuilder.cs is missing.")
        return 0

    source = BUILDER.read_text(encoding="utf-8")
    created = re.findall(
        r'CreateMission\(\s*"([^"]+)"\s*,\s*"([^"]+)"', source, re.S
    )
    names_match = re.search(
        r"string\[\]\s+names\s*=\s*\{(.*?)\};", source, re.S
    )
    if not names_match:
        print("FAIL: could not locate ConfigureMissionDirector() mission asset list.")
        return 1

    configured = re.findall(r'"([^"]+)"', names_match.group(1))
    created_names = [asset_name for asset_name, _ in created]
    created_name_set = set(created_names)

    errors: list[str] = []
    duplicate_created = sorted({name for name in created_names if created_names.count(name) > 1})
    duplicate_configured = sorted({name for name in configured if configured.count(name) > 1})
    if duplicate_created:
        errors.append("Duplicate generated mission filenames: " + ", ".join(duplicate_created))
    if duplicate_configured:
        errors.append("Duplicate configured mission filenames: " + ", ".join(duplicate_configured))

    missing = [name for name in configured if name not in created_name_set]
    unconfigured = [name for name in created_names if name not in configured]
    if missing:
        errors.append("ConfigureMissionDirector loads files never created by EnsureMissionAssets: " + ", ".join(missing))
    if unconfigured:
        errors.append("EnsureMissionAssets creates mission files not assigned to the director: " + ", ".join(unconfigured))
    if created_names != configured:
        errors.append("Mission asset order differs between generation and MissionDirector assignment.")

    ids = [mission_id for _, mission_id in created]
    duplicate_ids = sorted({mission_id for mission_id in ids if ids.count(mission_id) > 1})
    if duplicate_ids:
        errors.append("Duplicate generated mission IDs: " + ", ".join(duplicate_ids))

    compact_source = re.sub(r"\s+", "", source)
    expected_branch_fragments = [
        'Branch("ending.public-route",Condition("nia",4,100),Condition("dev",5,100),Condition("tomas",5,100))',
        'Branch("ending.shared-tide",Condition("evelyn",5,100),Condition("jo",5,100))',
    ]
    if any(fragment not in compact_source for fragment in expected_branch_fragments):
        errors.append("Finale relationship branch definitions/order differ from the documented Public Route → Shared Tide → Quiet Sale contract.")
    actual_branches = len(re.findall(r'Branch\("', source))

    terminal_flags = re.findall(r"campaign\[(\d+)\]\.endsCampaign\s*=\s*true", source)
    if terminal_flags != ["8", "9", "10"]:
        errors.append("The three mutually exclusive ending assets are not all marked terminal at campaign indices 8, 9 and 10.")

    print("Generated mission files:", len(created_names))
    print("MissionDirector load entries:", len(configured))
    print("Finale conditional branches:", actual_branches)
    print("Terminal ending flags:", terminal_flags)
    if errors:
        print(f"LEGACY MISSION SEQUENCE AUDIT: FAIL ({len(errors)} issues)")
        for error in errors:
            print(" -", error)
        return 1

    print("LEGACY MISSION SEQUENCE AUDIT: PASS")
    print("This is a source contract check only; Unity asset generation and scene compilation remain separate.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
