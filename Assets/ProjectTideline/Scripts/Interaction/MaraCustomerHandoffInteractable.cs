using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// The customer accepts the vehicle only after the player has completed the route
    /// and stopped the delivery car in its marked bay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraCustomerHandoffInteractable : InteractableBase
    {
        [SerializeField] private MaraDeliveryHandoffController handoffController;

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
            if (actor == null || handoffController == null || !handoffController.CanCompleteHandoff)
                return;

            handoffController.TryCompleteHandoff(actor);
        }
    }
}
