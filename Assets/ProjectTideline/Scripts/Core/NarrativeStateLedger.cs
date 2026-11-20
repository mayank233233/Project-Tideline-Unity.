using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Stores compact integer flags for authored narrative choices. This is campaign state,
    /// not a real-world crime, surveillance or law-enforcement simulation.
    /// </summary>
    [DefaultExecutionOrder(-45)]
    public sealed class NarrativeStateLedger : MonoBehaviour
    {
        [Serializable]
        private sealed class FlagEntry
        {
            public string flagId;
            public int value;
        }

        [Serializable]
        private sealed class SaveData
        {
            public List<FlagEntry> flags = new List<FlagEntry>();
        }

        public static NarrativeStateLedger Instance { get; private set; }

        private readonly Dictionary<string, int> _flags = new Dictionary<string, int>(StringComparer.Ordinal);

        public event Action<string, int> FlagChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool HasFlag(string flagId)
        {
            return !string.IsNullOrWhiteSpace(flagId) && _flags.ContainsKey(flagId);
        }

        public int GetFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId)) return 0;
            return _flags.TryGetValue(flagId, out int value) ? value : 0;
        }

        public void SetFlag(string flagId, int value, bool publishMissionSignal = true)
        {
            if (string.IsNullOrWhiteSpace(flagId)) return;
            _flags[flagId] = value;
            FlagChanged?.Invoke(flagId, value);
            if (publishMissionSignal)
                MissionSignalBus.Publish("story.flag.changed", 1);
        }

        public void ClearFlag(string flagId)
        {
            if (string.IsNullOrWhiteSpace(flagId) || !_flags.Remove(flagId)) return;
            FlagChanged?.Invoke(flagId, 0);
            MissionSignalBus.Publish("story.flag.changed", 1);
        }

        public string ExportJson()
        {
            var data = new SaveData();
            foreach (KeyValuePair<string, int> pair in _flags)
                data.flags.Add(new FlagEntry { flagId = pair.Key, value = pair.Value });
            return JsonUtility.ToJson(data);
        }

        public void ImportJson(string json)
        {
            _flags.Clear();
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.flags == null) return;

                foreach (FlagEntry flag in data.flags)
                {
                    if (flag == null || string.IsNullOrWhiteSpace(flag.flagId)) continue;
                    _flags[flag.flagId] = flag.value;
                }
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("Narrative flag save data was invalid and has been ignored.", this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}