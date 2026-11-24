using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Starts a configured RouteRaceController through the normal proximity-interaction path.</summary>
    [DisallowMultipleComponent]
    public sealed class RouteRaceStartInteractable : InteractableBase
    {
        [SerializeField] private RouteRaceController raceController;
        [SerializeField] private string startPrompt = "Start driving event";
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private string requiredMissionId;
        [SerializeField] private string requiredObjectiveId;

        public void Configure(RouteRaceController controller, MissionDirector director,
            string missionId, string objectiveId, string prompt = "Start customer delivery")
        {
            raceController = controller;
            missionDirector = director;
            requiredMissionId = missionId;
            requiredObjectiveId = objectiveId;
            startPrompt = prompt;
        }

        private bool IsCurrentObjective()
        {
            if (missionDirector == null || string.IsNullOrWhiteSpace(requiredMissionId)
                || string.IsNullOrWhiteSpace(requiredObjectiveId))
                return true;

            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            return mission != null && objective != null
                && string.Equals(mission.missionId, requiredMissionId, System.StringComparison.Ordinal)
                && string.Equals(objective.objectiveId, requiredObjectiveId, System.StringComparison.Ordinal);
        }

        public override string InteractionPrompt
        {
            get
            {
                if (raceController == null) return "Delivery route unavailable";
                if (raceController.HasFinished) return "Delivery route completed";
                if (raceController.IsActive) return "Delivery route in progress";
                if (!IsCurrentObjective()) return "Complete the previous objective first";
                return string.IsNullOrWhiteSpace(startPrompt) ? "Start driving event" : startPrompt;
            }
        }

        public override void Interact(GameObject actor)
        {
            if (actor == null || raceController == null || !IsCurrentObjective()
                || raceController.IsActive || raceController.HasFinished)
                return;

            raceController.BeginRace();
        }
    }
}
