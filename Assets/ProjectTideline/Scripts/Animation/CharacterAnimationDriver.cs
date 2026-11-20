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
        private int _speedHash;

        private void Awake()
        {
            if (animators == null || animators.Length == 0) animators = GetComponentsInChildren<Animator>(true);
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
            _speedHash = Animator.StringToHash(speedParameter);
        }

        private void Update()
        {
            if (animators == null || animators.Length == 0) return;
            float speed = 0f;
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                speed = navMeshAgent.velocity.magnitude;
            else if (characterController != null && characterController.enabled)
                speed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;
            foreach (Animator animator in animators)
                if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
                    animator.SetFloat(_speedHash, speed * speedScale, dampTime, Time.deltaTime);
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