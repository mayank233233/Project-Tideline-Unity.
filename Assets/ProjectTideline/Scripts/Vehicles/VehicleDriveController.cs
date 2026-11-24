using System;
using UnityEngine;

namespace ProjectTideline
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleDriveController : MonoBehaviour
    {
        [Serializable]
        public sealed class WheelBinding
        {
            public WheelCollider collider;
            public Transform visual;
            public bool steer;
            public bool driven = true;
        }

        [SerializeField] private WheelBinding[] wheels;
        [SerializeField] private float motorTorque = 210f;
        [SerializeField] private float brakeTorque = 620f;
        [SerializeField] private float maxSteerAngle = 29f;
        [SerializeField] private float handbrakeTorque = 950f;
        [SerializeField] private float downforce = 28f;
        [SerializeField] private Transform centerOfMass;
        [Header("Distant NPC Wheel Visual LOD")]
        [SerializeField] private float fullRateVisualDistance = 25f;
        [SerializeField] private float reducedRateVisualDistance = 60f;
        [SerializeField, Min(0.02f)] private float reducedVisualInterval = 0.1f;
        [SerializeField, Min(0.02f)] private float distantVisualInterval = 0.25f;
        private Rigidbody _body;
        private Transform _visualLodReference;
        private float _nextWheelVisualUpdateTime;
        private bool _isDriven;
        private bool _npcDriving;
        private float _npcSteer;
        private float _npcThrottle;
        private float _npcBrake;
        private float _nextCollisionSignalAt;

        public float SpeedKph => _body != null ? _body.linearVelocity.magnitude * 3.6f : 0f;
        public bool IsDriven => _isDriven;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            if (Camera.main != null) _visualLodReference = Camera.main.transform;
            if (centerOfMass != null) _body.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }

        public void SetDriven(bool value)
        {
            _isDriven = value;
            if (!value) return;
            _npcDriving = false;
            _npcSteer = 0f;
            _npcThrottle = 0f;
            _npcBrake = 0f;
        }

        public void SetNpcDriveInput(float steer, float throttle, float brake, bool active = true)
        {
            if (_isDriven) return;
            _npcDriving = active;
            _npcSteer = Mathf.Clamp(steer, -1f, 1f);
            _npcThrottle = Mathf.Clamp01(throttle);
            _npcBrake = Mathf.Clamp01(brake);
        }

        private void FixedUpdate()
        {
            if (wheels == null || wheels.Length == 0 || _body == null) return;
            PlayerInputRouter input = PlayerInputRouter.Instance;
            float steer = _isDriven && input != null ? input.Move.x : _npcDriving ? _npcSteer : 0f;
            float moveAxis = _isDriven && input != null ? input.Move.y : 0f;
            float throttle = _isDriven && input != null
                ? Mathf.Max(input.Throttle, Mathf.Max(0f, moveAxis))
                : _npcDriving ? _npcThrottle : 0f;
            float brake = _isDriven && input != null
                ? Mathf.Max(input.Brake, Mathf.Max(0f, -moveAxis))
                : _npcDriving ? _npcBrake : 0f;
            bool handbrake = _isDriven && input != null && input.HandbrakeHeld;

            foreach (WheelBinding wheel in wheels)
            {
                if (wheel == null || wheel.collider == null) continue;
                wheel.collider.steerAngle = wheel.steer ? steer * maxSteerAngle : 0f;
                wheel.collider.motorTorque = wheel.driven ? throttle * motorTorque : 0f;
                wheel.collider.brakeTorque = brake * brakeTorque;
                if (handbrake && !wheel.steer) wheel.collider.brakeTorque = handbrakeTorque;
            }
            _body.AddForce(-transform.up * downforce * _body.linearVelocity.magnitude, ForceMode.Force);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Only player-driven collisions change the fictional public-attention state.
            if (!_isDriven || collision == null || Time.time < _nextCollisionSignalAt) return;
            if (collision.relativeVelocity.sqrMagnitude < 64f) return;
            _nextCollisionSignalAt = Time.time + 2.5f;
            MissionSignalBus.Publish("vehicle.collision", 1);
        }

        private void LateUpdate()
        {
            if (wheels == null) return;
            if (!_isDriven && _visualLodReference != null)
            {
                float distanceSquared = (_visualLodReference.position - transform.position).sqrMagnitude;
                float fullRateSquared = fullRateVisualDistance * fullRateVisualDistance;
                float reducedRateSquared = reducedRateVisualDistance * reducedRateVisualDistance;
                float interval = distanceSquared <= fullRateSquared ? 0f
                    : distanceSquared <= reducedRateSquared ? Mathf.Max(0.02f, reducedVisualInterval)
                    : Mathf.Max(0.02f, distantVisualInterval);
                if (interval > 0f)
                {
                    if (Time.time < _nextWheelVisualUpdateTime) return;
                    _nextWheelVisualUpdateTime = Time.time + interval;
                }
            }
            foreach (WheelBinding wheel in wheels)
            {
                if (wheel == null || wheel.collider == null || wheel.visual == null) continue;
                wheel.collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
                wheel.visual.SetPositionAndRotation(position, rotation);
            }
        }
    }
}