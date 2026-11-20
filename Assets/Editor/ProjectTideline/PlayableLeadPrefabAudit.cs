#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Enforces three distinct, animated hero prefabs before a playable roster can be built.
    /// This tool deliberately refuses to manufacture primitive or crowd-model placeholders.
    /// </summary>
    public static class PlayableLeadPrefabAudit
    {
        private const string ReportPath = "ProjectTideline-Lead-Prefab-Audit.txt";

        private sealed class LeadAsset
        {
            public string Id;
            public string DisplayName;
            public string PrefabPath;
        }

        private static readonly LeadAsset[] RequiredLeads =
        {
            new LeadAsset { Id = "mara_kade", DisplayName = "Mara Kade", PrefabPath = "Assets/ProjectTideline/Characters/Leads/MaraKade.prefab" },
            new LeadAsset { Id = "rafi_sayegh", DisplayName = "Rafi Sayegh", PrefabPath = "Assets/ProjectTideline/Characters/Leads/RafiSayegh.prefab" },
            new LeadAsset { Id = "elias_ward", DisplayName = "Elias Ward", PrefabPath = "Assets/ProjectTideline/Characters/Leads/EliasWard.prefab" }
        };

        [MenuItem("Project Tideline/8 — Audit Bespoke Lead Prefabs", priority = 8)]
        public static void RunAudit()
        {
            var issues = new List<string>();
            var primaryMeshPaths = new HashSet<string>();
            var rows = new List<string>();

            foreach (LeadAsset lead in RequiredLeads)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(lead.PrefabPath);
                if (prefab == null)
                {
                    issues.Add(lead.DisplayName + ": missing bespoke prefab at " + lead.PrefabPath);
                    rows.Add(lead.DisplayName + ": MISSING");
                    continue;
                }

                bool rootComponents = prefab.GetComponent<CharacterController>() != null
                    && prefab.GetComponent<ThirdPersonMotor>() != null
                    && prefab.GetComponent<ProximityInteractor>() != null
                    && prefab.GetComponent<CharacterAnimationDriver>() != null;
                if (!rootComponents)
                    issues.Add(lead.DisplayName + ": prefab root must contain CharacterController, ThirdPersonMotor, ProximityInteractor and CharacterAnimationDriver.");

                Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
                bool validAnimator = false;
                foreach (Animator animator in animators)
                {
                    if (animator != null && animator.runtimeAnimatorController != null
                        && animator.avatar != null && animator.avatar.isValid)
                    {
                        validAnimator = true;
                        break;
                    }
                }
                if (!validAnimator)
                    issues.Add(lead.DisplayName + ": no child Animator has both a RuntimeAnimatorController and valid Avatar.");

                LODGroup lodGroup = prefab.GetComponentInChildren<LODGroup>(true);
                if (lodGroup == null || lodGroup.GetLODs().Length < 3)
                    issues.Add(lead.DisplayName + ": at least three authored LOD levels are required.");

                SkinnedMeshRenderer[] skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                Mesh primaryMesh = null;
                int primaryVertexCount = -1;
                foreach (SkinnedMeshRenderer skin in skins)
                {
                    if (skin == null || skin.sharedMesh == null) continue;
                    if (skin.sharedMesh.vertexCount > primaryVertexCount)
                    {
                        primaryMesh = skin.sharedMesh;
                        primaryVertexCount = skin.sharedMesh.vertexCount;
                    }
                }
                if (primaryMesh == null)
                {
                    issues.Add(lead.DisplayName + ": no skinned character mesh was found.");
                }
                else
                {
                    string meshPath = AssetDatabase.GetAssetPath(primaryMesh);
                    if (string.IsNullOrWhiteSpace(meshPath))
                        issues.Add(lead.DisplayName + ": primary skinned mesh is not backed by a project asset.");
                    else if (!primaryMeshPaths.Add(meshPath))
                        issues.Add(lead.DisplayName + ": primary hero geometry reuses another lead's model asset " + meshPath + ". Each protagonist needs bespoke geometry.");
                }

                rows.Add(lead.DisplayName + ": FOUND; root components=" + (rootComponents ? "PASS" : "FAIL")
                    + "; valid animated Avatar=" + (validAnimator ? "PASS" : "FAIL")
                    + "; 3+ LODs=" + (lodGroup != null && lodGroup.GetLODs().Length >= 3 ? "PASS" : "FAIL")
                    + "; skinned meshes=" + skins.Length);
            }

            string status = issues.Count == 0 ? "PASS" : "BLOCKED";
            var report = new List<string>
            {
                "Project Tideline — Bespoke Playable Lead Prefab Audit",
                "Campaign: " + CampaignCanon.CampaignTitle,
                "Status: " + status,
                "",
                "Lead prefab checks:"
            };
            report.AddRange(rows);
            report.Add("");
            report.Add("Issues:");
            if (issues.Count == 0) report.Add("No issues found.");
            else foreach (string issue in issues) report.Add("BLOCKER: " + issue);
            report.Add("");
            report.Add("This audit checks asset contracts only. It does not prove animation playback, scene integration, visual quality, performance or gameplay.");

            File.WriteAllLines(ReportPath, report);
            AssetDatabase.Refresh();
            if (issues.Count == 0)
                Debug.Log("Bespoke playable lead prefab audit PASS. Report: " + ReportPath);
            else
                Debug.LogError("Bespoke playable lead prefab audit BLOCKED:\n" + string.Join("\n", issues));
            EditorUtility.DisplayDialog("Playable lead prefab audit — " + status,
                string.Join("\n", rows) + (issues.Count > 0 ? "\n\n" + string.Join("\n", issues) : ""), "OK");
        }
    }
}
#endif