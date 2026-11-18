// Builds every level scene from one layout per level; the single source for the scenes.
// unity command eval 'return BuildLevels.Materials();' (once), then BuildLevels.Level01() to BuildLevels.Level15(), BuildLevels.BuildList()
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class BuildLevels
{
    const string FontAssetPath = "Assets/Fonts/Exo2-BlackItalic SDF.asset";
    static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Level01.unity", "Assets/Scenes/Level02.unity", "Assets/Scenes/Level03.unity", "Assets/Scenes/Level04.unity",
        "Assets/Scenes/Level05.unity", "Assets/Scenes/Level06.unity", "Assets/Scenes/Level07.unity", "Assets/Scenes/Level08.unity",
        "Assets/Scenes/Level09.unity", "Assets/Scenes/Level10.unity", "Assets/Scenes/Level11.unity", "Assets/Scenes/Level12.unity",
        "Assets/Scenes/Level13.unity", "Assets/Scenes/Level14.unity", "Assets/Scenes/Level15.unity"
    };

    struct GateSpec
    {
        public float x;
        public float z;
        public int value; // xN when positive, -N when negative
    }

    struct RingSpec
    {
        public float x;
        public float z;
        public UnitColour colour;
    }

    struct Layout
    {
        public int number;
        public int hive;
        public float firstWave;
        public float waveInterval;
        public int mitesPerWave;
        public float miteSpeed;
        public GateSpec[] gates;
        public RingSpec[] rings;
    }

    // Tutorial: gates, then rings.
    public static string Level01()
    {
        return Build(new Layout
        {
            number = 1,
            hive = 120,
            firstWave = 2f,
            waveInterval = 2f,
            mitesPerWave = 5,
            miteSpeed = 2.5f,
            gates = new[] { Gate(-2.5f, 10f, 2), Gate(2.5f, 10f, -5) },
            rings = Rings(19f)
        });
    }

    // Hive level with the ring-then-x3 stretch.
    public static string Level02()
    {
        return Build(new Layout
        {
            number = 2,
            hive = 600,
            firstWave = 2f,
            waveInterval = 1.6f,
            mitesPerWave = 6,
            miteSpeed = 2.8f,
            gates = new[] { Gate(-2.5f, 12f, 3), Gate(2.5f, 12f, -10) },
            rings = Rings(6.5f)
        });
    }

    // Trap: the -N gate sits on the left, where the first two levels put the good gate.
    public static string Level03()
    {
        return Build(new Layout
        {
            number = 3,
            hive = 300,
            firstWave = 2f,
            waveInterval = 1.8f,
            mitesPerWave = 5,
            miteSpeed = 2.6f,
            gates = new[] { Gate(-2.5f, 10f, -5), Gate(2.5f, 10f, 2) },
            rings = Rings(19f)
        });
    }

    // Gates only: no rings, so every drone stays white and trades one for one.
    public static string Level04()
    {
        return Build(new Layout
        {
            number = 4,
            hive = 500,
            firstWave = 2f,
            waveInterval = 1.7f,
            mitesPerWave = 6,
            miteSpeed = 2.7f,
            gates = new[] { Gate(-2.5f, 12f, 3), Gate(2.5f, 12f, -8) },
            rings = new RingSpec[0]
        });
    }

    // Two gate rows: the good side swaps between the rows.
    public static string Level05()
    {
        return Build(new Layout
        {
            number = 5,
            hive = 700,
            firstWave = 1.9f,
            waveInterval = 1.6f,
            mitesPerWave = 6,
            miteSpeed = 2.8f,
            gates = new[] { Gate(-2.5f, 9f, 2), Gate(2.5f, 9f, -6), Gate(-2.5f, 17f, -8), Gate(2.5f, 17f, 2) },
            rings = Rings(24f)
        });
    }

    // A ring row before the gates and another after them.
    public static string Level06()
    {
        return Build(new Layout
        {
            number = 6,
            hive = 1000,
            firstWave = 1.8f,
            waveInterval = 1.6f,
            mitesPerWave = 6,
            miteSpeed = 2.9f,
            gates = new[] { Gate(-2.5f, 12f, 3), Gate(2.5f, 12f, -10) },
            rings = Rings(6f).Concat(Rings(19f)).ToArray()
        });
    }

    // Gates only: x5 beside -20, the trap on the left.
    public static string Level07()
    {
        return Build(new Layout
        {
            number = 7,
            hive = 1100,
            firstWave = 1.7f,
            waveInterval = 1.5f,
            mitesPerWave = 7,
            miteSpeed = 2.9f,
            gates = new[] { Gate(-2.5f, 14f, -20), Gate(2.5f, 14f, 5) },
            rings = new RingSpec[0]
        });
    }

    // Breather after level 7: one x3 in the middle, no cut, slow waves.
    public static string Level08()
    {
        return Build(new Layout
        {
            number = 8,
            hive = 600,
            firstWave = 2.2f,
            waveInterval = 1.9f,
            mitesPerWave = 5,
            miteSpeed = 2.7f,
            gates = new[] { Gate(0f, 11f, 3) },
            rings = Rings(19f)
        });
    }

    // Two gate rows, the first with the trap on the left.
    public static string Level09()
    {
        return Build(new Layout
        {
            number = 9,
            hive = 1250,
            firstWave = 1.7f,
            waveInterval = 1.5f,
            mitesPerWave = 7,
            miteSpeed = 3f,
            gates = new[] { Gate(-2.5f, 8f, -8), Gate(2.5f, 8f, 3), Gate(-2.5f, 16f, 2), Gate(2.5f, 16f, -12) },
            rings = Rings(23f)
        });
    }

    // Rings before a big x4 beside a heavy -15.
    public static string Level10()
    {
        return Build(new Layout
        {
            number = 10,
            hive = 1350,
            firstWave = 1.6f,
            waveInterval = 1.4f,
            mitesPerWave = 7,
            miteSpeed = 3f,
            gates = new[] { Gate(-2.5f, 12f, 4), Gate(2.5f, 12f, -15) },
            rings = Rings(6.5f)
        });
    }

    // Rings before an x4 on the right, -12 on the left.
    public static string Level11()
    {
        return Build(new Layout
        {
            number = 11,
            hive = 1700,
            firstWave = 1.6f,
            waveInterval = 1.4f,
            mitesPerWave = 8,
            miteSpeed = 3.1f,
            gates = new[] { Gate(-2.5f, 13f, -12), Gate(2.5f, 13f, 4) },
            rings = Rings(7f)
        });
    }

    // Finale: rings, x4 beside -15, then a row of two cuts.
    public static string Level12()
    {
        return Build(new Layout
        {
            number = 12,
            hive = 1900,
            firstWave = 1.5f,
            waveInterval = 1.3f,
            mitesPerWave = 8,
            miteSpeed = 3.2f,
            gates = new[] { Gate(-2.5f, 12f, 4), Gate(2.5f, 12f, -15), Gate(-2.5f, 20f, -10), Gate(2.5f, 20f, -4) },
            rings = Rings(6.5f)
        });
    }

    // Hard, one route: sweep the right half through the x3; the -20 trap is on the left.
    public static string Level13()
    {
        return Build(new Layout
        {
            number = 13,
            hive = 2000,
            firstWave = 1.5f,
            waveInterval = 0.7f,
            mitesPerWave = 11,
            miteSpeed = 3.8f,
            gates = new[] { Gate(-2.5f, 13f, -20), Gate(2.5f, 13f, 3) },
            rings = Rings(6f)
        });
    }

    // Hard, one route: the x2 on the right, then right of the -40.
    public static string Level14()
    {
        return Build(new Layout
        {
            number = 14,
            hive = 2500,
            firstWave = 1.5f,
            waveInterval = 0.65f,
            mitesPerWave = 8,
            miteSpeed = 4f,
            gates = new[] { Gate(-2.5f, 7f, -20), Gate(2.5f, 7f, 2), Gate(-1f, 13f, -40) },
            rings = Rings(22f)
        });
    }

    // Hard, one route: the x2, right of the -40, then the second x2, out through the magenta ring.
    public static string Level15()
    {
        return Build(new Layout
        {
            number = 15,
            hive = 5000,
            firstWave = 3f,
            waveInterval = 0.55f,
            mitesPerWave = 7,
            miteSpeed = 4.7f,
            gates = new[] { Gate(-2.5f, 5f, -20), Gate(2.5f, 5f, 2), Gate(-0.5f, 9f, -40), Gate(-2.5f, 16f, -30), Gate(2.5f, 16f, 2) },
            rings = Rings(25f)
        });
    }

    // Lane 4: the mite rim (inverted hull), the halo sprite and the field backdrop
    public static string Materials()
    {
        var flat = Shader.Find("SWARMSHIFT/FlatUnlit");
        var log = new StringBuilder("materials:");

        var rim = MakeMaterial("Assets/Materials/UnitsRim.mat", flat, log);
        rim.enableInstancing = true;
        rim.SetFloat("_Cull", (float)CullMode.Front);

        var halo = MakeMaterial("Assets/Materials/Halo.mat", Shader.Find("SWARMSHIFT/Halo"), log);
        halo.enableInstancing = true;

        var field = MakeMaterial("Assets/Materials/Field.mat", flat, log);
        field.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        field.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        field.SetFloat("_ZWrite", 0f);
        field.renderQueue = (int)RenderQueue.Background; // drawn first, no depth write

        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    static Material MakeMaterial(string path, Shader shader, StringBuilder log)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            log.Append($" created {path} ({shader.name});");
        }
        else
        {
            material.shader = shader;
            log.Append($" updated {path} ({shader.name});");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    public static string BuildList()
    {
        EditorBuildSettings.scenes = ScenePaths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        var log = new StringBuilder("build scenes:");
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            log.Append($" {scene.path} ({scene.enabled})");
        }
        log.Append($"; open scene {EditorSceneManager.GetActiveScene().path}");
        return log.ToString();
    }

    static GateSpec Gate(float x, float z, int value)
    {
        return new GateSpec { x = x, z = z, value = value };
    }

    // Three abreast across the lane, one in the middle of each third (DESIGN 10).
    static RingSpec[] Rings(float z)
    {
        float third = Lane.HalfWidth * 2f / 3f;
        return new[]
        {
            new RingSpec { x = -third, z = z, colour = UnitColour.Cyan },
            new RingSpec { x = 0f, z = z, colour = UnitColour.Yellow },
            new RingSpec { x = third, z = z, colour = UnitColour.Magenta }
        };
    }

    static string Build(Layout layout)
    {
        string scenePath = ScenePaths[layout.number - 1];
        var log = new StringBuilder();
        log.AppendLine($"replacing open scene {EditorSceneManager.GetActiveScene().path} (dirty {EditorSceneManager.GetActiveScene().isDirty})");

        var world = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/World.mat");
        var fade = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WorldFade.mat");
        var units = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Units.mat");
        var unitsRim = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/UnitsRim.mat");
        var halo = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Halo.mat");
        var fieldMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Field.mat");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 15.5f, -11f), Quaternion.Euler(27f, 0f, 0f));
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Palette.FieldLow;
        camera.fieldOfView = 70f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 200f;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        cameraObject.AddComponent<AudioListener>();
        var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = false;
        cameraData.antialiasing = AntialiasingMode.None;
        cameraData.renderShadows = false;
        var field = Renderable("Field", cameraObject.transform, Vector3.zero, fieldMaterial).AddComponent<Field>();

        var platform = new GameObject("YTGameWrapper");
        platform.AddComponent<YTGameSDK.YTGameWrapper>();
        platform.AddComponent<PlatformServices>();

        var level = new GameObject("Level");
        var swarm = level.AddComponent<Swarm>();
        Set(swarm, "material", units);
        var mites = level.AddComponent<MiteWaves>();
        Set(mites, "material", units);
        Set(mites, "rimMaterial", unitsRim);
        var miteSettings = new SerializedObject(mites);
        miteSettings.FindProperty("firstWaveDelay").floatValue = layout.firstWave;
        miteSettings.FindProperty("waveInterval").floatValue = layout.waveInterval;
        miteSettings.FindProperty("mitesPerWave").intValue = layout.mitesPerWave;
        miteSettings.FindProperty("speed").floatValue = layout.miteSpeed;
        miteSettings.ApplyModifiedPropertiesWithoutUndo();
        var autopilot = level.AddComponent<Autopilot>();
        autopilot.enabled = false;
        var director = level.AddComponent<LevelDirector>();

        Renderable("Lane", null, Vector3.zero, world).AddComponent<Lane>();

        GameObject shipObject = Renderable("Mothership", null, Vector3.zero, world);
        GameObject meterObject = Renderable("Charge Meter", shipObject.transform, new Vector3(0f, 1.15f, -1.3f), world);
        meterObject.transform.localRotation = Quaternion.Euler(27f, 0f, 0f);
        var meter = meterObject.AddComponent<ChargeMeter>();
        GameObject spark = Renderable("Spark", null, Vector3.zero, world);
        var mothership = shipObject.AddComponent<Mothership>();
        Set(mothership, "autopilot", autopilot);
        Set(mothership, "meter", meter);
        Set(mothership, "spark", spark.transform);

        var champion = Renderable("Champion", null, Vector3.zero, world).AddComponent<Champion>();

        var gates = new Gate[layout.gates.Length];
        for (int i = 0; i < gates.Length; i++)
        {
            GateSpec spec = layout.gates[i];
            string sign = spec.value > 0 ? "×" + spec.value : "-" + -spec.value;
            gates[i] = MakeGate("Gate " + sign, spec, sign, world, fade, font);
        }

        var rings = new WarpRing[layout.rings.Length];
        for (int i = 0; i < rings.Length; i++)
        {
            rings[i] = MakeRing("Ring " + layout.rings[i].colour, layout.rings[i], world);
        }

        GameObject hiveObject = Renderable("Hive", null, new Vector3(0f, 0f, 37.5f), world);
        TextMeshPro hiveLabel = WorldLabel(hiveObject.transform, layout.hive.ToString(), 30f, new Vector2(4.6f, 2f), font);
        var hive = hiveObject.AddComponent<Hive>();
        Set(hive, "label", hiveLabel);
        var hiveSettings = new SerializedObject(hive);
        hiveSettings.FindProperty("startHitPoints").intValue = layout.hive;
        hiveSettings.ApplyModifiedPropertiesWithoutUndo();

        var hud = new GameObject("HUD");
        var canvas = hud.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        var scaler = hud.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390f, 844f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        hud.AddComponent<GraphicRaycaster>();
        HudText(hud.transform, "Level Label", "LEVEL " + layout.number, 24f, new Vector2(0.5f, 1f), new Vector2(0f, -76f), font);
        TextMeshProUGUI result = HudText(hud.transform, "Result", "HIVE CRACKED", 40f, new Vector2(0.5f, 0.5f), Vector2.zero, font);
        result.gameObject.SetActive(false);

        var failed = new GameObject("Failed Buttons", typeof(RectTransform));
        failed.transform.SetParent(hud.transform, false);
        Button revive = HudButton(failed.transform, "Revive", "REVIVE", new Vector2(0f, -76f), new Vector2(240f, 48f), 20f, Palette.Credit, font);
        TextMeshProUGUI reviveLabel = revive.GetComponentInChildren<TextMeshProUGUI>();
        reviveLabel.rectTransform.offsetMin = new Vector2(28f, 0f);
        Image chip = Panel(revive.transform, "Ad Chip", new Vector2(-98f, 0f), new Vector2(30f, 30f), Palette.Ink);
        TextMeshProUGUI chipLabel = HudText(chip.transform, "Label", "AD", 13f, new Vector2(0.5f, 0.5f), Vector2.zero, font);
        chipLabel.rectTransform.sizeDelta = new Vector2(30f, 30f);
        Button retry = HudButton(failed.transform, "Retry", "RETRY", new Vector2(0f, -144f), new Vector2(280f, 56f), 24f, Palette.Plus, font);
        failed.SetActive(false);

        var events = new GameObject("EventSystem");
        events.AddComponent<EventSystem>();
        events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

        var serialized = new SerializedObject(director);
        serialized.FindProperty("mothership").objectReferenceValue = mothership;
        serialized.FindProperty("swarm").objectReferenceValue = swarm;
        serialized.FindProperty("mites").objectReferenceValue = mites;
        serialized.FindProperty("hive").objectReferenceValue = hive;
        serialized.FindProperty("champion").objectReferenceValue = champion;
        serialized.FindProperty("autopilot").objectReferenceValue = autopilot;
        serialized.FindProperty("field").objectReferenceValue = field;
        serialized.FindProperty("haloMaterial").objectReferenceValue = halo;
        serialized.FindProperty("resultText").objectReferenceValue = result;
        serialized.FindProperty("failedButtons").objectReferenceValue = failed;
        serialized.FindProperty("reviveButton").objectReferenceValue = revive;
        serialized.FindProperty("retryButton").objectReferenceValue = retry;
        SetArray(serialized.FindProperty("gates"), gates);
        SetArray(serialized.FindProperty("rings"), rings);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, scenePath);
        log.AppendLine($"saved {scenePath}: level {layout.number}, hive {layout.hive}, waves of {layout.mitesPerWave} every {layout.waveInterval} s at {layout.miteSpeed} units/s, " +
            $"{gates.Length} gates, {rings.Length} rings");
        return log.ToString();
    }

    static GameObject Renderable(string name, Transform parent, Vector3 localPosition, Material material)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = localPosition;
        gameObject.AddComponent<MeshFilter>();
        var renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return gameObject;
    }

    static Gate MakeGate(string name, GateSpec spec, string sign, Material world, Material fade, TMP_FontAsset font)
    {
        GameObject gateObject = Renderable(name, null, new Vector3(spec.x, 0f, spec.z), world);
        GameObject field = Renderable("Field", gateObject.transform, Vector3.zero, fade);
        TextMeshPro label = WorldLabel(gateObject.transform, sign, 12f, new Vector2(2.4f, 1.2f), font);
        var gate = gateObject.AddComponent<Gate>();
        var serialized = new SerializedObject(gate);
        serialized.FindProperty("value").intValue = spec.value;
        serialized.FindProperty("frame").objectReferenceValue = gateObject.GetComponent<MeshFilter>();
        serialized.FindProperty("field").objectReferenceValue = field.GetComponent<MeshFilter>();
        serialized.FindProperty("label").objectReferenceValue = label;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return gate;
    }

    static WarpRing MakeRing(string name, RingSpec spec, Material world)
    {
        GameObject ringObject = Renderable(name, null, new Vector3(spec.x, 1.2f, spec.z), world);
        var ring = ringObject.AddComponent<WarpRing>();
        var serialized = new SerializedObject(ring);
        serialized.FindProperty("colour").enumValueIndex = (int)spec.colour;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return ring;
    }

    static TextMeshPro WorldLabel(Transform parent, string text, float size, Vector2 box, TMP_FontAsset font)
    {
        var labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent, false);
        var label = labelObject.AddComponent<TextMeshPro>();
        label.font = font;
        label.fontSharedMaterial = font.material;
        label.text = text;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.rectTransform.sizeDelta = box;
        return label;
    }

    static TextMeshProUGUI HudText(Transform parent, string name, string text, float size, Vector2 anchor, Vector2 position, TMP_FontAsset font)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        var label = textObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.fontSharedMaterial = font.material;
        label.text = text;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        RectTransform rect = label.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(390f, size * 1.6f);
        return label;
    }

    static Image Panel(Transform parent, string name, Vector2 position, Vector2 size, Color colour)
    {
        var panelObject = new GameObject(name, typeof(RectTransform));
        panelObject.transform.SetParent(parent, false);
        var image = panelObject.AddComponent<Image>();
        image.color = colour;
        RectTransform rect = image.rectTransform;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return image;
    }

    // A flat button in the HUD look: fill colour, 2 dp ink outline, ink Exo 2 label.
    static Button HudButton(Transform parent, string name, string text, Vector2 position, Vector2 size, float textSize, Color fill, TMP_FontAsset font)
    {
        Image image = Panel(parent, name, position, size, fill);
        var outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = Palette.Ink;
        outline.effectDistance = new Vector2(2f, -2f);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI label = HudText(image.transform, "Label", text, textSize, new Vector2(0.5f, 0.5f), Vector2.zero, font);
        label.color = Palette.Ink;
        RectTransform rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return button;
    }

    static void Set(Object target, string property, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetArray(SerializedProperty property, params Object[] values)
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
