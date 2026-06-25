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
        ShowStatus("Egg collected");
    }

    public void ReachGoal()
    {
        ShowStatus(collectedEggs >= totalEggs
            ? "Level complete! All eggs collected."
            : "Goal reached! Try collecting every egg.");
    }

    public void HitHazard()
    {
        if (player == null)
        {
            return;
        }

        player.Respawn();

        if (statusText != null)
        {
            ShowStatus("Watch out! You bounced back to the checkpoint.");
        }
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

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Transform topLeftPanel = CreatePanel("EggPanel", canvas.transform, new Vector2(36f, -28f), new Vector2(250f, 84f), new Vector2(0f, 1f));
        Transform topRightPanel = CreatePanel("TimerPanel", canvas.transform, new Vector2(-36f, -28f), new Vector2(220f, 84f), new Vector2(1f, 1f));
        Transform bottomPanel = CreatePanel("StatusPanel", canvas.transform, new Vector2(0f, 110f), new Vector2(760f, 78f), new Vector2(0.5f, 0f));

        eggIcon = CreateIcon("EggIcon", topLeftPanel, eggHudSprite, new Vector2(42f, -42f), new Vector2(52f, 52f));
        scoreText = CreateText("ScoreText", topLeftPanel, new Vector2(92f, -40f), TextAnchor.MiddleLeft, 40, new Vector2(0f, 0.5f), new Vector2(300f, 60f));

        clockIcon = CreateIcon("ClockIcon", topRightPanel, CreateClockSprite(), new Vector2(-178f, -42f), new Vector2(50f, 50f), true);
        timerText = CreateText("TimerText", topRightPanel, new Vector2(-32f, -40f), TextAnchor.MiddleRight, 40, new Vector2(1f, 0.5f), new Vector2(220f, 60f));

        statusText = CreateText("StatusText", bottomPanel, new Vector2(0f, -39f), TextAnchor.MiddleCenter, 28, new Vector2(0.5f, 0.5f), new Vector2(700f, 52f));
        statusText.text = "Arrows / WASD to move, Space to jump";
        statusGroup = bottomPanel.gameObject.AddComponent<CanvasGroup>();
        statusGroup.alpha = 1f;
        statusTimer = 2.2f;
    }

    private Transform CreatePanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchor)
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
        image.color = new Color(0f, 0f, 0f, 0.22f);

        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        outline.effectDistance = new Vector2(2f, -2f);

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
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text), typeof(Outline));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(pivotAnchor.x, 1f);
        rect.anchorMax = new Vector2(pivotAnchor.x, 1f);
        rect.pivot = pivotAnchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textObject.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        outline.effectDistance = new Vector2(3f, -3f);

        return text;
    }

    private void RefreshHud()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Eggs: {collectedEggs}/{totalEggs}";
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
