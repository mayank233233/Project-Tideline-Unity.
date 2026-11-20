using UnityEngine;

namespace ProjectTideline
{
    /// <summary>
    /// Connects the save snapshot to the active playable lead, mission director and world clock.
    /// Place one instance in the persistent bootstrap scene and wire references in the Inspector.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class SessionSaveCoordinator : MonoBehaviour
    {
        public static SessionSaveCoordinator Instance { get; private set; }

        [SerializeField] private Transform player;
        [SerializeField] private PlayableLeadSwitcher leadSwitcher;
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private NPCRelationshipLedger relationshipLedger;
        [SerializeField] private NarrativeStateLedger narrativeStateLedger;
        [SerializeField] private PublicAttentionState publicAttention;
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool saveOnApplicationPause = true;
        [SerializeField] private bool saveOnApplicationQuit = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ResolveReferences();
        }

        private void Start()
        {
            // Execution order 100 lets the lead roster, mission and clock initialize first.
            if (loadOnStart) LoadGame();
        }

        public bool SaveNow() => SaveCheckpoint("autosave");

        public bool SaveCheckpoint(string checkpointId)
        {
            ResolveReferences();
            Transform activePlayer = ResolveActivePlayer();
            if (leadSwitcher != null && !leadSwitcher.CanPersistActiveLead)
            {
                Debug.LogWarning("Save skipped: the active lead is inside a vehicle or in a vehicle transition. Exit first; the previous save is preserved.", this);
                return false;
            }
            if (activePlayer == null)
            {
                Debug.LogWarning("Save skipped: no active player Transform is assigned or discoverable.", this);
                return false;
            }

            Vector3 position = activePlayer.position;
            SaveSnapshot snapshot = new SaveSnapshot
            {
                schemaVersion = 2,
                checkpointId = string.IsNullOrWhiteSpace(checkpointId) ? "checkpoint" : checkpointId,
                activeLeadId = leadSwitcher != null ? leadSwitcher.ActiveLeadId : string.Empty,
                leadTransforms = leadSwitcher != null ? leadSwitcher.CaptureLeadTransforms() : null,
                missionIndex = missionDirector != null ? missionDirector.CurrentMissionIndex : -1,
                objectiveIndex = missionDirector != null ? missionDirector.CurrentObjectiveIndex : 0,
                objectiveCount = missionDirector != null ? missionDirector.CurrentObjectiveCount : 0,
                playerX = position.x,
                playerY = position.y,
                playerZ = position.z,
                playerYaw = activePlayer.eulerAngles.y,
                worldHour = worldClock != null ? worldClock.CurrentHour : 12f,
                relationshipData = relationshipLedger != null ? relationshipLedger.ExportJson()
                    : NPCRelationshipLedger.Instance != null ? NPCRelationshipLedger.Instance.ExportJson() : string.Empty,
                narrativeStateData = narrativeStateLedger != null ? narrativeStateLedger.ExportJson()
                    : NarrativeStateLedger.Instance != null ? NarrativeStateLedger.Instance.ExportJson() : string.Empty,
                publicAttention = publicAttention != null ? publicAttention.Value
                    : PublicAttentionState.Instance != null ? PublicAttentionState.Instance.Value : 0
            };

            SaveSnapshotStore.Write(snapshot);
            return true;
        }

        public bool LoadGame()
        {
            ResolveReferences();
            if (!SaveSnapshotStore.TryRead(out SaveSnapshot snapshot)) return false;
            bool restoredLeadPositions = false;
            if (leadSwitcher != null)
            {
                if (snapshot.leadTransforms != null && snapshot.leadTransforms.Length > 0)
                    restoredLeadPositions = leadSwitcher.RestoreLeadTransforms(snapshot.leadTransforms) > 0;

                if (!string.IsNullOrWhiteSpace(snapshot.activeLeadId) && !leadSwitcher.RestoreLead(snapshot.activeLeadId))
                    Debug.LogWarning("Saved playable lead could not be restored: " + snapshot.activeLeadId, this);
            }

            Transform activePlayer = ResolveActivePlayer();
            if (activePlayer == null)
            {
                Debug.LogWarning("Save exists but cannot be restored: no active player Transform is assigned or discoverable.", this);
                return false;
            }

            // Versions 1 and 2 only stored the selected actor. Version 3 stores all three lead poses.
            if (!restoredLeadPositions)
                activePlayer.SetPositionAndRotation(
                    new Vector3(snapshot.playerX, snapshot.playerY, snapshot.playerZ),
                    Quaternion.Euler(0f, snapshot.playerYaw, 0f));

            if (worldClock != null) worldClock.SetCurrentHour(snapshot.worldHour);
            if (relationshipLedger == null) relationshipLedger = NPCRelationshipLedger.Instance;
            if (relationshipLedger != null) relationshipLedger.ImportJson(snapshot.relationshipData);
            if (narrativeStateLedger == null) narrativeStateLedger = NarrativeStateLedger.Instance;
            if (narrativeStateLedger != null) narrativeStateLedger.ImportJson(snapshot.narrativeStateData);
            if (publicAttention == null) publicAttention = PublicAttentionState.Instance;
            if (publicAttention != null) publicAttention.Restore(snapshot.publicAttention);
            if (missionDirector != null && snapshot.missionIndex >= 0)
            {
                if (!missionDirector.RestoreProgress(snapshot.missionIndex, snapshot.objectiveIndex, snapshot.objectiveCount))
                    Debug.LogWarning("Player and time restored, but the saved mission position is incompatible with the current campaign.", this);
            }
            return true;
        }

        public void DeleteSave() => SaveSnapshotStore.Delete();

        private Transform ResolveActivePlayer()
        {
            if (leadSwitcher == null) leadSwitcher = PlayableLeadSwitcher.Instance;
            if (leadSwitcher != null && leadSwitcher.ActiveActor != null)
                return leadSwitcher.ActiveActor.transform;
            return player;
        }

        private void ResolveReferences()
        {
            if (leadSwitcher == null) leadSwitcher = PlayableLeadSwitcher.Instance;
            if (player == null)
            {
                Transform active = ResolveActivePlayer();
                if (active != null) player = active;
                else
                {
                    ThirdPersonMotor motor = FindFirstObjectByType<ThirdPersonMotor>();
                    if (motor != null) player = motor.transform;
                }
            }
            if (missionDirector == null) missionDirector = FindFirstObjectByType<MissionDirector>();
            if (worldClock == null) worldClock = FindFirstObjectByType<WorldClock>();
            if (relationshipLedger == null) relationshipLedger = NPCRelationshipLedger.Instance != null
                ? NPCRelationshipLedger.Instance : FindFirstObjectByType<NPCRelationshipLedger>();
            if (narrativeStateLedger == null) narrativeStateLedger = NarrativeStateLedger.Instance != null
                ? NarrativeStateLedger.Instance : FindFirstObjectByType<NarrativeStateLedger>();
            if (publicAttention == null) publicAttention = PublicAttentionState.Instance != null
                ? PublicAttentionState.Instance : FindFirstObjectByType<PublicAttentionState>();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && saveOnApplicationPause) SaveNow();
        }

        private void OnApplicationQuit()
        {
            if (saveOnApplicationQuit) SaveNow();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}