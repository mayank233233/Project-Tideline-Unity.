using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Converts completion of the configured customer-delivery route into Mara's mission-specific
    /// signal. RouteRaceController remains reusable and does not know about this prologue.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraDeliveryRouteCompletionRelay : MonoBehaviour
    {
        [SerializeField] private RouteRaceController routeController;

        private void OnEnable()
        {
            if (routeController != null)
                routeController.Finished += OnRouteFinished;
        }

        private void OnDisable()
        {
            if (routeController != null)
                routeController.Finished -= OnRouteFinished;
        }

        private void OnRouteFinished()
        {
            if (isActiveAndEnabled)
                MissionSignalBus.Publish("mara.delivery.finished", 1);
        }
    }
}
