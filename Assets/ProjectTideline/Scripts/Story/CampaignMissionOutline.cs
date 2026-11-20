using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Editor-generated narrative design record. It is intentionally separate from
    /// MissionDefinition so an outline cannot be accidentally played or auto-completed.
    /// </summary>
    [CreateAssetMenu(menuName = "Project Tideline/Campaign Mission Outline", fileName = "Outline_")]
    public sealed class CampaignMissionOutline : ScriptableObject
    {
        public string canonId;
        public string missionTitle;
        public string leadId;
        public bool isPrologue;
        public string districtId;
        public string primaryVerb;
        [TextArea(3, 6)] public string playerGoal;
        [TextArea(3, 6)] public string characterComplication;
        [TextArea(2, 5)] public string playerChoice;
        [TextArea(2, 5)] public string persistentConsequence;
        [TextArea(2, 4)] public string implementationStatus = "Narrative outline only — not a playable mission.";
    }
}