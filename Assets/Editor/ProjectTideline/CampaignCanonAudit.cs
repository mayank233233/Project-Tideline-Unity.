#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>Checks the campaign registry without requiring imported art or a generated scene.</summary>
    public static class CampaignCanonAudit
    {
        private const string ReportPath = "ProjectTideline-Campaign-Canon-Audit.txt";

        [MenuItem("Project Tideline/6 — Audit Campaign Canon", priority = 6)]
        public static void RunAudit()
        {
            var issues = new List<string>();
            CampaignCanon.CollectValidationIssues(issues);

            string status = issues.Count == 0 ? "PASS" : "BLOCKED";
            var lines = new List<string>
            {
                "Project Tideline — Campaign Canon Audit",
                "Campaign: " + CampaignCanon.CampaignTitle,
                "City: " + CampaignCanon.CityName,
                "Status: " + status,
                "Playable leads: " + CampaignCanon.Leads.Count,
                "Supporting cast: " + CampaignCanon.SupportingCast.Count,
                "Factions: " + CampaignCanon.Factions.Count,
                "Districts: " + CampaignCanon.Districts.Count,
                "Missions: " + CampaignCanon.Missions.Count,
                "Endings: " + CampaignCanon.Endings.Count,
                "",
                "Notes:",
                "This audit validates narrative metadata only. It does not prove that these missions, characters, districts or endings are implemented in a Unity scene."
            };

            if (issues.Count == 0) lines.Add("No registry issues found.");
            else
                foreach (string issue in issues) lines.Add("BLOCKER: " + issue);

            File.WriteAllLines(ReportPath, lines);
            AssetDatabase.Refresh();

            if (issues.Count == 0)
            {
                Debug.Log("Campaign canon audit PASS. Report: " + ReportPath);
                EditorUtility.DisplayDialog("Campaign canon audit passed",
                    "The campaign registry is internally consistent. This does not mean the narrative is implemented or playable.\n\nReport: " + ReportPath, "OK");
            }
            else
            {
                Debug.LogError("Campaign canon audit BLOCKED:\n" + string.Join("\n", issues));
                EditorUtility.DisplayDialog("Campaign canon audit blocked",
                    string.Join("\n", issues) + "\n\nReport: " + ReportPath, "OK");
            }
        }
    }
}
#endif