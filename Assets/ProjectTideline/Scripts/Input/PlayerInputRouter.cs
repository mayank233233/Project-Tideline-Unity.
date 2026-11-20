using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectTideline
{
    /// <summary>Single input state shared by on-screen controls, player movement and vehicles.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        public static PlayerInputRouter Instance { get; private set; }

        private Vector2 _touchMove;
        private Vector2 _lookDelta;
        private bool _sprintHeld;
        private bool _jumpPressed;
        private bool _interactPressed;
        private bool _confrontPressed;
        private bool _breakAwayPressed;
        private bool _handbrakeHeld;
        private float _throttle;
        private float _brake;

        public Vector2 Move { get; private set; }
        public bool GameplayBlocked { get; private set; }
        public bool SprintHeld => !GameplayBlocked && (_sprintHeld || (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed));
        public bool HandbrakeHeld => !GameplayBlocked && (_handbrakeHeld || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed));
        public float Throttle => GameplayBlocked ? 0f : Mathf.Max(_throttle, Keyboard.current != null && Keyboard.current.wKey.isPressed ? 1f : 0f);
        public float Brake => GameplayBlocked ? 0f : Mathf.Max(_brake, Keyboard.current != null && Keyboard.current.sKey.isPressed ? 1f : 0f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (GameplayBlocked)
            {
                Move = Vector2.zero;
                _lookDelta = Vector2.zero;
                return;
            }

            Vector2 keyboardMove = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                keyboardMove.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                               - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
                keyboardMove.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                               - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
                if (keyboard.spaceKey.wasPressedThisFrame) _jumpPressed = true;
                if (keyboard.eKey.wasPressedThisFrame) _interactPressed = true;
                if (keyboard.fKey.wasPressedThisFrame) _confrontPressed = true;
                if (keyboard.qKey.wasPressedThisFrame) _breakAwayPressed = true;
            }

            Move = _touchMove.sqrMagnitude > 0.0001f
                ? Vector2.ClampMagnitude(_touchMove, 1f)
                : Vector2.ClampMagnitude(keyboardMove, 1f);

            if (Mouse.current != null) _lookDelta += Mouse.current.delta.ReadValue() * 0.045f;
        }

        public void SetGameplayBlocked(bool blocked)
        {
            if (GameplayBlocked == blocked) return;
            GameplayBlocked = blocked;
            if (blocked) ResetTouchState();
        }

        /// <summary>Clears buffered movement/look/buttons at a character-control handoff.</summary>
        public void ClearTransientInput() => ResetTouchState();

        public void SetMove(Vector2 value)
        {
            if (!GameplayBlocked) _touchMove = Vector2.ClampMagnitude(value, 1f);
        }

        public void AddLookDelta(Vector2 value)
        {
            if (!GameplayBlocked) _lookDelta += value;
        }
        public Vector2 ConsumeLookDelta()
        {
            Vector2 value = _lookDelta;
            _lookDelta = Vector2.zero;
            return value;
        }

        public void SetSprint(bool value) { if (!GameplayBlocked) _sprintHeld = value; }
        public void SetHandbrake(bool value) { if (!GameplayBlocked) _handbrakeHeld = value; }
        public void SetThrottle(float value) { if (!GameplayBlocked) _throttle = Mathf.Clamp01(value); }
        public void SetBrake(float value) { if (!GameplayBlocked) _brake = Mathf.Clamp01(value); }
        public void PulseJump() { if (!GameplayBlocked) _jumpPressed = true; }
        public void PulseInteract() { if (!GameplayBlocked) _interactPressed = true; }
        public void PulseConfront() { if (!GameplayBlocked) _confrontPressed = true; }
        public void PulseBreakAway() { if (!GameplayBlocked) _breakAwayPressed = true; }

        public bool ConsumeJump()
        {
            bool value = _jumpPressed;
            _jumpPressed = false;
            return value;
        }

        public bool ConsumeInteract()
        {
            bool value = _interactPressed;
            _interactPressed = false;
            return value;
        }

        public bool ConsumeConfront()
        {
            bool value = _confrontPressed;
            _confrontPressed = false;
            return value;
        }

        public bool ConsumeBreakAway()
        {
            bool value = _breakAwayPressed;
            _breakAwayPressed = false;
            return value;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ResetTouchState();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) ResetTouchState();
        }

        private void OnDisable() => ResetTouchState();

        private void ResetTouchState()
        {
            _touchMove = Vector2.zero;
            _lookDelta = Vector2.zero;
            _sprintHeld = false;
            _jumpPressed = false;
            _interactPressed = false;
            _confrontPressed = false;
            _breakAwayPressed = false;
            _handbrakeHeld = false;
            _throttle = 0f;
            _brake = 0f;
            Move = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}