using System;
using UnityEngine;

namespace ProjectTideline
{
    public sealed class MissionDirector : MonoBehaviour
    {
        [SerializeField] private MissionDefinition[] campaign;
        [SerializeField] private bool autoStart = true;

        public event Action<MissionDefinition, MissionObjective, int, int> ProgressChanged;
        public event Action<MissionDefinition> MissionStarted;
        public event Action<MissionDefinition> MissionCompleted;
        public int CurrentMissionIndex => _missionIndex;
        public int CurrentObjectiveIndex => _objectiveIndex;
        public int CurrentObjectiveCount => _currentCount;
        public MissionDefinition CurrentMission
        {
            get
            {
                if (campaign == null || _missionIndex < 0 || _missionIndex >= campaign.Length) return null;
                return campaign[_missionIndex];
            }
        }

        public MissionObjective CurrentObjective
        {
            get
            {
                MissionDefinition mission = CurrentMission;
                if (mission == null || mission.objectives == null || _objectiveIndex < 0 || _objectiveIndex >= mission.objectives.Length)
                    return null;
                return mission.objectives[_objectiveIndex];
            }
        }

        private int _missionIndex = -1;
        private int _objectiveIndex;
        private int _currentCount;

        private void OnEnable() => MissionSignalBus.Raised += OnSignal;
        private void OnDisable() => MissionSignalBus.Raised -= OnSignal;

        private void Start()
        {
            if (autoStart && campaign != null && campaign.Length > 0) StartMission(0);
        }

        public void StartMission(int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Length || campaign[index] == null) return;
            _missionIndex = index;
            _objectiveIndex = 0;
            _currentCount = 0;
            MissionStarted?.Invoke(CurrentMission);
            PublishProgress();
            if (CurrentMission.objectives == null || CurrentMission.objectives.Length == 0) CompleteCurrentMission();
        }

        /// <summary>Restores a saved campaign position without replaying completion/start events.</summary>
        public bool RestoreProgress(int missionIndex, int objectiveIndex, int objectiveCount)
        {
            if (campaign == null || campaign.Length == 0 || missionIndex < 0 || missionIndex > campaign.Length)
                return false;

            if (missionIndex == campaign.Length)
            {
                _missionIndex = campaign.Length;
                _objectiveIndex = 0;
                _currentCount = 0;
                PublishProgress();
                return true;
            }

            MissionDefinition mission = campaign[missionIndex];
            if (mission == null || mission.objectives == null || mission.objectives.Length == 0)
                return false;

            int safeObjective = Mathf.Clamp(objectiveIndex, 0, mission.objectives.Length - 1);
            MissionObjective objective = mission.objectives[safeObjective];
            if (objective == null) return false;

            _missionIndex = missionIndex;
            _objectiveIndex = safeObjective;
            _currentCount = Mathf.Clamp(objectiveCount, 0, Mathf.Max(1, objective.requiredCount) - 1);
            PublishProgress();
            return true;
        }

        private void OnSignal(string signalId, int amount)
        {
            MissionObjective objective = CurrentObjective;
            if (objective == null || objective.completionSignal != signalId) return;
            int required = Mathf.Max(1, objective.requiredCount);
            _currentCount = (int)Math.Min(required, (long)_currentCount + amount);
            PublishProgress();
            if (_currentCount >= required) AdvanceObjective();
        }

        private void AdvanceObjective()
        {
            _objectiveIndex++;
            _currentCount = 0;
            if (CurrentMission == null || _objectiveIndex >= CurrentMission.objectives.Length)
                CompleteCurrentMission();
            else
                PublishProgress();
        }

        private void CompleteCurrentMission()
        {
            MissionDefinition completed = CurrentMission;
            if (completed != null) MissionCompleted?.Invoke(completed);
            int nextMissionIndex = ResolveNextMissionIndex(completed);
            if (nextMissionIndex >= 0) StartMission(nextMissionIndex);
            else
            {
                _missionIndex = campaign.Length;
                _objectiveIndex = 0;
                _currentCount = 0;
                PublishProgress();
            }
        }

        private int ResolveNextMissionIndex(MissionDefinition completed)
        {
            if (completed == null || completed.endsCampaign || campaign == null) return -1;

            if (completed.completionBranches != null)
            {
                foreach (MissionBranch branch in completed.completionBranches)
                {
                    if (branch == null || string.IsNullOrWhiteSpace(branch.nextMissionId) || !BranchConditionsMatch(branch))
                        continue;

                    int targetIndex = FindMissionIndex(branch.nextMissionId);
                    if (targetIndex > _missionIndex) return targetIndex;
                    Debug.LogWarning($"Mission '{completed.missionId}' references a missing or non-forward branch target '{branch.nextMissionId}'.", completed);
                }
            }

            if (!string.IsNullOrWhiteSpace(completed.fallbackNextMissionId))
            {
                int fallbackIndex = FindMissionIndex(completed.fallbackNextMissionId);
                if (fallbackIndex > _missionIndex) return fallbackIndex;
                Debug.LogWarning($"Mission '{completed.missionId}' has a missing or non-forward fallback target '{completed.fallbackNextMissionId}'.", completed);
            }

            return _missionIndex + 1 < campaign.Length ? _missionIndex + 1 : -1;
        }

        private bool BranchConditionsMatch(MissionBranch branch)
        {
            NPCRelationshipLedger relationshipLedger = NPCRelationshipLedger.Instance;
            NarrativeStateLedger narrativeLedger = NarrativeStateLedger.Instance;
            return MissionBranchEvaluator.ConditionsMatch(
                branch,
                key => relationshipLedger != null && relationshipLedger.HasValue(key)
                    ? (int?)relationshipLedger.GetValue(key) : null,
                flag => narrativeLedger != null && narrativeLedger.HasFlag(flag)
                    ? (int?)narrativeLedger.GetFlag(flag) : null);
        }

        private int FindMissionIndex(string missionId)
        {
            if (campaign == null || string.IsNullOrWhiteSpace(missionId)) return -1;
            for (int i = 0; i < campaign.Length; i++)
            {
                if (campaign[i] != null && string.Equals(campaign[i].missionId, missionId, StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

        private void PublishProgress()
        {
            MissionObjective objective = CurrentObjective;
            ProgressChanged?.Invoke(CurrentMission, objective, _currentCount, objective != null ? Mathf.Max(1, objective.requiredCount) : 0);
        }
    }
}