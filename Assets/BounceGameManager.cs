using UnityEngine;
using UnityEngine.UI;

public class BounceGameManager : MonoBehaviour
{
    public static BounceGameManager Instance { get; private set; }

    private Image eggIcon;
    private Image clockIcon;
    private Text scoreText;
    private Text timerText;
    private Text statusText;
    private CanvasGroup statusGroup;
    private int collectedEggs;
    private int totalEggs;
    private PlayerMovement player;
    private float elapsedTime;
    private float statusTimer;

    // Reset statics when domain reload is disabled (Unity 6 default)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    public void Initialize(PlayerMovement playerController, int eggCount, Sprite eggHudSprite)
    {
        Instance = this;
        player = playerController;
        totalEggs = eggCount;
        BuildHud(eggHudSprite);
        RefreshHud();
    }

    public void CollectEgg(EggCollectible egg)
    {
        if (egg.Collected)
        {
            return;
        }

        egg.MarkCollected();
        collectedEggs++;
        RefreshHud();
        ShowStatus("Egg collected!");
    }

    public void ReachGoal()
    {
        ShowStatus(collectedEggs >= totalEggs
            ? "Level complete! All eggs collected!"
            : "Goal reached! Try collecting every egg.");
    }

    public void HitHazard()
    {
        if (player == null)
        {
            return;
        }

        player.Respawn();
        ShowStatus("Watch out! You bounced back to the checkpoint.");
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (timerText != null)
        {
            int totalSeconds = Mathf.FloorToInt(elapsedTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = $"{minutes:0}:{seconds:00}";
        }

        if (statusGroup != null && statusTimer > 0f)
        {
            statusTimer -= Time.deltaTime;
            statusGroup.alpha = Mathf.Clamp01(statusTimer / 1.2f);
        }
    }

    private void BuildHud(Sprite eggHudSprite)
    {
        Canvas canvas = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // -- Top-left: Egg counter panel --
        Transform topLeftPanel = CreatePanel("EggPanel", canvas.transform, new Vector2(28f, -22f), new Vector2(220f, 72f), new Vector2(0f, 1f), new Color(0f, 0f, 0f, 0.35f));
        eggIcon = CreateIcon("EggIcon", topLeftPanel, eggHudSprite, new Vector2(38f, -36f), new Vector2(44f, 44f));
        scoreText = CreateText("ScoreText", topLeftPanel, new Vector2(82f, -35f), TextAnchor.MiddleLeft, 32, new Vector2(0f, 0.5f), new Vector2(250f, 50f));

        // -- Top-right: Timer panel --
        Transform topRightPanel = CreatePanel("TimerPanel", canvas.transform, new Vector2(-28f, -22f), new Vector2(200f, 72f), new Vector2(1f, 1f), new Color(0f, 0f, 0f, 0.35f));
        clockIcon = CreateIcon("ClockIcon", topRightPanel, CreateClockSprite(), new Vector2(-158f, -36f), new Vector2(42f, 42f), true);
        timerText = CreateText("TimerText", topRightPanel, new Vector2(-28f, -35f), TextAnchor.MiddleRight, 32, new Vector2(1f, 0.5f), new Vector2(200f, 50f));

        // -- Bottom-center: Status message panel --
        Transform bottomPanel = CreatePanel("StatusPanel", canvas.transform, new Vector2(0f, 90f), new Vector2(700f, 64f), new Vector2(0.5f, 0f), new Color(0f, 0f, 0f, 0.4f));
        statusText = CreateText("StatusText", bottomPanel, new Vector2(0f, -32f), TextAnchor.MiddleCenter, 24, new Vector2(0.5f, 0.5f), new Vector2(660f, 48f));
        statusText.text = "Arrows / WASD to move, Space to jump";
        statusGroup = bottomPanel.gameObject.AddComponent<CanvasGroup>();
        statusGroup.alpha = 1f;
        statusTimer = 3f;
    }

    private Transform CreatePanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Color bgColor)
    {
        GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = panelObject.GetComponent<Image>();
        image.color = bgColor;

        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        outline.effectDistance = new Vector2(1f, -1f);

        return panelObject.transform;
    }

    private Image CreateIcon(string objectName, Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size, bool rightAnchored = false)
    {
        GameObject iconObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(parent, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = iconObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;

        return image;
    }

    private Text CreateText(string objectName, Transform parent, Vector2 anchoredPosition, TextAnchor alignment, int fontSize, Vector2 pivotAnchor, Vector2 size)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text), typeof(Shadow));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(pivotAnchor.x, 1f);
        rect.anchorMax = new Vector2(pivotAnchor.x, 1f);
        rect.pivot = pivotAnchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.fontStyle = FontStyle.Bold;

        Shadow shadow = textObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private void RefreshHud()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{collectedEggs} / {totalEggs}";
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusTimer = 2.4f;

        if (statusGroup != null)
        {
            statusGroup.alpha = 1f;
        }
    }

    private Sprite CreateClockSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color white = Color.white;
        Color border = new Color(0.12f, 0.12f, 0.12f, 1f);
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                Color pixel = clear;

                if (dist <= 26f)
                {
                    pixel = white;
                }

                if (dist >= 23f && dist <= 26f)
                {
                    pixel = border;
                }

                if ((Mathf.Abs(x - center.x) <= 1f && y > center.y - 16f && y < center.y + 2f) ||
                    (Mathf.Abs(y - center.y) <= 1f && x > center.x - 1f && x < center.x + 14f))
                {
                    pixel = border;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
