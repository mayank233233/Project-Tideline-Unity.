#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTideline.Editor
{
    /// <summary>
    /// Generates a self-contained, keyboard-playable Old Quay blockout scene.
    /// It deliberately uses Unity primitives so the scene has no external art dependencies.
    /// This is a greybox gameplay foundation, not final PORT SABLE art or mission content.
    /// </summary>
    public static class PortSableBlockoutSceneBuilder
    {
        private const string SceneFolder = "Assets/ProjectTideline/Scenes";
        private const string ScenePath = SceneFolder + "/PortSable_OldQuay_Blockout.unity";

        [MenuItem("Project Tideline/14 — Build Playable Old Quay Blockout", priority = 14)]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder("Assets/ProjectTideline");
            EnsureFolder(SceneFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "PORT SABLE — OLD QUAY BLOCKOUT";

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

            // Scene metadata helps designers understand what is functional versus placeholder.
            GameObject marker = new GameObject("BLOCKOUT NOTES — read before production");
            marker.transform.position = new Vector3(0f, 0f, -42f);
            marker.AddComponent<PortSableBlockoutReadmeMarker>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Selection.activeGameObject = player;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EditorUtility.DisplayDialog("PORT SABLE blockout created",
                "Saved and added to Build Settings:\n" + ScenePath +
                "\n\nPlayable now: WASD / arrows to move, Shift to sprint, Space to jump, mouse to orbit camera.\n" +
                "This is a primitive greybox. It has no mission logic, dialogue UI, traffic, final character art or mobile touch controls yet.",
                "OK");
            Debug.Log("PORT SABLE Old Quay blockout scene generated: " + ScenePath);
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

    /// <summary>Visible in the hierarchy as a reminder that this scene is a greybox.</summary>
    public sealed class PortSableBlockoutReadmeMarker : MonoBehaviour
    {
        [TextArea(4, 10)]
        public string notes =
            "GREYBOX ONLY. Player movement and camera are wired. Replace primitive geometry and the player capsule with approved PORT SABLE art. Add the Mara prologue MissionDefinition, dialogue graphs, UI, interactables, route, vehicle and save coordinator before calling this a campaign slice.";
    }
}
#endif
