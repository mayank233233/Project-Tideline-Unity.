using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Opens the customer's authored acceptance conversation only after the assigned driver
    /// has completed the route, secured the delivery bay and exited the vehicle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraCustomerHandoffInteractable : InteractableBase
    {
        [SerializeField] private MaraDeliveryHandoffController handoffController;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private DialogueGraph dialogueGraph;

        public override string InteractionPrompt
        {
            get
            {
                if (handoffController == null) return "Customer unavailable";
                if (handoffController.HasHandedOff) return "Vehicle handed over";
                return handoffController.CanCompleteHandoff
                    ? "Hand over the vehicle"
                    : "Complete the route and park in the delivery bay";
            }
        }

        public override void Interact(GameObject actor)
        {
            if (actor == null || handoffController == null
                || !handoffController.CanCompleteHandoffFor(actor))
                return;

            // The authored dialogue choice publishes the handoff signal. Keep the old
            // transactional path as a fallback for scenes without a dialogue assignment.
            if (dialogueController != null && dialogueGraph != null)
            {
                dialogueController.Begin(dialogueGraph);
                return;
            }

            handoffController.TryCompleteHandoff(actor);
        }
    }
}