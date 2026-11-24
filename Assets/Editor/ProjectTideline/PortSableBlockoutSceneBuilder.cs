#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Generates a self-contained, keyboard- and touch-playable Old Quay tutorial scene.
    /// It deliberately uses Unity primitives so the scene has no external art dependencies.
    /// This is a greybox tutorial foundation, not final PORT SABLE art or full campaign content.
    /// </summary>
    public static class PortSableBlockoutSceneBuilder
    {
        private const string SceneFolder = "Assets/ProjectTideline/Scenes";
        private const string ScenePath = SceneFolder + "/PortSable_OldQuay_Blockout.unity";
        private const string TutorialMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_first_steps.asset";
        private const string TutorialDialoguePath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/old_quay_juno_intro.asset";

        [MenuItem("Project Tideline/14 — Build Playable Old Quay Blockout", priority = 14)]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder("Assets/ProjectTideline");
            EnsureFolder(SceneFolder);
            MissionDefinition tutorialMission = EnsureTutorialMission();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material ground = MakeMaterial("Quay Concrete", new Color(0.27f, 0.31f, 0.32f));
            Material road = MakeMaterial("Worn Asphalt", new Color(0.105f, 0.12f, 0.13f));
            Material paint = MakeMaterial("Road Marking", new Color(0.78f, 0.72f, 0.55f));
            Material warehouse = MakeMaterial("Harbour Warehouse", new Color(0.30f, 0.34f, 0.34f));
            Material brick = MakeMaterial("Old Quay Brick", new Color(0.39f, 0.25f, 0.20f));
            Material glass = MakeMaterial("Dark Window Glass", new Color(0.08f, 0.16f, 0.19f), 0.35f);
            Material metal = MakeMaterial("Salt-Worn Metal", new Color(0.20f, 0.25f, 0.26f), 0.72f);
            Material water = MakeMaterial("Harbour Water", new Color(0.035f, 0.16f, 0.19f), 0.18f);
            Material playerMaterial = MakeMaterial("Player Placeholder", new Color(0.68f, 0.39f, 0.22f));
            Material accent = MakeMaterial("Port Sable Safety Ochre", new Color(0.83f, 0.48f, 0.12f));

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.43f, 0.46f, 0.48f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.43f, 0.49f, 0.51f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 90f;
            RenderSettings.fogEndDistance = 260f;

            GameObject sun = new GameObject("Old Quay — overcast key light");
            Light sunlight = sun.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(38f, -32f, 0f);

            // Walkable quay, road, sidewalks and harbour edge.
            MakeBlock("Quay foundation", new Vector3(0f, -0.55f, 12f), new Vector3(110f, 1f, 110f), ground);
            MakeBlock("Main street asphalt", new Vector3(0f, 0.015f, 8f), new Vector3(13f, 0.12f, 94f), road);
            MakeBlock("West pavement", new Vector3(-9.2f, 0.08f, 8f), new Vector3(5.2f, 0.25f, 94f), ground);
            MakeBlock("East pavement", new Vector3(9.2f, 0.08f, 8f), new Vector3(5.2f, 0.25f, 94f), ground);
            MakeBlock("Harbour retaining wall", new Vector3(0f, 1.1f, 57f), new Vector3(110f, 3.2f, 2.2f), metal);
            MakeBlock("Harbour water", new Vector3(0f, -0.18f, 78f), new Vector3(110f, 0.2f, 38f), water);
            MakeBlock("Pier west", new Vector3(-26f, 0.1f, 65f), new Vector3(18f, 0.5f, 18f), ground);
            MakeBlock("Pier east", new Vector3(28f, 0.1f, 67f), new Vector3(22f, 0.5f, 15f), ground);

            for (int z = -34; z <= 48; z += 12)
            {
                MakeBlock("Centre lane dash", new Vector3(0f, 0.09f, z), new Vector3(0.18f, 0.025f, 4.2f), paint, false);
                BuildStreetLamp(new Vector3(-11.2f, 0f, z + 3f), metal, accent);
                BuildStreetLamp(new Vector3(11.2f, 0f, z - 3f), metal, accent);
            }

            // Large readable silhouettes first; finer facade elements establish scale.
            BuildWarehouse("Kade Motor Works — blockout", new Vector3(-28f, 0f, 6f),
                new Vector3(25f, 11f, 31f), warehouse, metal, glass, accent);
            BuildWarehouse("Old Quay cold store", new Vector3(27f, 0f, 13f),
                new Vector3(25f, 15f, 34f), brick, metal, glass, accent);
            BuildWarehouse("Harbour Exchange annex", new Vector3(-28f, 0f, 43f),
                new Vector3(25f, 13f, 18f), brick, metal, glass, accent);
            BuildWarehouse("Saltline freight office", new Vector3(28f, 0f, 44f),
                new Vector3(24f, 10f, 17f), warehouse, metal, glass, accent);

            // Container stacks and a simple dockside gantry give the harbour a distinct skyline.
            for (int row = 0; row < 2; row++)
            for (int col = 0; col < 3; col++)
            {
                Material containerMat = (row + col) % 2 == 0 ? accent : metal;
                MakeBlock("Stacked freight container", new Vector3(-39f + col * 8f, 1.35f + row * 2.7f, 61f),
                    new Vector3(7.6f, 2.6f, 5.8f), containerMat);
            }
            MakeBlock("Dock gantry crossbeam", new Vector3(24f, 15f, 64f), new Vector3(2f, 0.8f, 23f), accent);
            MakeBlock("Dock gantry left leg", new Vector3(15f, 7.5f, 64f), new Vector3(0.8f, 15f, 0.8f), metal);
            MakeBlock("Dock gantry right leg", new Vector3(33f, 7.5f, 64f), new Vector3(0.8f, 15f, 0.8f), metal);

            // Player and camera: keyboard controls work immediately through PlayerInputRouter.
            GameObject player = new GameObject("PLAYER — Mara placeholder");
            player.transform.position = new Vector3(-2f, 0.2f, -25f);
            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.9f;
            characterController.radius = 0.36f;
            characterController.center = new Vector3(0f, 0.95f, 0f);
            characterController.slopeLimit = 48f;
            characterController.stepOffset = 0.32f;
            ThirdPersonMotor motor = player.AddComponent<ThirdPersonMotor>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Temporary player silhouette — replace with Mara hero prefab";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            body.transform.localScale = new Vector3(0.72f, 0.95f, 0.72f);
            body.GetComponent<Renderer>().sharedMaterial = playerMaterial;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject input = new GameObject("Player Input Router");
            input.AddComponent<PlayerInputRouter>();

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 300f;
            cameraObject.AddComponent<AudioListener>();
            ThirdPersonCameraRig cameraRig = cameraObject.AddComponent<ThirdPersonCameraRig>();
            cameraRig.SetTarget(player.transform, new Vector3(0f, 1.45f, 0f), true);
            motor.SetMovementCamera(cameraObject.transform);
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 3f, -5f);
            Transform hudRoot = BuildMobileControls();
            BuildTutorialGameplay(player, cameraObject.transform, hudRoot, tutorialMission);

            // Scene metadata helps designers understand what is functional versus placeholder.
            new GameObject("BLOCKOUT NOTES — replace primitives before release");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Selection.activeGameObject = player;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EditorUtility.DisplayDialog("PORT SABLE blockout created",
                "Saved and added to Build Settings:\n" + ScenePath +
                "\n\nPlayable now: WASD / arrows or touch joystick to move; Shift / Sprint to sprint; Space / Jump to jump; mouse / right-side touch pad to look; E / Interact to use world objects.\n" +
                "Includes a three-step Juno-to-harbour-to-workshop tutorial mission. This is still a primitive greybox, not final art or a complete campaign.",
                "OK");
            Debug.Log("PORT SABLE Old Quay blockout scene generated: " + ScenePath);
        }

        private static Transform BuildMobileControls()
        {
            GameObject canvasObject = new GameObject("Mobile HUD — touch controls",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject eventSystemObject = new GameObject("EventSystem",
                typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            Image joystickImage = CreateTouchImage(canvas.transform, "MOVE — virtual joystick",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(270f, 270f), new Vector2(58f, 68f),
                new Color(0.08f, 0.12f, 0.14f, 0.48f), out RectTransform joystickRect);
            Image knobImage = CreateTouchImage(joystickRect, "Joystick thumb",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(112f, 112f), Vector2.zero,
                new Color(0.75f, 0.79f, 0.77f, 0.62f), out RectTransform knobRect);
            MobileTouchControl move = joystickImage.gameObject.AddComponent<MobileTouchControl>();
            move.Configure(MobileTouchControlMode.MoveStick, knobRect);

            Image lookImage = CreateTouchImage(canvas.transform, "LOOK — drag to orbit camera",
                new Vector2(0.43f, 0.12f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.01f), out _);
            lookImage.gameObject.AddComponent<MobileTouchControl>()
                .Configure(MobileTouchControlMode.LookPad, null, 0.055f);

            CreateActionButton(canvas.transform, "JUMP", MobileTouchControlMode.JumpButton,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(142f, 142f), new Vector2(-170f, 170f));
            CreateActionButton(canvas.transform, "INTERACT", MobileTouchControlMode.InteractButton,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(126f, 126f), new Vector2(-340f, 126f));
            CreateActionButton(canvas.transform, "SPRINT", MobileTouchControlMode.SprintButton,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(126f, 126f), new Vector2(-170f, 350f));
            return canvas.transform;
        }

        private static void CreateActionButton(Transform parent, string label,
            MobileTouchControlMode mode, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 size, Vector2 position)
        {
            Image image = CreateTouchImage(parent, label, anchorMin, anchorMax, pivot, size,
                position, new Color(0.12f, 0.18f, 0.20f, 0.72f), out RectTransform rect);
            image.gameObject.AddComponent<MobileTouchControl>().Configure(mode);

            GameObject textObject = new GameObject(label + " label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(rect, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 25;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.raycastTarget = false;
        }

        private static Image CreateTouchImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size,
            Vector2 position, Color color, out RectTransform rect)
        {
            GameObject surface = new GameObject(name, typeof(RectTransform), typeof(Image));
            surface.transform.SetParent(parent, false);
            rect = surface.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Image image = surface.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static MissionDefinition EnsureTutorialMission()
        {
            EnsureFolder("Assets/ProjectTideline/Content");
            EnsureFolder("Assets/ProjectTideline/Content/Dialogue");
            EnsureFolder("Assets/ProjectTideline/Content/Dialogue/Prologues");
            EnsureFolder("Assets/ProjectTideline/Content/Missions");
            EnsureFolder("Assets/ProjectTideline/Content/Missions/Prologues");

            DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(TutorialDialoguePath);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<DialogueGraph>();
                AssetDatabase.CreateAsset(graph, TutorialDialoguePath);
            }
            graph.graphId = "old_quay.juno_first_contact";
            graph.startNodeId = "juno_first_contact";
            graph.nodes = new[]
            {
                new DialogueGraph.DialogueNode
                {
                    nodeId = "juno_first_contact",
                    speaker = "JUNO",
                    line = "The harbour office has changed the delivery ledger again. I need you to check the entry at the quay and bring back what you find. Keep it simple. We have a customer waiting.",
                    choices = new[]
                    {
                        new DialogueGraph.DialogueChoice
                        {
                            label = "I'll check the harbour ledger.",
                            signalId = "old_quay.juno_briefing_accepted",
                            relationshipCharacterId = "juno_kade",
                            relationshipDelta = 1
                        },
                        new DialogueGraph.DialogueChoice
                        {
                            label = "Tell me what changed first.",
                            signalId = "old_quay.juno_briefing_accepted",
                            relationshipCharacterId = "juno_kade",
                            relationshipDelta = -1
                        }
                    }
                }
            };
            EditorUtility.SetDirty(graph);

            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(TutorialMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, TutorialMissionPath);
            }
            mission.missionId = "old_quay_first_steps";
            mission.title = "OLD QUAY — FIRST STEPS";
            mission.briefing = "Meet Juno, verify the harbour ledger, take the delivery car to the customer and complete the handoff.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "meet_juno",
                    description = "Talk to Juno at Kade Motor Works.",
                    completionSignal = "old_quay.juno_briefing_accepted",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "inspect_harbour_ledger",
                    description = "Go to the harbour office and inspect the shipment ledger.",
                    completionSignal = "old_quay.harbour_ledger_inspected",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "report_to_juno",
                    description = "Return to the workshop and report your findings.",
                    completionSignal = "old_quay.juno_reported_back",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "enter_delivery_car",
                    description = "Get into the Kade delivery car.",
                    completionSignal = "vehicle.entered",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "deliver_to_customer",
                    description = "Drive to the marked customer bay and bring the car to a stop.",
                    completionSignal = "old_quay.delivery_arrived",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "customer_handoff",
                    description = "Exit the car and hand the keys to the customer.",
                    completionSignal = "old_quay.customer_handoff_accepted",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = string.Empty;
            mission.endsCampaign = true;
            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mission;
        }

        private static void BuildTutorialGameplay(GameObject player, Transform cameraTransform,
            Transform hudRoot, MissionDefinition tutorialMission)
        {
            new GameObject("NPC Relationship Ledger").AddComponent<NPCRelationshipLedger>();
            new GameObject("Narrative State Ledger").AddComponent<NarrativeStateLedger>();

            GameObject missionObject = new GameObject("Mission Director — Old Quay first steps");
            MissionDirector missionDirector = missionObject.AddComponent<MissionDirector>();
            SerializedObject missionSerialized = new SerializedObject(missionDirector);
            SerializedProperty campaign = missionSerialized.FindProperty("campaign");
            campaign.arraySize = 1;
            campaign.GetArrayElementAtIndex(0).objectReferenceValue = tutorialMission;
            missionSerialized.FindProperty("autoStart").boolValue = true;
            missionSerialized.ApplyModifiedPropertiesWithoutUndo();

            // Persistent objective text remains readable without taking over the screen.
            Image missionPanel = CreateTouchImage(hudRoot, "Mission HUD background",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(760f, 150f), new Vector2(28f, -28f),
                new Color(0.035f, 0.055f, 0.06f, 0.76f), out RectTransform missionPanelRect);
            missionPanel.raycastTarget = false;
            Text missionText = CreateUiText(missionPanelRect, "Mission objective text",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 25, TextAnchor.MiddleLeft, Color.white);
            missionText.rectTransform.offsetMin = new Vector2(22f, 12f);
            missionText.rectTransform.offsetMax = new Vector2(-22f, -12f);
            missionObject.AddComponent<MissionStatusDisplay>().Configure(missionDirector, missionText);

            // Interaction prompt is driven by ProximityInteractor focus, not by a timer.
            Image promptPanel = CreateTouchImage(hudRoot, "World interaction prompt",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(620f, 76f), new Vector2(0f, 92f),
                new Color(0.035f, 0.055f, 0.06f, 0.82f), out RectTransform promptRect);
            promptPanel.raycastTarget = false;
            Text promptText = CreateUiText(promptRect, "Interaction prompt text",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 26, TextAnchor.MiddleCenter, Color.white);
            promptText.text = " ";
            promptPanel.gameObject.SetActive(false);

            ProximityInteractor interactor = player.AddComponent<ProximityInteractor>();
            SerializedObject interactorSerialized = new SerializedObject(interactor);
            interactorSerialized.FindProperty("viewOrigin").objectReferenceValue = cameraTransform;
            interactorSerialized.FindProperty("actorRoot").objectReferenceValue = player.transform;
            interactorSerialized.FindProperty("reach").floatValue = 3.6f;
            interactorSerialized.FindProperty("promptRoot").objectReferenceValue = promptPanel.gameObject;
            interactorSerialized.FindProperty("promptText").objectReferenceValue = promptText;
            interactorSerialized.ApplyModifiedPropertiesWithoutUndo();

            // Dialogue panel: two authored responses write persistent relationship/story state.
            Image dialogueImage = CreateTouchImage(hudRoot, "Juno dialogue panel",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(1180f, 430f), new Vector2(0f, 185f),
                new Color(0.025f, 0.04f, 0.045f, 0.96f), out RectTransform dialogueRect);
            CanvasGroup dialogueGroup = dialogueImage.gameObject.AddComponent<CanvasGroup>();
            dialogueGroup.alpha = 0f;
            dialogueGroup.interactable = false;
            dialogueGroup.blocksRaycasts = false;

            Text speakerText = CreateUiText(dialogueRect, "Dialogue speaker",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(-64f, 64f), new Vector2(32f, -22f),
                28, TextAnchor.MiddleLeft, new Color(0.91f, 0.62f, 0.28f));
            Text lineText = CreateUiText(dialogueRect, "Dialogue line",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(-64f, 150f), new Vector2(32f, -94f),
                27, TextAnchor.UpperLeft, Color.white);

            Button firstChoice = CreateUiButton(dialogueRect, "Choice — accept", "I'll check the ledger.",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f),
                new Vector2(470f, 84f), new Vector2(-18f, 28f));
            Button secondChoice = CreateUiButton(dialogueRect, "Choice — ask", "Tell me what changed.",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f),
                new Vector2(470f, 84f), new Vector2(18f, 28f));

            DialogueController dialogueController = dialogueImage.gameObject.AddComponent<DialogueController>();
            SerializedObject dialogueSerialized = new SerializedObject(dialogueController);
            dialogueSerialized.FindProperty("panel").objectReferenceValue = dialogueGroup;
            dialogueSerialized.FindProperty("speakerText").objectReferenceValue = speakerText;
            dialogueSerialized.FindProperty("lineText").objectReferenceValue = lineText;
            SerializedProperty choiceButtons = dialogueSerialized.FindProperty("choiceButtons");
            choiceButtons.arraySize = 2;
            choiceButtons.GetArrayElementAtIndex(0).objectReferenceValue = firstChoice;
            choiceButtons.GetArrayElementAtIndex(1).objectReferenceValue = secondChoice;
            SerializedProperty choiceLabels = dialogueSerialized.FindProperty("choiceLabels");
            choiceLabels.arraySize = 2;
            choiceLabels.GetArrayElementAtIndex(0).objectReferenceValue = firstChoice.GetComponentInChildren<Text>();
            choiceLabels.GetArrayElementAtIndex(1).objectReferenceValue = secondChoice.GetComponentInChildren<Text>();
            dialogueSerialized.ApplyModifiedPropertiesWithoutUndo();

            DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(TutorialDialoguePath);
            GameObject juno = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            juno.name = "JUNO — workshop contact placeholder";
            juno.transform.position = new Vector3(-15f, 1f, -5f);
            juno.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial("Juno Placeholder Material", new Color(0.24f, 0.38f, 0.40f));
            DialogueInteractable junoInteractable = juno.AddComponent<DialogueInteractable>();
            SerializedObject junoSerialized = new SerializedObject(junoInteractable);
            junoSerialized.FindProperty("dialogueController").objectReferenceValue = dialogueController;
            junoSerialized.FindProperty("dialogueGraph").objectReferenceValue = graph;
            junoSerialized.FindProperty("talkPrompt").stringValue = "Talk to Juno";
            junoSerialized.FindProperty("oneShot").boolValue = true;
            junoSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject ledger = MakeBlock("HARBOUR OFFICE — shipment ledger",
                new Vector3(7f, 0.9f, 47f), new Vector3(1.6f, 1.8f, 1.2f),
                MakeMaterial("Ledger Kiosk", new Color(0.32f, 0.37f, 0.35f)));
            ledger.AddComponent<MissionSignalInteractable>().Configure(
                MissionSignalInteractable.HarbourLedgerSignal, "Inspect shipment ledger", true,
                missionDirector, "old_quay_first_steps", "inspect_harbour_ledger");

            GameObject reportBoard = MakeBlock("KADE MOTOR WORKS — report board",
                new Vector3(-12.5f, 0.85f, -4f), new Vector3(1.5f, 1.7f, 0.35f),
                MakeMaterial("Workshop Report Board", new Color(0.65f, 0.39f, 0.18f)));
            reportBoard.AddComponent<MissionSignalInteractable>().Configure(
                MissionSignalInteractable.WorkshopReportSignal, "Report findings to Juno", true,
                missionDirector, "old_quay_first_steps", "report_to_juno");
        }

        private static Button CreateUiButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
        {
            Image image = CreateTouchImage(parent, name, anchorMin, anchorMax, pivot, size,
                position, new Color(0.13f, 0.19f, 0.21f, 0.96f), out RectTransform rect);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text labelText = CreateUiText(rect, name + " label", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
                24, TextAnchor.MiddleCenter, Color.white);
            labelText.text = label;
            return button;
        }

        private static Text CreateUiText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size,
            Vector2 position, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void BuildWarehouse(string name, Vector3 centre, Vector3 size,
            Material wall, Material trim, Material window, Material sign)
        {
            MakeBlock(name, centre + Vector3.up * (size.y * 0.5f), size, wall);
            MakeBlock(name + " roof lip", centre + Vector3.up * (size.y + 0.12f),
                new Vector3(size.x + 0.5f, 0.25f, size.z + 0.5f), trim);
            float frontZ = centre.z - size.z * 0.5f - 0.04f;
            for (int x = -1; x <= 1; x++)
            {
                float px = centre.x + x * size.x * 0.27f;
                MakeBlock(name + " upper window", new Vector3(px, centre.y + size.y * 0.68f, frontZ),
                    new Vector3(size.x * 0.17f, 1.25f, 0.12f), window, false);
                MakeBlock(name + " lower facade panel", new Vector3(px, centre.y + 2.0f, frontZ - 0.08f),
                    new Vector3(size.x * 0.19f, 2.2f, 0.16f), trim, false);
            }
            MakeBlock(name + " loading shutter", new Vector3(centre.x, 1.8f, frontZ - 0.12f),
                new Vector3(size.x * 0.31f, 3.4f, 0.18f), sign, false);
        }

        private static void BuildStreetLamp(Vector3 position, Material metal, Material lightMaterial)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Quay streetlamp pole";
            pole.transform.position = position + Vector3.up * 3.1f;
            pole.transform.localScale = new Vector3(0.12f, 3.1f, 0.12f);
            pole.GetComponent<Renderer>().sharedMaterial = metal;

            MakeBlock("Streetlamp arm", position + new Vector3(0.65f, 6.0f, 0f),
                new Vector3(1.35f, 0.12f, 0.12f), metal);
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Streetlamp housing";
            lamp.transform.position = position + new Vector3(1.2f, 5.85f, 0f);
            lamp.transform.localScale = Vector3.one * 0.38f;
            lamp.GetComponent<Renderer>().sharedMaterial = lightMaterial;
            UnityEngine.Object.DestroyImmediate(lamp.GetComponent<Collider>());
        }

        private static GameObject MakeBlock(string name, Vector3 position, Vector3 scale,
            Material material, bool collider = true)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
                UnityEngine.Object.DestroyImmediate(block.GetComponent<Collider>());
            return block;
        }

        private static Material MakeMaterial(string name, Color color, float metallic = 0f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            material.SetFloat("_Metallic", metallic);
            return material;
        }

        private static void AddToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
                if (string.Equals(scenes[i].path, path, StringComparison.Ordinal))
                {
                    scenes[i] = new EditorBuildSettingsScene(path, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(folder))
                throw new InvalidOperationException("Invalid asset folder path: " + path);
            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folder);
        }
    }

}
#endif
