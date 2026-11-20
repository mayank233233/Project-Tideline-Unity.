using UnityEngine;

namespace ProjectTideline
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonMotor : MonoBehaviour
    {
        [SerializeField] private Transform movementCamera;
        [SerializeField] private float walkSpeed = 3.4f;
        [SerializeField] private float sprintSpeed = 5.1f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float rotationSharpness = 14f;
        [SerializeField] private float jumpHeight = 1.05f;
        [SerializeField] private float gravity = -22f;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            if (PlayerInputRouter.Instance == null || _controller == null) return;
            Vector2 input = PlayerInputRouter.Instance.Move;
            Vector3 forward = movementCamera != null ? movementCamera.forward : transform.forward;
            Vector3 right = movementCamera != null ? movementCamera.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 desired = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
            float speed = PlayerInputRouter.Instance.SprintHeld ? sprintSpeed : walkSpeed;
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, desired * speed, acceleration * Time.deltaTime);

            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            if (_controller.isGrounded && PlayerInputRouter.Instance.ConsumeJump())
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move((_horizontalVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime);

            if (desired.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(desired, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSharpness * Time.deltaTime);
            }
        }

        public void SetMovementCamera(Transform value) => movementCamera = value;

        /// <summary>Prevents stale momentum from carrying across a playable-lead handoff.</summary>
        public void ResetControlState()
        {
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = -2f;
        }
    }
}