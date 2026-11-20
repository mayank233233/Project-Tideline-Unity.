using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectTideline
{
    /// <summary>
    /// Transfers player control between separate lead actors. This is not a visual/nameplate swap:
    /// each slot requires its own scene actor, CharacterController, ThirdPersonMotor,
    /// ProximityInteractor, CharacterAnimationDriver and valid animated child model.
    /// Switching is refused during dialogue, cinematic confrontation or vehicle occupancy.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class PlayableLeadSwitcher : MonoBehaviour
    {
        [Serializable]
        public sealed class LeadSlot
        {
            public string canonId;
            public GameObject actorRoot;
            public bool unlocked = true;
            public Vector3 cameraTargetOffset = new Vector3(0f, 1.45f, 0f);
        }

        private sealed class RuntimeSlot
        {
            public LeadSlot Definition;
            public CharacterController CharacterController;
            public ThirdPersonMotor Motor;
            public ProximityInteractor Interactor;
            public CharacterAnimationDriver AnimationDriver;
        }

        public static PlayableLeadSwitcher Instance { get; private set; }

        [SerializeField] private LeadSlot[] leads;
        [SerializeField] private string startingLeadId = "mara_kade";
        [SerializeField] private PlayerInputRouter inputRouter;
        [SerializeField] private ThirdPersonCameraRig cameraRig;
        [SerializeField] private OnFootControlVisibility onFootControlVisibility;
        [SerializeField] private bool enableKeyboardCycleInEditor = true;

        private readonly List<RuntimeSlot> _runtimeSlots = new List<RuntimeSlot>(3);
        private int _activeIndex = -1;
        private bool _initialized;

        public event Action<string> ActiveLeadChanged;
        public string ActiveLeadId => GetActiveSlot() != null ? GetActiveSlot().Definition.canonId : string.Empty;
        public GameObject ActiveActor => GetActiveSlot() != null ? GetActiveSlot().Definition.actorRoot : null;
        public bool IsInitialized => _initialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start() => InitializeRoster();

        private void Update()
        {
            if (!_initialized || !enableKeyboardCycleInEditor || Keyboard.current == null) return;
            if (Keyboard.current.tabKey.wasPressedThisFrame) RequestNextLead();
        }

        public bool InitializeRoster()
        {
            if (_initialized) return true;
            if (inputRouter == null) inputRouter = PlayerInputRouter.Instance;
            if (cameraRig == null) cameraRig = ThirdPersonCameraRig.Instance;
            if (onFootControlVisibility == null) onFootControlVisibility = FindFirstObjectByType<OnFootControlVisibility>();

            var issues = new List<string>();
            if (inputRouter == null) issues.Add("PlayerInputRouter is missing.");
            if (cameraRig == null) issues.Add("ThirdPersonCameraRig is missing.");
            if (leads == null || leads.Length != CampaignCanon.Leads.Count)
                issues.Add("Exactly three lead actor slots are required.");

            var canonIds = new HashSet<string>(StringComparer.Ordinal);
            var slotIds = new HashSet<string>(StringComparer.Ordinal);
            var actorInstanceIds = new HashSet<int>();
            _runtimeSlots.Clear();

            if (leads != null)
            {
                foreach (LeadSlot definition in leads)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.canonId) || definition.actorRoot == null)
                    {
                        issues.Add("A lead slot is missing its canon ID or actor root.");
                        continue;
                    }

                    if (!slotIds.Add(definition.canonId))
                        issues.Add("Duplicate lead actor slot: " + definition.canonId);
                    if (!actorInstanceIds.Add(definition.actorRoot.GetInstanceID()))
                        issues.Add("Each playable lead must have a separate actor root; shared actor root found at " + definition.canonId + ".");

                    bool knownLead = false;
                    foreach (CampaignCanon.Character lead in CampaignCanon.Leads)
                        if (string.Equals(lead.Id, definition.canonId, StringComparison.Ordinal)) knownLead = true;
                    if (!knownLead) issues.Add("Lead slot is not in CampaignCanon: " + definition.canonId);

                    CharacterController controller = definition.actorRoot.GetComponent<CharacterController>();
                    ThirdPersonMotor motor = definition.actorRoot.GetComponent<ThirdPersonMotor>();
                    ProximityInteractor interactor = definition.actorRoot.GetComponent<ProximityInteractor>();
                    CharacterAnimationDriver animationDriver = definition.actorRoot.GetComponent<CharacterAnimationDriver>();
                    Animator[] animators = definition.actorRoot.GetComponentsInChildren<Animator>(true);
                    bool hasUsableAnimator = false;
                    foreach (Animator animator in animators)
                        if (animator != null && animator.runtimeAnimatorController != null) hasUsableAnimator = true;

                    if (controller == null || motor == null || interactor == null || animationDriver == null || !hasUsableAnimator)
                    {
                        issues.Add("Lead " + definition.canonId + " needs root CharacterController, ThirdPersonMotor, ProximityInteractor and CharacterAnimationDriver, plus at least one child Animator with a RuntimeAnimatorController.");
                        continue;
                    }

                    _runtimeSlots.Add(new RuntimeSlot
                    {
                        Definition = definition,
                        CharacterController = controller,
                        Motor = motor,
                        Interactor = interactor,
                        AnimationDriver = animationDriver
                    });
                    canonIds.Add(definition.canonId);
                }
            }

            foreach (CampaignCanon.Character lead in CampaignCanon.Leads)
                if (!canonIds.Contains(lead.Id)) issues.Add("No actor slot is assigned for playable lead " + lead.Id + ".");

            if (issues.Count > 0)
            {
                Debug.LogError("Playable lead roster initialization blocked:\n" + string.Join("\n", issues), this);
                return false;
            }

            foreach (RuntimeSlot slot in _runtimeSlots)
            {
                slot.Definition.actorRoot.SetActive(true);
                slot.CharacterController.enabled = false;
                slot.Motor.enabled = false;
                slot.Interactor.enabled = false;
                slot.AnimationDriver.enabled = false;
                slot.Definition.actorRoot.tag = "Untagged";
            }

            _initialized = true;
            if (!SwitchToLeadInternal(startingLeadId, true))
            {
                _initialized = false;
                Debug.LogError("Playable lead roster could not activate starting lead: " + startingLeadId, this);
                return false;
            }
            return true;
        }

        public bool RequestNextLead() => SwitchByOffset(1);
        public bool RequestPreviousLead() => SwitchByOffset(-1);

        public bool SwitchToLead(string canonId)
        {
            if (!_initialized) return false;
            return SwitchToLeadInternal(canonId, false);
        }

        /// <summary>Used by save restoration after the roster has initialized.</summary>
        public bool RestoreLead(string canonId)
        {
            if (!_initialized) return false;
            return SwitchToLeadInternal(canonId, true);
        }

        private bool SwitchByOffset(int offset)
        {
            if (!_initialized || _runtimeSlots.Count == 0 || IsControlHandoffBlocked()) return false;
            RuntimeSlot active = GetActiveSlot();
            if (active == null || !active.Interactor.CanSwitchLead) return false;

            int start = _activeIndex;
            for (int step = 1; step <= _runtimeSlots.Count; step++)
            {
                int candidate = (start + offset * step) % _runtimeSlots.Count;
                if (candidate < 0) candidate += _runtimeSlots.Count;
                if (candidate == _activeIndex) continue;
                if (_runtimeSlots[candidate].Definition.unlocked)
                    return SwitchToLeadInternal(_runtimeSlots[candidate].Definition.canonId, false);
            }
            return false;
        }

        private bool SwitchToLeadInternal(string canonId, bool allowLocked)
        {
            RuntimeSlot target = FindSlot(canonId);
            if (target == null || (!allowLocked && !target.Definition.unlocked)) return false;

            if (_activeIndex >= 0 && _activeIndex < _runtimeSlots.Count)
            {
                RuntimeSlot current = _runtimeSlots[_activeIndex];
                if (current == target) return true;
                if (IsControlHandoffBlocked() || !current.Interactor.CanSwitchLead) return false;
                current.Interactor.enabled = false;
                current.Motor.enabled = false;
                current.AnimationDriver.enabled = false;
                current.CharacterController.enabled = false;
                current.Definition.actorRoot.tag = "Untagged";
            }

            if (inputRouter != null) inputRouter.ClearTransientInput();
            target.Definition.actorRoot.SetActive(true);
            target.CharacterController.enabled = true;
            target.Motor.ResetControlState();
            target.AnimationDriver.enabled = true;
            target.Motor.SetMovementCamera(cameraRig != null ? cameraRig.transform : null);
            target.Interactor.BindPlayer(target.Definition.actorRoot.transform, cameraRig != null ? cameraRig.transform : null);
            target.Interactor.enabled = true;
            target.Motor.enabled = true;
            target.Definition.actorRoot.tag = "Player";

            _activeIndex = _runtimeSlots.IndexOf(target);
            if (cameraRig != null)
                cameraRig.SetTarget(target.Definition.actorRoot.transform, target.Definition.cameraTargetOffset, true);
            if (onFootControlVisibility != null) onFootControlVisibility.SetPlayerMotor(target.Motor);
            ActiveLeadChanged?.Invoke(target.Definition.canonId);
            return true;
        }

        /// <summary>True only when the active lead is in a stable on-foot state.</summary>
        public bool CanPersistActiveLead
        {
            get
            {
                RuntimeSlot active = GetActiveSlot();
                return active != null && active.Interactor != null && active.Interactor.CanSwitchLead;
            }
        }

        /// <summary>Captures the last world-space pose for every playable lead.</summary>
        public LeadTransformSnapshot[] CaptureLeadTransforms()
        {
            if (!_initialized) return Array.Empty<LeadTransformSnapshot>();
            var snapshots = new LeadTransformSnapshot[_runtimeSlots.Count];
            for (int i = 0; i < _runtimeSlots.Count; i++)
            {
                Transform actor = _runtimeSlots[i].Definition.actorRoot.transform;
                Vector3 position = actor.position;
                snapshots[i] = new LeadTransformSnapshot
                {
                    leadId = _runtimeSlots[i].Definition.canonId,
                    x = position.x,
                    y = position.y,
                    z = position.z,
                    yaw = actor.eulerAngles.y
                };
            }
            return snapshots;
        }

        /// <summary>Restores known lead poses. Unknown IDs and non-finite transforms are ignored.</summary>
        public int RestoreLeadTransforms(LeadTransformSnapshot[] snapshots)
        {
            if (!_initialized || snapshots == null) return 0;
            int restored = 0;
            foreach (LeadTransformSnapshot snapshot in snapshots)
            {
                if (snapshot == null || float.IsNaN(snapshot.x) || float.IsInfinity(snapshot.x)
                    || float.IsNaN(snapshot.y) || float.IsInfinity(snapshot.y)
                    || float.IsNaN(snapshot.z) || float.IsInfinity(snapshot.z)
                    || float.IsNaN(snapshot.yaw) || float.IsInfinity(snapshot.yaw))
                    continue;

                RuntimeSlot slot = FindSlot(snapshot.leadId);
                if (slot == null) continue;
                slot.Definition.actorRoot.transform.SetPositionAndRotation(
                    new Vector3(snapshot.x, snapshot.y, snapshot.z),
                    Quaternion.Euler(0f, snapshot.yaw, 0f));
                restored++;
            }
            return restored;
        }

        private bool IsControlHandoffBlocked()
        {
            if (inputRouter != null && inputRouter.GameplayBlocked) return true;
            CinematicConfrontationController confrontation = FindFirstObjectByType<CinematicConfrontationController>();
            return confrontation != null && confrontation.IsActive;
        }

        private RuntimeSlot FindSlot(string canonId)
        {
            if (string.IsNullOrWhiteSpace(canonId)) return null;
            foreach (RuntimeSlot slot in _runtimeSlots)
                if (string.Equals(slot.Definition.canonId, canonId, StringComparison.Ordinal)) return slot;
            return null;
        }

        private RuntimeSlot GetActiveSlot()
        {
            return _activeIndex >= 0 && _activeIndex < _runtimeSlots.Count ? _runtimeSlots[_activeIndex] : null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}