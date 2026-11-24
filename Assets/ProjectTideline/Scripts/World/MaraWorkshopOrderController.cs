using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Tracks the three authored final checks on Mara's customer vehicle.
    /// Task progress is intentionally reset when the scene is recreated; the mission objective
    /// advances only when the entire order is complete, so partial task state is never mis-saved.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraWorkshopOrderController : MonoBehaviour
    {
        private static readonly string[] DefaultTaskIds =
        {
            "steering_check",
            "door_latch_check",
            "engine_rattle_check"
        };

        [SerializeField] private string[] requiredTaskIds =
        {
            "steering_check",
            "door_latch_check",
            "engine_rattle_check"
        };
        [SerializeField] private MissionDirector missionDirector;

        private readonly HashSet<string> _completedTaskIds = new HashSet<string>(StringComparer.Ordinal);
        private HashSet<string> _requiredTaskSet;
        private bool _configurationValid;

        public int CompletedTaskCount => _completedTaskIds.Count;
        public int RequiredTaskCount => _requiredTaskSet != null ? _requiredTaskSet.Count : 0;
        public bool IsUnlocked => MissionProgressHasPassedWorkOrder()
            || (NarrativeStateLedger.Instance != null
                && NarrativeStateLedger.Instance.HasFlag("mara_work_order_accepted")
                && NarrativeStateLedger.Instance.GetFlag("mara_work_order_accepted") == 1);

        public bool IsComplete => _configurationValid && IsUnlocked && RequiredTaskCount > 0
            && (CompletedTaskCount == RequiredTaskCount || MissionProgressHasPassedWorkOrder());

        private bool MissionProgressHasPassedWorkOrder()
        {
            MissionDefinition mission = missionDirector != null ? missionDirector.CurrentMission : null;
            return mission != null && mission.missionId == "two_minutes_to_close"
                && missionDirector.CurrentObjectiveIndex >= 1;
        }

        private string _pendingTaskId;
        private bool _pendingTaskAborted;

        private void Awake()
        {
            ValidateConfiguration();
            _completedTaskIds.Clear();
        }

        private void OnEnable()
        {
            MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;
        }

        private void OnDisable()
        {
            MissionSignalBus.DispatchAborted -= OnSignalDispatchAborted;
        }

        public bool IsTaskCompleted(string taskId)
        {
            return !string.IsNullOrWhiteSpace(taskId) && _completedTaskIds.Contains(taskId);
        }

        public bool TryCompleteTask(string taskId, GameObject actor)
        {
            if (!_configurationValid || !isActiveAndEnabled || actor == null || !IsUnlocked || IsComplete
                || string.IsNullOrWhiteSpace(taskId) || !_requiredTaskSet.Contains(taskId)
                || !_completedTaskIds.Add(taskId))
                return false;

            int completed = CompletedTaskCount;
            int required = RequiredTaskCount;
            _pendingTaskId = taskId;
            _pendingTaskAborted = false;

            if (completed == required)
            {
                // The last inspection both increments task telemetry and completes the order.
                // Enqueue both before dispatch so a mission cannot advance between same-action events.
                MissionSignalBus.PublishBatch(
                    new MissionSignalBus.Signal("mara.workshop.task.completed", 1),
                    new MissionSignalBus.Signal("mara.workshop.order.completed", 1));
            }
            else
            {
                MissionSignalBus.Publish("mara.workshop.task.completed", 1);
            }

            bool aborted = _pendingTaskAborted;
            _pendingTaskId = null;
            _pendingTaskAborted = false;
            return !aborted;
        }

        private void OnSignalDispatchAborted(int dispatched)
        {
            if (string.IsNullOrWhiteSpace(_pendingTaskId)) return;

            _completedTaskIds.Remove(_pendingTaskId);
            _pendingTaskAborted = true;
            Debug.LogWarning("Mara workshop task '" + _pendingTaskId +
                "' was rolled back because its mission-signal batch aborted. Repeat the inspection after the signal loop is fixed.", this);
        }

        private void ValidateConfiguration()
        {
            _requiredTaskSet = new HashSet<string>(StringComparer.Ordinal);
            _configurationValid = requiredTaskIds != null && requiredTaskIds.Length > 0;

            if (!_configurationValid)
            {
                Debug.LogError("Mara workshop order requires at least one task ID.", this);
                return;
            }

            foreach (string taskId in requiredTaskIds)
            {
                if (string.IsNullOrWhiteSpace(taskId) || !_requiredTaskSet.Add(taskId))
                {
                    _configurationValid = false;
                    Debug.LogError("Mara workshop order contains a blank or duplicate task ID.", this);
                    return;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (requiredTaskIds == null || requiredTaskIds.Length == 0)
                requiredTaskIds = (string[])DefaultTaskIds.Clone();
        }
#endif
    }
}
