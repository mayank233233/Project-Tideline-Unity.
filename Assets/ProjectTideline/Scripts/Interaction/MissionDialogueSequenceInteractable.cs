using System;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Routes one NPC's dialogue to the conversation authored for the active mission objective.</summary>
    [DisallowMultipleComponent]
    public sealed class MissionDialogueSequenceInteractable : InteractableBase
    {
        [Serializable]
        public sealed class DialogueStep
        {
            public string objectiveId;
            public DialogueGraph graph;
            public string prompt = "Talk";
        }

        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private string missionId;
        [SerializeField] private DialogueStep[] steps;

        public void Configure(MissionDirector director, DialogueController controller,
            string configuredMissionId, DialogueStep[] dialogueSteps)
        {
            missionDirector = director;
            dialogueController = controller;
            missionId = configuredMissionId;
            steps = dialogueSteps;
        }

        private DialogueStep GetActiveStep()
        {
            if (missionDirector == null || dialogueController == null
                || string.IsNullOrWhiteSpace(missionId) || steps == null)
                return null;

            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            if (mission == null || objective == null
                || !string.Equals(mission.missionId, missionId, StringComparison.Ordinal))
                return null;

            for (int i = 0; i < steps.Length; i++)
            {
                DialogueStep step = steps[i];
                if (step != null && step.graph != null
                    && string.Equals(step.objectiveId, objective.objectiveId, StringComparison.Ordinal))
                    return step;
            }
            return null;
        }

        public override string InteractionPrompt
        {
            get
            {
                DialogueStep step = GetActiveStep();
                return step != null && !string.IsNullOrWhiteSpace(step.prompt)
                    ? step.prompt : "No new conversation";
            }
        }

        public override void Interact(GameObject actor)
        {
            if (actor == null) return;
            DialogueStep step = GetActiveStep();
            if (step != null) dialogueController.Begin(step.graph);
        }
    }
}
