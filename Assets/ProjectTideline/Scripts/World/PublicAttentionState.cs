using System;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Fictional city-reaction state. It models witness attention and story pressure only;
    /// it is not a real-world law-enforcement or evasion simulator.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public sealed class PublicAttentionState : MonoBehaviour
    {
        public enum AttentionBand { Calm, Noticed, Escalated }

        public static PublicAttentionState Instance { get; private set; }

        [SerializeField, Range(0, 100)] private int startingAttention;
        [SerializeField, Range(0, 100)] private int noticedThreshold = 25;
        [SerializeField, Range(0, 100)] private int escalatedThreshold = 60;
        [Header("Quiet-time pressure decay")]
        [SerializeField, Min(0f)] private float quietPeriodBeforeDecay = 22f;
        [SerializeField, Min(0.1f)] private float attentionDecayPerSecond = 1.1f;

        private int _value;
        private float _quietSeconds;
        private float _decayFraction;
        public int Value => _value;
        public AttentionBand Band => GetBand(_value);
        public event Action<int, AttentionBand> AttentionChanged;
        public event Action<int, AttentionBand> AttentionRestored;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _value = Mathf.Clamp(startingAttention, 0, 100);
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_value <= 0) return;
            _quietSeconds += Time.deltaTime;
            if (_quietSeconds < quietPeriodBeforeDecay) return;

            _decayFraction += Time.deltaTime * attentionDecayPerSecond;
            int decay = Mathf.FloorToInt(_decayFraction);
            if (decay <= 0) return;
            _decayFraction -= decay;

            AttentionBand previousBand = Band;
            _value = Mathf.Max(0, _value - decay);
            AttentionBand nextBand = Band;
            AttentionChanged?.Invoke(_value, nextBand);
            if (nextBand != previousBand)
                MissionSignalBus.Publish("world.attention.band." + nextBand.ToString().ToLowerInvariant(), 1);
        }

        private void OnEnable() => MissionSignalBus.Raised += OnMissionSignal;
        private void OnDisable() => MissionSignalBus.Raised -= OnMissionSignal;

        public void AddAttention(int amount)
        {
            if (amount <= 0) return;
            long candidate = (long)_value + amount;
            SetAttention((int)Math.Min(100L, candidate));
        }

        public void SetAttention(int value)
        {
            int next = Mathf.Clamp(value, 0, 100);
            if (next == _value) return;
            _quietSeconds = 0f;
            _decayFraction = 0f;
            AttentionBand previousBand = Band;
            _value = next;
            AttentionBand nextBand = Band;
            AttentionChanged?.Invoke(_value, nextBand);
            if (nextBand != previousBand)
                MissionSignalBus.Publish("world.attention.band." + nextBand.ToString().ToLowerInvariant(), 1);
        }

        /// <summary>Used by authored hub/mission outcomes; it is not a free-form stealth mechanic.</summary>
        public void ResolveAtGarage(int reduction)
        {
            if (reduction > 0) SetAttention(_value - reduction);
        }

        public static AttentionBand GetBand(int value)
        {
            if (value >= (Instance != null ? Instance.escalatedThreshold : 60)) return AttentionBand.Escalated;
            if (value >= (Instance != null ? Instance.noticedThreshold : 25)) return AttentionBand.Noticed;
            return AttentionBand.Calm;
        }

        public void Restore(int value)
        {
            _value = Mathf.Clamp(value, 0, 100);
            _quietSeconds = 0f;
            _decayFraction = 0f;
            // Restore the HUD state without making every nearby pedestrian react to a loaded save.
            AttentionRestored?.Invoke(_value, Band);
        }

        private void OnMissionSignal(string signalId, int amount)
        {
            if (amount <= 0) return;
            switch (signalId)
            {
                case "vehicle.taken": AddAttention((int)Math.Min(100L, 18L * amount)); break;
                case "vehicle.collision": AddAttention((int)Math.Min(100L, 8L * amount)); break;
                case "confrontation.resolved": AddAttention((int)Math.Min(100L, 10L * amount)); break;
                case "confrontation.failed": AddAttention((int)Math.Min(100L, 6L * amount)); break;
                case "old_quay.score.case_recovered": AddAttention((int)Math.Min(100L, 42L * amount)); break;
                case "old_quay.score.safehouse_reached": ResolveAtGarage(100); break;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}