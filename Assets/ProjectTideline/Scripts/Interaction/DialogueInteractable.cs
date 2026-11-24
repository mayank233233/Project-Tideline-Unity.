using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Opens an authored dialogue graph through the normal proximity-interaction path.
    /// Assign the scene's DialogueController and this character's DialogueGraph in the Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DialogueInteractable : InteractableBase
    {
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private DialogueGraph dialogueGraph;
        [SerializeField] private string talkPrompt = "Talk";
        [SerializeField] private bool oneShot;
        [SerializeField] private string completedPrompt = "Conversation complete";

        private bool _used;

        public override string InteractionPrompt =>
            oneShot && _used ? completedPrompt
                : string.IsNullOrWhiteSpace(talkPrompt) ? "Talk" : talkPrompt;

        public override void Interact(GameObject actor)
        {
            if (actor == null || dialogueController == null || dialogueGraph == null || (oneShot && _used))
                return;

            if (dialogueController.Begin(dialogueGraph) && oneShot)
                _used = true;
        }
    }
}
