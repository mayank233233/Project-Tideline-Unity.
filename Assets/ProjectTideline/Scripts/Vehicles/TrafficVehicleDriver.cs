using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Lightweight waypoint traffic for NPC-owned cars. It yields immediately when the player takes over.
    /// This is a game-world traffic controller, not a real-world driving model.
    /// </summary>
    [RequireComponent(typeof(VehicleDriveController))]
    public sealed class TrafficVehicleDriver : MonoBehaviour
    {
        [SerializeField] private Transform[] route;
        [SerializeField, Min(5f)] private float cruisingSpeedKph = 28f;
        [SerializeField, Min(1f)] private float waypointRadius = 7f;
        [SerializeField, Min(1f)] private float obstacleCheckDistance = 9f;
        [SerializeField, Min(0.1f)] private float obstacleRadius = 0.62f;
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private bool loopRoute = true;

        private VehicleDriveController _vehicle;
        private Rigidbody _body;
        private readonly RaycastHit[] _obstacleHits = new RaycastHit[10];
        private int _routeIndex;
        private bool _takeoverYield;

        public void BeginTakeoverHandoff()
        {
            _takeoverYield = true;
            if (_vehicle != null) _vehicle.SetNpcDriveInput(0f, 0f, 1f, true);
        }

        public void CancelTakeoverHandoff()
        {
            _takeoverYield = false;
            if (_vehicle != null) _vehicle.SetNpcDriveInput(0f, 0f, 0f, true);
        }

        private void Awake()
        {
            _vehicle = GetComponent<VehicleDriveController>();
            _body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (_vehicle == null) return;
            if (_takeoverYield)
            {
                _vehicle.SetNpcDriveInput(0f, 0f, 1f, true);
                return;
            }
            if (_vehicle.IsDriven)
            {
                _vehicle.SetNpcDriveInput(0f, 0f, 0f, false);
                return;
            }
            if (route == null || route.Length == 0)
            {
                _vehicle.SetNpcDriveInput(0f, 0f, 1f, true);
                return;
            }

            Transform target = route[Mathf.Clamp(_routeIndex, 0, route.Length - 1)];
            if (target == null)
            {
                AdvanceRoute();
                _vehicle.SetNpcDriveInput(0f, 0f, 1f, true);
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= waypointRadius * waypointRadius)
            {
                if (!AdvanceRoute())
                {
                    _vehicle.SetNpcDriveInput(0f, 0f, 1f, true);
                    return;
                }
                target = route[_routeIndex];
                if (target == null)
                {
                    _vehicle.SetNpcDriveInput(0f, 0f, 1f, true);
                    return;
                }
                toTarget = target.position - transform.position;
                toTarget.y = 0f;
            }

            Vector3 localTarget = transform.InverseTransformDirection(toTarget.normalized);
            float steer = Mathf.Clamp(localTarget.x * 2.1f, -1f, 1f);
            float targetSpeed = cruisingSpeedKph * Mathf.Lerp(1f, 0.58f, Mathf.Abs(steer));
            bool blocked = false;
            Vector3 castOrigin = transform.position + transform.forward * 2.35f + Vector3.up * 1.12f;
            int hitCount = Physics.SphereCastNonAlloc(
                castOrigin, obstacleRadius, transform.forward, _obstacleHits, obstacleCheckDistance,
                obstacleMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                Collider obstacle = _obstacleHits[i].collider;
                if (obstacle == null || obstacle.transform.root == transform.root) continue;
                if (obstacle.attachedRigidbody != null && obstacle.attachedRigidbody == _body) continue;
                blocked = true;
                break;
            }

            float currentSpeed = _vehicle.SpeedKph;
            float throttle = blocked ? 0f : Mathf.Clamp01((targetSpeed - currentSpeed) / 9f);
            float brake = blocked ? 1f : Mathf.Clamp01((currentSpeed - targetSpeed) / 8f);
            _vehicle.SetNpcDriveInput(steer, throttle, brake);
        }

        private bool AdvanceRoute()
        {
            if (route == null || route.Length == 0) return false;
            for (int attempt = 0; attempt < route.Length; attempt++)
            {
                int next = _routeIndex + 1;
                if (next >= route.Length)
                {
                    if (!loopRoute) return false;
                    next = 0;
                }
                _routeIndex = next;
                if (route[_routeIndex] != null) return true;
            }
            return false;
        }

        private void OnDisable()
        {
            if (_vehicle != null && !_vehicle.IsDriven)
                _vehicle.SetNpcDriveInput(0f, 0f, 0f, false);
        }
    }
}