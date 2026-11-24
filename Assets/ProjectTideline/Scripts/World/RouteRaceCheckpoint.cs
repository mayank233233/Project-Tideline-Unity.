using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Trigger for one ordered point on a RouteRaceController. Only a player-driven vehicle
    /// can advance the authored route; NPC traffic and on-foot actors are ignored.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class RouteRaceCheckpoint : MonoBehaviour
    {
        [SerializeField] private RouteRaceController controller;
        [SerializeField, Min(0)] private int checkpointIndex;

        public RouteRaceController Controller => controller;
        public int CheckpointIndex => checkpointIndex;
        public bool HasTriggerCollider
        {
            get
            {
                Collider trigger = GetComponent<Collider>();
                return trigger != null && trigger.isTrigger;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled || controller == null || other == null)
                return;

            VehicleDriveController vehicle = other.GetComponentInParent<VehicleDriveController>();
            if (vehicle != null)
                controller.TryRegisterCheckpoint(this, vehicle);
        }
    }
}
