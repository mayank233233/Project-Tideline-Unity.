using UnityEngine;

namespace ProjectTideline
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        string SignalId { get; }
        void Interact(GameObject actor);
    }

    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Interact";
        [SerializeField] private string signalId = "interaction.generic";
        public virtual string InteractionPrompt => interactionPrompt;
        public string SignalId => signalId;
        public abstract void Interact(GameObject actor);
    }
}