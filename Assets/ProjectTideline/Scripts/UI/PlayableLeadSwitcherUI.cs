using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>Mobile-friendly controls for switching between the three playable leads.</summary>
    public sealed class PlayableLeadSwitcherUI : MonoBehaviour
    {
        [SerializeField] private Button previousLeadButton;
        [SerializeField] private Button nextLeadButton;
        [SerializeField] private Text activeLeadLabel;

        private PlayableLeadSwitcher _switcher;

        private void Start()
        {
            _switcher = PlayableLeadSwitcher.Instance;
            if (previousLeadButton != null) previousLeadButton.onClick.AddListener(SwitchPrevious);
            if (nextLeadButton != null) nextLeadButton.onClick.AddListener(SwitchNext);
            if (_switcher != null) _switcher.ActiveLeadChanged += OnActiveLeadChanged;
            RefreshLabel();
            RefreshButtons();
        }

        private void OnEnable()
        {
            RefreshLabel();
            RefreshButtons();
        }

        private void SwitchPrevious()
        {
            if (_switcher != null) _switcher.RequestPreviousLead();
            RefreshLabel();
            RefreshButtons();
        }

        private void SwitchNext()
        {
            if (_switcher != null) _switcher.RequestNextLead();
            RefreshLabel();
            RefreshButtons();
        }

        private void OnActiveLeadChanged(string leadId)
        {
            RefreshLabel();
            RefreshButtons();
        }

        private void RefreshLabel()
        {
            if (activeLeadLabel == null) return;
            string leadId = _switcher != null ? _switcher.ActiveLeadId : string.Empty;
            activeLeadLabel.text = string.IsNullOrEmpty(leadId) ? "NO ACTIVE LEAD" : DisplayNameFor(leadId);
        }

        private void RefreshButtons()
        {
            bool ready = _switcher != null && _switcher.IsInitialized;
            if (previousLeadButton != null) previousLeadButton.interactable = ready;
            if (nextLeadButton != null) nextLeadButton.interactable = ready;
        }

        private static string DisplayNameFor(string leadId)
        {
            foreach (CampaignCanon.Character lead in CampaignCanon.Leads)
                if (lead.Id == leadId) return lead.DisplayName.ToUpperInvariant();
            return leadId.ToUpperInvariant();
        }

        private void OnDestroy()
        {
            if (previousLeadButton != null) previousLeadButton.onClick.RemoveListener(SwitchPrevious);
            if (nextLeadButton != null) nextLeadButton.onClick.RemoveListener(SwitchNext);
            if (_switcher != null) _switcher.ActiveLeadChanged -= OnActiveLeadChanged;
        }
    }
}