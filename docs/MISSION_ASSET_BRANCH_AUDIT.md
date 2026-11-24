# Project Tideline — mission asset and branch audit

**Audit scope:** recovered v0.4.9 source at `ba72e8d`, its Old Quay mission builder and relationship-based finale, plus the PORT SABLE campaign source currently published in this repository.  
**Audit type:** source and serialized-asset availability review. Unity Editor asset generation, C# compilation and Play Mode were not run in this environment.

## Executive result

There is one confirmed legacy builder defect, one valid but unexecuted legacy ending graph, and a larger PORT SABLE content gap.

| Area | Result | Evidence |
|---|---|---|
| Old Quay mission filename contract | **Defect found and corrected in the recovered working copy** | Two configured names did not match names created by the builder |
| Old Quay three-ending branch logic | **Source logic is coherent and reachable** | Ordered relationship gates, explicit fallback and three terminal ending flags |
| Serialized Old Quay MissionDefinition assets | **Not present in the recovered source checkout** | They are generated under the ignored `Assets/ProjectTideline/Generated/Missions` folder |
| PORT SABLE MissionDefinition assets | **0 of 21 present** | CampaignCanon has metadata; it is not a playable MissionDefinition campaign |
| PORT SABLE prologue flags consumed by mission branches | **Not yet connected** | Dialogue builder writes flags; no PORT SABLE mission assets currently consume them |
| Unity Editor / Play Mode validation | **Not run** | Requires the full project in Unity 6000.3.28f1 |

## 1. Confirmed Old Quay builder defect

The recovered `EnsureMissionAssets()` method creates these filenames:

- `M05_Long_Way_Around.asset`
- `M06_High_Water_Mark.asset`

But `ConfigureMissionDirector()` attempted to load:

- `M05_Tide_Ledger.asset`
- `M06_A_Favor_Owed.asset`

The latter two files are never created by `EnsureMissionAssets()`. The builder's null-asset guard therefore throws before the MissionDirector receives its campaign array. The rest of the legacy scene setup must not be treated as build-verified until this is fixed.

**Exact correction:** change the MissionDirector filename array entries to `M05_Long_Way_Around` and `M06_High_Water_Mark`. The ready-to-apply patch is committed at [patches/0001-fix-oldquay-mission-asset-load-names.patch](../patches/0001-fix-oldquay-mission-asset-load-names.patch).

I applied that exact correction to the recovered working copy and ran `tools/validate_legacy_mission_sequence.py`:
- generated mission filenames: 11;
- MissionDirector load entries: 11;
- finale conditional branches: 2;
- terminal ending flags: indices 8, 9 and 10;
- result: **PASS**.

The complete 118 KB `OldQuayVerticalSliceBuilder.cs` is not yet included in the selected GitHub source snapshot, so the patch is published separately rather than claiming the missing source file was updated on GitHub. The source snapshot's Unity Editor audit will detect this mismatch once the full builder is merged.

## 2. Old Quay ending branch audit

The legacy campaign order is eight chapters followed by three mutually exclusive endings:

| Campaign index | Mission ID | Purpose |
|---:|---|---|
| 7 | `last-open-route` | Finale; evaluates relationship conditions |
| 8 | `ending.public-route` | Public Route epilogue; terminal |
| 9 | `ending.shared-tide` | Shared Tide epilogue; terminal |
| 10 | `ending.quiet-sale` | Quiet Sale fallback; terminal |

The finale's branch array is ordered:

1. **Public Route** — `nia >= 4 AND dev >= 5 AND tomas >= 5`.
2. **Shared Tide** — `evelyn >= 5 AND jo >= 5`, only when the earlier Public Route branch did not match.
3. **Quiet Sale** — explicit fallback via `fallbackNextMissionId = "ending.quiet-sale"`.

All branch targets are later in the campaign order. The three ending assets are marked `endsCampaign = true`, so they do not continue into each other.

### Are the relationship gates reachable?

Yes, from the authored source choices and relief interactions:

| Relationship | Authored positive values | Required | Reachability |
|---|---:|---:|---|
| Nia | +4 or +2 | 4 | The +4 careful-evidence choice reaches the threshold |
| Dev | +5 or +2, plus +3 from the clinic relief site | 5 | Directly reachable with +5; alternate choice can reach 5 with the site reward |
| Tomas | +5 or +2, plus +3 from the night-market relief site | 5 | Directly reachable with +5; alternate choice can reach 5 with the site reward |
| Evelyn | +5 or +2, plus +3 from the ferry relief site | 5 | Directly reachable with +5; alternate choice can reach 5 with the site reward |
| Jo | +5 or -1 | 5 | The +5 choice reaches the threshold |

The High Water Mark objective accepts any two of the three one-shot relief terminals. The Public Route branch can be reached by choosing the positive Nia option and either choosing the +5 Dev/Tomas options or combining their +2 options with the corresponding clinic/night-market rewards. Shared Tide can be reached with Jo's +5 choice and either Evelyn's +5 choice or her +2 choice plus the ferry reward.

**Branch priority matters:** a save that satisfies both endings' relationship gates routes to Public Route because it is first. Shared Tide is the second-priority outcome, not an equal-priority alternative. Quiet Sale is the fallback when neither condition matches.

The source-level branch design is coherent. Actual persisted ScriptableObject values, UI reward wiring and Play Mode transitions remain unverified because the generated mission assets and scene are not present in the checkout.

## 3. PORT SABLE mission asset gap

PORT SABLE currently defines 21 mission records in `CampaignCanon`: three lead prologues and eighteen core missions. Those entries are narrative metadata with goals, gameplay verbs, character complications, choices and consequences.

The optional `CampaignMissionOutline` assets are also **design records**, not `MissionDefinition` objects. The generator deliberately keeps them separate so an outline with no completion objectives cannot be attached to `MissionDirector` and instantly auto-complete.

At this audit:
- `MissionDefinition` assets for PORT SABLE: **0/21**;
- generated `CampaignMissionOutline` assets in the recovered source checkout: **0/21**;
- generated prologue `DialogueGraph` assets in the source checkout: not present;
- mission branch conditions consuming `mara_race_choice`, `rafi_event_choice`, `rafi_staff_responsibility` or `elias_settlement_choice`: **none yet**.

The four flags are authored by the PORT SABLE dialogue builder and are supported by the narrative-state ledger, save schema v3 and branch evaluator. They do not yet cause later mission routing because the PORT SABLE `MissionDefinition` assets and their objective/branch data have not been authored. This is a content-production blocker, not a reason to manufacture empty placeholder missions.

## 4. Runtime branch hardening committed

The current GitHub source now includes:

- `MissionBranchEvaluator`: a shared, deterministic evaluator for relationship and story-flag conditions;
- exactly one condition source is required per condition;
- all conditions in a branch are ANDed;
- numeric bounds are inclusive and normalized deterministically;
- absent relationship values and unset story flags do **not** become implicit zeroes;
- an empty condition array is an unconditional branch and must be last;
- `MissionDirector.StartMission` refuses a mission with no objectives instead of silently completing it;
- `NPCRelationshipLedger.HasValue` distinguishes a never-written key from an explicitly stored value of zero.

The Unity Editor menu **Project Tideline → 11 — Audit Mission Assets and Branches** checks:
The source-producer check establishes only that a matching publisher exists in C# or generated dialogue data. It does **not** establish that the component is present, configured, enabled or reachable in a playable scene. Scene/prefab wiring and runtime completion must be tested separately.

- stable/unique mission IDs and objective IDs;
- non-empty objective arrays, completion signals and positive required counts;
- every objective completion signal matched against a source-code publisher, a batched signal, or a generated dialogue-graph choice; supported dynamic patterns include `relationship.changed.<characterId>`, the three public-attention bands and the route controller's default `race.long_loop.*` prefix;
- branch/fallback target existence and forward ordering;
- branch condition source validity and range order;
- unconditional branches that shadow later branches;
- terminal endings with outgoing links;
- dialogue flags that are never consumed by a mission branch;
- mismatch between legacy builder asset creation names and MissionDirector load names;
- the current PORT SABLE MissionDefinition completion gap.

It also runs 15 deterministic in-process tests against the production `MissionBranchEvaluator`. The menu has been authored but has **not** been run in Unity yet; those 15 cases are therefore a test harness, not a claimed pass.

## 5. Required next implementation gates

1. Apply the committed legacy filename patch to the complete recovered `OldQuayVerticalSliceBuilder.cs`.
2. Run `python tools/validate_legacy_mission_sequence.py` against the full project root.
3. Open the project with Unity **6000.3.28f1** and generate the legacy Old Quay mission assets; run the Mission Asset and Branch Audit.
4. Verify the three legacy endings by testing each gate independently, a save where both relationship gates match, and the fallback case.
5. Author PORT SABLE `MissionDefinition` assets only when each has real objective signals, a non-optional completion path, explicit branches/fallbacks and terminal settings. Do not attach `CampaignMissionOutline` assets to the MissionDirector.
6. Add downstream mission branches consuming all four PORT SABLE prologue flags, then test each value and the unset-flag case from fresh and restored saves.
7. Capture Unity compile and Play Mode evidence before marking any mission branch complete.

## 6. Verification boundary

The recovered v0.4.9 source validator passes against the recovered source tree (45 C# files). The new legacy mission sequence validator passes against the recovered working copy **after the filename correction**. GitHub Actions checks the selected PORT SABLE source snapshot. None of these results proves Unity compilation, actual ScriptableObject generation, scene wiring, Play Mode completion, Android build or Vivo device behaviour.
