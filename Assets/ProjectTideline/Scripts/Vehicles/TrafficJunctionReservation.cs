using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Conservative single-vehicle reservation for a compact blockout junction.
    /// Only one NPC traffic car may claim the junction at a time; the reservation
    /// is released when that car leaves the trigger volume.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrafficJunctionReservation : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float reservationTimeout = 4f;

        private TrafficVehicleDriver _reservedBy;
        private float _reservationExpiresAt;

        public bool TryReserve(TrafficVehicleDriver vehicle)
        {
            if (vehicle == null) return false;

            if (_reservedBy == null || Time.time >= _reservationExpiresAt)
            {
                _reservedBy = vehicle;
                _reservationExpiresAt = Time.time + Mathf.Max(0.5f, reservationTimeout);
                return true;
            }

            if (_reservedBy != vehicle) return false;
            _reservationExpiresAt = Time.time + Mathf.Max(0.5f, reservationTimeout);
            return true;
        }

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TrafficVehicleDriver vehicle = other.GetComponentInParent<TrafficVehicleDriver>();
            if (vehicle == null) return;

            if (_reservedBy == null || Time.time >= _reservationExpiresAt)
            {
                _reservedBy = vehicle;
                _reservationExpiresAt = Time.time + Mathf.Max(0.5f, reservationTimeout);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            TrafficVehicleDriver vehicle = other.GetComponentInParent<TrafficVehicleDriver>();
            if (vehicle != null && vehicle == _reservedBy) _reservedBy = null;
        }

        private void LateUpdate()
        {
            if (_reservedBy == null || Time.time < _reservationExpiresAt) return;
            _reservedBy = null;
        }

        private void OnDisable()
        {
            _reservedBy = null;
        }
    }
}