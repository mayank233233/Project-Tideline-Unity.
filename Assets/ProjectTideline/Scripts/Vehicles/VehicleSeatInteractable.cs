using System.Collections;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Vehicle entry/exit plus an in-game, animation-driven handoff of an NPC-occupied vehicle.
    /// This is a fictional gameplay interaction and contains no real-world vehicle access procedure.
    /// </summary>
    public sealed class VehicleSeatInteractable : InteractableBase
    {
        [SerializeField] private VehicleDriveController vehicle;
        [SerializeField] private TrafficVehicleDriver trafficDriver;
        [SerializeField] private Transform driverSeat;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private ThirdPersonCameraRig cameraRig;
        [Header("Mission entry gate")]
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private string requiredMissionId;
        [SerializeField] private string requiredObjectiveId;
        [Header("NPC vehicle state")]
        [SerializeField] private bool startsWithNpcDriver;
        [SerializeField] private GameObject npcDriverVisual;
        [SerializeField] private Animator npcDriverAnimator;
        [SerializeField] private CharacterAnimationDriver npcDriverAnimationDriver;
        [SerializeField] private Transform npcExitPoint;
        [SerializeField] private string npcYieldTrigger = "VehicleYield";
        [SerializeField, Min(0.1f)] private float takeoverSequenceSeconds = 0.85f;

        private GameObject _occupant;
        private CharacterController _characterController;
        private ThirdPersonMotor _motor;
        private bool _npcOccupied;
        private bool _transitioning;
        private ThirdPersonMotor _takeoverMotor;

        public override string InteractionPrompt
        {
            get
            {
                if (!IsCurrentMissionObjective()) return "Complete the previous delivery step first";
                return _npcOccupied ? "Take vehicle" : "Enter vehicle";
            }
        }

        public bool IsTransitioning => _transitioning;
        public bool IsOccupied => _occupant != null;
        public bool IsOccupiedBy(GameObject actor) => actor != null && _occupant == actor;

        public void ConfigureMissionGate(MissionDirector director, string missionId, string objectiveId)
        {
            missionDirector = director;
            requiredMissionId = missionId;
            requiredObjectiveId = objectiveId;
        }

        private bool IsCurrentMissionObjective()
        {
            if (missionDirector == null || string.IsNullOrWhiteSpace(requiredMissionId)
                || string.IsNullOrWhiteSpace(requiredObjectiveId))
                return true;

            MissionDefinition mission = missionDirector.CurrentMission;
            MissionObjective objective = missionDirector.CurrentObjective;
            return mission != null && objective != null
                && string.Equals(mission.missionId, requiredMissionId, System.StringComparison.Ordinal)
                && string.Equals(objective.objectiveId, requiredObjectiveId, System.StringComparison.Ordinal);
        }

        private void Awake()
        {
            _npcOccupied = startsWithNpcDriver;
            if (trafficDriver == null) trafficDriver = GetComponent<TrafficVehicleDriver>();
        }

        public override void Interact(GameObject actor)
        {
            if (!IsCurrentMissionObjective() || actor == null || _occupant != null || _transitioning
                || vehicle == null || driverSeat == null
                || actor.GetComponent<CharacterController>() == null
                || actor.GetComponent<ThirdPersonMotor>() == null) return;
            if (_npcOccupied)
            {
                StartCoroutine(TakeoverSequence(actor));
                return;
            }
            TryEnter(actor);
        }

        public bool TryEnter(GameObject actor)
        {
            if (!IsCurrentMissionObjective() || _npcOccupied || _transitioning) return false;
            return EnterVehicle(actor);
        }

        private IEnumerator TakeoverSequence(GameObject actor)
        {
            _transitioning = true;
            _takeoverMotor = actor.GetComponent<ThirdPersonMotor>();
            if (_takeoverMotor != null) _takeoverMotor.enabled = false;
            if (trafficDriver != null) trafficDriver.BeginTakeoverHandoff();

            if (!string.IsNullOrEmpty(npcYieldTrigger))
            {
                if (npcDriverAnimationDriver != null) npcDriverAnimationDriver.Trigger(npcYieldTrigger);
                else if (npcDriverAnimator != null) npcDriverAnimator.SetTrigger(npcYieldTrigger);
            }

            yield return new WaitForSeconds(takeoverSequenceSeconds);

            if (actor == null || !isActiveAndEnabled || !IsCurrentMissionObjective())
            {
                if (_takeoverMotor != null) _takeoverMotor.enabled = true;
                if (trafficDriver != null && trafficDriver.enabled) trafficDriver.CancelTakeoverHandoff();
                _takeoverMotor = null;
                _transitioning = false;
                yield break;
            }

            if (npcDriverVisual != null)
            {
                npcDriverVisual.transform.SetParent(null, true);
                if (npcExitPoint != null)
                    npcDriverVisual.transform.SetPositionAndRotation(npcExitPoint.position, npcExitPoint.rotation);
            }
            _npcOccupied = false;
            _transitioning = false;

            if (!EnterVehicle(actor, true) && _takeoverMotor != null)
                _takeoverMotor.enabled = true;
            _takeoverMotor = null;
        }

        private bool EnterVehicle(GameObject actor, bool wasNpcTakeover = false)
        {
            if (!IsCurrentMissionObjective() || _occupant != null || actor == null
                || driverSeat == null || vehicle == null) return false;
            CharacterController controller = actor.GetComponent<CharacterController>();
            ThirdPersonMotor motor = actor.GetComponent<ThirdPersonMotor>();
            if (controller == null || motor == null) return false;

            if (cameraRig == null) cameraRig = ThirdPersonCameraRig.Instance;
            if (cameraRig == null) cameraRig = actor.GetComponentInChildren<ThirdPersonCameraRig>();

            _occupant = actor;
            _characterController = controller;
            _motor = motor;
            _characterController.enabled = false;
            _motor.enabled = false;
            actor.transform.SetParent(driverSeat, false);
            actor.transform.localPosition = Vector3.zero;
            actor.transform.localRotation = Quaternion.identity;
            vehicle.SetDriven(true);
            if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.SetVehicleControlMode(true);
            if (trafficDriver != null) trafficDriver.enabled = false;
            if (cameraRig != null) cameraRig.SetTarget(transform, new Vector3(0f, 2f, 0f));
            if (wasNpcTakeover)
            {
                MissionSignalBus.PublishBatch(
                    new MissionSignalBus.Signal("vehicle.entered", 1),
                    new MissionSignalBus.Signal("vehicle.taken", 1));
            }
            else
            {
                MissionSignalBus.Publish("vehicle.entered", 1);
            }
            return true;
        }

        public void Exit(GameObject actor)
        {
            if (_occupant == null || actor != _occupant || vehicle == null) return;
            vehicle.SetDriven(false);
            if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.SetVehicleControlMode(false);
            actor.transform.SetParent(null, true);
            if (exitPoint != null) actor.transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
            else actor.transform.position += transform.right * 1.8f + Vector3.up * 0.05f;
            if (_characterController != null) _characterController.enabled = true;
            if (_motor != null) _motor.enabled = true;
            if (cameraRig != null) cameraRig.SetTarget(actor.transform, new Vector3(0f, 1.45f, 0f), true);
            _occupant = null;
            _characterController = null;
            _motor = null;
            MissionSignalBus.Publish("vehicle.exited", 1);
        }

        private void OnDisable()
        {
            if (_occupant != null)
            {
                Exit(_occupant);
                return;
            }
            if (_transitioning)
            {
                if (_takeoverMotor != null) _takeoverMotor.enabled = true;
                _takeoverMotor = null;
                _transitioning = false;
            }
        }
    }
}
