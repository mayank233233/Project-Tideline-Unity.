using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>
    /// Animation-driven, non-graphic confrontation minigame. It uses short response cues and authored
    /// animation states only; it does not model real fighting techniques or physical injury.
    /// </summary>
    public sealed class CinematicConfrontationController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup promptPanel;
        [SerializeField] private Text promptText;
        [SerializeField, Range(1, 5)] private int successfulBeatsRequired = 3;
        [SerializeField, Min(0.25f)] private float responseWindowSeconds = 1.1f;
        [SerializeField, Min(0.1f)] private float cueDelayMin = 0.35f;
        [SerializeField, Min(0.1f)] private float cueDelayMax = 0.8f;
        [SerializeField] private string playerBeatTrigger = "ConfrontBeat";
        [SerializeField] private string opponentBeatTrigger = "ConfrontBeat";
        [SerializeField] private string opponentYieldTrigger = "ConfrontYield";
        [SerializeField] private string playerFailTrigger = "ConfrontStumble";

        private Animator _playerAnimator;
        private Animator _opponentAnimator;
        private ThirdPersonMotor _playerMotor;
        private float _cueAt;
        private float _deadline;
        private int _beats;
        private bool _active;
        private bool _cueVisible;

        public bool IsActive => _active;

        private void Awake() => SetPrompt(false, string.Empty);

        public bool Begin(GameObject actor, Animator opponentAnimator)
        {
            if (_active || actor == null) return false;
            _playerAnimator = actor.GetComponentInChildren<Animator>();
            _opponentAnimator = opponentAnimator;
            if (_playerAnimator == null || _opponentAnimator == null)
            {
                _playerAnimator = null;
                _opponentAnimator = null;
                return false;
            }

            if (PlayerInputRouter.Instance != null)
            {
                PlayerInputRouter.Instance.ConsumeConfront();
                PlayerInputRouter.Instance.ConsumeBreakAway();
                PlayerInputRouter.Instance.SetMove(Vector2.zero);
                PlayerInputRouter.Instance.SetSprint(false);
            }
            _playerMotor = actor.GetComponent<ThirdPersonMotor>();
            if (_playerMotor != null) _playerMotor.enabled = false;
            _beats = 0;
            _active = true;
            ScheduleBeat();
            SetPrompt(true, "Stay focused — wait for the cue");
            return true;
        }

        private void Update()
        {
            if (!_active || PlayerInputRouter.Instance == null) return;
            if (PlayerInputRouter.Instance.ConsumeBreakAway())
            {
                Finish(false, true);
                return;
            }

            bool pressed = PlayerInputRouter.Instance.ConsumeConfront();
            if (pressed)
            {
                if (!_cueVisible) { Finish(false, false); return; }
                _beats++;
                if (_playerAnimator != null && !string.IsNullOrEmpty(playerBeatTrigger)) _playerAnimator.SetTrigger(playerBeatTrigger);
                if (_opponentAnimator != null && !string.IsNullOrEmpty(opponentBeatTrigger)) _opponentAnimator.SetTrigger(opponentBeatTrigger);
                if (_beats >= successfulBeatsRequired) { Finish(true, false); return; }
                ScheduleBeat();
            }
            else if (_cueVisible && Time.time > _deadline)
            {
                Finish(false, false);
            }
            else if (!_cueVisible && Time.time >= _cueAt)
            {
                _cueVisible = true;
                _deadline = Time.time + responseWindowSeconds;
                SetPrompt(true, "NOW — tap CONFRONT");
            }
        }

        private void ScheduleBeat()
        {
            _cueVisible = false;
            _cueAt = Time.time + Random.Range(cueDelayMin, Mathf.Max(cueDelayMin, cueDelayMax));
            _deadline = _cueAt + responseWindowSeconds;
            SetPrompt(true, "Wait for the cue");
        }

        private void Finish(bool success, bool withdrew)
        {
            _active = false;
            SetPrompt(false, string.Empty);
            if (_playerAnimator != null && !success && !withdrew && !string.IsNullOrEmpty(playerFailTrigger))
                _playerAnimator.SetTrigger(playerFailTrigger);
            if (_opponentAnimator != null && success && !string.IsNullOrEmpty(opponentYieldTrigger))
                _opponentAnimator.SetTrigger(opponentYieldTrigger);

            if (success) MissionSignalBus.Publish("confrontation.resolved", 1);
            else if (withdrew) MissionSignalBus.Publish("confrontation.withdrawn", 1);
            else MissionSignalBus.Publish("confrontation.failed", 1);
            if (_playerMotor != null) _playerMotor.enabled = true;
            _playerMotor = null;
            _playerAnimator = null;
            _opponentAnimator = null;
        }

        private void SetPrompt(bool visible, string message)
        {
            if (promptPanel != null)
            {
                promptPanel.alpha = visible ? 1f : 0f;
                promptPanel.interactable = visible;
                promptPanel.blocksRaycasts = visible;
            }
            if (promptText != null) promptText.text = message;
        }

        private void OnDisable()
        {
            if (_active) Finish(false, true);
        }
    }
}