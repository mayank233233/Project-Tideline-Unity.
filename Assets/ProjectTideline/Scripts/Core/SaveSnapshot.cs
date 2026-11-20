using System;
using UnityEngine;

namespace ProjectTideline
{
    [Serializable]
    public sealed class LeadTransformSnapshot
    {
        public string leadId;
        public float x;
        public float y;
        public float z;
        public float yaw;
    }

    [Serializable]
    public sealed class SaveSnapshot
    {
        public int schemaVersion = 3;
        public string checkpointId;
        public string activeLeadId;
        public LeadTransformSnapshot[] leadTransforms;
        public int missionIndex;
        public int objectiveIndex;
        public int objectiveCount;
        public float playerX;
        public float playerY;
        public float playerZ;
        public float playerYaw;
        public float worldHour;
        public string relationshipData;
        public string narrativeStateData;
        public int publicAttention;
    }

    public static class SaveSnapshotStore
    {
        private const string SaveKeyV3 = "project_tideline.save.v3";
        private const string LegacySaveKeyV2 = "project_tideline.save.v2";
        private const string LegacySaveKeyV1 = "project_tideline.save.v1";

        public static void Write(SaveSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            snapshot.schemaVersion = 3;
            PlayerPrefs.SetString(SaveKeyV3, JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        public static bool TryRead(out SaveSnapshot snapshot)
        {
            snapshot = null;
            if (TryReadKey(SaveKeyV3, 3, out snapshot)) return true;
            if (TryReadKey(LegacySaveKeyV2, 2, out snapshot)) return true;
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
            PlayerPrefs.DeleteKey(SaveKeyV3);
            PlayerPrefs.DeleteKey(LegacySaveKeyV2);
            PlayerPrefs.DeleteKey(LegacySaveKeyV1);
        }
    }
}