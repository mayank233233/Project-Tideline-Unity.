#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Validates the four generated dialogue graphs across the three prologues, including branch links and saved outcome flags.
    /// It does not claim that the graphs are attached to scene NPCs or are playable.
    /// </summary>
    public static class PortSablePrologueDialogueAudit
    {
        private const string Root = "Assets/ProjectTideline/Content/Dialogue/Prologues";
        private const string ReportPath = "ProjectTideline-Prologue-Dialogue-Audit.txt";

        private sealed class ExpectedGraph
        {
            public string GraphId;
            public string DisplayName;
        }

        private static readonly ExpectedGraph[] ExpectedGraphs =
        {
            new ExpectedGraph { GraphId = "prologue.mara.two_minutes_to_close", DisplayName = "Mara — garage opening" },
            new ExpectedGraph { GraphId = "prologue.mara.nico_challenge", DisplayName = "Mara — Nico after delivery" },
            new ExpectedGraph { GraphId = "prologue.rafi.last_song_first_lie", DisplayName = "Rafi — LAST SONG, FIRST LIE" },
            new ExpectedGraph { GraphId = "prologue.elias.the_old_terms", DisplayName = "Elias — THE OLD TERMS" }
        };

        private static readonly Dictionary<string, HashSet<int>> RequiredFlags =
            new Dictionary<string, HashSet<int>>(StringComparer.Ordinal)
            {
                { "mara_race_choice", new HashSet<int> { 1, 2 } },
                { "rafi_event_choice", new HashSet<int> { 1, 2 } },
                { "rafi_staff_responsibility", new HashSet<int> { 1, 2 } },
                { "elias_settlement_choice", new HashSet<int> { 1, 2 } }
            };

        private static readonly string[] RequiredSignals =
        {
            "prologue.mara.work_order_accepted",
            "prologue.mara.complete"
        };

        private static readonly Dictionary<string, KeyValuePair<string, int>> RequiredChoiceOutcomes =
            new Dictionary<string, KeyValuePair<string, int>>(StringComparer.Ordinal)
            {
                { "prologue.mara.accept_challenge", new KeyValuePair<string, int>("mara_race_choice", 1) },
                { "prologue.mara.decline_challenge", new KeyValuePair<string, int>("mara_race_choice", 2) },
                { "prologue.rafi.honour_first_slot", new KeyValuePair<string, int>("rafi_event_choice", 1) },
                { "prologue.rafi.protect_headline", new KeyValuePair<string, int>("rafi_event_choice", 2) },
                { "prologue.rafi.own_promise", new KeyValuePair<string, int>("rafi_staff_responsibility", 1) },
                { "prologue.rafi.take_responsibility", new KeyValuePair<string, int>("rafi_staff_responsibility", 1) },
                { "prologue.rafi.shift_burden", new KeyValuePair<string, int>("rafi_staff_responsibility", 2) },
                { "prologue.elias.accept_invitation", new KeyValuePair<string, int>("elias_settlement_choice", 1) },
                { "prologue.elias.refuse_signature", new KeyValuePair<string, int>("elias_settlement_choice", 2) }
            };

        [MenuItem("Project Tideline/10 — Audit PORT SABLE Prologue Dialogue", priority = 10)]
        public static void RunAudit()
        {
            var issues = new List<string>();
            var foundFlags = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
            var rows = new List<string>();

            foreach (ExpectedGraph expected in ExpectedGraphs)
            {
                string path = Root + "/Dialogue_" + expected.GraphId + ".asset";
                DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                if (graph == null)
                {
                    issues.Add(expected.DisplayName + ": missing DialogueGraph asset at " + path);
                    rows.Add(expected.DisplayName + ": MISSING");
                    continue;
                }

                if (!string.Equals(graph.graphId, expected.GraphId, StringComparison.Ordinal))
                    issues.Add(expected.DisplayName + ": graphId does not match its expected ID.");
                if (graph.nodes == null || graph.nodes.Length == 0)
                {
                    issues.Add(expected.DisplayName + ": graph has no dialogue nodes.");
                    rows.Add(expected.DisplayName + ": EMPTY");
                    continue;
                }

                var nodeIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (DialogueGraph.DialogueNode node in graph.nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.nodeId))
                    {
                        issues.Add(expected.DisplayName + ": contains a null node or node without an ID.");
                        continue;
                    }
                    if (!nodeIds.Add(node.nodeId))
                        issues.Add(expected.DisplayName + ": duplicate node ID '" + node.nodeId + "'.");
                    if (string.IsNullOrWhiteSpace(node.speaker) || string.IsNullOrWhiteSpace(node.line))
                        issues.Add(expected.DisplayName + ": node '" + node.nodeId + "' has no speaker or dialogue line.");
                }

                if (string.IsNullOrWhiteSpace(graph.startNodeId) || !nodeIds.Contains(graph.startNodeId))
                {
                    issues.Add(expected.DisplayName + ": startNodeId is missing or does not point to a node.");
                    rows.Add(expected.DisplayName + ": INVALID START");
                    continue;
                }

                foreach (DialogueGraph.DialogueNode node in graph.nodes)
                {
                    if (node == null || node.choices == null) continue;
                    foreach (DialogueGraph.DialogueChoice choice in node.choices)
                    {
                        if (choice == null || string.IsNullOrWhiteSpace(choice.label))
                        {
                            issues.Add(expected.DisplayName + ": node '" + node.nodeId + "' contains an empty choice.");
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(choice.nextNodeId) && !nodeIds.Contains(choice.nextNodeId))
                            issues.Add(expected.DisplayName + ": choice '" + choice.label + "' points to missing node '" + choice.nextNodeId + "'.");

                        if (!string.IsNullOrWhiteSpace(choice.storyFlagId))
                        {
                            if (!foundFlags.TryGetValue(choice.storyFlagId, out HashSet<int> values))
                            {
                                values = new HashSet<int>();
                                foundFlags.Add(choice.storyFlagId, values);
                            }
                            values.Add(choice.storyFlagValue);
                        }
                    }
                }

                HashSet<string> reachable = CollectReachableNodes(graph);
                foreach (string nodeId in nodeIds)
                    if (!reachable.Contains(nodeId))
                        issues.Add(expected.DisplayName + ": node '" + nodeId + "' is unreachable from the start node.");

                rows.Add(expected.DisplayName + ": nodes=" + graph.nodes.Length + ", reachable=" + reachable.Count);
            }

            foreach (KeyValuePair<string, HashSet<int>> required in RequiredFlags)
            {
                if (!foundFlags.TryGetValue(required.Key, out HashSet<int> actual))
                {
                    issues.Add("Required outcome flag is missing: " + required.Key);
                    continue;
                }
                foreach (int value in required.Value)
                    if (!actual.Contains(value))
                        issues.Add("Outcome flag '" + required.Key + "' is missing required value " + value + ".");
            }

            foreach (string requiredSignal in RequiredSignals)
            {
                bool signalFound = false;
                foreach (ExpectedGraph graph in ExpectedGraphs)
                {
                    DialogueGraph asset = AssetDatabase.LoadAssetAtPath<DialogueGraph>(
                        Root + "/Dialogue_" + graph.GraphId + ".asset");
                    if (asset == null || asset.nodes == null) continue;
                    foreach (DialogueGraph.DialogueNode node in asset.nodes)
                    {
                        if (node == null || node.choices == null) continue;
                        foreach (DialogueGraph.DialogueChoice choice in node.choices)
                            if (choice != null && string.Equals(choice.signalId, requiredSignal, StringComparison.Ordinal))
                                signalFound = true;
                    }
                }

                if (!signalFound)
                    issues.Add("Required Mara prologue signal is missing: " + requiredSignal);
            }

            foreach (KeyValuePair<string, KeyValuePair<string, int>> expected in RequiredChoiceOutcomes)
            {
                bool choiceFound = false;
                bool exactWriteFound = false;
                foreach (ExpectedGraph graph in ExpectedGraphs)
                {
                    DialogueGraph asset = AssetDatabase.LoadAssetAtPath<DialogueGraph>(
                        Root + "/Dialogue_" + graph.GraphId + ".asset");
                    if (asset == null || asset.nodes == null) continue;
                    foreach (DialogueGraph.DialogueNode node in asset.nodes)
                    {
                        if (node == null || node.choices == null) continue;
                        foreach (DialogueGraph.DialogueChoice choice in node.choices)
                        {
                            if (choice == null || !string.Equals(choice.signalId, expected.Key, StringComparison.Ordinal))
                                continue;
                            choiceFound = true;
                            if (string.Equals(choice.storyFlagId, expected.Value.Key, StringComparison.Ordinal)
                                && choice.storyFlagValue == expected.Value.Value)
                                exactWriteFound = true;
                        }
                    }
                }

                if (!choiceFound)
                    issues.Add("Required dialogue choice signal is missing: " + expected.Key);
                else if (!exactWriteFound)
                    issues.Add("Choice '" + expected.Key + "' does not write " + expected.Value.Key + "=" + expected.Value.Value + ".");
            }

            string status = issues.Count == 0 ? "PASS" : "BLOCKED";
            var report = new List<string>
            {
                "Project Tideline — PORT SABLE Prologue Dialogue Audit",
                "Status: " + status,
                "",
                "Graphs:"
            };
            report.AddRange(rows);
            report.Add("");
            report.Add("Required persisted story flags: " + RequiredFlags.Count);
            report.Add("Notes: This validates dialogue data only. It does not prove that DialogueController, NPCs, scenes, voice, animation or mission triggers are wired.");
            report.Add("");
            if (issues.Count == 0) report.Add("No dialogue graph issues found.");
            else foreach (string issue in issues) report.Add("BLOCKER: " + issue);

            File.WriteAllLines(ReportPath, report);
            AssetDatabase.Refresh();
            if (issues.Count == 0)
                Debug.Log("PORT SABLE prologue dialogue audit PASS. Report: " + ReportPath);
            else
                Debug.LogError("PORT SABLE prologue dialogue audit BLOCKED:\n" + string.Join("\n", issues));

            EditorUtility.DisplayDialog("PORT SABLE prologue dialogue audit — " + status,
                string.Join("\n", rows) + "\n\nRequired flags: " + RequiredFlags.Count +
                (issues.Count > 0 ? "\n\n" + string.Join("\n", issues) : ""), "OK");
        }

        private static HashSet<string> CollectReachableNodes(DialogueGraph graph)
        {
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            var pending = new Queue<string>();
            pending.Enqueue(graph.startNodeId);

            while (pending.Count > 0)
            {
                string nodeId = pending.Dequeue();
                if (string.IsNullOrWhiteSpace(nodeId) || !reachable.Add(nodeId)) continue;
                DialogueGraph.DialogueNode node = graph.FindNode(nodeId);
                if (node == null || node.choices == null) continue;
                foreach (DialogueGraph.DialogueChoice choice in node.choices)
                    if (choice != null && !string.IsNullOrWhiteSpace(choice.nextNodeId) && !reachable.Contains(choice.nextNodeId))
                        pending.Enqueue(choice.nextNodeId);
            }

            return reachable;
        }
    }
}
#endif