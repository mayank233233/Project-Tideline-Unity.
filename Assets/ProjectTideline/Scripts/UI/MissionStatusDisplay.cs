using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>Compact mission/objective HUD driven by MissionDirector events.</summary>
    [DisallowMultipleComponent]
    public sealed class MissionStatusDisplay : MonoBehaviour
    {
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private Text statusText;

        public void Configure(MissionDirector director, Text text)
        {
            Unsubscribe();
            missionDirector = director;
            statusText = text;
            if (isActiveAndEnabled) Subscribe();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (missionDirector == null) return;
            missionDirector.ProgressChanged -= OnProgressChanged;
            missionDirector.MissionCompleted -= OnMissionCompleted;
            missionDirector.ProgressChanged += OnProgressChanged;
            missionDirector.MissionCompleted += OnMissionCompleted;
        }

        private void Unsubscribe()
        {
            if (missionDirector == null) return;
            missionDirector.ProgressChanged -= OnProgressChanged;
            missionDirector.MissionCompleted -= OnMissionCompleted;
        }

        private void OnProgressChanged(MissionDefinition mission, MissionObjective objective,
            int current, int required)
        {
            if (statusText == null) return;
            if (mission == null)
            {
                statusText.text = "PORT SABLE  •  Free roam";
                return;
            }

            if (objective == null)
            {
                statusText.text = mission.title + "\nCOMPLETE";
                return;
            }

            statusText.text = mission.title + "\n" + objective.description
                + (required > 1 ? "\n" + current + " / " + required : string.Empty);
        }

        private void OnMissionCompleted(MissionDefinition mission) => Refresh();

        private void Refresh()
        {
            if (missionDirector == null || statusText == null) return;
            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            if (mission == null)
            {
                statusText.text = "PORT SABLE  •  Free roam";
                return;
            }
            statusText.text = mission.title + (objective != null ? "\n" + objective.description : "\nCOMPLETE");
        }
    }
}
