using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Lightweight fictional arcade pursuit vehicle. It follows the player actor in-game;
    /// it is not intended to model real police tactics or real-world vehicle handling.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class PolicePursuitVehicle : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(1f)] private float topSpeed = 19f;
        [SerializeField, Min(0.1f)] private float acceleration = 8f;
        [SerializeField, Min(1f)] private float turnDegreesPerSecond = 105f;
        [SerializeField, Min(1f)] private float followDistance = 8f;
        [SerializeField, Min(0.1f)] private float braking = 11f;

        private Rigidbody _body;

        public void Configure(Transform chaseTarget, float speed, float followGap)
        {
            target = chaseTarget;
            topSpeed = Mathf.Max(1f, speed);
            followDistance = Mathf.Max(1f, followGap);
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.mass = 1450f;
            _body.linearDamping = 0.18f;
            _body.angularDamping = 3f;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void FixedUpdate()
        {
            if (target == null || _body == null)
            {
                ApplyBrakes();
                return;
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance < 0.01f)
            {
                ApplyBrakes();
                return;
            }

            Vector3 direction = offset / distance;
            Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion nextRotation = Quaternion.RotateTowards(
                _body.rotation, desiredRotation, turnDegreesPerSecond * Time.fixedDeltaTime);
            _body.MoveRotation(nextRotation);

            Vector3 planarVelocity = _body.linearVelocity;
            planarVelocity.y = 0f;
            float forwardSpeed = Vector3.Dot(planarVelocity, transform.forward);

            if (distance <= followDistance)
            {
                ApplyBrakes();
                return;
            }

            float turnPenalty = Mathf.Clamp01(Vector3.Angle(transform.forward, direction) / 100f);
            float desiredSpeed = Mathf.Lerp(topSpeed, topSpeed * 0.42f, turnPenalty);
            if (forwardSpeed < desiredSpeed)
                _body.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
            else if (forwardSpeed > desiredSpeed + 1.5f)
                _body.AddForce(-transform.forward * braking, ForceMode.Acceleration);

            Vector3 velocity = _body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontal.magnitude > topSpeed)
            {
                horizontal = horizontal.normalized * topSpeed;
                _body.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
            }
        }

        private void ApplyBrakes()
        {
            if (_body == null) return;
            Vector3 velocity = _body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, braking * Time.fixedDeltaTime);
            _body.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
        }

        public static PolicePursuitVehicle Create(string unitName, Vector3 position,
            Quaternion rotation, Transform chaseTarget, float speed, float followGap)
        {
            GameObject root = new GameObject(unitName);
            root.transform.SetPositionAndRotation(position, rotation);
            BoxCollider bodyCollider = root.AddComponent<BoxCollider>();
            bodyCollider.center = new Vector3(0f, 0.72f, 0f);
            bodyCollider.size = new Vector3(1.95f, 1.05f, 4.25f);
            root.AddComponent<Rigidbody>();

            CreateVisual(root.transform, "Patrol body", new Vector3(0f, 0.78f, 0f),
                new Vector3(1.9f, 0.62f, 3.8f), new Color(0.075f, 0.11f, 0.15f));
            CreateVisual(root.transform, "Patrol cabin", new Vector3(0f, 1.35f, -0.05f),
                new Vector3(1.52f, 0.58f, 1.85f), new Color(0.16f, 0.20f, 0.23f));
            CreateVisual(root.transform, "Roof lightbar", new Vector3(0f, 1.69f, 0.12f),
                new Vector3(1.05f, 0.12f, 0.28f), new Color(0.12f, 0.28f, 0.9f));
            CreateVisual(root.transform, "Left red light", new Vector3(-0.34f, 1.72f, 0.12f),
                new Vector3(0.28f, 0.1f, 0.22f), new Color(0.95f, 0.08f, 0.06f));
            CreateVisual(root.transform, "Right blue light", new Vector3(0.34f, 1.72f, 0.12f),
                new Vector3(0.28f, 0.1f, 0.22f), new Color(0.06f, 0.24f, 0.98f));
            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Patrol tyre";
                    wheel.transform.SetParent(root.transform, false);
                    wheel.transform.localPosition = new Vector3(side * 0.96f, 0.34f, axle * 1.28f);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    wheel.transform.localScale = new Vector3(0.23f, 0.34f, 0.34f);
                    Collider wheelCollider = wheel.GetComponent<Collider>();
                    if (wheelCollider != null) Object.Destroy(wheelCollider);
                    Renderer wheelRenderer = wheel.GetComponent<Renderer>();
                    if (wheelRenderer != null) wheelRenderer.material.color = new Color(0.025f, 0.03f, 0.035f);
                }
            }

            PolicePursuitVehicle pursuit = root.AddComponent<PolicePursuitVehicle>();
            pursuit.Configure(chaseTarget, speed, followGap);
            return pursuit;
        }

        private static void CreateVisual(Transform parent, string objectName, Vector3 localPosition,
            Vector3 localScale, Color color)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = objectName;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null) Object.Destroy(visualCollider);
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
        }
    }
}