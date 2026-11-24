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

        public override string InteractionPrompt =>
            string.IsNullOrWhiteSpace(talkPrompt) ? "Talk" : talkPrompt;

        public override void Interact(GameObject actor)
        {
            if (actor == null || dialogueController == null || dialogueGraph == null)
                return;

            dialogueController.Begin(dialogueGraph);
        }
    }
}
