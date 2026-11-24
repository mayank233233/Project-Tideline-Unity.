using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Procedural walk cycle for the low-poly Old Quay pedestrian rigs.</summary>
    [DisallowMultipleComponent]
    public sealed class AmbientPedestrianAnimation : MonoBehaviour
    {
        [SerializeField] private Transform[] arms;
        [SerializeField] private Transform[] legs;
        [SerializeField, Min(0f)] private float cycleSpeed = 7f;
        [SerializeField, Range(0f, 40f)] private float armSwingDegrees = 25f;
        [SerializeField, Range(0f, 35f)] private float legSwingDegrees = 21f;

        private Vector3 _previousPosition;
        private float _phase;

        public void Configure(Transform[] armPivots, Transform[] legPivots)
        {
            arms = armPivots;
            legs = legPivots;
            _previousPosition = transform.position;
        }

        private void OnEnable()
        {
            _previousPosition = transform.position;
        }

        private void Update()
        {
            Vector3 currentPosition = transform.position;
            bool moving = (currentPosition - _previousPosition).sqrMagnitude >= 0.000001f;
            _previousPosition = currentPosition;
            if (!moving)
            {
                SetSwing(0f);
                return;
            }

            _phase += Time.deltaTime * cycleSpeed;
            SetSwing(Mathf.Sin(_phase));
        }

        private void SetSwing(float amount)
        {
            if (arms != null)
                for (int i = 0; i < arms.Length; i++)
                    if (arms[i] != null)
                        arms[i].localRotation = Quaternion.Euler(amount * armSwingDegrees * (i == 0 ? 1f : -1f), 0f, 0f);

            if (legs != null)
                for (int i = 0; i < legs.Length; i++)
                    if (legs[i] != null)
                        legs[i].localRotation = Quaternion.Euler(amount * legSwingDegrees * (i == 0 ? -1f : 1f), 0f, 0f);
        }
    }
}
