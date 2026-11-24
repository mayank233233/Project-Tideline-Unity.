#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using ProjectTideline;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// GitHub Actions entry point: generate Old Quay, verify the loaded scene wiring,
    /// then build the configured standalone target. A successful build is not a Play Mode test.
    /// </summary>
    public static class PortSableCiBuild
    {
        private const string ScenePath = "Assets/ProjectTideline/Scenes/PortSable_OldQuay_Blockout.unity";
        private const string OutputDirectory = "Builds/CI";

        public static void BuildPlayer()
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("PortSableCiBuild must run in Unity batch mode.");

            PortSableBlockoutSceneBuilder.Build();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new InvalidOperationException("Old Quay scene generation did not create " + ScenePath + ".");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded || SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Generated Old Quay scene could not be loaded.");

            RequireComponent<ThirdPersonMotor>("player motor");
            RequireComponent<PlayerInputRouter>("player input router");
            RequireComponent<ProximityInteractor>("proximity interaction");
            RequireComponent<MissionDirector>("mission director");
            RequireComponent<DialogueController>("dialogue controller");
            RequireComponent<PoliceResponseDirector>("police response director");
            RequireComponent<TrafficSignalController>("traffic signal controller");
            RequireComponent<AmbientPedestrianWalker>("ambient pedestrian walker");
            RequireComponent<AmbientPedestrianAnimation>("procedural pedestrian animation");
            RequireComponent<NarrativeStateLedger>("narrative state ledger");
            RequireComponent<NPCRelationshipLedger>("NPC relationship ledger");

            bool sceneInBuild = EditorBuildSettings.scenes.Any(entry =>
                entry.enabled && string.Equals(entry.path, ScenePath, StringComparison.Ordinal));
            if (!sceneInBuild)
                throw new InvalidOperationException("Generated Old Quay scene is not enabled in Build Settings.");

            string[] scenes = EditorBuildSettings.scenes
                .Where(entry => entry.enabled)
                .Select(entry => entry.path)
                .ToArray();
            if (scenes.Length == 0)
                throw new InvalidOperationException("Build Settings contains no enabled scenes.");

            Directory.CreateDirectory(OutputDirectory);
            string executableName = EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64
                ? "PORT_SABLE.exe"
                : "PORT_SABLE";
            string outputPath = Path.Combine(OutputDirectory, executableName);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.StrictMode
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "PORT SABLE Unity build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors +
                    "; warnings=" + report.summary.totalWarnings + ".");
            }

            Debug.Log(
                "PORT SABLE UNITY CI PASS: generated and loaded Old Quay; required scene systems present; " +
                "standalone build succeeded. Target=" + report.summary.platform +
                "; output=" + outputPath +
                "; size=" + report.summary.totalSize + " bytes. Play Mode gameplay tests are not implied.");
        }

        private static void RequireComponent<T>(string label) where T : Component
        {
            T[] matches = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            if (matches == null || matches.Length == 0)
                throw new InvalidOperationException("Generated Old Quay scene is missing required " + label + ".");
        }
    }
}
#endif
