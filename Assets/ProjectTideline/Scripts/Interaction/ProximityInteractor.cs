using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    public sealed class ProximityInteractor : MonoBehaviour
    {
        [SerializeField] private Transform viewOrigin;
        [SerializeField] private Transform actorRoot;
        [SerializeField] private float reach = 2.8f;
        [SerializeField] private float sphereRadius = 0.18f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptText;

        private IInteractable _focused;
        private VehicleSeatInteractable _activeVehicle;
        private VehicleSeatInteractable _pendingVehicle;
        private readonly List<MonoBehaviour> _candidateBuffer = new List<MonoBehaviour>(8);

        public bool IsInVehicle => _activeVehicle != null;
        public bool IsVehicleTransitioning => _pendingVehicle != null || (_activeVehicle != null && _activeVehicle.IsTransitioning);
        public bool CanSwitchLead => _activeVehicle == null && _pendingVehicle == null;

        /// <summary>Rebinds this interactor when its actor becomes the active playable lead.</summary>
        public void BindPlayer(Transform newActorRoot, Transform newViewOrigin)
        {
            SetFocus(null);
            _activeVehicle = null;
            _pendingVehicle = null;
            actorRoot = newActorRoot;
            viewOrigin = newViewOrigin;
        }

        /// <summary>Clears a visible interaction prompt before this lead becomes inactive.</summary>
        public void ResetForLeadHandoff()
        {
            if (!CanSwitchLead) return;
            SetFocus(null);
            HidePrompt();
        }

        private void Awake()
        {
            if (actorRoot == null)
            {
                CharacterController controller = GetComponent<CharacterController>();
                if (controller == null) controller = GetComponentInParent<CharacterController>();
                actorRoot = controller != null ? controller.transform : transform;
            }
            if (viewOrigin == null && Camera.main != null) viewOrigin = Camera.main.transform;
        }

        private void Update()
        {
            if (PlayerInputRouter.Instance == null || viewOrigin == null || actorRoot == null) return;
            if (PlayerInputRouter.Instance.GameplayBlocked)
            {
                SetFocus(null);
                return;
            }

            if (_pendingVehicle != null)
            {
                SetFocus(null);
                if (_pendingVehicle.IsOccupiedBy(actorRoot.gameObject))
                {
                    _activeVehicle = _pendingVehicle;
                    _pendingVehicle = null;
                }
                else if (!_pendingVehicle.IsTransitioning)
                {
                    _pendingVehicle = null;
                }
                return;
            }

            if (_activeVehicle != null)
            {
                _focused = null;
                SetPrompt("INTERACT — EXIT VEHICLE", true);
                if (PlayerInputRouter.Instance.ConsumeInteract())
                {
                    _activeVehicle.Exit(actorRoot.gameObject);
                    _activeVehicle = null;
                    SetFocus(null);
                    HidePrompt();
                }
                return;
            }

            FindFocus();
            if (PlayerInputRouter.Instance.ConsumeInteract() && _focused != null)
            {
                if (_focused is VehicleSeatInteractable seat)
                {
                    seat.Interact(actorRoot.gameObject);
                    if (seat.IsOccupiedBy(actorRoot.gameObject)) _activeVehicle = seat;
                    else if (seat.IsTransitioning) _pendingVehicle = seat;
                }
                else
                {
                    _focused.Interact(actorRoot.gameObject);
                }
            }
        }

        private void FindFocus()
        {
            IInteractable found = null;
            if (Physics.SphereCast(viewOrigin.position, sphereRadius, viewOrigin.forward,
                    out RaycastHit hit, reach, interactionMask, QueryTriggerInteraction.Ignore))
            {
                hit.collider.GetComponentsInParent<MonoBehaviour>(true, _candidateBuffer);
                foreach (MonoBehaviour candidate in _candidateBuffer)
                {
                    if (candidate is IInteractable interactable)
                    {
                        found = interactable;
                        break;
                    }
                }
            }
            SetFocus(found);
        }

        private void SetFocus(IInteractable value)
        {
            _focused = value;
            SetPrompt(_focused != null ? _focused.InteractionPrompt : string.Empty, _focused != null);
        }

        private void SetPrompt(string value, bool visible)
        {
            if (promptRoot != null && promptRoot.activeSelf != visible)
                promptRoot.SetActive(visible);
            if (promptText != null && !string.Equals(promptText.text, value, System.StringComparison.Ordinal))
                promptText.text = value;
        }

        private void HidePrompt() => SetPrompt(string.Empty, false);
    }
}