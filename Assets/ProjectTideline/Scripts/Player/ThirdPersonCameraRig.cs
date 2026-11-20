using UnityEngine;

namespace ProjectTideline
{
    public sealed class ThirdPersonCameraRig : MonoBehaviour
    {
        public static ThirdPersonCameraRig Instance { get; private set; }
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.45f, 0f);
        [SerializeField] private float distance = 4.2f;
        [SerializeField] private float followSharpness = 14f;
        [SerializeField] private float yawSensitivity = 1f;
        [SerializeField] private float pitchSensitivity = 1f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float collisionRadius = 0.22f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private float _yaw;
        private float _pitch = 12f;
        private readonly RaycastHit[] _cameraHits = new RaycastHit[12];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple ThirdPersonCameraRig instances found. This duplicate rig has been disabled.", this);
                enabled = false;
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (target != null) _yaw = target.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (PlayerInputRouter.Instance != null)
            {
                Vector2 look = PlayerInputRouter.Instance.ConsumeLookDelta();
                _yaw += look.x * yawSensitivity;
                _pitch = Mathf.Clamp(_pitch - look.y * pitchSensitivity, minPitch, maxPitch);
            }

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 focus = target.position + targetOffset;
            Vector3 desired = focus - rotation * Vector3.forward * distance;
            Vector3 toCamera = desired - focus;
            float finalDistance = distance;
            int hitCount = Physics.SphereCastNonAlloc(focus, collisionRadius, toCamera.normalized,
                _cameraHits, distance, collisionMask, QueryTriggerInteraction.Ignore);
            float nearestExternalHit = distance;
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = _cameraHits[i].collider;
                if (hitCollider == null || IsTargetCollider(hitCollider.transform)) continue;
                if (_cameraHits[i].distance > 0.001f && _cameraHits[i].distance < nearestExternalHit)
                    nearestExternalHit = _cameraHits[i].distance;
            }
            if (nearestExternalHit < distance)
                finalDistance = Mathf.Max(0.5f, nearestExternalHit - 0.08f);

            desired = focus - rotation * Vector3.forward * finalDistance;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
            transform.rotation = rotation;
        }

        private bool IsTargetCollider(Transform candidate)
        {
            if (target == null || candidate == null) return false;
            return candidate == target || candidate.IsChildOf(target);
        }

        public void SetTarget(Transform value, Vector3? offset = null, bool resetYawToTarget = false)
        {
            target = value;
            if (offset.HasValue) targetOffset = offset.Value;
            if (resetYawToTarget && target != null) _yaw = target.eulerAngles.y;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}