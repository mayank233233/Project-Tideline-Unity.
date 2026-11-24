using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Starts the authored, non-graphic confrontation sequence from the normal proximity
    /// interaction path. Assign the opponent Animator and scene controller in the Inspector.
    /// Outcome signals remain owned by CinematicConfrontationController after the sequence ends.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CinematicConfrontationInteractable : InteractableBase
    {
        [SerializeField] private CinematicConfrontationController confrontationController;
        [SerializeField] private Animator opponentAnimator;
        [SerializeField] private string confrontationPrompt = "Confront";

        public override string InteractionPrompt =>
            string.IsNullOrWhiteSpace(confrontationPrompt) ? "Confront" : confrontationPrompt;

        public override void Interact(GameObject actor)
        {
            if (actor == null || confrontationController == null || opponentAnimator == null)
                return;

            confrontationController.Begin(actor, opponentAnimator);
        }
    }
}
