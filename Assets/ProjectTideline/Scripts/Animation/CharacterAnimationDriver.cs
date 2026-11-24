using UnityEngine;
using UnityEngine.AI;

namespace ProjectTideline
{
    /// <summary>Drives imported character LOD rigs from actual controller/agent velocity.</summary>
    public sealed class CharacterAnimationDriver : MonoBehaviour
    {
        [SerializeField] private Animator[] animators;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private string speedParameter = "MoveSpeed";
        [SerializeField] private float dampTime = 0.12f;
        [SerializeField] private float speedScale = 1f;
        [Header("Mobile distance update budget")]
        [SerializeField] private Transform lodReference;
        [SerializeField, Min(0f)] private float fullRateDistance = 18f;
        [SerializeField, Min(0f)] private float reducedRateDistance = 45f;
        [SerializeField, Min(0.02f)] private float reducedRateInterval = 0.1f;
        [SerializeField, Min(0.05f)] private float distantRateInterval = 0.25f;
        private int _speedHash;
        private float _nextAnimationUpdateTime;
        private float _lastAnimationUpdateTime;
        private float _fullRateDistanceSqr;
        private float _reducedRateDistanceSqr;

        private void Awake()
        {
            if (animators == null || animators.Length == 0) animators = GetComponentsInChildren<Animator>(true);
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
            _speedHash = Animator.StringToHash(speedParameter);
            if (lodReference == null && Camera.main != null) lodReference = Camera.main.transform;
            _fullRateDistanceSqr = fullRateDistance * fullRateDistance;
            float reducedDistance = Mathf.Max(fullRateDistance, reducedRateDistance);
            _reducedRateDistanceSqr = reducedDistance * reducedDistance;
            _lastAnimationUpdateTime = Time.time;
        }

        private void Update()
        {
            if (animators == null || animators.Length == 0) return;

            float updateInterval = GetDistanceUpdateInterval();
            if (Time.time < _nextAnimationUpdateTime) return;
            float animationDeltaTime = Mathf.Max(Time.deltaTime, Time.time - _lastAnimationUpdateTime);
            _lastAnimationUpdateTime = Time.time;
            _nextAnimationUpdateTime = Time.time + updateInterval;
            float speed = 0f;
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                speed = navMeshAgent.velocity.magnitude;
            else if (characterController != null && characterController.enabled)
                speed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;
            foreach (Animator animator in animators)
                if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
                    animator.SetFloat(_speedHash, speed * speedScale, dampTime, animationDeltaTime);
        }

        private float GetDistanceUpdateInterval()
        {
            if (lodReference == null) return 0f;
            float distanceSqr = (lodReference.position - transform.position).sqrMagnitude;
            if (distanceSqr <= _fullRateDistanceSqr) return 0f;
            if (distanceSqr <= _reducedRateDistanceSqr) return Mathf.Max(0.02f, reducedRateInterval);
            return Mathf.Max(0.05f, distantRateInterval);
        }

        public void Trigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName) || animators == null) return;
            int hash = Animator.StringToHash(triggerName);
            foreach (Animator animator in animators)
                if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
                    animator.SetTrigger(hash);
        }

        public void SetAnimators(Animator[] values) => animators = values;
    }
}