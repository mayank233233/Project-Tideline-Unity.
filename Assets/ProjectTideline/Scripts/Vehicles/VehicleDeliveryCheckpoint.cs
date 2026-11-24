using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Confirms the player's delivery car has reached and stopped in the marked handoff bay.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class VehicleDeliveryCheckpoint : MonoBehaviour
    {
        public const string DeliveryArrivedSignal = "old_quay.delivery_arrived";

        [SerializeField] private VehicleDriveController deliveryVehicle;
        [SerializeField] private VehicleSeatInteractable driverSeat;
        [SerializeField, Min(0f)] private float maximumHandoffSpeedKph = 3f;

        private bool _arrived;
        public bool HasArrived => _arrived;

        public void Configure(VehicleDriveController vehicle, VehicleSeatInteractable seat)
        {
            deliveryVehicle = vehicle;
            driverSeat = seat;
        }

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_arrived || deliveryVehicle == null || driverSeat == null || other == null) return;

            Rigidbody candidateBody = other.attachedRigidbody;
            if (candidateBody == null) candidateBody = other.GetComponentInParent<Rigidbody>();
            if (candidateBody != deliveryVehicle.GetComponent<Rigidbody>()) return;
            if (!driverSeat.IsOccupied || deliveryVehicle.SpeedKph > maximumHandoffSpeedKph) return;

            _arrived = true;
            MissionSignalBus.Publish("old_quay.delivery_arrived", 1);
        }
    }
}
