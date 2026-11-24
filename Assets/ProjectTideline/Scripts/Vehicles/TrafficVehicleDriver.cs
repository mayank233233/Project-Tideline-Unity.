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
        [SerializeField, Min(0.02f)] private float obstacleCheckInterval = 0.12f;
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] private bool loopRoute = true;
        [Header("Distant Traffic Decision LOD")]
        [SerializeField, Min(0f)] private float fullRateDecisionDistance = 35f;
        [SerializeField, Min(0f)] private float reducedRateDecisionDistance = 80f;
        [SerializeField, Min(0.02f)] private float reducedDecisionInterval = 0.1f;
        [SerializeField, Min(0.05f)] private float distantDecisionInterval = 0.25f;

        private VehicleDriveController _vehicle;
        private Transform _decisionLodReference;
        private float _nextTrafficDecisionTime;
        private Rigidbody _body;
        private readonly RaycastHit[] _obstacleHits = new RaycastHit[10];
        private int _routeIndex;
        private bool _takeoverYield;
        private bool _cachedObstacleBlocked;
        private float _nextObstacleCheckTime;
        private float _obstacleCheckPhase;

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
            if (Camera.main != null) _decisionLodReference = Camera.main.transform;
            _obstacleCheckPhase = (GetInstanceID() & 0x7fffffff) / (float)int.MaxValue;
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

            // Only throttle distant AI decisions. VehicleDriveController continues running
            // WheelCollider physics at the normal fixed timestep for every traffic vehicle.
            if (!ShouldUpdateTrafficDecision()) return;

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
            bool blocked = IsObstacleBlocking();

            float currentSpeed = _vehicle.SpeedKph;
            float throttle = blocked ? 0f : Mathf.Clamp01((targetSpeed - currentSpeed) / 9f);
            float brake = blocked ? 1f : Mathf.Clamp01((currentSpeed - targetSpeed) / 8f);
            _vehicle.SetNpcDriveInput(steer, throttle, brake);
        }

        private bool ShouldUpdateTrafficDecision()
        {
            if (_decisionLodReference == null) return true;

            float distanceSquared = (_decisionLodReference.position - transform.position).sqrMagnitude;
            float fullRateSquared = fullRateDecisionDistance * fullRateDecisionDistance;
            float reducedRateSquared = Mathf.Max(fullRateDecisionDistance, reducedRateDecisionDistance);
            reducedRateSquared *= reducedRateSquared;

            float interval = distanceSquared <= fullRateSquared ? 0f
                : distanceSquared <= reducedRateSquared ? Mathf.Max(0.02f, reducedDecisionInterval)
                : Mathf.Max(0.05f, distantDecisionInterval);

            if (interval <= 0f)
            {
                _nextTrafficDecisionTime = Time.fixedTime;
                return true;
            }

            if (Time.fixedTime < _nextTrafficDecisionTime) return false;
            _nextTrafficDecisionTime = Time.fixedTime + interval;
            return true;
        }

        /// <summary>
        /// Obstacle sensing is sampled less often than physics steering. The cached result
        /// is reused between samples, and per-instance phase offsets spread queries across ticks.
        /// </summary>
        private bool IsObstacleBlocking()
        {
            if (Time.fixedTime < _nextObstacleCheckTime) return _cachedObstacleBlocked;

            Vector3 castOrigin = transform.position + transform.forward * 2.35f + Vector3.up * 1.12f;
            int hitCount = Physics.SphereCastNonAlloc(
                castOrigin, obstacleRadius, transform.forward, _obstacleHits, obstacleCheckDistance,
                obstacleMask, QueryTriggerInteraction.Ignore);

            _cachedObstacleBlocked = false;
            for (int i = 0; i < hitCount; i++)
            {
                Collider obstacle = _obstacleHits[i].collider;
                if (obstacle == null || obstacle.transform.root == transform.root) continue;
                if (obstacle.attachedRigidbody != null && obstacle.attachedRigidbody == _body) continue;
                _cachedObstacleBlocked = true;
                break;
            }

            float interval = Mathf.Max(0.02f, obstacleCheckInterval);
            _nextObstacleCheckTime = Time.fixedTime + interval * (0.75f + _obstacleCheckPhase * 0.5f);
            return _cachedObstacleBlocked;
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