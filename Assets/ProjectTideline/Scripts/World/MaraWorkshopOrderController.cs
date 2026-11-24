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

        private readonly HashSet<string> _completedTaskIds = new HashSet<string>(StringComparer.Ordinal);
        private HashSet<string> _requiredTaskSet;
        private bool _configurationValid;

        public int CompletedTaskCount => _completedTaskIds.Count;
        public int RequiredTaskCount => _requiredTaskSet != null ? _requiredTaskSet.Count : 0;
        public bool IsComplete => _configurationValid && RequiredTaskCount > 0
            && CompletedTaskCount == RequiredTaskCount;

        public event Action<int, int> ProgressChanged;

        private void Awake()
        {
            ValidateConfiguration();
            _completedTaskIds.Clear();
        }

        public bool IsTaskCompleted(string taskId)
        {
            return !string.IsNullOrWhiteSpace(taskId) && _completedTaskIds.Contains(taskId);
        }

        public bool TryCompleteTask(string taskId, GameObject actor)
        {
            if (!_configurationValid || !isActiveAndEnabled || actor == null || IsComplete
                || string.IsNullOrWhiteSpace(taskId) || !_requiredTaskSet.Contains(taskId)
                || !_completedTaskIds.Add(taskId))
                return false;

            int completed = CompletedTaskCount;
            int required = RequiredTaskCount;
            ProgressChanged?.Invoke(completed, required);

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

            return true;
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
