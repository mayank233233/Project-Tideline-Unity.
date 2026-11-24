using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectTideline
{
    public enum MobileTouchControlMode
    {
        MoveStick,
        LookPad,
        SprintButton,
        JumpButton,
        InteractButton
    }

    /// <summary>Event-driven touch controls shared between on-foot and vehicle input.</summary>
    public sealed class MobileTouchControl : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private MobileTouchControlMode mode;
        [SerializeField] private RectTransform joystickKnob;
        [SerializeField] private Text contextualLabel;
        [SerializeField, Range(0.001f, 0.2f)] private float lookSensitivity = 0.055f;

        private RectTransform _rectTransform;

        private void Awake() => _rectTransform = transform as RectTransform;

        private void OnEnable()
        {
            if (PlayerInputRouter.Instance != null)
                PlayerInputRouter.Instance.VehicleControlModeChanged += OnVehicleControlModeChanged;
            RefreshContextualLabel();
        }

        public void Configure(MobileTouchControlMode controlMode, RectTransform knob = null,
            float sensitivity = 0.055f, Text label = null)
        {
            mode = controlMode;
            joystickKnob = knob;
            contextualLabel = label;
            lookSensitivity = Mathf.Clamp(sensitivity, 0.001f, 0.2f);
            _rectTransform = transform as RectTransform;
            RefreshContextualLabel();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (mode)
            {
                case MobileTouchControlMode.MoveStick:
                    UpdateMoveStick(eventData);
                    break;
                case MobileTouchControlMode.LookPad:
                    UpdateLook(eventData);
                    break;
                case MobileTouchControlMode.SprintButton:
                    if (PlayerInputRouter.Instance != null)
                    {
                        if (PlayerInputRouter.Instance.VehicleControlMode) PlayerInputRouter.Instance.SetThrottle(1f);
                        else PlayerInputRouter.Instance.SetSprint(true);
                    }
                    break;
                case MobileTouchControlMode.JumpButton:
                    if (PlayerInputRouter.Instance != null)
                    {
                        if (PlayerInputRouter.Instance.VehicleControlMode) PlayerInputRouter.Instance.SetBrake(1f);
                        else PlayerInputRouter.Instance.PulseJump();
                    }
                    break;
                case MobileTouchControlMode.InteractButton:
                    if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.PulseInteract();
                    break;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (mode == MobileTouchControlMode.MoveStick) UpdateMoveStick(eventData);
            else if (mode == MobileTouchControlMode.LookPad) UpdateLook(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (mode == MobileTouchControlMode.MoveStick)
            {
                if (PlayerInputRouter.Instance != null) PlayerInputRouter.Instance.SetMove(Vector2.zero);
                if (joystickKnob != null) joystickKnob.anchoredPosition = Vector2.zero;
            }
            else if (mode == MobileTouchControlMode.SprintButton
                     && PlayerInputRouter.Instance != null)
            {
                if (PlayerInputRouter.Instance.VehicleControlMode) PlayerInputRouter.Instance.SetThrottle(0f);
                else PlayerInputRouter.Instance.SetSprint(false);
            }
            else if (mode == MobileTouchControlMode.JumpButton
                     && PlayerInputRouter.Instance != null && PlayerInputRouter.Instance.VehicleControlMode)
            {
                PlayerInputRouter.Instance.SetBrake(0f);
            }
        }

        private void UpdateMoveStick(PointerEventData eventData)
        {
            if (PlayerInputRouter.Instance == null || _rectTransform == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 local))
                return;

            Rect rect = _rectTransform.rect;
            float radius = Mathf.Max(1f, Mathf.Min(rect.width, rect.height) * 0.38f);
            Vector2 direction = Vector2.ClampMagnitude(local / radius, 1f);
            PlayerInputRouter.Instance.SetMove(direction);
            if (joystickKnob != null) joystickKnob.anchoredPosition = direction * radius;
        }

        private void UpdateLook(PointerEventData eventData)
        {
            if (PlayerInputRouter.Instance != null)
                PlayerInputRouter.Instance.AddLookDelta(eventData.delta * lookSensitivity);
        }

        private void OnVehicleControlModeChanged(bool _) => RefreshContextualLabel();

        private void RefreshContextualLabel()
        {
            if (contextualLabel == null) return;
            bool inVehicle = PlayerInputRouter.Instance != null
                && PlayerInputRouter.Instance.VehicleControlMode;

            switch (mode)
            {
                case MobileTouchControlMode.SprintButton:
                    contextualLabel.text = inVehicle ? "ACCEL" : "SPRINT";
                    break;
                case MobileTouchControlMode.JumpButton:
                    contextualLabel.text = inVehicle ? "BRAKE" : "JUMP";
                    break;
                case MobileTouchControlMode.InteractButton:
                    contextualLabel.text = "INTERACT";
                    break;
            }
        }

        private void OnDisable()
        {
            if (PlayerInputRouter.Instance != null)
            {
                PlayerInputRouter.Instance.VehicleControlModeChanged -= OnVehicleControlModeChanged;
                if (mode == MobileTouchControlMode.MoveStick)
                    PlayerInputRouter.Instance.SetMove(Vector2.zero);
                else if (mode == MobileTouchControlMode.SprintButton)
                {
                    if (PlayerInputRouter.Instance.VehicleControlMode) PlayerInputRouter.Instance.SetThrottle(0f);
                    else PlayerInputRouter.Instance.SetSprint(false);
                }
                else if (mode == MobileTouchControlMode.JumpButton && PlayerInputRouter.Instance.VehicleControlMode)
                    PlayerInputRouter.Instance.SetHandbrake(false);
            }
        }
    }
}
