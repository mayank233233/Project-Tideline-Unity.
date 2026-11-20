using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTideline
{
    /// <summary>Small persistent relationship ledger for named story characters.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class NPCRelationshipLedger : MonoBehaviour
    {
        [Serializable]
        private sealed class Entry
        {
            public string characterId;
            public int value;
            public bool hasMet;
        }

        [Serializable]
        private sealed class SaveData
        {
            public List<Entry> entries = new List<Entry>();
        }

        public static NPCRelationshipLedger Instance { get; private set; }
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();
        private readonly HashSet<string> _metCharacters = new HashSet<string>();

        public event Action<string, int> RelationshipChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool HasMet(string characterId)
        {
            return !string.IsNullOrWhiteSpace(characterId) && _metCharacters.Contains(characterId);
        }

        public void MarkMet(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)) return;
            _metCharacters.Add(characterId);
        }

        public bool HasValue(string characterId)
        {
            return !string.IsNullOrWhiteSpace(characterId) && _values.ContainsKey(characterId);
        }

        public int GetValue(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)) return 0;
            return _values.TryGetValue(characterId, out int value) ? value : 0;
        }

        public int ChangeValue(string characterId, int delta)
        {
            if (string.IsNullOrWhiteSpace(characterId) || delta == 0) return GetValue(characterId);
            int next = Mathf.Clamp(GetValue(characterId) + delta, -100, 100);
            _values[characterId] = next;
            RelationshipChanged?.Invoke(characterId, next);
            MissionSignalBus.Publish("relationship.changed." + characterId, 1);
            return next;
        }

        public string ExportJson()
        {
            SaveData data = new SaveData();
            foreach (KeyValuePair<string, int> pair in _values)
                data.entries.Add(new Entry { characterId = pair.Key, value = pair.Value, hasMet = _metCharacters.Contains(pair.Key) });
            foreach (string characterId in _metCharacters)
            {
                if (!_values.ContainsKey(characterId))
                    data.entries.Add(new Entry { characterId = characterId, value = 0, hasMet = true });
            }
            return JsonUtility.ToJson(data);
        }

        public void ImportJson(string json)
        {
            _values.Clear();
            _metCharacters.Clear();
            if (string.IsNullOrWhiteSpace(json)) return;
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.entries == null) return;
                foreach (Entry entry in data.entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.characterId)) continue;
                    _values[entry.characterId] = Mathf.Clamp(entry.value, -100, 100);
                    if (entry.hasMet) _metCharacters.Add(entry.characterId);
                }
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("Relationship save data was invalid and has been ignored.", this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}