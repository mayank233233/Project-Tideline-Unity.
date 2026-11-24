using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// The customer accepts the vehicle only after Mara has completed the authored delivery route.
    /// This is the actual delivery objective event, not merely arrival at the final checkpoint.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraCustomerHandoffInteractable : InteractableBase
    {
        [SerializeField] private MaraDeliveryHandoffController handoffController;

        public override string InteractionPrompt =>
            handoffController != null && handoffController.HasHandedOff
                ? "Vehicle handed over"
                : "Hand over the vehicle";

        public override void Interact(GameObject actor)
        {
            if (actor == null || handoffController == null)
                return;

            handoffController.TryCompleteHandoff(actor);
        }
    }
}
