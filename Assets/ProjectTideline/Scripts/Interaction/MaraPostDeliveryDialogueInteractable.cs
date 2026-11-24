using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Opens Nico's post-delivery dialogue only after the assigned delivery route has fully
    /// completed its signal batch and emitted the Mara-specific delivery completion signal.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaraPostDeliveryDialogueInteractable : InteractableBase
    {
        [SerializeField] private MaraDeliveryHandoffController handoffController;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private DialogueGraph dialogueGraph;

        public override string InteractionPrompt =>
            handoffController != null && handoffController.HasHandedOff
                ? "Talk to Nico"
                : "Hand the vehicle over first";

        public override void Interact(GameObject actor)
        {
            if (actor == null || handoffController == null || !handoffController.HasHandedOff
                || dialogueController == null || dialogueGraph == null)
                return;

            dialogueController.Begin(dialogueGraph);
        }
    }
}
