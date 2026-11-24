using UnityEngine;

namespace ProjectTideline
{
    /// <summary>One-shot world interaction that publishes its configured mission signal on success.</summary>
    [DisallowMultipleComponent]
    public sealed class MissionSignalInteractable : InteractableBase
    {
        [SerializeField] private string signalId = "world.interaction.completed";
        [SerializeField] private string interactionPrompt = "Inspect";
        [SerializeField] private bool oneShot = true;

        private bool _completed;

        public override string InteractionPrompt =>
            _completed ? "Already inspected"
                : string.IsNullOrWhiteSpace(interactionPrompt) ? "Inspect" : interactionPrompt;

        public void Configure(string configuredSignalId, string prompt, bool isOneShot = true)
        {
            signalId = configuredSignalId;
            interactionPrompt = prompt;
            oneShot = isOneShot;
        }

        public override void Interact(GameObject actor)
        {
            if (actor == null || string.IsNullOrWhiteSpace(signalId) || (oneShot && _completed))
                return;

            if (oneShot) _completed = true;
            MissionSignalBus.Publish(signalId, 1);
        }
    }
}
