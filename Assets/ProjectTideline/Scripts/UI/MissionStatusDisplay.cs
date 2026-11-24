using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>Mission, delivery-route and vehicle-speed HUD driven by gameplay state.</summary>
    [DisallowMultipleComponent]
    public sealed class MissionStatusDisplay : MonoBehaviour
    {
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private RouteRaceController deliveryRoute;
        [SerializeField] private VehicleDriveController deliveryVehicle;
        [SerializeField] private VehicleDeliveryCheckpoint deliveryBay;
        [SerializeField] private GameObject vehicleStatusRoot;
        [SerializeField] private Text vehicleSpeedText;
        [SerializeField] private Text statusText;

        public void Configure(MissionDirector director, Text text)
        {
            Unsubscribe();
            missionDirector = director;
            statusText = text;
            if (isActiveAndEnabled) Subscribe();
            Refresh();
        }

        public void SetDeliveryRoute(RouteRaceController route)
        {
            if (deliveryRoute == route) return;
            Unsubscribe();
            deliveryRoute = route;
            if (isActiveAndEnabled) Subscribe();
            Refresh();
        }

        public void SetDeliveryBay(VehicleDeliveryCheckpoint bay)
        {
            deliveryBay = bay;
            RefreshVehicleStatus();
        }

        public void SetVehicleStatus(VehicleDriveController vehicle, GameObject panelRoot, Text speedText)
        {
            deliveryVehicle = vehicle;
            vehicleStatusRoot = panelRoot;
            vehicleSpeedText = speedText;
            if (vehicleStatusRoot != null)
                vehicleStatusRoot.SetActive(false);
            RefreshVehicleStatus();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
            RefreshVehicleStatus();
        }

        private void Update() => RefreshVehicleStatus();

        private void OnDisable()
        {
            Unsubscribe();
            if (vehicleStatusRoot != null) vehicleStatusRoot.SetActive(false);
        }

        private void Subscribe()
        {
            if (missionDirector != null)
            {
                missionDirector.ProgressChanged -= OnProgressChanged;
                missionDirector.MissionCompleted -= OnMissionCompleted;
                missionDirector.ProgressChanged += OnProgressChanged;
                missionDirector.MissionCompleted += OnMissionCompleted;
            }

            if (deliveryRoute != null)
            {
                deliveryRoute.ProgressChanged -= OnRouteProgressChanged;
                deliveryRoute.Finished -= OnRouteFinished;
                deliveryRoute.ProgressChanged += OnRouteProgressChanged;
                deliveryRoute.Finished += OnRouteFinished;
            }
        }

        private void Unsubscribe()
        {
            if (missionDirector != null)
            {
                missionDirector.ProgressChanged -= OnProgressChanged;
                missionDirector.MissionCompleted -= OnMissionCompleted;
            }

            if (deliveryRoute != null)
            {
                deliveryRoute.ProgressChanged -= OnRouteProgressChanged;
                deliveryRoute.Finished -= OnRouteFinished;
            }
        }

        private void RefreshVehicleStatus()
        {
            if (vehicleStatusRoot == null || vehicleSpeedText == null) return;
            bool driving = deliveryVehicle != null
                && PlayerInputRouter.Instance != null
                && PlayerInputRouter.Instance.VehicleControlMode;
            if (vehicleStatusRoot.activeSelf != driving)
                vehicleStatusRoot.SetActive(driving);
            if (!driving) return;

            float speed = deliveryVehicle.SpeedKph;
            MissionObjective objective = missionDirector != null ? missionDirector.CurrentObjective : null;
            bool parkingObjective = objective != null && objective.objectiveId == "park_in_delivery_bay";
            bool deliverySecured = deliveryBay != null && deliveryBay.HasArrived;
            string instruction = deliverySecured
                ? "DELIVERY BAY SECURED"
                : parkingObjective
                    ? speed <= 3f ? "PARKING SPEED OK" : "BRAKE TO 3 KM/H"
                    : "DELIVERY SPEED";
            string guidance = parkingObjective
                ? GetDeliveryBayHint()
                : deliverySecured ? "EXIT VEHICLE TO HAND OVER" : string.Empty;
            vehicleSpeedText.text = Mathf.RoundToInt(speed) + " KM/H\n" + instruction
                + (string.IsNullOrEmpty(guidance) ? string.Empty : "\n" + guidance);
            vehicleSpeedText.color = deliverySecured || (parkingObjective && speed <= 3f)
                ? new Color(0.70f, 0.91f, 0.72f) : Color.white;
        }

        private string GetDeliveryBayHint()
        {
            if (deliveryBay == null || deliveryVehicle == null)
                return "FOLLOW ROAD TO DELIVERY BAY";

            Vector3 offset = deliveryBay.transform.position - deliveryVehicle.transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance <= 3.5f)
                return "BAY CLOSE — STOP INSIDE MARKINGS";

            Vector3 localTarget = deliveryVehicle.transform.InverseTransformPoint(deliveryBay.transform.position);
            string direction = localTarget.x < -1.75f ? "LEFT"
                : localTarget.x > 1.75f ? "RIGHT" : "AHEAD";
            return "BAY " + Mathf.RoundToInt(distance) + " M — " + direction;
        }

        private void OnProgressChanged(MissionDefinition mission, MissionObjective objective,
            int current, int required)
        {
            if (statusText == null) return;
            if (mission == null)
            {
                statusText.text = "PORT SABLE  •  Free roam";
                return;
            }

            if (objective == null)
            {
                statusText.text = mission.title + "\nCOMPLETE";
                return;
            }

            statusText.text = mission.title + "\n" + objective.description
                + (required > 1 ? "\n" + current + " / " + required : string.Empty);

            if (deliveryRoute != null && deliveryRoute.IsActive)
                AppendRouteProgress();
        }

        private void OnRouteProgressChanged(int completed, int total)
        {
            if (statusText == null || missionDirector == null || total <= 0) return;
            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            if (mission == null || objective == null) return;

            statusText.text = mission.title + "\n" + objective.description
                + "\nROUTE CHECKPOINTS  " + completed + " / " + total
                + "\n" + GetNextCheckpointHint();
        }

        private string GetNextCheckpointHint()
        {
            if (deliveryRoute == null || deliveryRoute.HasFinished)
                return "ROUTE COMPLETE";
            Transform next = deliveryRoute.NextCheckpoint;
            VehicleDriveController vehicle = deliveryRoute.PlayerVehicle;
            if (next == null || vehicle == null)
                return "NEXT GATE — AHEAD";

            Vector3 localTarget = vehicle.transform.InverseTransformPoint(next.position);
            if (localTarget.x < -1.25f) return "NEXT GATE — LEFT";
            if (localTarget.x > 1.25f) return "NEXT GATE — RIGHT";
            return "NEXT GATE — AHEAD";
        }

        private void OnRouteFinished()
        {
            Refresh();
            if (statusText != null && deliveryRoute != null && deliveryRoute.HasFinished)
                statusText.text += "\nROUTE COMPLETE — stop in the marked bay";
        }

        private void OnMissionCompleted(MissionDefinition mission) => Refresh();

        private void AppendRouteProgress()
        {
            if (statusText == null || deliveryRoute == null || deliveryRoute.CheckpointCount <= 0) return;
            statusText.text += "\nROUTE CHECKPOINTS  "
                + deliveryRoute.NextCheckpointIndex + " / " + deliveryRoute.CheckpointCount
                + "\n" + GetNextCheckpointHint();
        }

        private void Refresh()
        {
            if (missionDirector == null || statusText == null) return;
            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            if (mission == null)
            {
                statusText.text = "PORT SABLE  •  Free roam";
                return;
            }
            statusText.text = mission.title + (objective != null ? "\n" + objective.description : "\nCOMPLETE");
            if (deliveryRoute != null && deliveryRoute.IsActive)
                AppendRouteProgress();
        }
    }
}
