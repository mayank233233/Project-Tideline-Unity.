using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Completes an authored vehicle-arrival objective when the designated occupied player
    /// vehicle enters a trigger. It is a mission gate, not a real-world driving instruction.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class MissionVehicleArrivalZone : MonoBehaviour
    {
        [SerializeField] private VehicleDriveController requiredVehicle;
        [SerializeField] private VehicleSeatInteractable requiredSeat;
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private string requiredMissionId;
        [SerializeField] private string requiredObjectiveId;
        [SerializeField] private string completionSignal = "mission.vehicle_arrived";

        private bool _completed;

        public void Configure(VehicleDriveController vehicle, VehicleSeatInteractable seat,
            MissionDirector director, string missionId, string objectiveId, string signal)
        {
            requiredVehicle = vehicle;
            requiredSeat = seat;
            missionDirector = director;
            requiredMissionId = missionId;
            requiredObjectiveId = objectiveId;
            completionSignal = signal;
            Collider trigger = GetComponent<Collider>();
            if (trigger != null) trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_completed || requiredVehicle == null || requiredSeat == null
                || missionDirector == null || other == null)
                return;

            Rigidbody enteredBody = other.attachedRigidbody;
            if (enteredBody == null || enteredBody != requiredVehicle.GetComponent<Rigidbody>()
                || !requiredVehicle.IsDriven || !requiredSeat.IsOccupied)
                return;

            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            if (mission == null || objective == null
                || !string.Equals(mission.missionId, requiredMissionId, System.StringComparison.Ordinal)
                || !string.Equals(objective.objectiveId, requiredObjectiveId, System.StringComparison.Ordinal))
                return;

            _completed = true;
            MissionSignalBus.Publish(completionSignal, 1);
        }
    }
}