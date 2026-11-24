using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTideline
{
    /// <summary>
    /// Converts fictional public-attention bands into a small, staged arcade police response.
    /// This is a gameplay system, not a real-world law-enforcement simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoliceResponseDirector : MonoBehaviour
    {
        [SerializeField] private PublicAttentionState attentionState;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Text responseText;
        [SerializeField, Min(0f)] private float calmClearDelay = 12f;
        [SerializeField, Min(1f)] private float firstUnitSpeed = 17f;
        [SerializeField, Min(1f)] private float secondUnitSpeed = 19f;

        private readonly List<PolicePursuitVehicle> _units = new List<PolicePursuitVehicle>(2);
        private float _calmTimer = -1f;
        private PublicAttentionState.AttentionBand _currentBand = PublicAttentionState.AttentionBand.Calm;

        public int ActiveUnitCount => _units.Count;
        public event Action<int, PublicAttentionState.AttentionBand> ResponseChanged;

        public void Configure(PublicAttentionState state, Transform target, Text label)
        {
            if (attentionState != null)
                attentionState.AttentionChanged -= OnAttentionChanged;

            attentionState = state;
            playerTarget = target;
            responseText = label;

            if (isActiveAndEnabled && attentionState != null)
                attentionState.AttentionChanged += OnAttentionChanged;

            RefreshFromState();
        }

        private void OnEnable()
        {
            if (attentionState != null)
                attentionState.AttentionChanged += OnAttentionChanged;
            RefreshFromState();
        }

        private void OnDisable()
        {
            if (attentionState != null)
                attentionState.AttentionChanged -= OnAttentionChanged;
            ClearUnits();
        }

        private void Update()
        {
            if (_calmTimer < 0f) return;
            _calmTimer -= Time.deltaTime;
            if (_calmTimer <= 0f)
            {
                _calmTimer = -1f;
                ClearUnits();
                RenderStatus();
                ResponseChanged?.Invoke(0, _currentBand);
            }
        }

        private void OnAttentionChanged(int value, PublicAttentionState.AttentionBand band)
        {
            ApplyBand(band);
        }

        private void RefreshFromState()
        {
            if (attentionState == null) return;
            ApplyBand(attentionState.Band);
        }

        private void ApplyBand(PublicAttentionState.AttentionBand band)
        {
            _currentBand = band;
            if (band == PublicAttentionState.AttentionBand.Calm)
            {
                if (_units.Count > 0 && _calmTimer < 0f)
                    _calmTimer = calmClearDelay;
                RenderStatus();
                return;
            }

            _calmTimer = -1f;
            EnsureUnitCount(band == PublicAttentionState.AttentionBand.Escalated ? 2 : 1);
            RenderStatus();
            ResponseChanged?.Invoke(_units.Count, band);
        }

        private void EnsureUnitCount(int required)
        {
            if (playerTarget == null) return;
            while (_units.Count < required)
            {
                int index = _units.Count;
                Vector3 right = playerTarget.right * (index == 0 ? -5f : 5f);
                Vector3 spawnPosition = playerTarget.position - playerTarget.forward * (22f + index * 8f) + right;
                // The Old Quay blockout road is at world Y=0; keep the patrol tyres grounded.
                spawnPosition.y = 0.05f;
                Quaternion spawnRotation = Quaternion.LookRotation(
                    (playerTarget.position - spawnPosition).normalized, Vector3.up);
                PolicePursuitVehicle unit = PolicePursuitVehicle.Create(
                    index == 0 ? "PORT SABLE — patrol response A" : "PORT SABLE — patrol response B",
                    spawnPosition, spawnRotation, playerTarget,
                    index == 0 ? firstUnitSpeed : secondUnitSpeed, 7f + index * 2f);
                _units.Add(unit);
            }
        }

        private void ClearUnits()
        {
            for (int i = 0; i < _units.Count; i++)
                if (_units[i] != null)
                    Destroy(_units[i].gameObject);
            _units.Clear();
        }

        private void RenderStatus()
        {
            if (responseText == null) return;
            if (_units.Count == 0)
                responseText.text = _currentBand == PublicAttentionState.AttentionBand.Calm
                    ? "RESPONSE: NONE" : "RESPONSE: AWAITING TARGET";
            else if (_units.Count == 1)
                responseText.text = "RESPONSE: PATROL DISPATCHED";
            else
                responseText.text = "RESPONSE: MULTIPLE PATROLS";
        }

        private void OnDestroy()
        {
            if (attentionState != null)
                attentionState.AttentionChanged -= OnAttentionChanged;
        }
    }
}