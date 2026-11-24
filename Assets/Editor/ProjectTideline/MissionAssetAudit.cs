#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Audits serialized MissionDefinition assets, objective signal contracts, branch targets,
    /// ending terminals, the legacy builder's create/load filename contract and pure branch semantics.
    /// Missing Port Sable mission assets are reported as BLOCKED; narrative outlines are not playable missions.
    /// </summary>
    public static class MissionAssetAudit
    {
        private const string LegacyBuilderPath = "Assets/Editor/ProjectTideline/OldQuayVerticalSliceBuilder.cs";
        private const string LegacyMissionFolder = "Assets/ProjectTideline/Generated/Missions";
        private const string ReportPath = "ProjectTideline-Mission-Asset-Audit.txt";

        private sealed class BuilderMission
        {
            public string AssetName;
            public string MissionId;
        }

        private sealed class SignalProducerCatalog
        {
            private readonly Dictionary<string, HashSet<string>> exactSignals =
                new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            public bool HasRelationshipPattern;
            public bool HasAttentionBandPattern;
            public string RouteRacePrefix;
            public int ExactSignalCount => exactSignals.Count;

            public void Add(string signalId, string producer)
            {
                if (string.IsNullOrWhiteSpace(signalId)) return;
                if (!exactSignals.TryGetValue(signalId, out HashSet<string> producers))
                {
                    producers = new HashSet<string>(StringComparer.Ordinal);
                    exactSignals.Add(signalId, producers);
                }
                producers.Add(producer);
            }

            public bool TryResolve(string signalId, out string producer)
            {
                producer = string.Empty;
                if (string.IsNullOrWhiteSpace(signalId)) return false;

                if (exactSignals.TryGetValue(signalId, out HashSet<string> exact))
                {
                    producer = string.Join(", ", exact.OrderBy(x => x, StringComparer.Ordinal));
                    return true;
                }

                const string relationshipPrefix = "relationship.changed.";
                if (HasRelationshipPattern && signalId.StartsWith(relationshipPrefix, StringComparison.Ordinal)
                    && signalId.Length > relationshipPrefix.Length)
                {
                    producer = "NPCRelationshipLedger.ChangeValue (dynamic character ID)";
                    return true;
                }

                const string attentionPrefix = "world.attention.band.";
                if (HasAttentionBandPattern && signalId.StartsWith(attentionPrefix, StringComparison.Ordinal))
                {
                    string band = signalId.Substring(attentionPrefix.Length);
                    if (band == "calm" || band == "noticed" || band == "escalated")
                    {
                        producer = "PublicAttentionState.SetAttention (band transition)";
                        return true;
                    }
                }

                if (!string.IsNullOrWhiteSpace(RouteRacePrefix))
                {
                    foreach (string suffix in new[] { "started", "checkpoint", "finished", "cancelled" })
                    {
                        if (string.Equals(signalId, RouteRacePrefix + "." + suffix, StringComparison.Ordinal))
                        {
                            producer = "RouteRaceController (" + suffix + ")";
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [MenuItem("Project Tideline/11 — Audit Mission Assets and Branches", priority = 11)]
        public static void RunAudit()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var rows = new List<string>();

            Dictionary<string, int> legacyOrder = AuditLegacyBuilderContract(errors, rows);
            SignalProducerCatalog signalProducers = CollectSignalProducerCatalog();
            int objectiveSignalsWithProducers = 0;
            int objectiveSignalsMissingProducers = 0;
            var portSableOrder = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < CampaignCanon.Missions.Count; i++)
                portSableOrder[CampaignCanon.Missions[i].Id] = i;

            string[] guids = AssetDatabase.FindAssets("t:MissionDefinition");
            var assets = new List<MissionDefinition>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MissionDefinition asset = AssetDatabase.LoadAssetAtPath<MissionDefinition>(path);
                if (asset != null) assets.Add(asset);
            }

            var idToAsset = new Dictionary<string, MissionDefinition>(StringComparer.Ordinal);
            var pathById = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (MissionDefinition asset in assets)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrWhiteSpace(asset.missionId))
                {
                    errors.Add("Mission asset has no stable missionId: " + path);
                    continue;
                }
                if (idToAsset.ContainsKey(asset.missionId))
                    errors.Add("Duplicate missionId '" + asset.missionId + "' at " + path +
                               " and " + pathById[asset.missionId] + ".");
                else
                {
                    idToAsset.Add(asset.missionId, asset);
                    pathById.Add(asset.missionId, path);
                }
            }

            var storyFlagsInDialogue = CollectDialogueStoryFlags();
            var storyFlagsUsedByMissions = new HashSet<string>(StringComparer.Ordinal);
            int objectiveCount = 0;
            int branchCount = 0;
            int portSableAssetCount = 0;
            var legacyIdsByIndex = new Dictionary<int, string>();
            foreach (KeyValuePair<string, int> pair in legacyOrder)
                legacyIdsByIndex[pair.Value] = pair.Key;

            foreach (MissionDefinition mission in assets)
            {
                string path = AssetDatabase.GetAssetPath(mission);
                if (portSableOrder.ContainsKey(mission.missionId)) portSableAssetCount++;

                if (string.IsNullOrWhiteSpace(mission.title))
                    errors.Add(mission.missionId + ": mission title is blank.");
                if (mission.objectives == null || mission.objectives.Length == 0)
                {
                    errors.Add(mission.missionId + ": has no objectives; MissionDirector must not auto-complete design placeholders.");
                }
                else
                {
                    var objectiveIds = new HashSet<string>(StringComparer.Ordinal);
                    foreach (MissionObjective objective in mission.objectives)
                    {
                        objectiveCount++;
                        if (objective == null)
                        {
                            errors.Add(mission.missionId + ": contains a null objective.");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(objective.objectiveId))
                            errors.Add(mission.missionId + ": contains an objective with a blank objectiveId.");
                        else if (!objectiveIds.Add(objective.objectiveId))
                            errors.Add(mission.missionId + ": duplicate objectiveId '" + objective.objectiveId + "'.");
                        if (string.IsNullOrWhiteSpace(objective.description))
                            errors.Add(mission.missionId + "/" + objective.objectiveId + ": objective description is blank.");
                        if (string.IsNullOrWhiteSpace(objective.completionSignal))
                        {
                            errors.Add(mission.missionId + "/" + objective.objectiveId + ": completionSignal is blank.");
                        }
                        else if (signalProducers.TryResolve(objective.completionSignal, out string producer))
                        {
                            objectiveSignalsWithProducers++;
                            rows.Add("Signal source: " + mission.missionId + "/" +
                                objective.objectiveId + " -> " + objective.completionSignal + " [" + producer + "]");
                        }
                        else
                        {
                            objectiveSignalsMissingProducers++;
                            errors.Add(mission.missionId + "/" + objective.objectiveId +
                                ": completionSignal '" + objective.completionSignal +
                                "' has no matching source-code publisher or generated dialogue choice.");
                        }
                        if (objective.requiredCount < 1)
                            errors.Add(mission.missionId + "/" + objective.objectiveId + ": requiredCount must be at least one.");
                    }
                }

                MissionBranch[] branches = mission.completionBranches ?? Array.Empty<MissionBranch>();
                branchCount += branches.Length;
                bool catchAllSeen = false;
                var branchSignatures = new HashSet<string>(StringComparer.Ordinal);

                for (int i = 0; i < branches.Length; i++)
                {
                    MissionBranch branch = branches[i];
                    if (branch == null)
                    {
                        errors.Add(mission.missionId + ": contains a null completion branch at index " + i + ".");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(branch.nextMissionId))
                        errors.Add(mission.missionId + ": branch " + i + " has no nextMissionId.");
                    else
                        ValidateForwardTarget(mission, branch.nextMissionId, idToAsset, legacyOrder, portSableOrder, errors, "branch");

                    MissionBranchCondition[] conditions = branch.conditions ?? Array.Empty<MissionBranchCondition>();
                    if (conditions.Length == 0)
                    {
                        if (catchAllSeen)
                            errors.Add(mission.missionId + ": contains more than one unconditional branch.");
                        catchAllSeen = true;
                        if (i != branches.Length - 1)
                            errors.Add(mission.missionId + ": unconditional branch at index " + i +
                                       " shadows all later branches; move it to the end.");
                    }
                    else if (catchAllSeen)
                    {
                        errors.Add(mission.missionId + ": conditional branch appears after an unconditional branch and can never run.");
                    }

                    var conditionParts = new List<string>();
                    foreach (MissionBranchCondition condition in conditions)
                    {
                        if (condition == null)
                        {
                            errors.Add(mission.missionId + ": branch " + i + " contains a null condition.");
                            continue;
                        }
                        bool relationship = !string.IsNullOrWhiteSpace(condition.relationshipKey);
                        bool storyFlag = !string.IsNullOrWhiteSpace(condition.storyFlagId);
                        if (relationship == storyFlag)
                        {
                            errors.Add(mission.missionId + ": branch " + i +
                                       " condition must set exactly one of relationshipKey or storyFlagId.");
                            continue;
                        }
                        if (condition.minimumValue > condition.maximumValue)
                            errors.Add(mission.missionId + ": branch " + i + " has an inverted value range.");
                        if (condition.minimumValue < -100 || condition.maximumValue > 100)
                            errors.Add(mission.missionId + ": branch " + i + " condition range is outside [-100, 100].");

                        if (relationship)
                            conditionParts.Add("rel:" + condition.relationshipKey + ":" +
                                condition.minimumValue + ":" + condition.maximumValue);
                        else
                        {
                            conditionParts.Add("flag:" + condition.storyFlagId + ":" +
                                condition.minimumValue + ":" + condition.maximumValue);
                            storyFlagsUsedByMissions.Add(condition.storyFlagId);
                            if (!storyFlagsInDialogue.Contains(condition.storyFlagId))
                                errors.Add(mission.missionId + ": branch " + i +
                                           " references story flag '" + condition.storyFlagId +
                                           "' that no generated dialogue choice writes.");
                        }
                    }

                    conditionParts.Sort(StringComparer.Ordinal);
                    string signature = branch.nextMissionId + "|" + string.Join("&", conditionParts);
                    if (!branchSignatures.Add(signature))
                        errors.Add(mission.missionId + ": duplicate branch signature at index " + i + ".");
                }

                if (!string.IsNullOrWhiteSpace(mission.fallbackNextMissionId))
                    ValidateForwardTarget(mission, mission.fallbackNextMissionId, idToAsset, legacyOrder, portSableOrder, errors, "fallback");

                if (mission.missionId.StartsWith("ending.", StringComparison.Ordinal) && !mission.endsCampaign)
                    errors.Add(mission.missionId + ": ending asset must set endsCampaign = true.");

                if (mission.endsCampaign)
                {
                    if (branches.Length > 0)
                        errors.Add(mission.missionId + ": terminal ending has outgoing completion branches.");
                    if (!string.IsNullOrWhiteSpace(mission.fallbackNextMissionId))
                        errors.Add(mission.missionId + ": terminal ending has a fallback target.");
                }

                if (legacyOrder.TryGetValue(mission.missionId, out int legacyIndex))
                    rows.Add("Legacy[" + legacyIndex + "] " + mission.missionId + ": objectives=" +
                             (mission.objectives == null ? 0 : mission.objectives.Length) +
                             ", branches=" + branches.Length + ", terminal=" + mission.endsCampaign);
                else if (portSableOrder.TryGetValue(mission.missionId, out int storyIndex))
                    rows.Add("PORT SABLE[" + storyIndex + "] " + mission.missionId + ": objectives=" +
                             (mission.objectives == null ? 0 : mission.objectives.Length) +
                             ", branches=" + branches.Length + ", terminal=" + mission.endsCampaign);
            }

            foreach (string flag in storyFlagsInDialogue)
                if (!storyFlagsUsedByMissions.Contains(flag))
                    warnings.Add("Dialogue writes story flag '" + flag + "', but no serialized MissionDefinition branch consumes it yet.");

            int outlineCount = AssetDatabase.FindAssets("t:CampaignMissionOutline").Length;
            int portSableMissing = CampaignCanon.Missions.Count - portSableAssetCount;
            rows.Add("MissionDefinition assets discovered: " + assets.Count);
            rows.Add("Objectives discovered: " + objectiveCount);
            rows.Add("Branch records discovered: " + branchCount);
            rows.Add("Objective signals with source/data producers: " + objectiveSignalsWithProducers);
            rows.Add("Objective signals without source/data producers: " + objectiveSignalsMissingProducers);
            rows.Add("Exact signal IDs in source/data: " + signalProducers.ExactSignalCount);
            rows.Add("Signal catalog limitation: source/data presence does not prove that a producer component is wired into a playable scene.");
            rows.Add("PORT SABLE MissionDefinition assets: " + portSableAssetCount + "/" + CampaignCanon.Missions.Count);
            rows.Add("Campaign outline assets (design only): " + outlineCount + "/" + CampaignCanon.Missions.Count);
            if (portSableMissing > 0)
            {
                foreach (CampaignCanon.Mission mission in CampaignCanon.Missions)
                    if (!idToAsset.ContainsKey(mission.Id))
                        warnings.Add("Missing PORT SABLE MissionDefinition: " + mission.Id + " — " + mission.Title);
            }

            if (portSableMissing > 0)
                warnings.Add("PORT SABLE is not playable yet: " + portSableMissing +
                             " of " + CampaignCanon.Missions.Count +
                             " MissionDefinition assets are missing. CampaignCanon entries and CampaignMissionOutline assets are not executable missions.");

            if (assets.Count == 0)
                warnings.Add("No serialized MissionDefinition assets exist in this checkout. Running a Unity scene builder is required to create the legacy testbed assets; PORT SABLE still needs its own authored mission assets.");

            string status = errors.Count == 0
                ? (portSableMissing == 0 ? "PASS" : "BLOCKED — PORT SABLE MISSION ASSETS MISSING")
                : "FAIL";

            var report = new List<string>
            {
                "Project Tideline — Mission Asset and Branch Audit",
                "Status: " + status,
                "Unity Editor: " + Application.unityVersion,
                "",
                "Campaign assets:"
            };
            report.AddRange(rows);
            report.Add("");
            report.Add("Errors:");
            if (errors.Count == 0) report.Add("None.");
            else foreach (string error in errors) report.Add("ERROR: " + error);
            report.Add("");
            report.Add("Warnings / incomplete milestones:");
            if (warnings.Count == 0) report.Add("None.");
            else foreach (string warning in warnings) report.Add("BLOCKED: " + warning);
            report.Add("");
            report.Add("Pure branch evaluator tests:");
            report.AddRange(RunEvaluatorSelfTests());

            File.WriteAllLines(ReportPath, report);
            AssetDatabase.Refresh();

            if (errors.Count == 0 && portSableMissing == 0)
                Debug.Log("Mission asset and branch audit PASS. Report: " + ReportPath);
            else
                Debug.LogError("Mission asset and branch audit " + status + ". Errors: " +
                               errors.Count + "; PORT SABLE mission assets missing: " + portSableMissing + ". See " + ReportPath);

            EditorUtility.DisplayDialog("Mission asset and branch audit — " + status,
                "Serialized mission assets: " + assets.Count +
                "\nObjectives: " + objectiveCount +
                "\nBranches: " + branchCount +
                "\nPORT SABLE MissionDefinitions: " + portSableAssetCount + "/" + CampaignCanon.Missions.Count +
                "\nErrors: " + errors.Count +
                "\nWarnings: " + warnings.Count +
                "\n\nReport: " + ReportPath, "OK");
        }

        private static SignalProducerCatalog CollectSignalProducerCatalog()
        {
            var catalog = new SignalProducerCatalog();
            string[] sourcePaths = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);

            foreach (string sourcePath in sourcePaths)
            {
                string source;
                try
                {
                    source = File.ReadAllText(sourcePath);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                string producer = sourcePath.Replace('\\', '/');
                foreach (Match match in Regex.Matches(source,
                    @"MissionSignalBus\.Publish\s*\(\s*""(?<signal>[^""]+)"""))
                    catalog.Add(match.Groups["signal"].Value, producer);

                foreach (Match match in Regex.Matches(source,
                    @"new\s+MissionSignalBus\.Signal\s*\(\s*""(?<signal>[^""]+)"""))
                    catalog.Add(match.Groups["signal"].Value, producer);

                if (source.Contains("MissionSignalBus.Publish(\"relationship.changed.\" +"))
                    catalog.HasRelationshipPattern = true;
                if (source.Contains("MissionSignalBus.Publish(\"world.attention.band.\" +"))
                    catalog.HasAttentionBandPattern = true;

                if (Path.GetFileName(sourcePath) == "RouteRaceController.cs")
                {
                    Match prefix = Regex.Match(source,
                        @"signalPrefix\s*=\s*""(?<prefix>[^""]+)""");
                    if (prefix.Success
                        && source.Contains('"' + ".started" + '"')
                        && source.Contains('"' + ".checkpoint" + '"')
                        && source.Contains('"' + ".finished" + '"')
                        && source.Contains('"' + ".cancelled" + '"'))
                        catalog.RouteRacePrefix = prefix.Groups["prefix"].Value;
                }
            }

            // Dialogue choice signal IDs are data-driven; include generated graph assets
            // rather than treating arbitrary dynamic strings as registered producers.
            string[] dialogueGuids = AssetDatabase.FindAssets("t:DialogueGraph");
            foreach (string guid in dialogueGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                if (graph == null || graph.nodes == null) continue;

                foreach (DialogueGraph.DialogueNode node in graph.nodes)
                {
                    if (node == null || node.choices == null) continue;
                    foreach (DialogueGraph.DialogueChoice choice in node.choices)
                    {
                        if (choice != null && !string.IsNullOrWhiteSpace(choice.signalId))
                            catalog.Add(choice.signalId, "DialogueGraph " + graph.graphId);
                    }
                }
            }

            return catalog;
        }

        private static Dictionary<string, int> AuditLegacyBuilderContract(List<string> errors, List<string> rows)
        {
            var order = new Dictionary<string, int>(StringComparer.Ordinal);
            if (!File.Exists(LegacyBuilderPath))
            {
                errors.Add("Recovered legacy builder source is missing: " + LegacyBuilderPath +
                           ". The selected GitHub source snapshot does not yet include the complete v0.4.9 builder.");
                return order;
            }

            string source = File.ReadAllText(LegacyBuilderPath);
            var created = new List<BuilderMission>();
            MatchCollection createMatches = Regex.Matches(source,
                @"CreateMission\(\s*""(?<asset>[^""]+)""\s*,\s*""(?<id>[^""]+)""",
                RegexOptions.Singleline);
            foreach (Match match in createMatches)
            {
                created.Add(new BuilderMission
                {
                    AssetName = match.Groups["asset"].Value,
                    MissionId = match.Groups["id"].Value
                });
            }

            Match namesMatch = Regex.Match(source,
                @"string\[\]\s+names\s*=\s*\{(?<body>.*?)\};",
                RegexOptions.Singleline);
            if (!namesMatch.Success)
            {
                errors.Add("Could not parse ConfigureMissionDirector() asset-name array in OldQuayVerticalSliceBuilder.cs.");
                return order;
            }

            var configured = new List<string>();
            foreach (Match match in Regex.Matches(namesMatch.Groups["body"].Value, @"""(?<name>[^""]+)"""))
                configured.Add(match.Groups["name"].Value);

            var createdNames = new HashSet<string>(created.Select(x => x.AssetName), StringComparer.Ordinal);
            for (int i = 0; i < configured.Count; i++)
            {
                if (!createdNames.Contains(configured[i]))
                {
                    errors.Add("Legacy mission setup loads '" + configured[i] +
                               "' but EnsureMissionAssets() never creates that asset filename.");
                    continue;
                }

                BuilderMission mission = created.First(x => x.AssetName == configured[i]);
                if (!order.ContainsKey(mission.MissionId))
                    order.Add(mission.MissionId, i);
                else
                    errors.Add("Legacy builder creates duplicate missionId '" + mission.MissionId + "'.");
            }

            foreach (BuilderMission mission in created)
            {
                if (!configured.Contains(mission.AssetName))
                    warningsAddUnconfigured(rows, mission.AssetName, mission.MissionId);
            }

            if (configured.Count != created.Count)
                errors.Add("Legacy mission creation/configuration counts differ: creates=" +
                           created.Count + ", configured=" + configured.Count + ".");

            rows.Add("Legacy builder: created mission assets=" + created.Count +
                     ", director load entries=" + configured.Count +
                     ", filename/order contract=" + (configured.SequenceEqual(created.Select(x => x.AssetName)) ? "PASS" : "FAIL"));
            return order;
        }

        private static void warningsAddUnconfigured(List<string> rows, string assetName, string missionId)
        {
            rows.Add("UNCONFIGURED legacy asset: " + assetName + " (" + missionId + ")");
        }

        private static void ValidateForwardTarget(
            MissionDefinition source,
            string targetId,
            Dictionary<string, MissionDefinition> knownAssets,
            Dictionary<string, int> legacyOrder,
            Dictionary<string, int> portSableOrder,
            List<string> errors,
            string kind)
        {
            if (!knownAssets.TryGetValue(targetId, out MissionDefinition target))
            {
                errors.Add(source.missionId + ": " + kind + " target '" + targetId + "' is not present among serialized MissionDefinition assets.");
                return;
            }

            bool sourceLegacy = legacyOrder.TryGetValue(source.missionId, out int sourceLegacyIndex);
            bool targetLegacy = legacyOrder.TryGetValue(targetId, out int targetLegacyIndex);
            if (sourceLegacy && targetLegacy)
            {
                if (targetLegacyIndex <= sourceLegacyIndex)
                    errors.Add(source.missionId + ": " + kind + " target '" + targetId + "' is not later in the configured legacy campaign.");
                return;
            }

            bool sourcePortSable = portSableOrder.TryGetValue(source.missionId, out int sourceStoryIndex);
            bool targetPortSable = portSableOrder.TryGetValue(targetId, out int targetStoryIndex);
            if (sourcePortSable && targetPortSable)
            {
                if (targetStoryIndex <= sourceStoryIndex)
                    errors.Add(source.missionId + ": " + kind + " target '" + targetId + "' is not later in the PORT SABLE canon order.");
                return;
            }

            errors.Add(source.missionId + ": cannot establish campaign order for " + kind +
                       " target '" + targetId + "'; source and target must belong to the same registered campaign.");
        }

        private static HashSet<string> CollectDialogueStoryFlags()
        {
            var flags = new HashSet<string>(StringComparer.Ordinal);
            string[] guids = AssetDatabase.FindAssets("t:DialogueGraph");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                if (graph == null || graph.nodes == null) continue;
                foreach (DialogueGraph.DialogueNode node in graph.nodes)
                {
                    if (node == null || node.choices == null) continue;
                    foreach (DialogueGraph.DialogueChoice choice in node.choices)
                        if (choice != null && !string.IsNullOrWhiteSpace(choice.storyFlagId))
                            flags.Add(choice.storyFlagId);
                }
            }
            return flags;
        }

        private static List<string> RunEvaluatorSelfTests()
        {
            var failures = new List<string>();
            Func<string, int?> relationship = key => key == "nia" ? (int?)4 : key == "dev" ? 5 : null;
            Func<string, int?> flags = key => key == "mara_race_choice" ? (int?)2 : key == "explicit_zero" ? 0 : null;

            Test("relationship inclusive bound", Match(Relation("nia", 4, 4), relationship, flags), failures);
            Test("relationship below threshold rejected", !Match(Relation("nia", 5, 100), relationship, flags), failures);
            Test("missing relationship not treated as zero", !Match(Relation("missing", -100, 100), relationship, flags), failures);
            Test("relationship condition accepts reversed editor bounds deterministically",
                Match(Relation("nia", 4, 2), relationship, flags), failures);
            Test("story flag exact value", Match(Flag("mara_race_choice", 2, 2), relationship, flags), failures);
            Test("unset story flag does not match a range containing zero",
                !Match(Flag("unset_flag", -1, 1), relationship, flags), failures);
            Test("explicitly stored zero is distinguishable from unset",
                Match(Flag("explicit_zero", 0, 0), relationship, flags), failures);
            Test("ambiguous dual-source condition rejected",
                !Match(new MissionBranch { conditions = new[] {
                    new MissionBranchCondition { relationshipKey = "nia", storyFlagId = "mara_race_choice", minimumValue = -100, maximumValue = 100 }
                } }, relationship, flags), failures);
            Test("condition with no source rejected",
                !Match(new MissionBranch { conditions = new[] { new MissionBranchCondition() } }, relationship, flags), failures);
            Test("multiple conditions are ANDed",
                Match(new MissionBranch { conditions = new[] { RelationCondition("nia", 4, 4), RelationCondition("dev", 5, 5) } }, relationship, flags), failures);
            Test("one failed condition rejects the whole branch",
                !Match(new MissionBranch { conditions = new[] { RelationCondition("nia", 4, 4), RelationCondition("dev", 6, 6) } }, relationship, flags), failures);
            Test("null branch rejected", !Match(null, relationship, flags), failures);
            Test("empty condition array is unconditional",
                Match(new MissionBranch { conditions = Array.Empty<MissionBranchCondition>() }, relationship, flags), failures);

            MissionBranch[] orderedBranches =
            {
                Relation("nia", 0, 100),
                Relation("nia", 4, 4),
                new MissionBranch { conditions = Array.Empty<MissionBranchCondition>() }
            };
            int firstMatch = -1;
            for (int i = 0; i < orderedBranches.Length; i++)
            {
                if (!Match(orderedBranches[i], relationship, flags)) continue;
                firstMatch = i;
                break;
            }
            Test("the first matching branch wins when multiple conditions are true", firstMatch == 0, failures);

            MissionBranch[] fallbackBranches =
            {
                Relation("nia", 5, 100),
                new MissionBranch { conditions = Array.Empty<MissionBranchCondition>() }
            };
            int fallbackMatch = -1;
            for (int i = 0; i < fallbackBranches.Length; i++)
            {
                if (!Match(fallbackBranches[i], relationship, flags)) continue;
                fallbackMatch = i;
                break;
            }
            Test("unconditional fallback is selected only after earlier conditions fail", fallbackMatch == 1, failures);

            if (failures.Count == 0) failures.Add("PASS — 15 branch evaluator cases.");
            else failures.Insert(0, "FAIL — " + failures.Count + " branch evaluator cases.");
            return failures;
        }

        private static bool Match(MissionBranch branch, Func<string, int?> relationship, Func<string, int?> flags)
        {
            return MissionBranchEvaluator.ConditionsMatch(branch, relationship, flags);
        }

        private static MissionBranch Relation(string key, int min, int max)
        {
            return new MissionBranch { conditions = new[] { RelationCondition(key, min, max) } };
        }

        private static MissionBranchCondition RelationCondition(string key, int min, int max)
        {
            return new MissionBranchCondition { relationshipKey = key, minimumValue = min, maximumValue = max };
        }

        private static MissionBranch Flag(string key, int min, int max)
        {
            return new MissionBranch { conditions = new[] {
                new MissionBranchCondition { storyFlagId = key, minimumValue = min, maximumValue = max }
            } };
        }

        private static void Test(string name, bool pass, List<string> failures)
        {
            if (!pass) failures.Add("FAIL — " + name);
        }

    }
}
#endif