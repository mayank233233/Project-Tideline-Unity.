using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Commits the customer's vehicle handoff after the assigned route has finished.
    /// The MissionDirector objective index restores this stage after a save/load.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraDeliveryHandoffController : MonoBehaviour
    {
        [SerializeField] private RouteRaceController deliveryRoute;
        [SerializeField] private MissionDirector missionDirector;

        private bool _handedOff;
        private bool _handoffPending;
        private bool _handoffAborted;

        public bool HasHandedOff => _handedOff || MissionProgressHasPassedHandoff;

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
            if (actor == null || deliveryRoute == null || !deliveryRoute.HasFinished
                || HasHandedOff || _handoffPending)
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
            Debug.LogWarning("Mara's customer handoff was rolled back because its signal batch aborted after " +
                dispatched + " events. Repeat the handoff after the signal loop is fixed.", this);
        }
    }
}
