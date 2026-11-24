using System;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Owns one authored driving route. A race completes only after the player-driven vehicle
    /// crosses every assigned trigger in order. It reports route completion, not race placement.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RouteRaceController : MonoBehaviour
    {
        [SerializeField] private string signalPrefix = "race.long_loop";
        [SerializeField] private VehicleDriveController playerVehicle;
        [SerializeField] private RouteRaceCheckpoint[] checkpoints;

        private bool _isActive;
        private int _nextCheckpointIndex;
        private bool _startSignalPending;
        private bool _startSignalAborted;
        private bool _finishSignalPending;

        public bool IsActive => _isActive;
        public bool HasFinished { get; private set; }
        public int NextCheckpointIndex => _nextCheckpointIndex;
        public int CheckpointCount => checkpoints != null ? checkpoints.Length : 0;
        public VehicleDriveController PlayerVehicle => playerVehicle;
        public event Action Finished;
        public event Action<int, int> ProgressChanged;

        private void OnEnable()
        {
            MissionSignalBus.DispatchCompleted += OnSignalDispatchCompleted;
            MissionSignalBus.DispatchAborted += OnSignalDispatchAborted;
        }

        private void OnDisable()
        {
            MissionSignalBus.DispatchCompleted -= OnSignalDispatchCompleted;
            MissionSignalBus.DispatchAborted -= OnSignalDispatchAborted;
        }

        public bool BeginRace()
        {
            if (_isActive || HasFinished || !HasValidRoute())
                return false;

            _nextCheckpointIndex = 0;
            _isActive = true;
            _startSignalPending = true;
            _startSignalAborted = false;
            NotifyProgressChanged();
            MissionSignalBus.Publish(signalPrefix + ".started", 1);
            _startSignalPending = false;
            if (_startSignalAborted)
            {
                _isActive = false;
                _nextCheckpointIndex = 0;
                NotifyProgressChanged();
                return false;
            }
            return true;
        }

        public bool TryRegisterCheckpoint(RouteRaceCheckpoint checkpoint, VehicleDriveController vehicle)
        {
            if (!_isActive || checkpoint == null || vehicle == null || vehicle != playerVehicle || !vehicle.IsDriven
                || checkpoints == null || _nextCheckpointIndex < 0
                || _nextCheckpointIndex >= checkpoints.Length)
                return false;

            // Object identity and authored index both matter: duplicate, skipped or out-of-order
            // trigger entries cannot increment route progress.
            if (checkpoints[_nextCheckpointIndex] != checkpoint
                || checkpoint.Controller != this
                || checkpoint.CheckpointIndex != _nextCheckpointIndex)
                return false;

            _nextCheckpointIndex++;
            NotifyProgressChanged();
            if (_nextCheckpointIndex == checkpoints.Length)
            {
                _isActive = false;
                _finishSignalPending = true;
                // The final checkpoint and route finish are outcomes of one trigger entry.
                // The specific route-finished callback is raised only after the whole batch drains.
                MissionSignalBus.PublishBatch(
                    new MissionSignalBus.Signal(signalPrefix + ".checkpoint", 1),
                    new MissionSignalBus.Signal(signalPrefix + ".finished", 1));
            }
            else
            {
                MissionSignalBus.Publish(signalPrefix + ".checkpoint", 1);
            }

            return true;
        }

        public bool CancelRace()
        {
            if (!_isActive) return false;

            _isActive = false;
            _nextCheckpointIndex = 0;
            NotifyProgressChanged();
            MissionSignalBus.Publish(signalPrefix + ".cancelled", 1);
            return true;
        }

        private void OnSignalDispatchCompleted()
        {
            if (!_finishSignalPending) return;

            _finishSignalPending = false;
            HasFinished = true;
            NotifyFinished();
        }

        private void OnSignalDispatchAborted(int dispatched)
        {
            if (_startSignalPending)
            {
                _startSignalAborted = true;
                _isActive = false;
                _nextCheckpointIndex = 0;
                NotifyProgressChanged();
                Debug.LogWarning("Route race '" + signalPrefix +
                    "' could not start because its mission-signal batch aborted after " +
                    dispatched + " events. Interact with the delivery board again after the signal loop is fixed.", this);
                return;
            }

            if (!_finishSignalPending) return;

            _finishSignalPending = false;
            HasFinished = false;
            // The final trigger action was discarded. Re-arm the final checkpoint so the
            // player can repeat that action instead of receiving a false route completion.
            _isActive = true;
            _nextCheckpointIndex = Mathf.Max(0, checkpoints != null ? checkpoints.Length - 1 : 0);
            NotifyProgressChanged();
            Debug.LogWarning("Route race '" + signalPrefix +
                "' rolled back its final checkpoint because the signal batch aborted after " +
                dispatched + " events. Repeat the final checkpoint after the signal loop is fixed.", this);
        }

        private void NotifyProgressChanged()
        {
            Action<int, int> handlers = ProgressChanged;
            if (handlers == null) return;

            foreach (Delegate subscriber in handlers.GetInvocationList())
            {
                try
                {
                    ((Action<int, int>)subscriber).Invoke(_nextCheckpointIndex, CheckpointCount);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void NotifyFinished()
        {
            Action handlers = Finished;
            if (handlers == null) return;

            foreach (Delegate subscriber in handlers.GetInvocationList())
            {
                try
                {
                    ((Action)subscriber).Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private bool HasValidRoute()
        {
            if (string.IsNullOrWhiteSpace(signalPrefix) || playerVehicle == null
                || checkpoints == null || checkpoints.Length == 0)
            {
                Debug.LogError("Route race requires a signal prefix, its designated player vehicle and at least one checkpoint.", this);
                return false;
            }

            for (int i = 0; i < checkpoints.Length; i++)
            {
                RouteRaceCheckpoint checkpoint = checkpoints[i];
                if (checkpoint == null || checkpoint.Controller != this
                    || checkpoint.CheckpointIndex != i || !checkpoint.HasTriggerCollider)
                {
                    Debug.LogError("Route race '" + signalPrefix + "' has an invalid checkpoint at index " +
                        i + ". Assign each trigger exactly once, in order, and use a trigger collider.", this);
                    return false;
                }
            }

            return true;
        }
    }
}
