#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Materializes the campaign registry as editable ScriptableObject design records.
    /// These are outlines only and are deliberately not MissionDefinition assets.
    /// </summary>
    public static class CampaignOutlineAssetBuilder
    {
        private const string OutputFolder = "Assets/ProjectTideline/Content/CampaignOutlines";

        [MenuItem("Project Tideline/7 — Generate Campaign Outline Assets", priority = 7)]
        public static void GenerateOrUpdateOutlines()
        {
            var issues = new List<string>();
            CampaignCanon.CollectValidationIssues(issues);
            if (issues.Count > 0)
            {
                EditorUtility.DisplayDialog("Campaign outline generation blocked",
                    string.Join("\n", issues), "OK");
                return;
            }

            EnsureFolder("Assets/ProjectTideline/Content");
            EnsureFolder(OutputFolder);

            int created = 0;
            int updated = 0;
            var expectedPaths = new HashSet<string>(StringComparer.Ordinal);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (CampaignCanon.Mission mission in CampaignCanon.Missions)
                {
                    string path = OutputFolder + "/" + mission.Id + ".asset";
                    expectedPaths.Add(path);
                    CampaignMissionOutline outline = AssetDatabase.LoadAssetAtPath<CampaignMissionOutline>(path);
                    if (outline == null)
                    {
                        outline = ScriptableObject.CreateInstance<CampaignMissionOutline>();
                        AssetDatabase.CreateAsset(outline, path);
                        created++;
                    }
                    else
                    {
                        updated++;
                    }

                    outline.canonId = mission.Id;
                    outline.missionTitle = mission.Title;
                    outline.leadId = mission.LeadId;
                    outline.isPrologue = mission.IsPrologue;
                    outline.districtId = mission.DistrictId;
                    outline.primaryVerb = mission.PrimaryVerb;
                    outline.playerGoal = mission.PlayerGoal;
                    outline.characterComplication = mission.CharacterComplication;
                    outline.playerChoice = mission.PlayerChoice;
                    outline.persistentConsequence = mission.PersistentConsequence;
                    outline.implementationStatus = "Narrative outline only — objectives, triggers, scenes, dialogue playback and rewards are not implemented.";
                    EditorUtility.SetDirty(outline);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string[] allOutlineGuids = AssetDatabase.FindAssets("t:CampaignMissionOutline", new[] { OutputFolder });
            var stalePaths = new List<string>();
            foreach (string guid in allOutlineGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!expectedPaths.Contains(path)) stalePaths.Add(path);
            }

            string report = "Campaign outlines created: " + created +
                            "\nCampaign outlines updated: " + updated +
                            "\nExpected records: " + CampaignCanon.Missions.Count +
                            "\nUnexpected stale records: " + stalePaths.Count +
                            "\n\nThese assets are design records, not playable MissionDefinition assets.";
            if (stalePaths.Count > 0)
                report += "\n\nReview manually; the tool never deletes stale assets:\n" + string.Join("\n", stalePaths);

            Debug.Log("Campaign outline generation finished.\n" + report);
            EditorUtility.DisplayDialog("Campaign outline generation complete", report, "OK");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
                throw new InvalidOperationException("Invalid campaign outline folder path: " + path);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif