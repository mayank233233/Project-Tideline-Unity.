#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Generates the executable MissionDefinition for Mara's opening prologue.
    /// Dialogue and scene wiring remain separate assets and must be configured explicitly.
    /// </summary>
    public static class MaraPrologueMissionBuilder
    {
        private const string MissionFolder = "Assets/ProjectTideline/Content/Missions/Prologues";
        private const string MissionPath = MissionFolder + "/two_minutes_to_close.asset";
        private const string IntroGraphPath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/Dialogue_prologue.mara.two_minutes_to_close.asset";
        private const string NicoGraphPath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/Dialogue_prologue.mara.nico_challenge.asset";

        [MenuItem("Project Tideline/12 — Generate Mara Prologue Mission", priority = 12)]
        public static void Generate()
        {
            if (!ValidateDialogueGraphs(out string reason))
            {
                EditorUtility.DisplayDialog("Mara prologue not generated", reason +
                    "\n\nRun Project Tideline → 9 — Generate PORT SABLE Prologue Dialogue first.", "OK");
                return;
            }

            EnsureFolder("Assets/ProjectTideline/Content");
            EnsureFolder("Assets/ProjectTideline/Content/Missions");
            EnsureFolder(MissionFolder);

            string[] guids = AssetDatabase.FindAssets("t:MissionDefinition");
            foreach (string guid in guids)
            {
                string existingPath = AssetDatabase.GUIDToAssetPath(guid);
                MissionDefinition existing = AssetDatabase.LoadAssetAtPath<MissionDefinition>(existingPath);
                if (existing != null && existing.missionId == "two_minutes_to_close"
                    && existingPath != MissionPath)
                {
                    EditorUtility.DisplayDialog("Mara prologue asset conflict",
                        "Mission ID 'two_minutes_to_close' already exists at:\n" + existingPath +
                        "\n\nResolve the duplicate manually; no asset was overwritten.", "OK");
                    return;
                }
            }

            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, MissionPath);
            }

            mission.missionId = "two_minutes_to_close";
            mission.title = "TWO MINUTES TO CLOSE";
            mission.briefing =
                "Finish the customer's workshop checks, deliver the car across Old Quay and the Saltline approach, then answer Nico's challenge.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "complete_customer_work_order",
                    description = "Inspect the steering, driver's door latch and engine rattle; complete all three final checks.",
                    completionSignal = "mara.workshop.order.completed",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "deliver_customer_vehicle",
                    description = "Drive the customer's vehicle through the configured Old Quay-to-Saltline delivery route.",
                    completionSignal = "mara.delivery.finished",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "resolve_nico_challenge",
                    description = "Respond to Nico after the delivery and close the shop.",
                    completionSignal = "prologue.mara.complete",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = string.Empty;
            mission.endsCampaign = false;

            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Mara prologue mission generated",
                "Created or updated:\n" + MissionPath +
                "\n\nObjectives: 3\n" +
                "1. Three workshop checks\n" +
                "2. Customer delivery route\n" +
                "3. Nico choice and shop close\n\n" +
                "This asset is executable mission data, but it is not yet assigned to a scene MissionDirector. " +
                "Configure the workshop stations, route checkpoints, vehicle relay and two dialogue NPCs.", "OK");
            Debug.Log("Mara prologue MissionDefinition generated: " + MissionPath);
        }

        private static bool ValidateDialogueGraphs(out string reason)
        {
            reason = string.Empty;
            if (!HasChoiceSignal(IntroGraphPath, "prologue.mara.work_order_accepted"))
            {
                reason = "Mara's opening dialogue graph is missing the work-order handoff choice signal.";
                return false;
            }

            if (!HasChoiceSignal(NicoGraphPath, "prologue.mara.complete")
                || !HasChoiceSignal(NicoGraphPath, "prologue.mara.accept_challenge")
                || !HasChoiceSignal(NicoGraphPath, "prologue.mara.decline_challenge"))
            {
                reason = "Mara's post-delivery Nico graph is missing its completion or challenge-choice signals.";
                return false;
            }

            return true;
        }

        private static bool HasChoiceSignal(string path, string signalId)
        {
            DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
            if (graph == null || graph.nodes == null) return false;

            foreach (DialogueGraph.DialogueNode node in graph.nodes)
            {
                if (node == null || node.choices == null) continue;
                foreach (DialogueGraph.DialogueChoice choice in node.choices)
                    if (choice != null && string.Equals(choice.signalId, signalId, StringComparison.Ordinal))
                        return true;
            }

            return false;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(folder))
                throw new InvalidOperationException("Invalid asset folder path: " + path);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
