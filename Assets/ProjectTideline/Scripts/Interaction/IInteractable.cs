using UnityEngine;

namespace ProjectTideline
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void Interact(GameObject actor);
    }

    /// <summary>
    /// Base class for focused player interactions. Mission signals belong in concrete
    /// interaction methods and must be published only after their own success conditions pass.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Interact";
        public virtual string InteractionPrompt => interactionPrompt;
        public abstract void Interact(GameObject actor);
    }
}
