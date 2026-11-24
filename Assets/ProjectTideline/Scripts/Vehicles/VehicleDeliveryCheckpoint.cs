using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Confirms the player's delivery vehicle has finished its route and stopped in the marked handoff bay.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class VehicleDeliveryCheckpoint : MonoBehaviour
    {
        public const string DeliveryArrivedSignal = "old_quay.delivery_arrived";

        [SerializeField] private VehicleDriveController deliveryVehicle;
        [SerializeField] private VehicleSeatInteractable driverSeat;
        [SerializeField] private RouteRaceController requiredRoute;
        [SerializeField, Min(0f)] private float maximumHandoffSpeedKph = 3f;

        private bool _arrived;
        private bool _arrivalPending;
        private bool _arrivalAborted;
        public bool HasArrived => _arrived;

        public void Configure(VehicleDriveController vehicle, VehicleSeatInteractable seat,
            RouteRaceController route = null)
        {
            deliveryVehicle = vehicle;
            driverSeat = seat;
            requiredRoute = route;
        }

        private void OnEnable()
        {
            MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;
        }

        private void OnDisable()
        {
            MissionSignalBus.DispatchAborted -= OnSignalDispatchAborted;
        }

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_arrived || deliveryVehicle == null || driverSeat == null || other == null) return;
            if (requiredRoute != null && !requiredRoute.HasFinished) return;

            Rigidbody candidateBody = other.attachedRigidbody;
            if (candidateBody == null) candidateBody = other.GetComponentInParent<Rigidbody>();
            if (candidateBody != deliveryVehicle.GetComponent<Rigidbody>()) return;
            if (!driverSeat.IsOccupied || deliveryVehicle.SpeedKph > maximumHandoffSpeedKph) return;

            _arrived = true;
            _arrivalPending = true;
            _arrivalAborted = false;
            MissionSignalBus.Publish("old_quay.delivery_arrived", 1);
            _arrivalPending = false;
            if (_arrivalAborted) _arrived = false;
        }

        private void OnSignalDispatchAborted(int dispatched)
        {
            if (!_arrivalPending) return;

            _arrivalAborted = true;
            _arrived = false;
            Debug.LogWarning("Delivery-bay arrival was rolled back because its signal batch aborted after " +
                dispatched + " events. Stop inside the bay again after the signal loop is fixed.", this);
        }
    }
}
