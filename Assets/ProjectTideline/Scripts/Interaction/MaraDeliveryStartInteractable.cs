using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Starts Mara's customer delivery only after all configured workshop checks are complete.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraDeliveryStartInteractable : InteractableBase
    {
        [SerializeField] private MaraWorkshopOrderController workshopOrder;
        [SerializeField] private RouteRaceController deliveryRoute;

        public override string InteractionPrompt
        {
            get
            {
                if (workshopOrder == null || !workshopOrder.IsComplete)
                    return "Finish the three workshop checks";
                if (deliveryRoute == null)
                    return "Delivery route unavailable";
                if (deliveryRoute.HasFinished)
                    return "Delivery completed";
                if (deliveryRoute.IsActive)
                    return "Delivery route in progress";
                return "Start customer delivery";
            }
        }

        public override void Interact(GameObject actor)
        {
            if (actor == null || workshopOrder == null || !workshopOrder.IsComplete
                || deliveryRoute == null || deliveryRoute.IsActive || deliveryRoute.HasFinished)
                return;

            deliveryRoute.BeginRace();
        }
    }
}
