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
        private const string MaterialFolder = "Assets/ProjectTideline/Generated/Materials";
        private const string ScenePath = SceneFolder + "/PortSable_OldQuay_Blockout.unity";
        private const string TutorialMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_first_steps.asset";
        private const string FirstScoreMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_first_score.asset";
        private const string PaperTrailMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_paper_trail.asset";
        private const string MarketLedgerMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_market_ledger.asset";
        private const string MarketBackingMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_market_backing.asset";
        private const string MarketColdShoulderMissionPath =
            "Assets/ProjectTideline/Content/Missions/Prologues/old_quay_market_cold_shoulder.asset";
        private const string TutorialDialoguePath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/old_quay_juno_intro.asset";
        private const string TutorialDebriefDialoguePath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/old_quay_juno_debrief.asset";
        private const string CustomerHandoffDialoguePath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/old_quay_customer_handoff.asset";
        private const string MarketVendorDialoguePath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/old_quay_market_vendor.asset";
        private const string CafeOwnerDialoguePath =
            "Assets/ProjectTideline/Content/Dialogue/Prologues/old_quay_cafe_owner.asset";

        [MenuItem("Project Tideline/14 — Build Playable Old Quay Blockout", priority = 14)]
        public static void Build()
        {
            // Batch-mode CI starts from a clean editor session and must never wait for an interactive save prompt.
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder("Assets/ProjectTideline");
            EnsureFolder(SceneFolder);
            EnsureFolder("Assets/ProjectTideline/Generated");
            EnsureFolder(MaterialFolder);
            MissionDefinition tutorialMission = EnsureTutorialMission();
            MissionDefinition firstScoreMission = EnsureFirstScoreMission();
            MissionDefinition paperTrailMission = EnsurePaperTrailMission();
            MissionDefinition marketLedgerMission = EnsureMarketLedgerMission();
            MissionDefinition marketBackingMission = EnsureMarketBackingMission();
            MissionDefinition marketColdShoulderMission = EnsureMarketColdShoulderMission();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material ground = MakeMaterial("Quay Concrete", new Color(0.27f, 0.31f, 0.32f));
            Material road = MakeMaterial("Worn Asphalt", new Color(0.105f, 0.12f, 0.13f));
            Material paint = MakeMaterial("Road Marking", new Color(0.78f, 0.72f, 0.55f));
            Material warehouse = MakeMaterial("Harbour Warehouse", new Color(0.30f, 0.34f, 0.34f));
            Material brick = MakeMaterial("Old Quay Brick", new Color(0.39f, 0.25f, 0.20f));
            Material glass = MakeMaterial("Dark Window Glass", new Color(0.08f, 0.16f, 0.19f), 0.35f, 0.88f);
            Material metal = MakeMaterial("Salt-Worn Metal", new Color(0.20f, 0.25f, 0.26f), 0.72f, 0.56f);
            Material water = MakeMaterial("Harbour Water", new Color(0.035f, 0.16f, 0.19f), 0.18f, 0.94f);
            Material playerMaterial = MakeMaterial("Player Placeholder", new Color(0.68f, 0.39f, 0.22f));
            Material accent = MakeMaterial("Port Sable Safety Ochre", new Color(0.83f, 0.48f, 0.12f));
            Material wetRoad = MakeMaterial("Old Quay rain-dark asphalt repairs", new Color(0.055f, 0.071f, 0.078f), 0.12f, 0.92f);
            Material lampGlow = MakeEmissiveMaterial("Old Quay sodium lamp glow", new Color(1f, 0.52f, 0.22f), 1.35f);

            // A restrained overcast-to-late-afternoon palette: warm key light, cool harbour fill.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.49f, 0.57f, 0.62f);
            RenderSettings.ambientEquatorColor = new Color(0.43f, 0.45f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.17f, 0.19f, 0.19f);
            RenderSettings.ambientIntensity = 0.92f;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.47f, 0.53f, 0.54f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 72f;
            RenderSettings.fogEndDistance = 210f;

            GameObject sun = new GameObject("Old Quay — warm coastal key light");
            Light sunlight = sun.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.color = new Color(1f, 0.89f, 0.76f);
            sunlight.intensity = 1.22f;
            sunlight.shadows = LightShadows.Soft;
            sunlight.shadowStrength = 0.64f;
            sunlight.shadowBias = 0.045f;
            sunlight.shadowNormalBias = 0.32f;
            sun.transform.rotation = Quaternion.Euler(32f, -28f, 0f);

            // Walkable quay, road, sidewalks and harbour edge.
            MakeBlock("Quay foundation", new Vector3(0f, -0.55f, 12f), new Vector3(110f, 1f, 110f), ground);
            MakeBlock("Main street asphalt", new Vector3(0f, 0.015f, 8f), new Vector3(13f, 0.12f, 94f), road);
            MakeBlock("Old Quay southern cross street", new Vector3(0f, 0.025f, -23f),
                new Vector3(94f, 0.12f, 9.5f), road);
            MakeBlock("Cross street centre dash A", new Vector3(-31f, 0.095f, -23f),
                new Vector3(4.2f, 0.025f, 0.16f), paint, false);
            MakeBlock("Cross street centre dash B", new Vector3(31f, 0.095f, -23f),
                new Vector3(4.2f, 0.025f, 0.16f), paint, false);
            MakeBlock("West pavement", new Vector3(-9.2f, 0.08f, 8f), new Vector3(5.2f, 0.25f, 94f), ground);
            MakeBlock("East pavement", new Vector3(9.2f, 0.08f, 8f), new Vector3(5.2f, 0.25f, 94f), ground);
            MakeBlock("Harbour retaining wall", new Vector3(0f, 1.1f, 57f), new Vector3(110f, 3.2f, 2.2f), metal);
            MakeBlock("Harbour water", new Vector3(0f, -0.18f, 78f), new Vector3(110f, 0.2f, 38f), water);
            MakeBlock("Pier west", new Vector3(-26f, 0.1f, 65f), new Vector3(18f, 0.5f, 18f), ground);
            MakeBlock("Pier east", new Vector3(28f, 0.1f, 67f), new Vector3(22f, 0.5f, 15f), ground);

            for (int z = -34; z <= 48; z += 12)
            {
                MakeBlock("Centre lane dash", new Vector3(0f, 0.09f, z), new Vector3(0.18f, 0.025f, 4.2f), paint, false);
                BuildStreetLamp(new Vector3(-11.2f, 0f, z + 3f), metal, lampGlow);
                BuildStreetLamp(new Vector3(11.2f, 0f, z - 3f), metal, lampGlow);
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

            // Road-facing storefronts and loading facades give the four main blocks a readable identity.
            BuildRoadsideFacade("KADE MOTOR WORKS", new Vector3(-15.5f, 0f, 6f),
                31f, 11f, true, metal, glass, accent);
            BuildRoadsideFacade("OLD QUAY COLD STORE", new Vector3(14.5f, 0f, 13f),
                34f, 15f, false, metal, glass, accent);
            BuildRoadsideFacade("HARBOUR EXCHANGE", new Vector3(-15.5f, 0f, 43f),
                18f, 13f, true, metal, glass, accent);
            BuildRoadsideFacade("SALTLlNE FREIGHT", new Vector3(16f, 0f, 44f),
                17f, 10f, false, metal, glass, accent);
            BuildQuaySurfaceDetails(wetRoad, metal, accent);
            BuildOldQuayMarketAndSouthLane(ground, road, paint, warehouse, brick, glass, metal, accent, lampGlow);
            BuildAccentLight("Kade workshop warm pool", new Vector3(-13.8f, 4.2f, -4f),
                new Color(1f, 0.62f, 0.35f), 2.0f, 9f);
            BuildAccentLight("Saltline handoff warm pool", new Vector3(5.4f, 4.4f, 49f),
                new Color(1f, 0.72f, 0.46f), 1.7f, 8f);

            // Container stacks and a simple dockside gantry give the harbour a distinct skyline.
            for (int row = 0; row < 2; row++)
            for (int col = 0; col < 3; col++)
            {
                Material containerMat = (row + col) % 2 == 0 ? accent : metal;
                float containerX = -39f + col * 8f;
                float containerY = 1.35f + row * 2.7f;
                MakeBlock("Stacked freight container", new Vector3(containerX, containerY, 61f),
                    new Vector3(7.6f, 2.6f, 5.8f), containerMat);
                for (int rib = -2; rib <= 2; rib++)
                    MakeBlock("Freight container corrugation rib",
                        new Vector3(containerX + rib * 1.25f, containerY, 58.06f),
                        new Vector3(0.075f, 2.36f, 0.075f), metal, false);
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
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 220f;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            cameraObject.AddComponent<AudioListener>();
            ThirdPersonCameraRig cameraRig = cameraObject.AddComponent<ThirdPersonCameraRig>();
            cameraRig.SetTarget(player.transform, new Vector3(0f, 1.45f, 0f), true);
            motor.SetMovementCamera(cameraObject.transform);
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 3f, -5f);
            Transform hudRoot = BuildMobileControls();
            BuildTutorialGameplay(player, cameraObject.transform, hudRoot, tutorialMission, firstScoreMission,
                paperTrailMission, marketLedgerMission, marketBackingMission, marketColdShoulderMission);

            // Scene metadata helps designers understand what is functional versus placeholder.
            new GameObject("BLOCKOUT NOTES — replace primitives before release");

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Selection.activeGameObject = player;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("PORT SABLE blockout created",
                    "Saved and added to Build Settings:\n" + ScenePath +
                    "\n\nPlayable now: WASD / arrows or touch joystick to move; Shift / Sprint to sprint; Space / Jump to jump; mouse / right-side touch pad to look; E / Interact to use world objects.\n" +
                    "Includes Juno dialogue, the eight-stage delivery tutorial, a four-stage first score, staged police patrol dispatch and a vehicle-gated safehouse arrival. This is still a primitive greybox, not final art or a complete campaign.",
                    "OK");
                }
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

            CreateActionButton(canvas.transform, "JUMP / HANDBRAKE", MobileTouchControlMode.JumpButton,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(142f, 142f), new Vector2(-170f, 170f));
            CreateActionButton(canvas.transform, "INTERACT", MobileTouchControlMode.InteractButton,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(126f, 126f), new Vector2(-340f, 126f));
            CreateActionButton(canvas.transform, "SPRINT / THROTTLE", MobileTouchControlMode.SprintButton,
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
            image.gameObject.AddComponent<MobileTouchControl>().Configure(mode, null, 0.055f, text);
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

        private static MissionDefinition EnsureMarketBackingMission()
        {
            EnsureFolder("Assets/ProjectTideline/Content/Missions/Prologues");
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MarketBackingMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, MarketBackingMissionPath);
            }

            mission.missionId = "old_quay_market_backing";
            mission.title = "A Place at the Table";
            mission.briefing = "The vendor remembers your promise, and the cafe owner has heard you carried their concern. They have set aside a crate of supplies for Kade Motor Works. Collect it from the pier market.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "collect_market_backing",
                    description = "Collect the reserved supplies from the Breakwater Pier market.",
                    completionSignal = "old_quay.market_backing.supplies_collected",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = string.Empty;
            mission.endsCampaign = true;
            EditorUtility.SetDirty(mission);
            return mission;
        }

        private static MissionDefinition EnsureMarketColdShoulderMission()
        {
            EnsureFolder("Assets/ProjectTideline/Content/Missions/Prologues");
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MarketColdShoulderMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, MarketColdShoulderMissionPath);
            }

            mission.missionId = "old_quay_market_cold_shoulder";
            mission.title = "No More Favors";
            mission.briefing = "The market's business is settled, but neither side is ready to extend a hand. Find the independent quay supplier and collect the replacement order for Kade Motor Works.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "collect_independent_order",
                    description = "Collect the replacement order from the independent quay supplier.",
                    completionSignal = "old_quay.market_cold_shoulder.order_collected",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = string.Empty;
            mission.endsCampaign = true;
            EditorUtility.SetDirty(mission);
            return mission;
        }

        private static DialogueGraph EnsureMarketVendorDialogue()
        {
            DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(MarketVendorDialoguePath);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<DialogueGraph>();
                AssetDatabase.CreateAsset(graph, MarketVendorDialoguePath);
            }

            graph.graphId = "old_quay.breakwater_vendor_missing_tally";
            graph.startNodeId = "vendor_explains_missing_tally";
            graph.nodes = new[]
            {
                new DialogueGraph.DialogueNode
                {
                    nodeId = "vendor_explains_missing_tally",
                    speaker = "BREAKWATER MARKET VENDOR",
                    line = "The morning delivery came in short, and the tally sheet vanished before I could check the crates. The cafe owner needs the count before closing. I marked the storage crate, but I need someone I can trust to bring the sheet back.",
                    choices = new[]
                    {
                        new DialogueGraph.DialogueChoice
                        {
                            label = "I'll bring it straight back. You have my word.",
                            signalId = "old_quay.market_ledger.vendor_met",
                            relationshipCharacterId = "breakwater_vendor",
                            relationshipDelta = 2,
                            storyFlagId = "market_vendor_promised_return",
                            storyFlagValue = 1
                        },
                        new DialogueGraph.DialogueChoice
                        {
                            label = "Just point me to the crate. I'll see what I find.",
                            signalId = "old_quay.market_ledger.vendor_met",
                            relationshipCharacterId = "breakwater_vendor",
                            relationshipDelta = -1,
                            storyFlagId = "market_vendor_promised_return",
                            storyFlagValue = 0
                        }
                    }
                }
            };
            EditorUtility.SetDirty(graph);
            return graph;
        }

        private static DialogueGraph EnsureCafeOwnerDialogue()
        {
            DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(CafeOwnerDialoguePath);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<DialogueGraph>();
                AssetDatabase.CreateAsset(graph, CafeOwnerDialoguePath);
            }

            graph.graphId = "old_quay.breakwater_cafe_tally_return";
            graph.startNodeId = "owner_checks_stock_tally";
            graph.nodes = new[]
            {
                new DialogueGraph.DialogueNode
                {
                    nodeId = "owner_checks_stock_tally",
                    speaker = "BREAKWATER CAFE OWNER",
                    line = "That's the sheet. I can finally reconcile the market order before the evening rush. The vendor said the crate count was wrong again. Did they ask you to pass anything on?",
                    choices = new[]
                    {
                        new DialogueGraph.DialogueChoice
                        {
                            label = "They were worried about the missing count. I said I'd return it.",
                            signalId = "old_quay.market_ledger.tally_returned",
                            relationshipCharacterId = "breakwater_cafe_owner",
                            relationshipDelta = 2,
                            storyFlagId = "market_tally_returned_with_context",
                            storyFlagValue = 1
                        },
                        new DialogueGraph.DialogueChoice
                        {
                            label = "I found the sheet. That's all I was asked to do.",
                            signalId = "old_quay.market_ledger.tally_returned",
                            relationshipCharacterId = "breakwater_cafe_owner",
                            relationshipDelta = 0,
                            storyFlagId = "market_tally_returned_with_context",
                            storyFlagValue = 0
                        }
                    }
                }
            };
            EditorUtility.SetDirty(graph);
            return graph;
        }

        private static DialogueGraph EnsureCustomerHandoffDialogue()
        {
            DialogueGraph graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(CustomerHandoffDialoguePath);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<DialogueGraph>();
                AssetDatabase.CreateAsset(graph, CustomerHandoffDialoguePath);
            }

            graph.graphId = "old_quay.saltline_customer_handoff";
            graph.startNodeId = "customer_checks_delivery";
            graph.nodes = new[]
            {
                new DialogueGraph.DialogueNode
                {
                    nodeId = "customer_checks_delivery",
                    speaker = "SALTLINE CUSTOMER",
                    line = "You made it. I was starting to think the harbour office had sent the wrong vehicle. Is everything accounted for?",
                    choices = new[]
                    {
                        new DialogueGraph.DialogueChoice
                        {
                            label = "Keys and paperwork. Everything is accounted for.",
                            signalId = "mara.delivery.finished",
                            relationshipCharacterId = "saltline_customer",
                            relationshipDelta = 2
                        },
                        new DialogueGraph.DialogueChoice
                        {
                            label = "The vehicle is here. Sign for it.",
                            signalId = "mara.delivery.finished",
                            relationshipCharacterId = "saltline_customer",
                            relationshipDelta = -1
                        }
                    }
                }
            };
            EditorUtility.SetDirty(graph);
            return graph;
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
                    line = "The harbour ledger is wrong, and the customer is already waiting at Saltline. Check the entry, take the finished car from the workshop, and get it to the delivery bay. Keep it clean. I do not want another call from the harbour office.",
                    choices = new[]
                    {
                        new DialogueGraph.DialogueChoice
                        {
                            label = "I'll verify the ledger and make the delivery.",
                            signalId = "old_quay.juno_briefing_accepted",
                            relationshipCharacterId = "juno_kade",
                            relationshipDelta = 1
                        },
                        new DialogueGraph.DialogueChoice
                        {
                            label = "You could have told me sooner.",
                            signalId = "old_quay.juno_briefing_accepted",
                            relationshipCharacterId = "juno_kade",
                            relationshipDelta = -1
                        }
                    }
                }
            };
            EditorUtility.SetDirty(graph);

            DialogueGraph debriefGraph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(TutorialDebriefDialoguePath);
            if (debriefGraph == null)
            {
                debriefGraph = ScriptableObject.CreateInstance<DialogueGraph>();
                AssetDatabase.CreateAsset(debriefGraph, TutorialDebriefDialoguePath);
            }
            debriefGraph.graphId = "old_quay.juno_debrief";
            debriefGraph.startNodeId = "juno_debrief";
            debriefGraph.nodes = new[]
            {
                new DialogueGraph.DialogueNode
                {
                    nodeId = "juno_debrief",
                    speaker = "JUNO",
                    line = "Customer signed. That's one less problem between this workshop and next week's rent. You handled the road; I'll handle the paperwork. We are not done yet, but for tonight, the doors stay open.",
                    choices = new[]
                    {
                        new DialogueGraph.DialogueChoice
                        {
                            label = "We keep the place running.",
                            signalId = "old_quay.juno_debrief_completed",
                            relationshipCharacterId = "juno_kade",
                            relationshipDelta = 1
                        },
                        new DialogueGraph.DialogueChoice
                        {
                            label = "Next time, tell me the whole story.",
                            signalId = "old_quay.juno_debrief_completed",
                            relationshipCharacterId = "juno_kade",
                            relationshipDelta = 0
                        }
                    }
                }
            };
            EditorUtility.SetDirty(debriefGraph);

            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(TutorialMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, TutorialMissionPath);
            }
            mission.missionId = "old_quay_first_steps";
            mission.title = "OLD QUAY — THE CUSTOMER'S RUN";
            mission.briefing = "Verify the harbour ledger, accept the delivery job, enter the customer's car, drive the Old Quay route, stop in the marked bay, hand over the vehicle and report back to Juno.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "meet_juno",
                    description = "Talk to Juno outside Kade Motor Works.",
                    completionSignal = "old_quay.juno_briefing_accepted",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "inspect_harbour_ledger",
                    description = "Travel to the harbour office and inspect the shipment ledger.",
                    completionSignal = "old_quay.harbour_ledger_inspected",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "start_customer_delivery",
                    description = "Return to the workshop and accept the customer delivery job at the order board.",
                    completionSignal = "race.long_loop.started",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "enter_delivery_vehicle",
                    description = "Get into the Kade delivery car.",
                    completionSignal = "vehicle.entered",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "drive_customer_delivery_route",
                    description = "Drive through all three Old Quay-to-Saltline route checkpoints.",
                    completionSignal = "race.long_loop.finished",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "park_in_delivery_bay",
                    description = "Slow down and stop the car inside the marked Saltline delivery bay.",
                    completionSignal = "old_quay.delivery_arrived",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "hand_over_customer_vehicle",
                    description = "Exit the car and hand the vehicle to the waiting customer.",
                    completionSignal = "mara.delivery.finished",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "return_to_juno",
                    description = "Return to Kade Motor Works and tell Juno the customer accepted the delivery.",
                    completionSignal = "old_quay.juno_debrief_completed",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = "old_quay_first_score";
            mission.endsCampaign = false;
            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mission;
        }

        private static MissionDefinition EnsureFirstScoreMission()
        {
            EnsureFolder("Assets/ProjectTideline/Content/Missions/Prologues");
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(FirstScoreMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, FirstScoreMissionPath);
            }

            mission.missionId = "old_quay_first_score";
            mission.title = "The First Score";
            mission.briefing = "A Saltline contact offers Mara a quick recovery job. Take the sealed case, get back to the car and bring it to the safehouse before the situation escalates.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "accept_score",
                    description = "Meet the Saltline contact and accept the sealed-case score.",
                    completionSignal = "old_quay.score.accepted",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "recover_case",
                    description = "Recover the marked sealed case from the quay.",
                    completionSignal = "old_quay.score.case_recovered",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "enter_score_getaway_vehicle",
                    description = "Get back into the Kade car.",
                    completionSignal = "vehicle.entered",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "escape_to_safehouse",
                    description = "Drive the case to the marked Old Quay safehouse.",
                    completionSignal = "old_quay.score.safehouse_reached",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = string.Empty;
            mission.endsCampaign = false;
            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mission;
        }

        private static MissionDefinition EnsurePaperTrailMission()
        {
            EnsureFolder("Assets/ProjectTideline/Content/Missions/Prologues");
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(PaperTrailMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, PaperTrailMissionPath);
            }

            mission.missionId = "old_quay_paper_trail";
            mission.title = "The Paper Trail";
            mission.briefing = "The sealed case points to a second set of freight records. Find the dock clerk, recover the duplicate manifest and bring it to the Saltline contact before the trail goes cold.";
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "meet_freight_clerk",
                    description = "Find the freight clerk beside the cold store and ask about the duplicate manifest.",
                    completionSignal = "old_quay.paper_trail.clerk_met",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "recover_duplicate_manifest",
                    description = "Recover the duplicate freight manifest from the marked loading desk.",
                    completionSignal = "old_quay.paper_trail.manifest_recovered",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "deliver_manifest_copy",
                    description = "Deliver the manifest copy to the Saltline contact.",
                    completionSignal = "old_quay.paper_trail.manifest_delivered",
                    requiredCount = 1
                }
            };
            mission.completionBranches = Array.Empty<MissionBranch>();
            mission.fallbackNextMissionId = string.Empty;
            mission.endsCampaign = false;
            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mission;
        }

        private static MissionDefinition EnsureMarketLedgerMission()
        {
            EnsureFolder("Assets/ProjectTideline/Content/Missions/Prologues");
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MarketLedgerMissionPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDefinition>();
                AssetDatabase.CreateAsset(mission, MarketLedgerMissionPath);
            }

            mission.missionId = "old_quay_market_ledger";
            mission.title = "The Market Ledger";
            mission.briefing = "A Breakwater Pier stallholder is missing a tally sheet after a late delivery. Find the vendor, recover the marked stock tally and return it to the Breakwater Cafe owner."; 
            mission.objectives = new[]
            {
                new MissionObjective
                {
                    objectiveId = "meet_market_vendor",
                    description = "Speak to the Breakwater Pier market vendor about the missing stock tally.",
                    completionSignal = "old_quay.market_ledger.vendor_met",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "recover_stock_tally",
                    description = "Recover the marked stock tally from the pier-side storage crate.",
                    completionSignal = "old_quay.market_ledger.tally_recovered",
                    requiredCount = 1
                },
                new MissionObjective
                {
                    objectiveId = "return_stock_tally",
                    description = "Return the stock tally to the Breakwater Cafe owner.",
                    completionSignal = "old_quay.market_ledger.tally_returned",
                    requiredCount = 1
                }
            };
            mission.completionBranches = new[]
            {
                new MissionBranch
                {
                    nextMissionId = "old_quay_market_backing",
                    conditions = new[]
                    {
                        new MissionBranchCondition
                        {
                            storyFlagId = "market_vendor_promised_return",
                            minimumValue = 1,
                            maximumValue = 1
                        },
                        new MissionBranchCondition
                        {
                            storyFlagId = "market_tally_returned_with_context",
                            minimumValue = 1,
                            maximumValue = 1
                        },
                        new MissionBranchCondition
                        {
                            relationshipKey = "breakwater_vendor",
                            minimumValue = 2,
                            maximumValue = 100
                        },
                        new MissionBranchCondition
                        {
                            relationshipKey = "breakwater_cafe_owner",
                            minimumValue = 2,
                            maximumValue = 100
                        }
                    }
                }
            };
            mission.fallbackNextMissionId = "old_quay_market_cold_shoulder";
            mission.endsCampaign = false;
            EditorUtility.SetDirty(mission);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mission;
        }

        private static void BuildTutorialGameplay(GameObject player, Transform cameraTransform,
            Transform hudRoot, MissionDefinition tutorialMission, MissionDefinition firstScoreMission,
            MissionDefinition paperTrailMission, MissionDefinition marketLedgerMission,
            MissionDefinition marketBackingMission, MissionDefinition marketColdShoulderMission)
        {
            new GameObject("NPC Relationship Ledger").AddComponent<NPCRelationshipLedger>();
            new GameObject("Narrative State Ledger").AddComponent<NarrativeStateLedger>();

            GameObject missionObject = new GameObject("Mission Director — Old Quay customer's run");
            MissionDirector missionDirector = missionObject.AddComponent<MissionDirector>();
            SerializedObject missionSerialized = new SerializedObject(missionDirector);
            SerializedProperty campaign = missionSerialized.FindProperty("campaign");
            campaign.arraySize = 6;
            campaign.GetArrayElementAtIndex(0).objectReferenceValue = tutorialMission;
            campaign.GetArrayElementAtIndex(1).objectReferenceValue = firstScoreMission;
            campaign.GetArrayElementAtIndex(2).objectReferenceValue = paperTrailMission;
            campaign.GetArrayElementAtIndex(3).objectReferenceValue = marketLedgerMission;
            campaign.GetArrayElementAtIndex(4).objectReferenceValue = marketBackingMission;
            campaign.GetArrayElementAtIndex(5).objectReferenceValue = marketColdShoulderMission;
            missionSerialized.FindProperty("autoStart").boolValue = true;
            missionSerialized.ApplyModifiedPropertiesWithoutUndo();

            // Mission objective HUD.
            Image missionPanel = CreateTouchImage(hudRoot, "Mission HUD background",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(760f, 168f), new Vector2(28f, -28f),
                new Color(0.035f, 0.055f, 0.06f, 0.76f), out RectTransform missionPanelRect);
            missionPanel.raycastTarget = false;
            Text missionText = CreateUiText(missionPanelRect, "Mission objective text",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 25, TextAnchor.MiddleLeft, Color.white);
            missionText.rectTransform.offsetMin = new Vector2(22f, 12f);
            missionText.rectTransform.offsetMax = new Vector2(-22f, -12f);
            MissionStatusDisplay missionDisplay = missionObject.AddComponent<MissionStatusDisplay>();
            missionDisplay.Configure(missionDirector, missionText);

            GameObject attentionObject = new GameObject("Public Attention / Police Pressure State");
            PublicAttentionState attentionState = attentionObject.AddComponent<PublicAttentionState>();
            Image pressurePanel = CreateTouchImage(hudRoot, "Police pressure HUD background",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(340f, 92f), new Vector2(-28f, -198f),
                new Color(0.025f, 0.045f, 0.05f, 0.84f), out RectTransform pressureRect);
            pressurePanel.raycastTarget = false;
            Text pressureText = CreateUiText(pressureRect, "Police pressure status",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 23, TextAnchor.MiddleCenter, Color.white);
            PolicePressureHUD pressureHud = missionObject.AddComponent<PolicePressureHUD>();
            pressureHud.Configure(attentionState, pressureText);

            Image responsePanel = CreateTouchImage(hudRoot, "Police response HUD background",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(340f, 62f), new Vector2(-28f, -266f),
                new Color(0.025f, 0.045f, 0.05f, 0.84f), out RectTransform responseRect);
            responsePanel.raycastTarget = false;
            Text responseText = CreateUiText(responseRect, "Police response status",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 19, TextAnchor.MiddleCenter, Color.white);
            PoliceResponseDirector responseDirector = missionObject.AddComponent<PoliceResponseDirector>();
            responseDirector.Configure(attentionState, player.transform, responseText);

            Image vehicleStatusPanel = CreateTouchImage(hudRoot, "Vehicle speedometer panel",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(340f, 154f), new Vector2(-28f, -28f),
                new Color(0.025f, 0.045f, 0.05f, 0.84f), out RectTransform vehicleStatusRect);
            vehicleStatusPanel.raycastTarget = false;
            Text vehicleSpeedText = CreateUiText(vehicleStatusRect, "Vehicle speed and parking guidance",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 25, TextAnchor.MiddleCenter, Color.white);
            vehicleSpeedText.text = "0 KM/H";
            vehicleSpeedText.raycastTarget = false;
            vehicleStatusPanel.gameObject.SetActive(false);

            // World interaction prompt.
            Image promptPanel = CreateTouchImage(hudRoot, "World interaction prompt",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(760f, 76f), new Vector2(0f, 92f),
                new Color(0.035f, 0.055f, 0.06f, 0.84f), out RectTransform promptRect);
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

            // Juno's opening dialogue: either choice advances, while changing the relationship ledger.
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

            Button firstChoice = CreateUiButton(dialogueRect, "Choice — accept", "I'll verify the ledger and deliver it.",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f),
                new Vector2(500f, 84f), new Vector2(-18f, 28f));
            Button secondChoice = CreateUiButton(dialogueRect, "Choice — push back", "You could have told me sooner.",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f),
                new Vector2(500f, 84f), new Vector2(18f, 28f));

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
            MissionDialogueSequenceInteractable junoInteractable =
                juno.AddComponent<MissionDialogueSequenceInteractable>();
            junoInteractable.Configure(missionDirector, dialogueController, "old_quay_first_steps",
                new[]
                {
                    new MissionDialogueSequenceInteractable.DialogueStep
                    {
                        objectiveId = "meet_juno",
                        graph = graph,
                        prompt = "Talk to Juno"
                    },
                    new MissionDialogueSequenceInteractable.DialogueStep
                    {
                        objectiveId = "return_to_juno",
                        graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(TutorialDebriefDialoguePath),
                        prompt = "Report back to Juno"
                    }
                });

            GameObject ledger = MakeBlock("HARBOUR OFFICE — shipment ledger",
                new Vector3(7f, 0.9f, 47f), new Vector3(1.6f, 1.8f, 1.2f),
                MakeMaterial("Ledger Kiosk", new Color(0.32f, 0.37f, 0.35f)));
            ledger.AddComponent<MissionSignalInteractable>().Configure(
                MissionSignalInteractable.HarbourLedgerSignal, "Inspect shipment ledger", true,
                missionDirector, "old_quay_first_steps", "inspect_harbour_ledger");

            VehicleSeatInteractable deliverySeat;
            VehicleDriveController deliveryVehicle = BuildCustomerDeliveryVehicle(cameraTransform, missionDirector, out deliverySeat);
            deliverySeat.ConfigureAdditionalMissionGate(missionDirector,
                "old_quay_first_score", "enter_score_getaway_vehicle");
            RouteRaceController deliveryRoute = BuildCustomerDeliveryRoute(deliveryVehicle);
            missionDisplay.SetDeliveryRoute(deliveryRoute);
            missionDisplay.SetVehicleStatus(deliveryVehicle, vehicleStatusPanel.gameObject, vehicleSpeedText);

            GameObject orderBoard = MakeBlock("KADE MOTOR WORKS — customer delivery order board",
                new Vector3(-3.0f, 0.95f, -13.2f), new Vector3(0.45f, 1.9f, 0.28f),
                MakeMaterial("Delivery Order Board", new Color(0.78f, 0.43f, 0.14f)));
            RouteRaceStartInteractable routeStart = orderBoard.AddComponent<RouteRaceStartInteractable>();
            routeStart.Configure(deliveryRoute, missionDirector, "old_quay_first_steps",
                "start_customer_delivery", "Accept customer delivery");

            VehicleDeliveryCheckpoint deliveryBay = BuildCustomerDeliveryBay(deliveryVehicle, deliverySeat, deliveryRoute);
            missionDisplay.SetDeliveryBay(deliveryBay);
            BuildCustomerHandoffActor(deliveryRoute, deliveryBay, deliverySeat, missionDirector, dialogueController);
            BuildFirstScoreWorld(deliveryVehicle, deliverySeat, missionDirector);
            BuildPaperTrailWorld(missionDirector);
            BuildMarketLedgerWorld(missionDirector, dialogueController);
            BuildBreakwaterConsequenceWorld(missionDirector);
            // A compact four-way junction sits in the open southern block, clear of warehouse footprints.
            TrafficSignalController junctionSignals = BuildOldQuayJunction(road, paint, metal, accent);
            Transform northboundStop = MakeTrafficStopLine("Northbound stop line", -3.8f, -30.5f);
            Transform southboundStop = MakeTrafficStopLine("Southbound stop line", 3.8f, -15.5f);
            Transform eastboundStop = MakeTrafficStopLine("Eastbound stop line", -7.5f, -26.8f);
            Transform westboundStop = MakeTrafficStopLine("Westbound stop line", 7.5f, -19.2f);
            TrafficPedestrianCrossing crosswalk = BuildOldQuayPedestrianCrossing(junctionSignals);
            TrafficJunctionReservation junctionReservation = BuildOldQuayJunctionReservation();

            // Main-road traffic: two cars in each direction, with stop-line awareness.
            BuildAmbientTrafficVehicle("Old Quay traffic — northbound A",
                new[] { new Vector3(-3.8f, 0.1f, -34f), new Vector3(-3.8f, 0.1f, -12f),
                        new Vector3(-3.8f, 0.1f, 12f), new Vector3(-3.8f, 0.1f, 46f) },
                new Color(0.22f, 0.30f, 0.34f), junctionSignals, northboundStop, true, crosswalk, junctionReservation);
            BuildAmbientTrafficVehicle("Old Quay traffic — northbound B",
                new[] { new Vector3(-3.8f, 0.1f, -14f), new Vector3(-3.8f, 0.1f, 8f),
                        new Vector3(-3.8f, 0.1f, 30f), new Vector3(-3.8f, 0.1f, 46f) },
                new Color(0.43f, 0.34f, 0.22f), junctionSignals, northboundStop, true, crosswalk);
            BuildAmbientTrafficVehicle("Old Quay traffic — southbound A",
                new[] { new Vector3(3.8f, 0.1f, 46f), new Vector3(3.8f, 0.1f, 22f),
                        new Vector3(3.8f, 0.1f, -2f), new Vector3(3.8f, 0.1f, -34f) },
                new Color(0.38f, 0.27f, 0.19f), junctionSignals, southboundStop, true, crosswalk, junctionReservation);
            BuildAmbientTrafficVehicle("Old Quay traffic — southbound B",
                new[] { new Vector3(3.8f, 0.1f, 24f), new Vector3(3.8f, 0.1f, 2f),
                        new Vector3(3.8f, 0.1f, -20f), new Vector3(3.8f, 0.1f, -34f) },
                new Color(0.20f, 0.34f, 0.30f), junctionSignals, southboundStop, true, crosswalk);

            // Cross-street traffic gives the second signal phase a real purpose.
            BuildAmbientTrafficVehicle("Old Quay traffic — eastbound",
                new[] { new Vector3(-42f, 0.1f, -26.8f), new Vector3(-20f, 0.1f, -26.8f),
                        new Vector3(-10f, 0.1f, -26.8f), new Vector3(-6f, 0.1f, -25.8f),
                        new Vector3(-3.8f, 0.1f, -22f), new Vector3(-3.8f, 0.1f, -14f),
                        new Vector3(-3.8f, 0.1f, 12f), new Vector3(-3.8f, 0.1f, 46f) },
                new Color(0.34f, 0.28f, 0.40f), junctionSignals, eastboundStop, false, crosswalk, junctionReservation);
            BuildAmbientTrafficVehicle("Old Quay traffic — westbound",
                new[] { new Vector3(42f, 0.1f, -19.2f), new Vector3(20f, 0.1f, -19.2f),
                        new Vector3(-20f, 0.1f, -19.2f), new Vector3(-42f, 0.1f, -19.2f) },
                new Color(0.36f, 0.37f, 0.25f), junctionSignals, westboundStop, false, crosswalk, junctionReservation);

            BuildAmbientPedestrian("DOCK WORKER — east sidewalk", 9.35f,
                new[] { -29f, -5f, 20f, 49f }, new Color(0.54f, 0.36f, 0.25f), 1.2f);
            BuildAmbientPedestrian("LOCAL RESIDENT — west sidewalk", -9.35f,
                new[] { 49f, 20f, -5f, -29f }, new Color(0.30f, 0.39f, 0.52f), 1.05f);
            BuildAmbientPedestrianRoute("BREAKWATER CAFE CUSTOMER", new[]
            {
                new Vector3(-44f, 1f, -43.2f), new Vector3(-24f, 1f, -43.2f),
                new Vector3(-24f, 1f, -42.4f), new Vector3(-44f, 1f, -42.4f)
            }, new Color(0.48f, 0.28f, 0.34f), 0.92f, 1.1f);
            BuildAmbientPedestrianRoute("QUAY TOOL AND TACKLE CUSTOMER", new[]
            {
                new Vector3(24f, 1f, -43.2f), new Vector3(44f, 1f, -43.2f),
                new Vector3(44f, 1f, -42.4f), new Vector3(24f, 1f, -42.4f)
            }, new Color(0.28f, 0.43f, 0.38f), 1.0f, 2.4f);
            BuildAmbientPedestrianRoute("PIER MARKET VENDOR", new[]
            {
                new Vector3(18f, 1f, 57f), new Vector3(38f, 1f, 57f),
                new Vector3(38f, 1f, 75f), new Vector3(18f, 1f, 75f)
            }, new Color(0.55f, 0.43f, 0.25f), 0.82f, 0.7f);
            BuildAmbientPedestrianRoute("DOCK PORTER — pier approach", new[]
            {
                new Vector3(15f, 1f, 52f), new Vector3(42f, 1f, 52f),
                new Vector3(42f, 1f, 78f), new Vector3(15f, 1f, 78f)
            }, new Color(0.26f, 0.36f, 0.48f), 1.12f, 3.2f);
        }

        private static void BuildFirstScoreWorld(VehicleDriveController getawayVehicle,
            VehicleSeatInteractable getawaySeat, MissionDirector missionDirector)
        {
            GameObject contact = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            contact.name = "SALTLINE CONTACT — first score";
            contact.transform.position = new Vector3(8.5f, 1f, 7f);
            contact.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial("Saltline contact — muted green", new Color(0.25f, 0.36f, 0.30f));
            contact.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.score.accepted", "Accept the score", true,
                missionDirector, "old_quay_first_score", "accept_score");

            GameObject caseObject = MakeBlock("SEALED CASE — marked recovery item",
                new Vector3(-5.5f, 0.72f, 28f), new Vector3(1.05f, 0.82f, 0.72f),
                MakeMaterial("Sealed case — dark graphite", new Color(0.12f, 0.15f, 0.16f)));
            caseObject.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.score.case_recovered", "Recover sealed case", true,
                missionDirector, "old_quay_first_score", "recover_case");

            GameObject safehouse = new GameObject("OLD QUAY SAFEHOUSE — score arrival zone");
            safehouse.transform.position = new Vector3(0f, 1.2f, -34f);
            BoxCollider safehouseTrigger = safehouse.AddComponent<BoxCollider>();
            safehouseTrigger.isTrigger = true;
            safehouseTrigger.size = new Vector3(11f, 3.2f, 8f);
            MissionVehicleArrivalZone arrival = safehouse.AddComponent<MissionVehicleArrivalZone>();
            arrival.Configure(getawayVehicle, getawaySeat, missionDirector,
                "old_quay_first_score", "escape_to_safehouse",
                "old_quay.score.safehouse_reached");

            Material safehousePaint = MakeMaterial("Safehouse arrival paint", new Color(0.72f, 0.52f, 0.25f));
            MakeBlock("Safehouse arrival bay left marker", new Vector3(-4.2f, 0.10f, -34f),
                new Vector3(0.16f, 0.035f, 7.4f), safehousePaint, false);
            MakeBlock("Safehouse arrival bay right marker", new Vector3(4.2f, 0.10f, -34f),
                new Vector3(0.16f, 0.035f, 7.4f), safehousePaint, false);
            MakeBlock("Safehouse arrival bay stop marker", new Vector3(0f, 0.10f, -37.6f),
                new Vector3(8.4f, 0.035f, 0.16f), safehousePaint, false);
        }

        private static void BuildBreakwaterConsequenceWorld(MissionDirector missionDirector)
        {
            GameObject reservedSupplies = MakeBlock("BREAKWATER MARKET — reserved Kade supplies",
                new Vector3(32f, 0.92f, 69f), new Vector3(1.25f, 1.1f, 0.9f),
                MakeMaterial("Reserved market supplies — canvas", new Color(0.47f, 0.34f, 0.22f)));
            reservedSupplies.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.market_backing.supplies_collected", "Collect reserved supplies", true,
                missionDirector, "old_quay_market_backing", "collect_market_backing");

            GameObject independentOrder = MakeBlock("QUAY SUPPLIER — replacement order envelope",
                new Vector3(-40f, 0.9f, 39f), new Vector3(0.9f, 0.08f, 0.62f),
                MakeMaterial("Independent supplier order — paper", new Color(0.74f, 0.68f, 0.51f)), false);
            independentOrder.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.market_cold_shoulder.order_collected", "Collect replacement order", true,
                missionDirector, "old_quay_market_cold_shoulder", "collect_independent_order");

            Material marker = MakeMaterial("Market consequence objective marker", new Color(0.80f, 0.56f, 0.24f));
            MakeBlock("Reserved supplies marker", new Vector3(32f, 0.10f, 69f),
                new Vector3(2.0f, 0.035f, 1.4f), marker, false);
            MakeBlock("Independent supplier marker", new Vector3(-40f, 0.10f, 39f),
                new Vector3(2.0f, 0.035f, 1.4f), marker, false);
        }

        private static void BuildPaperTrailWorld(MissionDirector missionDirector)
        {
            Material clerkMaterial = MakeMaterial("Freight clerk — slate workwear", new Color(0.34f, 0.40f, 0.43f));
            GameObject clerk = BuildPedestrianCharacter("FREIGHT CLERK — duplicate manifest lead",
                new Vector3(9.4f, 1f, 38f), new Color(0.34f, 0.40f, 0.43f));
            clerk.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.paper_trail.clerk_met", "Ask about the duplicate manifest", true,
                missionDirector, "old_quay_paper_trail", "meet_freight_clerk");

            Material paperMaterial = MakeMaterial("Duplicate manifest — pale stock", new Color(0.78f, 0.72f, 0.56f));
            GameObject manifest = MakeBlock("DUPLICATE FREIGHT MANIFEST — loading desk",
                new Vector3(-5.5f, 0.76f, 43f), new Vector3(0.9f, 0.08f, 0.62f), paperMaterial, false);
            manifest.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.paper_trail.manifest_recovered", "Recover duplicate manifest", true,
                missionDirector, "old_quay_paper_trail", "recover_duplicate_manifest");

            GameObject dropContact = BuildPedestrianCharacter("SALTLlNE CONTACT — manifest handoff",
                new Vector3(10.2f, 1f, 7f), new Color(0.25f, 0.34f, 0.43f));
            dropContact.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.paper_trail.manifest_delivered", "Hand over manifest copy", true,
                missionDirector, "old_quay_paper_trail", "deliver_manifest_copy");

            Material marker = MakeMaterial("Paper trail objective marker", new Color(0.80f, 0.56f, 0.24f));
            MakeBlock("Manifest desk marker", new Vector3(-5.5f, 0.11f, 43f),
                new Vector3(2.2f, 0.035f, 1.4f), marker, false);
            MakeBlock("Clerk meeting marker", new Vector3(9.4f, 0.10f, 38f),
                new Vector3(2.0f, 0.035f, 1.4f), marker, false);
        }

        private static void BuildMarketLedgerWorld(MissionDirector missionDirector, DialogueController dialogueController)
        {
            GameObject vendor = BuildPedestrianCharacter("BREAKWATER PIER MARKET — stock tally vendor",
                new Vector3(29f, 1f, 63f), new Color(0.48f, 0.31f, 0.22f));
            MissionDialogueSequenceInteractable vendorDialogue =
                vendor.AddComponent<MissionDialogueSequenceInteractable>();
            vendorDialogue.Configure(missionDirector, dialogueController, "old_quay_market_ledger",
                new[]
                {
                    new MissionDialogueSequenceInteractable.DialogueStep
                    {
                        objectiveId = "meet_market_vendor",
                        graph = EnsureMarketVendorDialogue(),
                        prompt = "Talk to the market vendor"
                    }
                });

            GameObject tally = MakeBlock("STOCK TALLY — pier storage crate",
                new Vector3(39f, 0.86f, 67f), new Vector3(0.82f, 0.10f, 0.58f),
                MakeMaterial("Stock tally — faded paper", new Color(0.78f, 0.70f, 0.52f)), false);
            tally.AddComponent<MissionSignalInteractable>().Configure(
                "old_quay.market_ledger.tally_recovered", "Recover stock tally", true,
                missionDirector, "old_quay_market_ledger", "recover_stock_tally");

            GameObject cafeOwner = BuildPedestrianCharacter("BREAKWATER CAFE — owner awaiting stock tally",
                new Vector3(-35f, 1f, -40f), new Color(0.24f, 0.38f, 0.32f));
            MissionDialogueSequenceInteractable ownerDialogue =
                cafeOwner.AddComponent<MissionDialogueSequenceInteractable>();
            ownerDialogue.Configure(missionDirector, dialogueController, "old_quay_market_ledger",
                new[]
                {
                    new MissionDialogueSequenceInteractable.DialogueStep
                    {
                        objectiveId = "return_stock_tally",
                        graph = EnsureCafeOwnerDialogue(),
                        prompt = "Speak to the cafe owner"
                    }
                });

            Material marker = MakeMaterial("Market ledger objective marker", new Color(0.80f, 0.56f, 0.24f));
            MakeBlock("Market vendor meeting marker", new Vector3(29f, 0.10f, 63f),
                new Vector3(2.0f, 0.035f, 1.4f), marker, false);
            MakeBlock("Stock tally crate marker", new Vector3(39f, 0.10f, 67f),
                new Vector3(2.0f, 0.035f, 1.4f), marker, false);
            MakeBlock("Cafe owner meeting marker", new Vector3(-35f, 0.10f, -40f),
                new Vector3(2.0f, 0.035f, 1.4f), marker, false);
        }

        private static void BuildAmbientTrafficVehicle(string name, Vector3[] routePoints,
            Color bodyColor, TrafficSignalController signal, Transform stopLine, bool usesMainRoadSignal,
            TrafficPedestrianCrossing pedestrianCrossing, TrafficJunctionReservation junctionReservation)
        {
            if (routePoints == null || routePoints.Length < 2) return;

            Vector3 initialDirection = routePoints[1] - routePoints[0];
            initialDirection.y = 0f;
            if (initialDirection.sqrMagnitude < 0.01f) return;

            GameObject car = new GameObject(name);
            car.transform.position = new Vector3(routePoints[0].x, 0f, routePoints[0].z);
            car.transform.rotation = Quaternion.LookRotation(initialDirection.normalized, Vector3.up);

            Rigidbody body = car.AddComponent<Rigidbody>();
            body.mass = 1220f;
            body.linearDamping = 0.16f;
            body.angularDamping = 1.4f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            BoxCollider chassisCollider = car.AddComponent<BoxCollider>();
            chassisCollider.center = new Vector3(0f, 0.72f, 0f);
            chassisCollider.size = new Vector3(1.9f, 0.62f, 3.8f);

            Material paint = MakeMaterial(name + " — body paint", bodyColor, 0.32f, 0.48f);
            Material trim = MakeMaterial(name + " — dark trim", new Color(0.045f, 0.055f, 0.06f), 0.28f);
            GameObject chassis = MakeBlock(name + " — chassis visual", Vector3.zero,
                new Vector3(1.86f, 0.58f, 3.72f), paint, false);
            chassis.transform.SetParent(car.transform, false);
            chassis.transform.localPosition = new Vector3(0f, 0.76f, 0f);
            GameObject cabin = MakeBlock(name + " — cabin visual", Vector3.zero,
                new Vector3(1.48f, 0.56f, 1.82f), trim, false);
            cabin.transform.SetParent(car.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 1.28f, -0.08f);

            // Small emissive lamps improve vehicle readability at dusk without adding real-time
            // Light components or extra physics colliders to every ambient car.
            Material headlamp = MakeEmissiveMaterial(name + " — headlamp lens", new Color(1f, 0.82f, 0.56f), 1.15f);
            Material tailLamp = MakeEmissiveMaterial(name + " — tail lamp lens", new Color(0.72f, 0.035f, 0.018f), 0.8f);
            const float frontZ = 1.86f;
            const float rearZ = -1.86f;
            GameObject frontLeft = MakeBlock(name + " — left headlamp", Vector3.zero,
                new Vector3(0.34f, 0.13f, 0.055f), headlamp, false);
            frontLeft.transform.SetParent(car.transform, false);
            frontLeft.transform.localPosition = new Vector3(-0.58f, 0.82f, frontZ);
            GameObject frontRight = MakeBlock(name + " — right headlamp", Vector3.zero,
                new Vector3(0.34f, 0.13f, 0.055f), headlamp, false);
            frontRight.transform.SetParent(car.transform, false);
            frontRight.transform.localPosition = new Vector3(0.58f, 0.82f, frontZ);
            GameObject rearLeft = MakeBlock(name + " — left tail lamp", Vector3.zero,
                new Vector3(0.30f, 0.14f, 0.055f), tailLamp, false);
            rearLeft.transform.SetParent(car.transform, false);
            rearLeft.transform.localPosition = new Vector3(-0.58f, 0.82f, rearZ);
            GameObject rearRight = MakeBlock(name + " — right tail lamp", Vector3.zero,
                new Vector3(0.30f, 0.14f, 0.055f), tailLamp, false);
            rearRight.transform.SetParent(car.transform, false);
            rearRight.transform.localPosition = new Vector3(0.58f, 0.82f, rearZ);

            Vector3[] wheelPositions =
            {
                new Vector3(-0.82f, 0.36f, 1.14f),
                new Vector3(0.82f, 0.36f, 1.14f),
                new Vector3(-0.82f, 0.36f, -1.14f),
                new Vector3(0.82f, 0.36f, -1.14f)
            };
            WheelCollider[] wheelColliders = new WheelCollider[4];
            Transform[] wheelVisuals = new Transform[4];
            for (int i = 0; i < wheelPositions.Length; i++)
            {
                GameObject wheelObject = new GameObject("Traffic WheelCollider " + i);
                wheelObject.transform.SetParent(car.transform, false);
                wheelObject.transform.localPosition = wheelPositions[i];
                WheelCollider wheel = wheelObject.AddComponent<WheelCollider>();
                wheel.radius = 0.34f;
                wheel.suspensionDistance = 0.18f;
                wheel.mass = 24f;
                wheel.forceAppPointDistance = 0.12f;
                wheelColliders[i] = wheel;

                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "Traffic tyre visual " + i;
                visual.transform.SetParent(car.transform, false);
                visual.transform.localPosition = wheelPositions[i];
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                visual.transform.localScale = new Vector3(0.23f, 0.34f, 0.34f);
                visual.GetComponent<Renderer>().sharedMaterial = trim;
                UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
                wheelVisuals[i] = visual.transform;
            }

            VehicleDriveController drive = car.AddComponent<VehicleDriveController>();
            SerializedObject driveSerialized = new SerializedObject(drive);
            SerializedProperty bindings = driveSerialized.FindProperty("wheels");
            bindings.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("collider").objectReferenceValue = wheelColliders[i];
                binding.FindPropertyRelative("visual").objectReferenceValue = wheelVisuals[i];
                binding.FindPropertyRelative("steer").boolValue = i < 2;
                binding.FindPropertyRelative("driven").boolValue = i >= 2;
                binding.FindPropertyRelative("visualRotationOffset").vector3Value = new Vector3(0f, 0f, 90f);
            }
            driveSerialized.FindProperty("motorTorque").floatValue = 1050f;
            driveSerialized.FindProperty("brakeTorque").floatValue = 2200f;
            driveSerialized.FindProperty("maxSteerAngle").floatValue = 28f;
            driveSerialized.FindProperty("handbrakeTorque").floatValue = 3200f;
            driveSerialized.FindProperty("downforce").floatValue = 18f;
            driveSerialized.ApplyModifiedPropertiesWithoutUndo();

            TrafficVehicleDriver traffic = car.AddComponent<TrafficVehicleDriver>();
            SerializedObject trafficSerialized = new SerializedObject(traffic);
            SerializedProperty route = trafficSerialized.FindProperty("route");
            route.arraySize = routePoints.Length;
            GameObject routeRoot = new GameObject(name + " — waypoint route");
            for (int i = 0; i < routePoints.Length; i++)
            {
                GameObject waypoint = new GameObject(name + " — route node " + (i + 1));
                waypoint.transform.SetParent(routeRoot.transform, false);
                waypoint.transform.position = routePoints[i];
                route.GetArrayElementAtIndex(i).objectReferenceValue = waypoint.transform;
            }
            trafficSerialized.FindProperty("cruisingSpeedKph").floatValue = 25f;
            trafficSerialized.FindProperty("waypointRadius").floatValue = 5f;
            trafficSerialized.FindProperty("obstacleCheckDistance").floatValue = 7f;
            trafficSerialized.FindProperty("obstacleRadius").floatValue = 0.55f;
            trafficSerialized.ApplyModifiedPropertiesWithoutUndo();
            traffic.ConfigureSignal(signal, stopLine, usesMainRoadSignal, pedestrianCrossing);
            traffic.ConfigureJunctionReservation(junctionReservation, stopLine);
        }

        private static TrafficSignalController BuildOldQuayJunction(Material road, Material paint,
            Material metal, Material accent)
        {
            GameObject junction = new GameObject("Old Quay — timed four-way traffic junction");
            TrafficSignalController controller = junction.AddComponent<TrafficSignalController>();

            // Two compact signal heads use enabled emissive lenses, avoiding extra real-time lights.
            Renderer mainRed = BuildTrafficSignalHead("Main road red", new Vector3(-7f, 3.1f, -30.8f),
                new Color(0.85f, 0.035f, 0.02f));
            Renderer mainGreen = BuildTrafficSignalHead("Main road green", new Vector3(-7f, 2.65f, -30.8f),
                new Color(0.08f, 0.8f, 0.16f));
            Renderer crossRed = BuildTrafficSignalHead("Cross street red", new Vector3(7f, 3.1f, -15.2f),
                new Color(0.85f, 0.035f, 0.02f));
            Renderer crossGreen = BuildTrafficSignalHead("Cross street green", new Vector3(7f, 2.65f, -15.2f),
                new Color(0.08f, 0.8f, 0.16f));

            MakeBlock("Northwest signal pole", new Vector3(-7f, 1.5f, -30.8f),
                new Vector3(0.18f, 3f, 0.18f), metal, false);
            MakeBlock("Southeast signal pole", new Vector3(7f, 1.5f, -15.2f),
                new Vector3(0.18f, 3f, 0.18f), metal, false);

            Renderer pedestrianRed = BuildTrafficSignalHead("Pedestrian dont-walk", new Vector3(-8.5f, 2.25f, -23f),
                new Color(0.85f, 0.035f, 0.02f));
            Renderer pedestrianGreen = BuildTrafficSignalHead("Pedestrian walk", new Vector3(-8.5f, 1.8f, -23f),
                new Color(0.08f, 0.8f, 0.16f));
            controller.Configure(mainRed, mainGreen, crossRed, crossGreen, 16f, 2f, 12f, 2f);
            controller.ConfigurePedestrianSignals(pedestrianRed, pedestrianGreen);
            return controller;
        }

        private static Renderer BuildTrafficSignalHead(string name, Vector3 position, Color glow)
        {
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lens.name = "Traffic signal lens — " + name;
            lens.transform.position = position;
            lens.transform.localScale = Vector3.one * 0.34f;
            Renderer renderer = lens.GetComponent<Renderer>();
            renderer.sharedMaterial = MakeEmissiveMaterial(name + " emissive", glow, 1.4f);
            UnityEngine.Object.DestroyImmediate(lens.GetComponent<Collider>());
            return renderer;
        }

        private static Transform MakeTrafficStopLine(string name, float x, float z)
        {
            GameObject stop = new GameObject(name);
            stop.transform.position = new Vector3(x, 0.1f, z);
            return stop.transform;
        }

        private static TrafficJunctionReservation BuildOldQuayJunctionReservation()
        {
            GameObject zone = new GameObject("Old Quay — single-car junction reservation zone");
            zone.transform.position = new Vector3(0f, 1.2f, -23f);
            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(18f, 3f, 18f);
            return zone.AddComponent<TrafficJunctionReservation>();
        }

        private static TrafficPedestrianCrossing BuildOldQuayPedestrianCrossing(TrafficSignalController signals)
        {
            GameObject crossing = new GameObject("Old Quay — marked pedestrian crossing occupancy");
            crossing.transform.position = new Vector3(0f, 1f, -23f);
            BoxCollider volume = crossing.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = new Vector3(11f, 2.5f, 3.2f);
            TrafficPedestrianCrossing occupancy = crossing.AddComponent<TrafficPedestrianCrossing>();

            Material marking = MakeMaterial("Crosswalk safety marking", new Color(0.86f, 0.84f, 0.73f));
            for (int stripe = 0; stripe < 7; stripe++)
            {
                MakeBlock("Crosswalk stripe " + (stripe + 1),
                    new Vector3(-4.5f + stripe * 1.5f, 0.12f, -23f),
                    new Vector3(0.72f, 0.035f, 2.4f), marking, false);
            }

            Transform[] route = new Transform[2];
            GameObject routeRoot = new GameObject("Crossing pedestrian — signal-gated route");
            for (int i = 0; i < route.Length; i++)
            {
                GameObject node = new GameObject("Crosswalk route node " + (i + 1));
                node.transform.SetParent(routeRoot.transform, false);
                node.transform.position = new Vector3(i == 0 ? -9f : 9f, 1f, -23f);
                route[i] = node.transform;
            }
            GameObject pedestrian = BuildPedestrianCharacter(
                "DOCK WORKER — signal-gated crosswalk pedestrian",
                route[0].position, new Color(0.58f, 0.39f, 0.24f));
            AmbientPedestrianWalker walker = pedestrian.AddComponent<AmbientPedestrianWalker>();
            walker.Configure(route, 2f, 18f);
            walker.ConfigureCrossingSignal(signals);
            return occupancy;
        }

        private static void BuildAmbientPedestrian(string name, float x, float[] routeZ,
            Color clothingColor, float walkingSpeed)
        {
            Transform[] route = new Transform[routeZ.Length];
            GameObject routeRoot = new GameObject(name + " — waypoint route");
            for (int i = 0; i < routeZ.Length; i++)
            {
                GameObject node = new GameObject("Walk node " + (i + 1));
                node.transform.SetParent(routeRoot.transform, false);
                node.transform.position = new Vector3(x, 1f, routeZ[i]);
                route[i] = node.transform;
            }

            GameObject pedestrian = BuildPedestrianCharacter(name, route[0].position, clothingColor);
            pedestrian.AddComponent<AmbientPedestrianWalker>()
                .Configure(route, walkingSpeed, 1.1f);
        }


        private static void BuildAmbientPedestrianRoute(string name, Vector3[] routePositions,
            Color clothingColor, float walkingSpeed, float startDelay)
        {
            if (routePositions == null || routePositions.Length < 2) return;
            Transform[] route = new Transform[routePositions.Length];
            GameObject routeRoot = new GameObject(name + " — waypoint route");
            for (int i = 0; i < routePositions.Length; i++)
            {
                GameObject node = new GameObject("Walk node " + (i + 1));
                node.transform.SetParent(routeRoot.transform, false);
                node.transform.position = routePositions[i];
                route[i] = node.transform;
            }

            GameObject pedestrian = BuildPedestrianCharacter(name, route[0].position, clothingColor);
            pedestrian.AddComponent<AmbientPedestrianWalker>()
                .Configure(route, walkingSpeed, 1.35f, startDelay);
        }


        private static GameObject BuildPedestrianCharacter(string name, Vector3 position, Color clothingColor)
        {
            // A single root collider preserves crossing occupancy; child visual parts have no colliders.
            GameObject root = new GameObject(name);
            root.transform.position = position;
            CapsuleCollider bodyCollider = root.AddComponent<CapsuleCollider>();
            bodyCollider.height = 1.85f;
            bodyCollider.radius = 0.31f;
            bodyCollider.center = new Vector3(0f, 0.02f, 0f);

            Material clothing = MakeMaterial(name + " — workwear", clothingColor);
            Material skin = MakeMaterial(name + " — skin", new Color(0.63f, 0.45f, 0.34f));
            Material trousers = MakeMaterial(name + " — trousers", new Color(0.13f, 0.16f, 0.18f));
            Material shoes = MakeMaterial(name + " — shoes", new Color(0.055f, 0.06f, 0.065f));

            CreatePedestrianPart(root.transform, name + " — torso", PrimitiveType.Capsule,
                new Vector3(0f, 0.12f, 0f), new Vector3(0.52f, 0.72f, 0.34f), clothing);
            CreatePedestrianPart(root.transform, name + " — head", PrimitiveType.Sphere,
                new Vector3(0f, 0.91f, 0f), new Vector3(0.36f, 0.39f, 0.34f), skin);

            Transform[] arms = new Transform[2];
            Transform[] legs = new Transform[2];
            for (int side = 0; side < 2; side++)
            {
                float sign = side == 0 ? -1f : 1f;
                GameObject arm = new GameObject(side == 0 ? "Left arm pivot" : "Right arm pivot");
                arm.transform.SetParent(root.transform, false);
                arm.transform.localPosition = new Vector3(sign * 0.34f, 0.39f, 0f);
                arms[side] = arm.transform;
                CreatePedestrianPart(arm.transform, name + " — sleeve", PrimitiveType.Capsule,
                    new Vector3(0f, -0.27f, 0f), new Vector3(0.17f, 0.34f, 0.17f), clothing);

                GameObject leg = new GameObject(side == 0 ? "Left leg pivot" : "Right leg pivot");
                leg.transform.SetParent(root.transform, false);
                leg.transform.localPosition = new Vector3(sign * 0.15f, -0.38f, 0f);
                legs[side] = leg.transform;
                CreatePedestrianPart(leg.transform, name + " — trouser leg", PrimitiveType.Capsule,
                    new Vector3(0f, -0.27f, 0f), new Vector3(0.19f, 0.36f, 0.20f), trousers);
                CreatePedestrianPart(leg.transform, name + " — shoe", PrimitiveType.Cube,
                    new Vector3(0f, -0.57f, 0.075f), new Vector3(0.22f, 0.11f, 0.36f), shoes);
            }

            root.AddComponent<AmbientPedestrianAnimation>().Configure(arms, legs);
            return root;
        }

        private static void CreatePedestrianPart(Transform parent, string name, PrimitiveType shape,
            Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(shape);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
        }

        private static VehicleDriveController BuildCustomerDeliveryVehicle(Transform cameraTransform,
            MissionDirector missionDirector, out VehicleSeatInteractable seat)
        {
            GameObject car = new GameObject("CUSTOMER DELIVERY CAR — Kade Motor Works");
            car.transform.position = new Vector3(0f, 0f, -13.5f);

            Rigidbody body = car.AddComponent<Rigidbody>();
            body.mass = 1280f;
            body.linearDamping = 0.12f;
            body.angularDamping = 1.35f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.maxAngularVelocity = 18f;

            Material bodyPaint = MakeMaterial("Customer car — deep harbour blue", new Color(0.10f, 0.22f, 0.29f), 0.48f);
            Material trim = MakeMaterial("Customer car — dark trim", new Color(0.055f, 0.07f, 0.075f), 0.62f);
            Material glass = MakeMaterial("Customer car — dark smoked glass", new Color(0.08f, 0.16f, 0.18f), 0.18f);
            Material lamp = MakeMaterial("Customer car — warm lamp", new Color(0.92f, 0.78f, 0.46f), 0.12f);

            GameObject chassis = MakeBlock("Car chassis and body", Vector3.zero,
                new Vector3(1.92f, 0.58f, 3.75f), bodyPaint);
            chassis.transform.SetParent(car.transform, false);
            chassis.transform.localPosition = new Vector3(0f, 0.78f, 0f);

            GameObject hood = MakeBlock("Front bonnet", Vector3.zero,
                new Vector3(1.84f, 0.26f, 1.02f), bodyPaint, false);
            hood.transform.SetParent(car.transform, false);
            hood.transform.localPosition = new Vector3(0f, 1.04f, 1.12f);

            GameObject rearDeck = MakeBlock("Rear utility deck", Vector3.zero,
                new Vector3(1.78f, 0.22f, 0.92f), trim, false);
            rearDeck.transform.SetParent(car.transform, false);
            rearDeck.transform.localPosition = new Vector3(0f, 1.02f, -1.18f);

            GameObject frontBumper = MakeBlock("Front bumper", Vector3.zero,
                new Vector3(1.98f, 0.18f, 0.18f), trim, false);
            frontBumper.transform.SetParent(car.transform, false);
            frontBumper.transform.localPosition = new Vector3(0f, 0.53f, 1.9f);

            GameObject rearBumper = MakeBlock("Rear bumper", Vector3.zero,
                new Vector3(1.98f, 0.18f, 0.18f), trim, false);
            rearBumper.transform.SetParent(car.transform, false);
            rearBumper.transform.localPosition = new Vector3(0f, 0.53f, -1.9f);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject frontLamp = MakeBlock("Headlamp", Vector3.zero,
                    new Vector3(0.34f, 0.17f, 0.08f), lamp, false);
                frontLamp.transform.SetParent(car.transform, false);
                frontLamp.transform.localPosition = new Vector3(side * 0.64f, 0.86f, 1.92f);

                GameObject sideTrim = MakeBlock("Door sill", Vector3.zero,
                    new Vector3(0.12f, 0.22f, 1.55f), trim, false);
                sideTrim.transform.SetParent(car.transform, false);
                sideTrim.transform.localPosition = new Vector3(side * 0.91f, 0.66f, -0.02f);
            }

            // Add a readable utility-car cab silhouette. The driver placeholder is hidden while seated.
            MakeBlock("Cab roof", new Vector3(0f, 1.73f, 0.12f),
                new Vector3(1.82f, 0.18f, 1.72f), bodyPaint, false);
            MakeBlock("Front windscreen", new Vector3(0f, 1.42f, 0.91f),
                new Vector3(1.55f, 0.52f, 0.07f), glass, false);
            MakeBlock("Rear cab window", new Vector3(0f, 1.40f, -0.72f),
                new Vector3(1.50f, 0.44f, 0.07f), glass, false);
            MakeBlock("Left side window", new Vector3(-0.92f, 1.42f, 0.10f),
                new Vector3(0.07f, 0.48f, 1.28f), glass, false);
            MakeBlock("Right side window", new Vector3(0.92f, 1.42f, 0.10f),
                new Vector3(0.07f, 0.48f, 1.28f), glass, false);
            for (int side = -1; side <= 1; side += 2)
            {
                MakeBlock("Cab A-pillar", new Vector3(side * 0.80f, 1.41f, 0.88f),
                    new Vector3(0.12f, 0.68f, 0.14f), bodyPaint, false);
                MakeBlock("Cab B-pillar", new Vector3(side * 0.80f, 1.41f, -0.57f),
                    new Vector3(0.12f, 0.68f, 0.14f), bodyPaint, false);
                MakeBlock("Side mirror", new Vector3(side * 1.02f, 1.35f, 0.73f),
                    new Vector3(0.16f, 0.12f, 0.22f), trim, false);
                MakeBlock("Front door lower panel", new Vector3(side * 0.94f, 0.91f, 0.08f),
                    new Vector3(0.08f, 0.30f, 1.48f), bodyPaint, false);
            }

            Transform centreOfMass = new GameObject("Centre of mass").transform;
            centreOfMass.SetParent(car.transform, false);
            centreOfMass.localPosition = new Vector3(0f, 0.32f, -0.12f);

            Vector3[] wheelPositions =
            {
                new Vector3(-0.84f, 0.40f, 1.18f),
                new Vector3(0.84f, 0.40f, 1.18f),
                new Vector3(-0.84f, 0.40f, -1.18f),
                new Vector3(0.84f, 0.40f, -1.18f)
            };

            WheelCollider[] wheelColliders = new WheelCollider[4];
            Transform[] wheelVisuals = new Transform[4];
            for (int i = 0; i < wheelPositions.Length; i++)
            {
                GameObject wheelColliderObject = new GameObject("WheelCollider " + i);
                wheelColliderObject.transform.SetParent(car.transform, false);
                wheelColliderObject.transform.localPosition = wheelPositions[i];
                WheelCollider wheelCollider = wheelColliderObject.AddComponent<WheelCollider>();
                wheelCollider.radius = 0.36f;
                wheelCollider.suspensionDistance = 0.20f;
                wheelCollider.mass = 28f;
                wheelCollider.forceAppPointDistance = 0.15f;
                wheelColliders[i] = wheelCollider;

                GameObject wheelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheelVisual.name = "Tyre visual " + i;
                wheelVisual.transform.SetParent(car.transform, false);
                wheelVisual.transform.localPosition = wheelPositions[i];
                wheelVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheelVisual.transform.localScale = new Vector3(0.25f, 0.36f, 0.36f);
                wheelVisual.GetComponent<Renderer>().sharedMaterial = trim;
                UnityEngine.Object.DestroyImmediate(wheelVisual.GetComponent<Collider>());
                wheelVisuals[i] = wheelVisual.transform;
            }

            VehicleDriveController driveController = car.AddComponent<VehicleDriveController>();
            SerializedObject driveSerialized = new SerializedObject(driveController);
            SerializedProperty wheelBindings = driveSerialized.FindProperty("wheels");
            wheelBindings.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                SerializedProperty binding = wheelBindings.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("collider").objectReferenceValue = wheelColliders[i];
                binding.FindPropertyRelative("visual").objectReferenceValue = wheelVisuals[i];
                binding.FindPropertyRelative("steer").boolValue = i < 2;
                binding.FindPropertyRelative("driven").boolValue = i >= 2;
                binding.FindPropertyRelative("visualRotationOffset").vector3Value = new Vector3(0f, 0f, 90f);
            }
            driveSerialized.FindProperty("motorTorque").floatValue = 1650f;
            driveSerialized.FindProperty("brakeTorque").floatValue = 2600f;
            driveSerialized.FindProperty("maxSteerAngle").floatValue = 31f;
            driveSerialized.FindProperty("handbrakeTorque").floatValue = 4200f;
            driveSerialized.FindProperty("downforce").floatValue = 24f;
            driveSerialized.FindProperty("centerOfMass").objectReferenceValue = centreOfMass;
            driveSerialized.ApplyModifiedPropertiesWithoutUndo();

            Transform driverSeat = new GameObject("Driver seat").transform;
            driverSeat.SetParent(car.transform, false);
            driverSeat.localPosition = new Vector3(-0.32f, 0.32f, 0.22f);

            Transform exitPoint = new GameObject("Driver exit point — face customer side").transform;
            exitPoint.SetParent(car.transform, false);
            exitPoint.localPosition = new Vector3(1.55f, 0.02f, 0.05f);
            exitPoint.localRotation = Quaternion.Euler(0f, 90f, 0f);

            seat = car.AddComponent<VehicleSeatInteractable>();
            SerializedObject seatSerialized = new SerializedObject(seat);
            seatSerialized.FindProperty("vehicle").objectReferenceValue = driveController;
            seatSerialized.FindProperty("driverSeat").objectReferenceValue = driverSeat;
            seatSerialized.FindProperty("exitPoint").objectReferenceValue = exitPoint;
            seatSerialized.FindProperty("cameraRig").objectReferenceValue =
                cameraTransform.GetComponent<ThirdPersonCameraRig>();
            seatSerialized.ApplyModifiedPropertiesWithoutUndo();
            seat.ConfigureMissionGate(missionDirector, "old_quay_first_steps", "enter_delivery_vehicle", "drive_customer_delivery_route", "park_in_delivery_bay");

            return driveController;
        }

        private static RouteRaceController BuildCustomerDeliveryRoute(VehicleDriveController deliveryVehicle)
        {
            GameObject routeObject = new GameObject("DELIVERY ROUTE — Old Quay to Saltline");
            RouteRaceController route = routeObject.AddComponent<RouteRaceController>();

            float[] checkpointPositions = { 0f, 18f, 36f };
            float[] checkpointLateralOffsets = { 0f, -3.2f, 3.2f };
            RouteRaceCheckpoint[] checkpoints = new RouteRaceCheckpoint[checkpointPositions.Length];
            Material gatePaint = MakeMaterial("Delivery route checkpoint marker", new Color(0.86f, 0.48f, 0.13f));
            Material routePaint = MakeMaterial("Delivery route lane guidance", new Color(0.84f, 0.75f, 0.52f));

            for (int i = 0; i < checkpointPositions.Length; i++)
            {
                float checkpointX = checkpointLateralOffsets[i];
                GameObject checkpointObject = new GameObject("Delivery checkpoint " + (i + 1));
                checkpointObject.transform.position = new Vector3(checkpointX, 1.2f, checkpointPositions[i]);
                BoxCollider trigger = checkpointObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(2.8f, 3f, 2.6f);

                RouteRaceCheckpoint checkpoint = checkpointObject.AddComponent<RouteRaceCheckpoint>();
                SerializedObject checkpointSerialized = new SerializedObject(checkpoint);
                checkpointSerialized.FindProperty("controller").objectReferenceValue = route;
                checkpointSerialized.FindProperty("checkpointIndex").intValue = i;
                checkpointSerialized.ApplyModifiedPropertiesWithoutUndo();
                checkpoints[i] = checkpoint;

                MakeBlock("Route gate post — left", new Vector3(checkpointX - 2.8f, 1.2f, checkpointPositions[i]),
                    new Vector3(0.32f, 2.4f, 0.32f), gatePaint, false);
                MakeBlock("Route gate post — right", new Vector3(checkpointX + 2.8f, 1.2f, checkpointPositions[i]),
                    new Vector3(0.32f, 2.4f, 0.32f), gatePaint, false);
                MakeBlock("Route gate crossbar", new Vector3(checkpointX, 2.48f, checkpointPositions[i]),
                    new Vector3(5.6f, 0.28f, 0.32f), gatePaint, false);

                if (i > 0)
                {
                    float midpointZ = (checkpointPositions[i - 1] + checkpointPositions[i]) * 0.5f;
                    float midpointX = (checkpointLateralOffsets[i - 1] + checkpointLateralOffsets[i]) * 0.5f;
                    GameObject laneGuide = MakeBlock("S-bend lane guide " + i,
                        new Vector3(midpointX, 0.105f, midpointZ),
                        new Vector3(0.16f, 0.025f, 3.4f), routePaint, false);
                    laneGuide.transform.rotation = Quaternion.Euler(0f,
                        checkpointLateralOffsets[i] > checkpointLateralOffsets[i - 1] ? -18f : 18f, 0f);
                }
            }

            SerializedObject routeSerialized = new SerializedObject(route);
            routeSerialized.FindProperty("signalPrefix").stringValue = "race.long_loop";
            routeSerialized.FindProperty("playerVehicle").objectReferenceValue = deliveryVehicle;
            SerializedProperty routeCheckpoints = routeSerialized.FindProperty("checkpoints");
            routeCheckpoints.arraySize = checkpoints.Length;
            for (int i = 0; i < checkpoints.Length; i++)
                routeCheckpoints.GetArrayElementAtIndex(i).objectReferenceValue = checkpoints[i];
            routeSerialized.ApplyModifiedPropertiesWithoutUndo();

            return route;
        }

        private static VehicleDeliveryCheckpoint BuildCustomerDeliveryBay(
            VehicleDriveController deliveryVehicle, VehicleSeatInteractable deliverySeat,
            RouteRaceController deliveryRoute)
        {
            GameObject bayObject = new GameObject("SALTLINE — marked customer delivery bay");
            bayObject.transform.position = new Vector3(0f, 1.25f, 48f);
            BoxCollider bayTrigger = bayObject.AddComponent<BoxCollider>();
            bayTrigger.isTrigger = true;
            bayTrigger.size = new Vector3(9f, 3f, 6f);

            Material bayPaint = MakeMaterial("Saltline delivery bay marking", new Color(0.88f, 0.57f, 0.21f));
            MakeBlock("Delivery bay left boundary", new Vector3(-2.7f, 0.095f, 48f),
                new Vector3(0.14f, 0.035f, 5.5f), bayPaint, false);
            MakeBlock("Delivery bay right boundary", new Vector3(2.7f, 0.095f, 48f),
                new Vector3(0.14f, 0.035f, 5.5f), bayPaint, false);
            MakeBlock("Delivery bay stop line", new Vector3(0f, 0.095f, 50.6f),
                new Vector3(5.4f, 0.035f, 0.16f), bayPaint, false);

            VehicleDeliveryCheckpoint bay = bayObject.AddComponent<VehicleDeliveryCheckpoint>();
            bay.Configure(deliveryVehicle, deliverySeat, deliveryRoute);
            return bay;
        }

        private static void BuildCustomerHandoffActor(RouteRaceController deliveryRoute,
            VehicleDeliveryCheckpoint deliveryBay, VehicleSeatInteractable deliverySeat,
            MissionDirector missionDirector, DialogueController dialogueController)
        {
            GameObject handoffObject = new GameObject("Customer vehicle handoff controller");
            MaraDeliveryHandoffController handoff = handoffObject.AddComponent<MaraDeliveryHandoffController>();
            handoff.Configure(deliveryRoute, missionDirector, deliveryBay, deliverySeat);

            GameObject customer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            customer.name = "CUSTOMER — Saltline delivery contact";
            customer.transform.position = new Vector3(3.6f, 1f, 49.5f);
            customer.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial("Customer placeholder clothing", new Color(0.42f, 0.34f, 0.27f));

            MaraCustomerHandoffInteractable interactable = customer.AddComponent<MaraCustomerHandoffInteractable>();
            SerializedObject customerSerialized = new SerializedObject(interactable);
            customerSerialized.FindProperty("handoffController").objectReferenceValue = handoff;
            customerSerialized.FindProperty("dialogueController").objectReferenceValue = dialogueController;
            customerSerialized.FindProperty("dialogueGraph").objectReferenceValue = EnsureCustomerHandoffDialogue();
            customerSerialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static void BuildRoadsideFacade(string label, Vector3 wallCentre,
            float wallLength, float wallHeight, bool facesPositiveX,
            Material trim, Material glass, Material signMaterial)
        {
            float outward = facesPositiveX ? 1f : -1f;
            float faceX = wallCentre.x + outward * 0.16f;

            // The sign is placed on the road-facing side, not the warehouse end wall.
            MakeBlock(label + " — painted fascia",
                new Vector3(faceX, wallHeight * 0.75f, wallCentre.z),
                new Vector3(0.16f, 1.08f, Mathf.Min(8.4f, wallLength * 0.56f)),
                signMaterial, false);

            GameObject lettering = new GameObject(label + " — facade lettering");
            lettering.transform.position = new Vector3(faceX + outward * 0.10f,
                wallHeight * 0.75f, wallCentre.z);
            lettering.transform.rotation = Quaternion.Euler(0f, facesPositiveX ? 90f : 270f, 0f);
            TextMesh text = lettering.AddComponent<TextMesh>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 54;
            text.characterSize = 0.105f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.96f, 0.88f, 0.70f);
            text.richText = false;

            // Three broad industrial windows, each with a simple painted steel frame.
            float windowHeight = Mathf.Clamp(wallHeight * 0.17f, 1.35f, 2.0f);
            float windowY = wallHeight * 0.48f;
            float windowLength = Mathf.Min(3.1f, wallLength * 0.19f);
            for (int i = -1; i <= 1; i++)
            {
                float z = wallCentre.z + i * wallLength * 0.27f;
                MakeBlock(label + " — industrial glass",
                    new Vector3(faceX, windowY, z),
                    new Vector3(0.075f, windowHeight, windowLength), glass, false);
                for (int side = -1; side <= 1; side += 2)
                    MakeBlock(label + " — window steel jamb",
                        new Vector3(faceX + outward * 0.055f, windowY, z + side * (windowLength * 0.5f + 0.055f)),
                        new Vector3(0.11f, windowHeight + 0.18f, 0.11f), trim, false);
                MakeBlock(label + " — window sill",
                    new Vector3(faceX + outward * 0.055f, windowY - windowHeight * 0.5f, z),
                    new Vector3(0.14f, 0.12f, windowLength + 0.2f), trim, false);
                MakeBlock(label + " — window lintel",
                    new Vector3(faceX + outward * 0.055f, windowY + windowHeight * 0.5f, z),
                    new Vector3(0.14f, 0.12f, windowLength + 0.2f), trim, false);
            }

            // A single service shutter and raised curb make the entrance legible from the street.
            float shutterLength = Mathf.Min(4.4f, wallLength * 0.28f);
            float shutterZ = wallCentre.z;
            MakeBlock(label + " — roller shutter",
                new Vector3(faceX + outward * 0.045f, 2.05f, shutterZ),
                new Vector3(0.11f, 3.05f, shutterLength), trim, false);
            for (int stripe = 0; stripe < 5; stripe++)
                MakeBlock(label + " — shutter slat",
                    new Vector3(faceX + outward * 0.11f, 0.95f + stripe * 0.45f, shutterZ),
                    new Vector3(0.035f, 0.045f, shutterLength - 0.18f), signMaterial, false);
        }

        private static void BuildQuaySurfaceDetails(Material wetRoad, Material metal, Material safety)
        {
            // Scattered rain-dark repairs break up the road's single flat material without decals.
            Vector3[] patchPositions =
            {
                new Vector3(-3.5f, 0.086f, -18f),
                new Vector3(2.7f, 0.086f, -2f),
                new Vector3(-1.8f, 0.086f, 22f),
                new Vector3(3.8f, 0.086f, 39f)
            };
            Vector3[] patchScales =
            {
                new Vector3(2.5f, 0.014f, 1.2f),
                new Vector3(1.7f, 0.014f, 2.0f),
                new Vector3(2.2f, 0.014f, 1.0f),
                new Vector3(1.5f, 0.014f, 2.2f)
            };
            for (int i = 0; i < patchPositions.Length; i++)
            {
                GameObject patch = MakeBlock("Rain-dark road repair", patchPositions[i],
                    patchScales[i], wetRoad, false);
                patch.transform.rotation = Quaternion.Euler(0f, (i % 2 == 0 ? -13f : 19f), 0f);
            }

            // Drain covers and ochre quay bollards add scale cues beside the road.
            for (int z = -24; z <= 48; z += 18)
            {
                MakeBlock("Pavement storm drain", new Vector3(-6.15f, 0.218f, z),
                    new Vector3(0.72f, 0.022f, 0.42f), metal, false);
                MakeBlock("Pavement storm drain", new Vector3(6.15f, 0.218f, z + 5f),
                    new Vector3(0.72f, 0.022f, 0.42f), metal, false);
            }

            for (int x = -40; x <= 40; x += 20)
            {
                GameObject bollard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bollard.name = "Quayside mooring bollard";
                bollard.transform.position = new Vector3(x, 0.48f, 54.2f);
                bollard.transform.localScale = new Vector3(0.36f, 0.48f, 0.36f);
                bollard.GetComponent<Renderer>().sharedMaterial = safety;
                UnityEngine.Object.DestroyImmediate(bollard.GetComponent<Collider>());
                MakeBlock("Bollard dark cap", new Vector3(x, 0.94f, 54.2f),
                    new Vector3(0.4f, 0.12f, 0.4f), metal, false);
            }
        }


        private static void BuildOldQuayMarketAndSouthLane(Material ground, Material road, Material paint,
            Material warehouse, Material brick, Material glass, Material metal, Material accent, Material lampGlow)
        {
            // A short southern market lane extends the street grid beyond the original central junction.
            MakeBlock("Southline market lane asphalt", new Vector3(0f, 0.018f, -35f),
                new Vector3(49f, 0.12f, 7.2f), road);
            MakeBlock("Market lane west pavement", new Vector3(0f, 0.12f, -40.15f),
                new Vector3(49f, 0.22f, 2.8f), ground);
            MakeBlock("Market lane east pavement", new Vector3(0f, 0.12f, -29.85f),
                new Vector3(49f, 0.22f, 2.8f), ground);
            for (int x = -20; x <= 20; x += 10)
            {
                MakeBlock("Market lane centre dash", new Vector3(x, 0.092f, -35f),
                    new Vector3(4.2f, 0.022f, 0.14f), paint, false);
                MakeBlock("Market lane curb drain", new Vector3(x + 4f, 0.245f, -30.95f),
                    new Vector3(0.65f, 0.025f, 0.32f), metal, false);
            }

            BuildWarehouse("Breakwater Cafe", new Vector3(-34f, 0f, -35f),
                new Vector3(15f, 7.5f, 12f), brick, metal, glass, accent);
            BuildRoadsideFacade("BREAKWATER CAFE", new Vector3(-34f, 0f, -35f),
                12f, 7.5f, true, metal, glass, accent);
            BuildWarehouse("Quay Tool and Tackle", new Vector3(34f, 0f, -35f),
                new Vector3(15f, 8f, 12f), warehouse, metal, glass, accent);
            BuildRoadsideFacade("QUAY TOOL & TACKLE", new Vector3(34f, 0f, -35f),
                12f, 8f, false, metal, glass, accent);

            Material canopy = MakeMaterial("Breakwater market faded teal canvas", new Color(0.12f, 0.31f, 0.32f));
            Material timber = MakeMaterial("Breakwater market timber", new Color(0.34f, 0.24f, 0.16f));
            Material crate = MakeMaterial("Harbour market crate wood", new Color(0.43f, 0.30f, 0.19f));
            Material produce = MakeMaterial("Harbour market produce", new Color(0.56f, 0.34f, 0.18f));
            // Open-sided fish and salvage stalls sit on the east pier, leaving a clear route around the quay.
            MakeBlock("Breakwater market raised timber deck", new Vector3(28f, 0.43f, 66.2f),
                new Vector3(13.5f, 0.42f, 8.5f), timber);
            MakeBlock("Breakwater market canopy", new Vector3(28f, 4.35f, 66.2f),
                new Vector3(14.2f, 0.28f, 9.2f), canopy);
            for (int x = 0; x < 4; x++)
            {
                float px = 22.1f + x * 3.95f;
                MakeBlock("Market canopy support post", new Vector3(px, 2.35f, 62.25f),
                    new Vector3(0.18f, 4.0f, 0.18f), timber);
                MakeBlock("Market canopy support post", new Vector3(px, 2.35f, 70.15f),
                    new Vector3(0.18f, 4.0f, 0.18f), timber);
            }
            MakeBlock("Fish stall counter", new Vector3(24.2f, 1.05f, 65.1f),
                new Vector3(4.6f, 1.05f, 1.1f), timber);
            MakeBlock("Salvage stall counter", new Vector3(31.7f, 1.05f, 65.1f),
                new Vector3(4.6f, 1.05f, 1.1f), timber);
            MakeBlock("Fish stall sign", new Vector3(24.2f, 3.1f, 62.55f),
                new Vector3(4.5f, 0.82f, 0.12f), accent, false);
            MakeBlock("Salvage stall sign", new Vector3(31.7f, 3.1f, 62.55f),
                new Vector3(4.5f, 0.82f, 0.12f), metal, false);
            for (int i = 0; i < 6; i++)
            {
                float x = 21.7f + (i % 3) * 4.4f;
                float z = 68.1f + (i / 3) * 1.25f;
                MakeBlock("Market stacked cargo crate", new Vector3(x, 1.02f, z),
                    new Vector3(1.15f, 1.05f, 1.05f), crate);
                MakeBlock("Market crate lid", new Vector3(x, 1.57f, z),
                    new Vector3(1.2f, 0.08f, 1.1f), timber, false);
            }
            for (int i = 0; i < 4; i++)
            {
                GameObject goods = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                goods.name = "Market produce bundle";
                goods.transform.position = new Vector3(30.3f + (i % 2) * 1.1f, 1.85f, 65.1f + (i / 2) * 0.38f);
                goods.transform.localScale = new Vector3(0.58f, 0.38f, 0.48f);
                goods.GetComponent<Renderer>().sharedMaterial = produce;
                UnityEngine.Object.DestroyImmediate(goods.GetComponent<Collider>());
            }
            MakeBlock("Pier market wayfinding sign", new Vector3(18.4f, 2.1f, 62.2f),
                new Vector3(0.16f, 3.8f, 1.2f), metal);
            MakeBlock("Pier market sign face", new Vector3(18.29f, 3.1f, 62.2f),
                new Vector3(0.08f, 1.1f, 1.0f), accent, false);
            BuildAccentLight("Breakwater market warm lantern", new Vector3(28f, 5.0f, 66f),
                new Color(1f, 0.67f, 0.36f), 1.3f, 11f);

            // Street furniture is placed against the outer pavement edge, not in the driving lanes.
            for (int i = 0; i < 3; i++)
            {
                float z = -7f + i * 22f;
                BuildQuayBench(new Vector3(-10.45f, 0f, z), timber, metal);
                BuildQuayBench(new Vector3(10.45f, 0f, z + 6f), timber, metal);
                BuildStreetBin(new Vector3(-8.0f, 0f, z + 8f), metal, accent);
                BuildStreetBin(new Vector3(8.0f, 0f, z - 4f), metal, accent);
            }
            BuildStreetLamp(new Vector3(-11.1f, 0f, -36f), metal, lampGlow);
            BuildStreetLamp(new Vector3(11.1f, 0f, -36f), metal, lampGlow);
            // Marked curbside loading bays make the market lane read as a working service street.
            Material bayPaint = MakeMaterial("Market lane loading bay paint", new Color(0.88f, 0.67f, 0.31f));
            for (int i = 0; i < 3; i++)
            {
                float x = -14f + i * 14f;
                MakeBlock("Market lane loading bay outline", new Vector3(x, 0.095f, -39.0f),
                    new Vector3(7.6f, 0.025f, 0.12f), bayPaint, false);
                MakeBlock("Market lane loading bay side mark", new Vector3(x - 3.75f, 0.095f, -36.7f),
                    new Vector3(0.12f, 0.025f, 4.6f), bayPaint, false);
                MakeBlock("Market lane loading bay side mark", new Vector3(x + 3.75f, 0.095f, -36.7f),
                    new Vector3(0.12f, 0.025f, 4.6f), bayPaint, false);
            }
        }

        private static void BuildQuayBench(Vector3 position, Material timber, Material metal)
        {
            MakeBlock("Old Quay public bench seat", position + new Vector3(0f, 0.52f, 0f),
                new Vector3(1.9f, 0.14f, 0.62f), timber, false);
            MakeBlock("Old Quay public bench back", position + new Vector3(0f, 0.95f, -0.25f),
                new Vector3(1.9f, 0.72f, 0.12f), timber, false);
            for (int side = -1; side <= 1; side += 2)
                MakeBlock("Old Quay bench leg", position + new Vector3(side * 0.72f, 0.25f, 0f),
                    new Vector3(0.12f, 0.5f, 0.48f), metal, false);
        }

        private static void BuildStreetBin(Vector3 position, Material metal, Material accent)
        {
            GameObject bin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bin.name = "Old Quay public litter bin";
            bin.transform.position = position + Vector3.up * 0.58f;
            bin.transform.localScale = new Vector3(0.52f, 0.58f, 0.52f);
            bin.GetComponent<Renderer>().sharedMaterial = metal;
            UnityEngine.Object.DestroyImmediate(bin.GetComponent<Collider>());
            MakeBlock("Old Quay litter bin rim", position + Vector3.up * 1.12f,
                new Vector3(0.62f, 0.1f, 0.62f), accent, false);
        }

        private static void BuildAccentLight(string name, Vector3 position, Color color,
            float intensity, float range)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.position = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.Auto;
        }

        private static Material MakeEmissiveMaterial(string name, Color color, float intensity)
        {
            Material material = MakeMaterial(name, color, 0f, 0.82f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0f, intensity));
                EditorUtility.SetDirty(material);
            }
            return material;
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

        private static Material MakeMaterial(string name, Color color, float metallic = 0f,
            float smoothness = 0.32f)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Generated material requires a stable name.", nameof(name));

            string fileName = name;
            foreach (char invalid in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalid, '_');
            fileName = fileName.Replace('/', '_').Replace('\\', '_');
            string materialPath = MaterialFolder + "/" + fileName + ".mat";

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("Neither URP/Lit nor Standard shader is available.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
            EditorUtility.SetDirty(material);
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
