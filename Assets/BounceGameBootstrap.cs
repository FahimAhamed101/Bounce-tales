using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class BounceGameBootstrap : MonoBehaviour
{
    private static bool initialized;

    private Texture2D textureAtlas;
    private Texture2D blocksTexture;
    private Texture2D backgroundTexture;
    private Texture2D skyTreesTexture;

    private Sprite squareSprite;
    private Sprite circleSprite;
    private Sprite[] bounceFrames;
    private Sprite eggSprite;
    private Sprite jumpButtonSprite;
    private Sprite leftRightButtonSprite;
    private Sprite goalFlagSprite;

    // Reset statics when domain reload is disabled (Unity 6 default)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (initialized)
        {
            return;
        }

        // Clean up any stale objects from a previous play session
        CleanupStaleObjects();

        GameObject bootstrapObject = new GameObject("BounceGameBootstrap");
        bootstrapObject.AddComponent<BounceGameBootstrap>();
        initialized = true;
    }

    private static void CleanupStaleObjects()
    {
        string[] staleNames = {
            "BounceGameBootstrap", "Bounce", "Ground", "HUD", "TouchControls",
            "BounceGameManager", "EventSystem", "Background", "SkyTreesBack",
            "SkyTreesFront", "Cloud", "Flower", "ArrowSign",
            "PlatformA", "PlatformB", "PlatformC", "PlatformD", "PlatformE", "PlatformF",
            "PlatformA_Backfill", "PlatformB_Backfill", "PlatformC_Backfill",
            "PlatformD_Backfill", "PlatformE_Backfill", "PlatformF_Backfill",
            "PitHazard", "PitHazard2", "Egg", "GoalPole", "GoalFlag",
            "TreeNearStart", "TreeMid", "TreeNearGoal"
        };

        foreach (string objName in staleNames)
        {
            GameObject[] found = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject go in found)
            {
                if (go != null && go.name == objName)
                {
                    DestroyImmediate(go);
                }
            }
        }
    }

    private void Awake()
    {
        squareSprite = CreateSolidSprite(32, 32);
        circleSprite = CreateCircleSprite(64);
        LoadBounceAssets();

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5.5f;
        mainCamera.backgroundColor = new Color(0.21f, 0.64f, 0.88f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;

        CreateBackground();
        PlayerMovement player = CreatePlayer();
        List<EggCollectible> eggs = BuildLevel(player);
        SetupCamera(mainCamera, player.transform);
        SetupGameManager(player, eggs.Count);
        CreateTouchControls(player);
    }

    private void LoadBounceAssets()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, "BounceTales");

        textureAtlas = LoadTexture(Path.Combine(basePath, "texture.png"), 512, 512, new Color(0f, 0f, 0f, 0f));
        blocksTexture = LoadTexture(Path.Combine(basePath, "blocks.png"), 434, 582, new Color(0f, 0f, 0f, 0f));
        backgroundTexture = LoadTexture(Path.Combine(basePath, "background.png"), 64, 64, new Color(0.21f, 0.64f, 0.88f, 1f));
        skyTreesTexture = LoadTexture(Path.Combine(basePath, "sky_trees.png"), 64, 64, new Color(0f, 0f, 0f, 0f));

        bounceFrames = new[]
        {
            CreateSpriteFromAtlas(textureAtlas, new Rect(375f, 430f, 40f, 40f), 40f),
            CreateSpriteFromAtlas(textureAtlas, new Rect(425f, 138f, 40f, 40f), 40f),
            CreateSpriteFromAtlas(textureAtlas, new Rect(0f, 430f, 40f, 40f), 40f),
            CreateSpriteFromAtlas(textureAtlas, new Rect(415f, 430f, 40f, 40f), 40f)
        };

        eggSprite = CreateSpriteFromAtlas(textureAtlas, new Rect(436f, 0f, 28f, 28f), 28f);
        jumpButtonSprite = CreateSpriteFromAtlas(textureAtlas, new Rect(358f, 0f, 78f, 78f), 78f);
        leftRightButtonSprite = CreateSpriteFromAtlas(textureAtlas, new Rect(194f, 195f, 161f, 161f), 161f);
        goalFlagSprite = CreateSpriteFromAtlas(blocksTexture, new Rect(154f, 513f, 64f, 59f), 64f);
    }

    private void SetupGameManager(PlayerMovement player, int eggCount)
    {
        BounceGameManager manager = new GameObject("BounceGameManager").AddComponent<BounceGameManager>();
        manager.Initialize(player, eggCount, eggSprite);
    }

    private void SetupCamera(Camera mainCamera, Transform target)
    {
        SimpleCameraFollow cameraFollow = mainCamera.GetComponent<SimpleCameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<SimpleCameraFollow>();
        }

        cameraFollow.SetTarget(target);
    }

    private void CreateBackground()
    {
        if (backgroundTexture != null)
        {
            Sprite backgroundSprite = Sprite.Create(backgroundTexture, new Rect(0f, 0f, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f), 100f);
            CreateSpriteObject("Background", backgroundSprite, new Vector2(5.5f, 2.75f), new Vector2(32f, 18f), Color.white, -10);
        }
        else
        {
            CreateSpriteObject("Background", squareSprite, new Vector2(5.5f, 2.75f), new Vector2(32f, 18f), new Color(0.56f, 0.86f, 0.98f), -10);
        }

        if (skyTreesTexture != null)
        {
            Sprite skyTreesSprite = Sprite.Create(skyTreesTexture, new Rect(0f, 0f, skyTreesTexture.width, skyTreesTexture.height), new Vector2(0.5f, 0.5f), 100f);
            CreateSpriteObject("SkyTreesBack", skyTreesSprite, new Vector2(4f, 1.6f), new Vector2(18f, 4.2f), Color.white, -8);
            CreateSpriteObject("SkyTreesFront", skyTreesSprite, new Vector2(15.5f, 1.4f), new Vector2(20f, 4.5f), Color.white, -7);
        }

        CreateCloud(new Vector2(-4f, 2.8f), new Vector2(2.2f, 0.9f), -9);
        CreateCloud(new Vector2(3.5f, 2.4f), new Vector2(1.7f, 0.75f), -9);
        CreateCloud(new Vector2(11.2f, 3.1f), new Vector2(2.4f, 1f), -9);
        CreateCloud(new Vector2(17.8f, 2.55f), new Vector2(1.8f, 0.75f), -9);
    }

    private PlayerMovement CreatePlayer()
    {
        GameObject player = new GameObject("Bounce");
        player.transform.position = new Vector3(-7.5f, -1.6f, 0f);

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 2.8f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.38f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(player.transform, false);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = bounceFrames != null && bounceFrames.Length > 0 ? bounceFrames[0] : squareSprite;
        renderer.sortingOrder = 5;
        visual.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

        PlayerMovement movement = player.AddComponent<PlayerMovement>();

        movement.SetAnimationFrames(
            bounceFrames != null && bounceFrames.Length > 0 ? bounceFrames[0] : null,
            bounceFrames,
            bounceFrames != null && bounceFrames.Length > 0 ? bounceFrames[0] : null);

        return movement;
    }

    private List<EggCollectible> BuildLevel(PlayerMovement player)
    {
        List<EggCollectible> eggs = new List<EggCollectible>();

        CreateGroundStrip("Ground", new Vector2(-8.8f, -4.15f), 15);
        CreatePlatform("PlatformA", new Vector2(-4f, -1.75f), new Vector2(2.4f, 0.55f));
        CreatePlatform("PlatformB", new Vector2(-0.1f, -0.45f), new Vector2(3.2f, 0.55f));
        CreatePlatform("PlatformC", new Vector2(4.1f, 1f), new Vector2(3f, 0.55f));
        CreatePlatform("PlatformD", new Vector2(8.1f, -0.25f), new Vector2(2.8f, 0.55f));
        CreatePlatform("PlatformE", new Vector2(12f, 1.15f), new Vector2(3.2f, 0.55f));
        CreatePlatform("PlatformF", new Vector2(16f, 2.45f), new Vector2(3f, 0.55f));

        CreateHazard("PitHazard", new Vector2(6.2f, -3.3f), new Vector2(2.2f, 0.45f));
        CreateHazard("PitHazard2", new Vector2(14.8f, -3.3f), new Vector2(2.4f, 0.45f));

        eggs.Add(CreateEgg(new Vector2(-4f, -0.9f)));
        eggs.Add(CreateEgg(new Vector2(-0.1f, 0.3f)));
        eggs.Add(CreateEgg(new Vector2(4.1f, 1.75f)));
        eggs.Add(CreateEgg(new Vector2(8.1f, 0.45f)));
        eggs.Add(CreateEgg(new Vector2(12f, 1.9f)));
        eggs.Add(CreateEgg(new Vector2(16f, 3.1f)));

        CreateGoal(new Vector2(18.3f, 3.2f));
        CreateDecor("TreeNearStart", new Vector2(-6.8f, -2.35f), new Vector2(1.2f, 2.3f));
        CreateDecor("TreeMid", new Vector2(2.6f, -2.35f), new Vector2(1.2f, 2.3f));
        CreateDecor("TreeNearGoal", new Vector2(14.2f, -2.35f), new Vector2(1.2f, 2.3f));
        CreateFlower(new Vector2(-1.6f, -2.75f), 1.15f, new Color(0.96f, 0.27f, 0.4f));
        CreateFlower(new Vector2(6.9f, -2.8f), 1.05f, Color.white);
        CreateFlower(new Vector2(10.7f, -2.75f), 1.1f, new Color(0.98f, 0.39f, 0.46f));
        CreateArrowSign(new Vector2(5.8f, -2.85f), ">");
        CreateArrowSign(new Vector2(13.6f, -2.85f), ">>");

        player.SetSpawnPoint(new Vector2(-7.5f, -1.6f));

        return eggs;
    }

    private void CreateTouchControls(PlayerMovement player)
    {
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        Canvas canvas = new GameObject("TouchControls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        // Always show controls on all platforms so they work in editor too
        canvas.gameObject.SetActive(true);

        CreateButton(canvas.transform, player, MobileControlButton.ControlType.Left, leftRightButtonSprite, "<", new Vector2(170f, 170f), new Vector2(170f, 170f), true);
        CreateButton(canvas.transform, player, MobileControlButton.ControlType.Right, leftRightButtonSprite, ">", new Vector2(360f, 170f), new Vector2(170f, 170f), false);
        CreateButton(canvas.transform, player, MobileControlButton.ControlType.Jump, jumpButtonSprite, "JUMP", new Vector2(1730f, 170f), new Vector2(170f, 170f), false);
    }

    private void CreateButton(Transform parent, PlayerMovement player, MobileControlButton.ControlType type, Sprite sprite, string fallbackLabel, Vector2 anchoredPosition, Vector2 size, bool flipX)
    {
        GameObject buttonObject = new GameObject($"{type}Button", typeof(RectTransform), typeof(Image), typeof(MobileControlButton));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = new Color(1f, 1f, 1f, 0.65f);
        if (flipX)
        {
            image.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        }

        MobileControlButton controlButton = buttonObject.GetComponent<MobileControlButton>();
        controlButton.Initialize(player, type);

        if (sprite == null)
        {
            // Fallback: create a semi-transparent circle background with label
            image.color = new Color(0f, 0f, 0f, 0.3f);

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = fallbackLabel;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 44;
            text.color = Color.white;
        }
    }

    private EggCollectible CreateEgg(Vector2 position)
    {
        Sprite sprite = eggSprite != null ? eggSprite : squareSprite;
        GameObject egg = CreateSpriteObject("Egg", sprite, position, new Vector2(0.55f, 0.55f), Color.white, 4);
        egg.AddComponent<EggCollectible>();

        CircleCollider2D collider = egg.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.3f;

        return egg.GetComponent<EggCollectible>();
    }

    private void CreateGoal(Vector2 position)
    {
        GameObject pole = CreateSpriteObject("GoalPole", squareSprite, position + new Vector2(-0.3f, -0.9f), new Vector2(0.14f, 2.4f), new Color(0.93f, 0.94f, 0.97f), 3);
        CreateSpriteObject("GoalFlag", goalFlagSprite != null ? goalFlagSprite : squareSprite, position + new Vector2(0.2f, 0f), new Vector2(0.85f, 0.78f), Color.white, 4);

        BoxCollider2D collider = pole.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.05f, 2.4f);
        collider.isTrigger = true;
        pole.AddComponent<GoalTrigger>();
    }

    private void CreateHazard(string objectName, Vector2 position, Vector2 size)
    {
        GameObject hazard = CreateSpriteObject(objectName, squareSprite, position, size, new Color(0.87f, 0.18f, 0.16f), 2);
        BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        hazard.AddComponent<HazardZone>();
    }

    private void CreateGroundStrip(string objectName, Vector2 startPosition, int segmentCount)
    {
        GameObject root = new GameObject(objectName);
        CreateSpriteObject($"{objectName}_Backfill", squareSprite, startPosition + new Vector2(segmentCount * 0.62f - 0.2f, -0.8f), new Vector2(segmentCount * 1.26f, 2.2f), new Color(0.02f, 0.33f, 0.23f), 0, root.transform);

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 segmentPosition = startPosition + new Vector2(i * 1.25f, 0f);
            GameObject segment = CreatePlatformSegment($"{objectName}_{i}", segmentPosition, new Vector2(1.25f, 1f), root.transform);
            BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
        }
    }

    private void CreatePlatform(string objectName, Vector2 centerPosition, Vector2 size)
    {
        CreateSpriteObject($"{objectName}_Backfill", squareSprite, centerPosition + new Vector2(0f, -0.14f), new Vector2(size.x * 0.96f, size.y * 0.85f), new Color(0.02f, 0.33f, 0.23f), 0);
        GameObject platform = CreatePlatformSegment(objectName, centerPosition, size, null);
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
    }

    private GameObject CreatePlatformSegment(string objectName, Vector2 position, Vector2 size, Transform parent)
    {
        Sprite sprite = GetPlatformSprite();
        GameObject platform = CreateSpriteObject(objectName, sprite, position, size, Color.white, 1, parent);
        return platform;
    }

    private Sprite GetPlatformSprite()
    {
        if (blocksTexture == null)
        {
            return squareSprite;
        }

        return CreateSpriteFromAtlas(blocksTexture, new Rect(2f, 146f, 64f, 64f), 64f);
    }

    private void CreateDecor(string objectName, Vector2 position, Vector2 size)
    {
        if (skyTreesTexture == null)
        {
            return;
        }

        Sprite decorSprite = CreateSpriteFromAtlas(skyTreesTexture, new Rect(63f, 17f, 40f, 112f), 100f, new Vector2(0.5f, 0.05f));
        CreateSpriteObject(objectName, decorSprite, position, size, Color.white, -1);
    }

    private void CreateCloud(Vector2 position, Vector2 size, int sortingOrder)
    {
        GameObject cloudRoot = new GameObject("Cloud");
        CreateSpriteObject("PuffA", circleSprite, position + new Vector2(-0.4f * size.x, 0f), new Vector2(0.75f * size.x, 0.48f * size.y), Color.white, sortingOrder, cloudRoot.transform);
        CreateSpriteObject("PuffB", circleSprite, position + new Vector2(0f, 0.12f * size.y), new Vector2(0.95f * size.x, 0.58f * size.y), Color.white, sortingOrder, cloudRoot.transform);
        CreateSpriteObject("PuffC", circleSprite, position + new Vector2(0.45f * size.x, 0f), new Vector2(0.7f * size.x, 0.44f * size.y), Color.white, sortingOrder, cloudRoot.transform);
    }

    private void CreateFlower(Vector2 position, float scale, Color petalColor)
    {
        GameObject root = new GameObject("Flower");
        CreateSpriteObject("Stem", squareSprite, position + new Vector2(0f, 0.38f * scale), new Vector2(0.08f * scale, 0.82f * scale), new Color(0.19f, 0.72f, 0.22f), 2, root.transform);
        CreateSpriteObject("LeafLeft", squareSprite, position + new Vector2(-0.13f * scale, 0.15f * scale), new Vector2(0.2f * scale, 0.1f * scale), new Color(0.22f, 0.76f, 0.25f), 2, root.transform);
        CreateSpriteObject("LeafRight", squareSprite, position + new Vector2(0.13f * scale, 0.22f * scale), new Vector2(0.22f * scale, 0.1f * scale), new Color(0.22f, 0.76f, 0.25f), 2, root.transform);
        CreateSpriteObject("PetalTop", circleSprite, position + new Vector2(0f, 0.9f * scale), new Vector2(0.28f * scale, 0.28f * scale), petalColor, 3, root.transform);
        CreateSpriteObject("PetalBottom", circleSprite, position + new Vector2(0f, 0.62f * scale), new Vector2(0.28f * scale, 0.28f * scale), petalColor, 3, root.transform);
        CreateSpriteObject("PetalLeft", circleSprite, position + new Vector2(-0.16f * scale, 0.76f * scale), new Vector2(0.28f * scale, 0.28f * scale), petalColor, 3, root.transform);
        CreateSpriteObject("PetalRight", circleSprite, position + new Vector2(0.16f * scale, 0.76f * scale), new Vector2(0.28f * scale, 0.28f * scale), petalColor, 3, root.transform);
        CreateSpriteObject("Center", circleSprite, position + new Vector2(0f, 0.76f * scale), new Vector2(0.2f * scale, 0.2f * scale), new Color(0.97f, 0.83f, 0.16f), 4, root.transform);
    }

    private void CreateArrowSign(Vector2 position, string label)
    {
        GameObject signRoot = new GameObject("ArrowSign");
        CreateSpriteObject("Post", squareSprite, position + new Vector2(0f, -0.22f), new Vector2(0.08f, 0.5f), new Color(0.53f, 0.39f, 0.23f), 2, signRoot.transform);
        GameObject board = CreateSpriteObject("Board", squareSprite, position + new Vector2(0.32f, 0.08f), new Vector2(0.72f, 0.42f), new Color(0.78f, 0.58f, 0.32f), 3, signRoot.transform);

        GameObject textObject = new GameObject("Label", typeof(TextMesh));
        textObject.transform.SetParent(board.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        TextMesh mesh = textObject.GetComponent<TextMesh>();
        mesh.text = label;
        mesh.characterSize = 0.14f;
        mesh.fontSize = 64;
        mesh.alignment = TextAlignment.Center;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.color = new Color(1f, 0.76f, 0.17f);
    }

    private GameObject CreateSpriteObject(string objectName, Sprite sprite, Vector2 position, Vector2 size, Color color, int sortingOrder, Transform parent = null)
    {
        GameObject createdObject = new GameObject(objectName);
        if (parent != null)
        {
            createdObject.transform.SetParent(parent, false);
        }

        createdObject.transform.position = new Vector3(position.x, position.y, 0f);
        createdObject.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = createdObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        return createdObject;
    }

    private Texture2D LoadTexture(string filePath, int fallbackWidth, int fallbackHeight, Color fallbackColor)
    {
        if (!File.Exists(filePath))
        {
            Texture2D fallbackTexture = new Texture2D(fallbackWidth, fallbackHeight, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[fallbackWidth * fallbackHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = fallbackColor;
            }

            fallbackTexture.SetPixels(pixels);
            fallbackTexture.Apply();
            return fallbackTexture;
        }

        byte[] imageBytes = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.LoadImage(imageBytes);
        return texture;
    }

    private Sprite CreateSpriteFromAtlas(Texture2D texture, Rect rect, float pixelsPerUnit, Vector2? pivot = null)
    {
        if (texture == null)
        {
            return null;
        }

        float correctedY = texture.height - rect.y - rect.height;
        Rect correctedRect = new Rect(rect.x, correctedY, rect.width, rect.height);
        Vector2 actualPivot = pivot ?? new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, correctedRect, actualPivot, pixelsPerUnit);
    }

    private Sprite CreateSolidSprite(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
    }

    private Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
