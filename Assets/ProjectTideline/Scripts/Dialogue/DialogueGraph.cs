using System;
using UnityEngine;

namespace ProjectTideline
{
    [CreateAssetMenu(menuName = "Project Tideline/Dialogue Graph", fileName = "Dialogue_")]
    public sealed class DialogueGraph : ScriptableObject
    {
        public string graphId;
        public string startNodeId;
        public DialogueNode[] nodes;

        [Serializable]
        public sealed class DialogueNode
        {
            public string nodeId;
            public string speaker;
            [TextArea(3, 7)] public string line;
            public DialogueChoice[] choices;
        }

        [Serializable]
        public sealed class DialogueChoice
        {
            public string label;
            public string nextNodeId;
            [Tooltip("Optional mission/world signal raised when this choice is selected.")]
            public string signalId;
            [Min(1)] public int signalAmount = 1;
            [Tooltip("Optional named character whose relationship changes when this choice is selected.")]
            public string relationshipCharacterId;
            [Range(-25, 25)] public int relationshipDelta;
            [Tooltip("Optional persistent campaign flag written when this choice is selected.")]
            public string storyFlagId;
            [Tooltip("Integer value stored for storyFlagId.")]
            public int storyFlagValue;
        }

        public DialogueNode FindNode(string nodeId)
        {
            if (nodes == null) return null;
            foreach (DialogueNode node in nodes)
                if (node != null && node.nodeId == nodeId) return node;
            return null;
        }
    }
}