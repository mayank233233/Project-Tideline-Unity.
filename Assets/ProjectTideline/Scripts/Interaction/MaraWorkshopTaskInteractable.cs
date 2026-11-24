using UnityEngine;

namespace ProjectTideline
{
    /// <summary>One inspectable final-check station in Mara's customer work order.</summary>
    [DisallowMultipleComponent]
    public sealed class MaraWorkshopTaskInteractable : InteractableBase
    {
        [SerializeField] private MaraWorkshopOrderController orderController;
        [SerializeField] private string taskId = "steering_check";
        [SerializeField] private string interactionPrompt = "Inspect vehicle";

        public override string InteractionPrompt
        {
            get
            {
                if (orderController != null && orderController.IsComplete)
                    return "Work order complete";
                if (orderController != null && !orderController.IsUnlocked)
                    return "Talk to Juno first";
                if (orderController != null && orderController.IsTaskCompleted(taskId))
                    return "Check complete";
                return string.IsNullOrWhiteSpace(interactionPrompt) ? "Inspect vehicle" : interactionPrompt;
            }
        }

        public override void Interact(GameObject actor)
        {
            if (actor == null || orderController == null)
                return;

            orderController.TryCompleteTask(taskId, actor);
        }
    }
}
