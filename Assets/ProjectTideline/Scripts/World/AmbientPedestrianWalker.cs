using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Lightweight ambient sidewalk walker for greybox scenes. It follows authored waypoint
    /// transforms and deliberately does not claim NavMesh pathfinding or crowd avoidance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AmbientPedestrianWalker : MonoBehaviour
    {
        [SerializeField] private Transform[] route;
        [SerializeField] private TrafficSignalController crossingSignal;
        [SerializeField, Min(0.1f)] private float walkingSpeed = 1.25f;
        [SerializeField, Min(0.1f)] private float turnSpeed = 360f;
        [SerializeField, Min(0f)] private float arrivalDistance = 0.45f;
        [SerializeField, Min(0f)] private float pauseAtWaypoint = 0.8f;

        private int _targetIndex;
        private float _resumeAt;

        public void Configure(Transform[] waypoints, float speed = 1.25f, float pause = 0.8f, float startDelay = 0f)
        {
            route = waypoints;
            walkingSpeed = Mathf.Max(0.1f, speed);
            pauseAtWaypoint = Mathf.Max(0f, pause);
            _targetIndex = route != null && route.Length > 1 ? 1 : 0;
            _resumeAt = Time.time + Mathf.Max(0f, startDelay);
        }

        public void ConfigureCrossingSignal(TrafficSignalController signal)
        {
            crossingSignal = signal;
        }

        private void Update()
        {
            if (crossingSignal != null && !crossingSignal.IsPedestrianWalkAllowed) return;
            if (route == null || route.Length < 2 || Time.time < _resumeAt) return;

            for (int attempts = 0; attempts < route.Length; attempts++)
            {
                Transform target = route[_targetIndex];
                if (target == null)
                {
                    AdvanceWaypoint();
                    continue;
                }

                Vector3 toTarget = target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude <= arrivalDistance * arrivalDistance)
                {
                    AdvanceWaypoint();
                    _resumeAt = Time.time + pauseAtWaypoint;
                    return;
                }

                Vector3 direction = toTarget.normalized;
                transform.position += direction * (walkingSpeed * Time.deltaTime);
                Quaternion facing = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, facing, turnSpeed * Time.deltaTime);
                return;
            }
        }

        private void AdvanceWaypoint()
        {
            _targetIndex = (_targetIndex + 1) % route.Length;
        }
    }
}
