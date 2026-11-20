using System;
using UnityEngine;

namespace ProjectTideline
{
    [Serializable]
    public sealed class SaveSnapshot
    {
        public int schemaVersion = 2;
        public string checkpointId;
        public string activeLeadId;
        public int missionIndex;
        public int objectiveIndex;
        public int objectiveCount;
        public float playerX;
        public float playerY;
        public float playerZ;
        public float playerYaw;
        public float worldHour;
        public string relationshipData;
        public int publicAttention;
    }

    public static class SaveSnapshotStore
    {
        private const string SaveKeyV2 = "project_tideline.save.v2";
        private const string LegacySaveKeyV1 = "project_tideline.save.v1";

        public static void Write(SaveSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            snapshot.schemaVersion = 2;
            PlayerPrefs.SetString(SaveKeyV2, JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        public static bool TryRead(out SaveSnapshot snapshot)
        {
            snapshot = null;
            if (TryReadKey(SaveKeyV2, 2, out snapshot)) return true;
            return TryReadKey(LegacySaveKeyV1, 1, out snapshot);
        }

        private static bool TryReadKey(string key, int expectedVersion, out SaveSnapshot snapshot)
        {
            snapshot = null;
            if (!PlayerPrefs.HasKey(key)) return false;
            try
            {
                SaveSnapshot candidate = JsonUtility.FromJson<SaveSnapshot>(PlayerPrefs.GetString(key));
                if (candidate == null || candidate.schemaVersion != expectedVersion) return false;
                snapshot = candidate;
                return true;
            }
            catch (ArgumentException)
            {
                snapshot = null;
                return false;
            }
        }

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(SaveKeyV2);
            PlayerPrefs.DeleteKey(LegacySaveKeyV1);
        }
    }
}