using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Shows sprint/jump controls only while the player motor is active on foot.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class OnFootControlVisibility : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float refreshSeconds = 0.15f;
        [SerializeField] private ThirdPersonMotor playerMotor;

        private CanvasGroup _group;
        private float _nextRefresh;
        private bool _visible;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            RefreshVisibility();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + refreshSeconds;
            bool onFoot = playerMotor != null && playerMotor.isActiveAndEnabled;
            if (onFoot != _visible) SetVisible(onFoot);
        }

        public void SetPlayerMotor(ThirdPersonMotor value)
        {
            playerMotor = value;
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool onFoot = playerMotor != null && playerMotor.isActiveAndEnabled;
            SetVisible(onFoot);
            _nextRefresh = Time.unscaledTime + refreshSeconds;
        }

        private void SetVisible(bool visible)
        {
            _visible = visible;
            if (_group == null) return;
            _group.alpha = visible ? 1f : 0f;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
        }
    }
}