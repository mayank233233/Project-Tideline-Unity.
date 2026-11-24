using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Commits the customer's vehicle handoff after the assigned route has finished
    /// and, when configured, the vehicle has stopped inside its delivery bay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraDeliveryHandoffController : MonoBehaviour
    {
        [SerializeField] private RouteRaceController deliveryRoute;
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private VehicleDeliveryCheckpoint deliveryBay;

        private bool _handedOff;
        private bool _handoffPending;
        private bool _handoffAborted;

        public bool HasHandedOff => _handedOff || MissionProgressHasPassedHandoff;
        public bool CanCompleteHandoff => deliveryRoute != null && deliveryRoute.HasFinished
            && (deliveryBay == null || deliveryBay.HasArrived) && !HasHandedOff && !_handoffPending;

        public void Configure(RouteRaceController route, MissionDirector director, VehicleDeliveryCheckpoint bay = null)
        {
            deliveryRoute = route;
            missionDirector = director;
            deliveryBay = bay;
        }

        private bool MissionProgressHasPassedHandoff
        {
            get
            {
                MissionDefinition mission = missionDirector != null ? missionDirector.CurrentMission : null;
                return mission != null && mission.missionId == "two_minutes_to_close"
                    && missionDirector.CurrentObjectiveIndex >= 2;
            }
        }

        private void OnEnable()
        {
            MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;
        }

        private void OnDisable()
        {
            MissionSignalBus.DispatchAborted -= OnSignalDispatchAborted;
        }

        public bool TryCompleteHandoff(GameObject actor)
        {
            if (actor == null || !CanCompleteHandoff)
                return false;

            _handoffPending = true;
            _handoffAborted = false;
            _handedOff = true;

            MissionSignalBus.Publish("mara.delivery.finished", 1);

            bool aborted = _handoffAborted;
            _handoffPending = false;
            if (aborted) _handedOff = false;
            return !aborted;
        }

        private void OnSignalDispatchAborted(int dispatched)
        {
            if (!_handoffPending) return;

            _handoffAborted = true;
            _handedOff = false;
            Debug.LogWarning("The customer handoff was rolled back because its signal batch aborted after " +
                dispatched + " events. Repeat the handoff after the signal loop is fixed.", this);
        }
    }
}
