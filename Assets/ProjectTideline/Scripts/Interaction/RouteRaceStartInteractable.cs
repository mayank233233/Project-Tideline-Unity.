using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Starts a configured RouteRaceController through the normal proximity-interaction path.</summary>
    [DisallowMultipleComponent]
    public sealed class RouteRaceStartInteractable : InteractableBase
    {
        [SerializeField] private RouteRaceController raceController;
        [SerializeField] private string startPrompt = "Start driving event";

        public override string InteractionPrompt =>
            string.IsNullOrWhiteSpace(startPrompt) ? "Start driving event" : startPrompt;

        public override void Interact(GameObject actor)
        {
            if (actor == null || raceController == null)
                return;

            raceController.BeginRace();
        }
    }
}
