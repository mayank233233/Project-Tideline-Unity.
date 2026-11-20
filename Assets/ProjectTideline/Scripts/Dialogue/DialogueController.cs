using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>Inspector-wired dialogue view with persistent relationship and story-flag outcomes.</summary>
    public sealed class DialogueController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text lineText;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private Text[] choiceLabels;
        [SerializeField] private Button closeButton;

        private DialogueGraph _graph;
        private DialogueGraph.DialogueNode _node;

        public bool IsOpen => _graph != null;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            Close();
        }

        private void OnDisable()
        {
            // A scene transition or UI disable must never leave movement/vehicle input locked.
            if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.SetGameplayBlocked(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        public void Begin(DialogueGraph graph)
        {
            if (graph == null) return;
            _graph = graph;
            if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.SetGameplayBlocked(true);
            ShowNode(_graph.FindNode(_graph.startNodeId));
        }

        public void Close()
        {
            _graph = null;
            if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.SetGameplayBlocked(false);
            _node = null;
            if (panel != null)
            {
                panel.alpha = 0f;
                panel.interactable = false;
                panel.blocksRaycasts = false;
            }

            if (choiceButtons != null)
            {
                foreach (Button button in choiceButtons)
                {
                    if (button == null) continue;
                    button.onClick.RemoveAllListeners();
                    button.gameObject.SetActive(false);
                }
            }
        }

        private void SelectChoice(string nextId, DialogueGraph.DialogueChoice choice)
        {
            if (_graph == null) return;
            if (choice != null)
            {
                if (!string.IsNullOrWhiteSpace(choice.signalId))
                    MissionSignalBus.Publish(choice.signalId, Mathf.Max(1, choice.signalAmount));

                if (!string.IsNullOrWhiteSpace(choice.relationshipCharacterId) && choice.relationshipDelta != 0
                    && NPCRelationshipLedger.Instance != null)
                    NPCRelationshipLedger.Instance.ChangeValue(choice.relationshipCharacterId, choice.relationshipDelta);

                if (!string.IsNullOrWhiteSpace(choice.storyFlagId))
                {
                    if (NarrativeStateLedger.Instance != null)
                        NarrativeStateLedger.Instance.SetFlag(choice.storyFlagId, choice.storyFlagValue);
                    else
                        Debug.LogError("Dialogue choice '" + choice.label + "' attempted to set story flag '" +
                            choice.storyFlagId + "', but no NarrativeStateLedger exists in the scene.", this);
                }
            }

            DialogueGraph graph = _graph;
            ShowNode(string.IsNullOrEmpty(nextId) ? null : graph.FindNode(nextId));
        }

        private void ShowNode(DialogueGraph.DialogueNode node)
        {
            if (node == null) { Close(); return; }
            _node = node;
            if (panel != null)
            {
                panel.alpha = 1f;
                panel.interactable = true;
                panel.blocksRaycasts = true;
            }
            if (speakerText != null) speakerText.text = node.speaker;
            if (lineText != null) lineText.text = node.line;

            int count = choiceButtons == null ? 0 : choiceButtons.Length;
            for (int i = 0; i < count; i++)
            {
                Button button = choiceButtons[i];
                bool active = node.choices != null && i < node.choices.Length;
                if (button == null) continue;
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(active);
                if (!active) continue;
                DialogueGraph.DialogueChoice choice = node.choices[i];
                if (choiceLabels != null && i < choiceLabels.Length && choiceLabels[i] != null)
                    choiceLabels[i].text = choice.label;
                string nextId = choice.nextNodeId;
                DialogueGraph.DialogueChoice selectedChoice = choice;
                button.onClick.AddListener(() => SelectChoice(nextId, selectedChoice));
            }
        }
    }
}