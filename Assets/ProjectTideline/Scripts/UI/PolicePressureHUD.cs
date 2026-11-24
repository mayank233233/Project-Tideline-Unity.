using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>
    /// Displays the existing fictional public-attention value as a compact police-pressure
    /// indicator. It is presentation only: it does not spawn officers or implement pursuits.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PolicePressureHUD : MonoBehaviour
    {
        [SerializeField] private PublicAttentionState attentionState;
        [SerializeField] private Text pressureText;

        private bool _subscribed;

        public void Configure(PublicAttentionState state, Text label)
        {
            attentionState = state;
            pressureText = label;
            if (isActiveAndEnabled)
                Subscribe();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed || attentionState == null)
                return;

            attentionState.AttentionChanged += OnAttentionChanged;
            attentionState.AttentionRestored += OnAttentionRestored;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || attentionState == null)
                return;

            attentionState.AttentionChanged -= OnAttentionChanged;
            attentionState.AttentionRestored -= OnAttentionRestored;
            _subscribed = false;
        }

        private void OnAttentionChanged(int value, PublicAttentionState.AttentionBand band)
        {
            Render(value, band);
        }

        private void OnAttentionRestored(int value, PublicAttentionState.AttentionBand band)
        {
            Render(value, band);
        }

        private void Refresh()
        {
            if (attentionState != null)
                Render(attentionState.Value, attentionState.Band);
        }

        private void Render(int value, PublicAttentionState.AttentionBand band)
        {
            if (pressureText == null)
                return;

            string status;
            int level;
            switch (band)
            {
                case PublicAttentionState.AttentionBand.Escalated:
                    status = "ELEVATED";
                    level = 2;
                    break;
                case PublicAttentionState.AttentionBand.Noticed:
                    status = "NOTICED";
                    level = 1;
                    break;
                default:
                    status = "LOW";
                    level = 0;
                    break;
            }

            pressureText.text = "POLICE PRESSURE  " + level + "/2\n" + status;
        }
    }
}