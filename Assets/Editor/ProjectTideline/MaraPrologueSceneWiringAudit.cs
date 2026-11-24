#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Audits actual loaded scene instances and serialized references for Mara's first prologue.
    /// It never manufactures placeholder actors, vehicles, dialogue UI or world geometry.
    /// </summary>
    public static class MaraPrologueSceneWiringAudit
    {
        private const string ReportPath = "ProjectTideline-Mara-Scene-Wiring-Audit.txt";
        private const string IntroGraphId = "prologue.mara.two_minutes_to_close";
        private const string NicoGraphId = "prologue.mara.nico_challenge";
        private const string MissionId = "two_minutes_to_close";

        [MenuItem("Project Tideline/13 — Audit Mara Prologue Scene Wiring", priority = 13)]
        public static void RunAudit()
        {
            var issues = new List<string>();
            var rows = new List<string>();

            MissionDirector director = FindSingle<MissionDirector>("MissionDirector", issues);
            PlayerInputRouter input = FindSingle<PlayerInputRouter>("PlayerInputRouter", issues);
            DialogueController dialogue = FindSingle<DialogueController>("DialogueController", issues);
            ProximityInteractor[] interactors = FindAll<ProximityInteractor>();
            NarrativeStateLedger narrative = FindSingle<NarrativeStateLedger>("NarrativeStateLedger", issues);
            NPCRelationshipLedger relationships = FindSingle<NPCRelationshipLedger>("NPCRelationshipLedger", issues);

            if (interactors.Length == 0)
            {
                issues.Add("No ProximityInteractor exists in a loaded scene; no player can invoke the authored dialogue or workshop interactions.");
            }
            else
            {
                rows.Add("ProximityInteractor instances: " + interactors.Length);
                if (interactors.Length != 1)
                    issues.Add("Mara's vertical slice expects exactly one active ProximityInteractor; found " + interactors.Length + ".");
                foreach (ProximityInteractor interactor in interactors)
                {
                    SerializedObject interactorData = new SerializedObject(interactor);
                    if (interactorData.FindProperty("actorRoot")?.objectReferenceValue == null)
                        issues.Add("ProximityInteractor on '" + interactor.name + "' has no actorRoot reference.");
                    if (interactorData.FindProperty("viewOrigin")?.objectReferenceValue == null)
                        issues.Add("ProximityInteractor on '" + interactor.name + "' has no viewOrigin reference.");
                }
            }

            if (input == null) rows.Add("Player input: MISSING");
            if (dialogue == null) rows.Add("Dialogue controller: MISSING");
            if (dialogue != null) ValidateDialogueView(dialogue, issues);
            if (narrative == null) rows.Add("Narrative state ledger: MISSING");
            if (relationships == null) rows.Add("NPC relationship ledger: MISSING");

            if (director != null)
            {
                SerializedObject directorData = new SerializedObject(director);
                if (!directorData.FindProperty("autoStart").boolValue)
                    issues.Add("MissionDirector autoStart is disabled; the Mara prologue scene has no authored start trigger in this wiring contract.");
            }

            MissionDefinition mission = FindMission(director, issues);
            DialogueGraph introGraph = FindGraph(IntroGraphId, issues);
            DialogueGraph nicoGraph = FindGraph(NicoGraphId, issues);

            DialogueInteractable[] introTalk = FindAll<DialogueInteractable>();
            int introBindings = 0;
            foreach (DialogueInteractable interactable in introTalk)
            {
                SerializedObject serialized = new SerializedObject(interactable);
                DialogueGraph graph = serialized.FindProperty("dialogueGraph")?.objectReferenceValue as DialogueGraph;
                DialogueController controller = serialized.FindProperty("dialogueController")?.objectReferenceValue as DialogueController;
                if (graph != null && graph.graphId == IntroGraphId && controller == dialogue)
                {
                    introBindings++;
                    if (!HasSolidInteractionCollider(interactable))
                        issues.Add("Mara's opening DialogueInteractable on '" + interactable.name + "' needs a non-trigger collider for ProximityInteractor.");
                }
            }
            if (introBindings != 1)
                issues.Add("Expected exactly one DialogueInteractable assigned to the Mara opening graph and the scene DialogueController; found " + introBindings + ".");

            MaraWorkshopOrderController workshop = FindSingle<MaraWorkshopOrderController>("MaraWorkshopOrderController", issues);
            ValidateWorkshop(workshop, director, issues, rows);

            RouteRaceController route = FindSingle<RouteRaceController>("Mara delivery RouteRaceController", issues);
            ValidateRoute(route, issues, rows);

            MaraDeliveryStartInteractable[] starts = FindAll<MaraDeliveryStartInteractable>();
            int validStarts = 0;
            foreach (MaraDeliveryStartInteractable start in starts)
            {
                SerializedObject serialized = new SerializedObject(start);
                if (serialized.FindProperty("workshopOrder")?.objectReferenceValue == workshop
                    && serialized.FindProperty("deliveryRoute")?.objectReferenceValue == route)
                {
                    validStarts++;
                    if (!HasSolidInteractionCollider(start))
                        issues.Add("MaraDeliveryStartInteractable on '" + start.name + "' needs a non-trigger collider.");
                }
            }
            if (validStarts != 1)
                issues.Add("Expected exactly one MaraDeliveryStartInteractable wired to the audited workshop order and route; found " + validStarts + ".");

            MaraDeliveryHandoffController handoff = FindSingle<MaraDeliveryHandoffController>("MaraDeliveryHandoffController", issues);
            if (handoff != null)
            {
                SerializedObject serialized = new SerializedObject(handoff);
                if (serialized.FindProperty("deliveryRoute")?.objectReferenceValue != route)
                    issues.Add("MaraDeliveryHandoffController must reference the same delivery RouteRaceController.");
                if (serialized.FindProperty("missionDirector")?.objectReferenceValue != director)
                    issues.Add("MaraDeliveryHandoffController must reference the scene MissionDirector.");
            }

            MaraCustomerHandoffInteractable[] handoffInteractables = FindAll<MaraCustomerHandoffInteractable>();
            int validHandoffs = 0;
            foreach (MaraCustomerHandoffInteractable interactable in handoffInteractables)
            {
                SerializedObject serialized = new SerializedObject(interactable);
                if (serialized.FindProperty("handoffController")?.objectReferenceValue == handoff)
                {
                    validHandoffs++;
                    if (!HasSolidInteractionCollider(interactable))
                        issues.Add("MaraCustomerHandoffInteractable on '" + interactable.name + "' needs a non-trigger collider.");
                }
            }
            if (validHandoffs != 1)
                issues.Add("Expected exactly one MaraCustomerHandoffInteractable wired to the MaraDeliveryHandoffController; found " + validHandoffs + ".");

            MaraPostDeliveryDialogueInteractable[] nicoTalk = FindAll<MaraPostDeliveryDialogueInteractable>();
            int validNico = 0;
            foreach (MaraPostDeliveryDialogueInteractable interactable in nicoTalk)
            {
                SerializedObject serialized = new SerializedObject(interactable);
                DialogueGraph graph = serialized.FindProperty("dialogueGraph")?.objectReferenceValue as DialogueGraph;
                DialogueController controller = serialized.FindProperty("dialogueController")?.objectReferenceValue as DialogueController;
                MaraDeliveryHandoffController linkedHandoff =
                    serialized.FindProperty("handoffController")?.objectReferenceValue as MaraDeliveryHandoffController;
                if (graph == nicoGraph && controller == dialogue && linkedHandoff == handoff)
                {
                    validNico++;
                    if (!HasSolidInteractionCollider(interactable))
                        issues.Add("MaraPostDeliveryDialogueInteractable on '" + interactable.name + "' needs a non-trigger collider.");
                }
            }
            if (validNico != 1)
                issues.Add("Expected exactly one MaraPostDeliveryDialogueInteractable wired to Nico's graph, the shared DialogueController and handoff gate; found " + validNico + ".");

            if (mission != null)
            {
                string[] expectedSignals =
                {
                    "mara.workshop.order.completed",
                    "mara.delivery.finished",
                    "prologue.mara.complete"
                };
                if (mission.objectives == null || mission.objectives.Length != expectedSignals.Length)
                    issues.Add("Mara MissionDefinition must contain exactly three ordered objectives.");
                else
                {
                    for (int i = 0; i < expectedSignals.Length; i++)
                        if (mission.objectives[i] == null || mission.objectives[i].completionSignal != expectedSignals[i])
                            issues.Add("Mara mission objective " + (i + 1) + " must listen for '" + expectedSignals[i] + "'.");
                }
            }

            int loadedSceneCount = CountLoadedScenes();
            rows.Insert(0, "Loaded scenes: " + loadedSceneCount);
            rows.Add("Mara opening graph bindings: " + introBindings);
            rows.Add("Mara delivery-start bindings: " + validStarts);
            rows.Add("Mara customer-handoff bindings: " + validHandoffs);
            rows.Add("Mara Nico-dialogue bindings: " + validNico);

            string status = issues.Count == 0 ? "PASS" : "BLOCKED";
            var report = new List<string>
            {
                "Project Tideline — Mara Prologue Scene Wiring Audit",
                "Status: " + status,
                "Loaded scenes: " + loadedSceneCount,
                "",
                "Wiring summary:"
            };
            report.AddRange(rows);
            report.Add("");
            report.Add("Issues:");
            if (issues.Count == 0) report.Add("No scene-wiring issues found.");
            else foreach (string issue in issues) report.Add("BLOCKER: " + issue);
            report.Add("");
            report.Add("This checks loaded scene instances and serialized references only. It does not prove Play Mode behaviour, visual quality, route geometry, physics, animation, save/load or device performance.");

            File.WriteAllLines(ReportPath, report);
            AssetDatabase.Refresh();
            if (issues.Count == 0)
                Debug.Log("Mara prologue scene wiring audit PASS. Report: " + ReportPath);
            else
                Debug.LogError("Mara prologue scene wiring audit BLOCKED:\n" + string.Join("\n", issues));
            EditorUtility.DisplayDialog("Mara prologue scene wiring — " + status,
                string.Join("\n", rows) + (issues.Count > 0 ? "\n\n" + string.Join("\n", issues) : ""), "OK");
        }

        private static void ValidateDialogueView(DialogueController dialogue, List<string> issues)
        {
            SerializedObject serialized = new SerializedObject(dialogue);
            if (serialized.FindProperty("panel")?.objectReferenceValue == null)
                issues.Add("DialogueController has no CanvasGroup panel reference.");
            if (serialized.FindProperty("speakerText")?.objectReferenceValue == null)
                issues.Add("DialogueController has no speaker Text reference.");
            if (serialized.FindProperty("lineText")?.objectReferenceValue == null)
                issues.Add("DialogueController has no dialogue-line Text reference.");
            if (serialized.FindProperty("closeButton")?.objectReferenceValue == null)
                issues.Add("DialogueController has no close Button reference.");

            SerializedProperty buttons = serialized.FindProperty("choiceButtons");
            SerializedProperty labels = serialized.FindProperty("choiceLabels");
            if (buttons == null || labels == null || !buttons.isArray || !labels.isArray
                || buttons.arraySize == 0 || buttons.arraySize != labels.arraySize)
            {
                issues.Add("DialogueController choiceButtons and choiceLabels must be non-empty arrays of equal length.");
                return;
            }
            for (int i = 0; i < buttons.arraySize; i++)
            {
                if (buttons.GetArrayElementAtIndex(i).objectReferenceValue == null
                    || labels.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    issues.Add("DialogueController choice button/label slot " + i + " is unassigned.");
            }
        }

        private static MissionDefinition FindMission(MissionDirector director, List<string> issues)
        {
            if (director == null) return null;
            SerializedObject serialized = new SerializedObject(director);
            SerializedProperty campaign = serialized.FindProperty("campaign");
            if (campaign == null || !campaign.isArray)
            {
                issues.Add("MissionDirector campaign array is missing.");
                return null;
            }

            MissionDefinition found = null;
            for (int i = 0; i < campaign.arraySize; i++)
            {
                MissionDefinition candidate = campaign.GetArrayElementAtIndex(i).objectReferenceValue as MissionDefinition;
                if (candidate == null || candidate.missionId != MissionId) continue;
                if (found != null) issues.Add("MissionDirector campaign contains duplicate '" + MissionId + "' assets.");
                found = candidate;
            }
            if (found == null) issues.Add("MissionDirector campaign does not contain the generated '" + MissionId + "' MissionDefinition.");
            return found;
        }

        private static DialogueGraph FindGraph(string graphId, List<string> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueGraph");
            DialogueGraph found = null;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialogueGraph candidate = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                if (candidate == null || candidate.graphId != graphId) continue;
                if (found != null) issues.Add("Duplicate DialogueGraph graphId '" + graphId + "'.");
                found = candidate;
            }
            if (found == null) issues.Add("Missing DialogueGraph asset '" + graphId + "'.");
            return found;
        }

        private static void ValidateWorkshop(MaraWorkshopOrderController workshop, MissionDirector director, List<string> issues, List<string> rows)
        {
            if (workshop == null) return;
            SerializedObject serialized = new SerializedObject(workshop);
            SerializedProperty tasks = serialized.FindProperty("requiredTaskIds");
            if (serialized.FindProperty("missionDirector")?.objectReferenceValue != director)
                issues.Add("MaraWorkshopOrderController must reference the scene MissionDirector for save/restore gating.");
            string[] expected = { "steering_check", "door_latch_check", "engine_rattle_check" };
            if (tasks == null || !tasks.isArray || tasks.arraySize != expected.Length)
            {
                issues.Add("MaraWorkshopOrderController must require exactly the three authored workshop task IDs.");
                return;
            }
            for (int i = 0; i < expected.Length; i++)
                if (tasks.GetArrayElementAtIndex(i).stringValue != expected[i])
                    issues.Add("Workshop task " + i + " must be '" + expected[i] + "'.");

            MaraWorkshopTaskInteractable[] stations = FindAll<MaraWorkshopTaskInteractable>();
            var found = new HashSet<string>(StringComparer.Ordinal);
            if (stations.Length != expected.Length)
                issues.Add("Expected exactly three MaraWorkshopTaskInteractable stations; found " + stations.Length + ".");

            foreach (MaraWorkshopTaskInteractable station in stations)
            {
                SerializedObject stationData = new SerializedObject(station);
                string taskId = stationData.FindProperty("taskId")?.stringValue;
                MaraWorkshopOrderController linked =
                    stationData.FindProperty("orderController")?.objectReferenceValue as MaraWorkshopOrderController;
                if (linked == workshop && !string.IsNullOrWhiteSpace(taskId))
                {
                    found.Add(taskId);
                    if (!HasSolidInteractionCollider(station))
                        issues.Add("Workshop station '" + taskId + "' needs a non-trigger collider for proximity interaction.");
                }
            }
            foreach (string taskId in expected)
                if (!found.Contains(taskId))
                    issues.Add("No MaraWorkshopTaskInteractable is wired to the shared order controller for '" + taskId + "'.");
            rows.Add("Workshop stations: " + found.Count + "/3 distinct task IDs wired");
        }

        private static void ValidateRoute(RouteRaceController route, List<string> issues, List<string> rows)
        {
            if (route == null) return;
            SerializedObject serialized = new SerializedObject(route);
            VehicleDriveController vehicle =
                serialized.FindProperty("playerVehicle")?.objectReferenceValue as VehicleDriveController;
            SerializedProperty checkpoints = serialized.FindProperty("checkpoints");
            if (vehicle == null) issues.Add("Mara delivery route has no designated player VehicleDriveController.");
            else
            {
                VehicleSeatInteractable[] seats = FindAll<VehicleSeatInteractable>();
                int matchingSeats = 0;
                foreach (VehicleSeatInteractable seat in seats)
                {
                    SerializedObject seatData = new SerializedObject(seat);
                    if (seatData.FindProperty("vehicle")?.objectReferenceValue == vehicle)
                    {
                        matchingSeats++;
                        if (!HasSolidInteractionCollider(seat))
                            issues.Add("VehicleSeatInteractable on '" + seat.name + "' needs a non-trigger collider for proximity interaction.");
                    }
                }
                if (matchingSeats != 1)
                    issues.Add("Expected exactly one VehicleSeatInteractable wired to the route's designated player vehicle; found " + matchingSeats + ".");
            }
            if (checkpoints == null || !checkpoints.isArray || checkpoints.arraySize < 2)
            {
                issues.Add("Mara delivery route needs at least two ordered checkpoints.");
                return;
            }

            for (int i = 0; i < checkpoints.arraySize; i++)
            {
                RouteRaceCheckpoint checkpoint = checkpoints.GetArrayElementAtIndex(i).objectReferenceValue as RouteRaceCheckpoint;
                if (checkpoint == null)
                {
                    issues.Add("Mara delivery route checkpoint " + i + " is unassigned.");
                    continue;
                }
                if (checkpoint.Controller != route || checkpoint.CheckpointIndex != i || !checkpoint.HasTriggerCollider)
                    issues.Add("Mara delivery checkpoint " + i + " must reference the route, use index " + i + " and have a trigger collider.");
            }
            rows.Add("Delivery route checkpoints: " + checkpoints.arraySize + "; designated vehicle: " + (vehicle != null ? vehicle.name : "MISSING"));
        }

        private static T FindSingle<T>(string label, List<string> issues) where T : Component
        {
            T[] found = FindAll<T>();
            if (found.Length == 0)
            {
                issues.Add("No " + label + " exists in any loaded scene.");
                return null;
            }
            if (found.Length > 1)
                issues.Add("Expected one " + label + " in loaded scenes; found " + found.Length + ".");
            return found[0];
        }

        private static T[] FindAll<T>() where T : Component
        {
            List<T> result = new List<T>();
            foreach (T component in Resources.FindObjectsOfTypeAll<T>())
                if (component != null && component.isActiveAndEnabled
                    && component.gameObject.scene.IsValid() && component.gameObject.scene.isLoaded)
                    result.Add(component);
            return result.ToArray();
        }

        private static bool HasSolidInteractionCollider(Component component)
        {
            if (component == null) return false;
            Collider[] colliders = component.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
                if (collider != null && !collider.isTrigger) return true;
            return false;
        }

        private static int CountLoadedScenes()
        {
            int count = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isLoaded) count++;
            return count;
        }
    }
}
#endif
