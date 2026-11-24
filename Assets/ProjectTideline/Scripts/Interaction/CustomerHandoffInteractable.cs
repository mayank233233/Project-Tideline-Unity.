using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Customer accepts the delivery only after the car is parked and the player exits it.</summary>
    [DisallowMultipleComponent]
    public sealed class CustomerHandoffInteractable : InteractableBase
    {
        public const string HandoffAcceptedSignal = "old_quay.customer_handoff_accepted";

        [SerializeField] private VehicleDeliveryCheckpoint deliveryCheckpoint;
        [SerializeField] private VehicleSeatInteractable deliverySeat;
        [SerializeField] private string prompt = "Hand over the keys";

        private bool _completed;

        public override string InteractionPrompt
        {
            get
            {
                if (_completed) return "Delivery complete";
                if (deliveryCheckpoint == null || !deliveryCheckpoint.HasArrived)
                    return "Park the delivery car in the marked bay";
                if (deliverySeat != null && deliverySeat.IsOccupied)
                    return "Exit the delivery car first";
                return prompt;
            }
        }

        public void Configure(VehicleDeliveryCheckpoint checkpoint, VehicleSeatInteractable seat)
        {
            deliveryCheckpoint = checkpoint;
            deliverySeat = seat;
        }

        public override void Interact(GameObject actor)
        {
            if (_completed || actor == null || deliveryCheckpoint == null
                || !deliveryCheckpoint.HasArrived
                || (deliverySeat != null && deliverySeat.IsOccupiedBy(actor)))
                return;

            _completed = true;
            MissionSignalBus.Publish("old_quay.customer_handoff_accepted", 1);
        }
    }
}
