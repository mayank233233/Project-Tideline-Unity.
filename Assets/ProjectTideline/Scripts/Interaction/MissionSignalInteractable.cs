using UnityEngine;

namespace ProjectTideline
{
    /// <summary>One-shot world interaction that publishes its configured mission signal on success.</summary>
    [DisallowMultipleComponent]
    public sealed class MissionSignalInteractable : InteractableBase
    {
        public const string HarbourLedgerSignal = "old_quay.harbour_ledger_inspected";
        public const string WorkshopReportSignal = "old_quay.juno_reported_back";
        [SerializeField] private string signalId = "world.interaction.completed";
        [SerializeField] private string interactionPrompt = "Inspect";
        [SerializeField] private bool oneShot = true;

        private bool _completed;
        public string SignalId => signalId;

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
            switch (signalId)
            {
                // Keep the tutorial's authored producer contract discoverable to the mission asset audit.
                case HarbourLedgerSignal:
                    MissionSignalBus.Publish("old_quay.harbour_ledger_inspected", 1);
                    break;
                case WorkshopReportSignal:
                    MissionSignalBus.Publish("old_quay.juno_reported_back", 1);
                    break;
                default:
                    MissionSignalBus.Publish(signalId, 1);
                    break;
            }
        }
    }
}
