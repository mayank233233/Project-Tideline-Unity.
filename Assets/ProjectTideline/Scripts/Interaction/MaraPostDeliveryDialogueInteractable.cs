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
        [SerializeField] private RouteRaceController deliveryRoute;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private DialogueGraph dialogueGraph;

        public override string InteractionPrompt =>
            deliveryRoute != null && deliveryRoute.HasFinished ? "Talk to Nico" : "Finish the delivery first";

        public override void Interact(GameObject actor)
        {
            if (actor == null || deliveryRoute == null || !deliveryRoute.HasFinished
                || dialogueController == null || dialogueGraph == null)
                return;

            dialogueController.Begin(dialogueGraph);
        }
    }
}
