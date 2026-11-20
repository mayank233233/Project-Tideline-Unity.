using System;
using UnityEngine;

namespace ProjectTideline
{
    [CreateAssetMenu(menuName = "Project Tideline/Mission", fileName = "Mission_")]
    public sealed class MissionDefinition : ScriptableObject
    {
        public string missionId;
        public string title;
        [TextArea(2, 4)] public string briefing;
        public MissionObjective[] objectives;
        [Tooltip("Branches are evaluated in order when this mission completes. The first matching branch wins.")]
        public MissionBranch[] completionBranches;
        [Tooltip("Optional missionId used when no completion branch matches. Empty means next asset in the campaign array.")]
        public string fallbackNextMissionId;
        [Tooltip("Enable for mutually exclusive ending/epilogue mission assets so the campaign stops here.")]
        public bool endsCampaign;
    }

    [Serializable]
    public sealed class MissionObjective
    {
        public string objectiveId;
        [TextArea(1, 3)] public string description;
        public string completionSignal;
        [Min(1)] public int requiredCount = 1;
    }

    [Serializable]
    public sealed class MissionBranch
    {
        public string nextMissionId;
        public MissionBranchCondition[] conditions;
    }

    [Serializable]
    public sealed class MissionBranchCondition
    {
        [Tooltip("Set exactly one condition source: relationshipKey or storyFlagId.")]
        public string relationshipKey;
        [Tooltip("Persistent integer story flag set by a dialogue choice.")]
        public string storyFlagId;
        [Range(-100, 100)] public int minimumValue = -100;
        [Range(-100, 100)] public int maximumValue = 100;
    }
}