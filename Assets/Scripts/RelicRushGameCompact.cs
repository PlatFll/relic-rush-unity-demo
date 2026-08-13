using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace RelicRush
{
    public sealed class RelicRushGameCompact : MonoBehaviour
    {
        private sealed class Target
        {
            public string Name;
            public string Clue;
            public Rect Hotspot;

            public Target(string name, string clue, Rect hotspot)
            {
                Name = name;
                Clue = clue;
                Hotspot = hotspot;
            }
        }

        private readonly List<Target> allTargets = new List<Target>();
        private readonly List<Target> wantedTargets = new List<Target>();
        private readonly HashSet<string> foundTargets = new HashSet<string>();
        private readonly Dictionary<string, Image> hotspotImages = new Dictionary<string, Image>();

        private readonly Color32 gold = new Color32(220, 170, 60, 255);
        private readonly Color32 cream = new Color32(239, 226, 193, 255);
        private readonly Color32 panel = new Color32(20, 22, 30, 246);
        private readonly Color32 green = new Color32(54, 112, 69, 255);

        private System.Random rng;
        private Canvas canvas;
        private Font font;
        private Sprite pixelSprite;

        private GameObject menuRoot;
        private GameObject gameRoot;
        private GameObject gameOverRoot;

        private Image roomImage;
        private Image darknessOverlay;

        private Text roomTitle;
        private Text scoreText;
        private Text timeText;
        private Text targetText;
        private Text hintText;

        private int round;
        private int score;
        private int combo;
        private int hints;
        private float timeRemaining;
        private float lastFindTime;
        private bool running;
        private bool cryptic;

        private void Awake()
        {
            rng = new System.Random();
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            EnsureCamera();
            EnsureInputSystemEventSystem();
            BuildData();
            BuildUI();
            ShowMenu();
        }

        private void Update()
        {
            if (!running)
                return;

            timeRemaining -= Time.deltaTime * (1f + Mathf.Max(0, round - 8) * 0.04f);

            if (combo > 0 && Time.time - lastFindTime > 3f)
                combo = 0;

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                EndRun();
            }

            UpdateHud();
        }

        private static void EnsureCamera()
        {
            Camera existing = Object.FindFirstObjectByType<Camera>();
            if (existing != null)
            {
                existing.enabled = true;
                return;
            }

            GameObject cameraObject = new GameObject("Relic Rush Camera");
            Object.DontDestroyOnLoad(cameraObject);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = ~0;
            camera.depth = -100f;
            camera.targetDisplay = 0;
            camera.enabled = true;
        }

        private static void EnsureInputSystemEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();

            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                Object.DontDestroyOnLoad(eventSystemObject);
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                legacyModule.enabled = false;
                Object.Destroy(legacyModule);
            }

            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            inputModule.enabled = true;
        }

        private void BuildData()
        {
            string[] names =
            {
                "KEY", "SKULL", "HOURGLASS", "FEATHER",
                "CANDLE", "SCROLL", "CRYSTAL", "DAGGER",
                "COIN", "MUSHROOM", "GOBLET", "RING",
                "CAT", "LANTERN", "APPLE", "CROWN"
            };

            string[] clues =
            {
                "opens what is locked", "what remains after flesh", "time trapped in glass", "once belonged to a bird",
                "a flame on wax", "a rolled message", "a purple mineral", "a short blade",
                "small treasure", "a red-capped fungus", "a feast cup", "finger jewelry",
                "a quiet watcher", "portable flame", "a red fruit", "worn by royalty"
            };

            Rect[] hotspots =
            {
                new Rect(.09f,.70f,.055f,.065f), new Rect(.29f,.43f,.06f,.10f),
                new Rect(.71f,.69f,.055f,.09f), new Rect(.51f,.16f,.065f,.11f),
                new Rect(.78f,.34f,.05f,.13f), new Rect(.32f,.10f,.075f,.08f),
                new Rect(.61f,.17f,.06f,.11f), new Rect(.84f,.14f,.055f,.12f),
                new Rect(.27f,.19f,.045f,.07f), new Rect(.16f,.31f,.06f,.10f),
                new Rect(.90f,.41f,.05f,.10f), new Rect(.45f,.33f,.045f,.08f),
                new Rect(.58f,.34f,.08f,.11f), new Rect(.04f,.39f,.055f,.13f),
                new Rect(.73f,.10f,.05f,.08f), new Rect(.86f,.78f,.07f,.09f)
            };

            for (int i = 0; i < names.Length; i++)
                allTargets.Add(new Target(names[i], clues[i], hotspots[i]));
        }

        private void BuildUI()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pixelSprite = MakePixelSprite();

            BuildMenu();
            BuildGameScreen();
            BuildGameOverScreen();
        }

        private void BuildMenu()
        {
            menuRoot = CreateRoot("Menu");

            Image background = CreateImage(menuRoot.transform, Color.white);
            background.sprite = CreateSprite(CreateRoomArt(false));
            Stretch(background.rectTransform);

            Image shade = CreateImage(menuRoot.transform, new Color(0f, 0f, 0f, .42f));
            Stretch(shade.rectTransform);

            GameObject box = CreatePanel(menuRoot.transform, new Vector2(.10f, .14f), new Vector2(.52f, .86f));
            CreateText(box.transform, "RELIC RUSH", 60, gold, new Vector2(.06f, .70f), new Vector2(.94f, .93f));
            CreateText(box.transform, "ENDLESS HIDDEN-OBJECT ROGUELITE", 19, cream, new Vector2(.06f, .61f), new Vector2(.94f, .72f));
            CreateText(box.transform, "Find objects, chain combos, survive harder rounds and chase your best score.", 22, cream, new Vector2(.09f, .37f), new Vector2(.91f, .60f));
            CreateButton(box.transform, "START HUNT", new Vector2(.18f, .15f), new Vector2(.82f, .31f), StartRun, out _);
        }

        private void BuildGameScreen()
        {
            gameRoot = CreateRoot("Game");

            roomImage = CreateImage(gameRoot.transform, Color.white);
            roomImage.rectTransform.anchorMin = new Vector2(0f, .14f);
            roomImage.rectTransform.anchorMax = new Vector2(1f, .88f);
            roomImage.rectTransform.offsetMin = Vector2.zero;
            roomImage.rectTransform.offsetMax = Vector2.zero;

            Button backgroundButton = roomImage.gameObject.AddComponent<Button>();
            backgroundButton.transition = Selectable.Transition.None;
            backgroundButton.onClick.AddListener(WrongTap);

            darknessOverlay = CreateImage(gameRoot.transform, new Color(0f, 0f, .02f, .68f));
            darknessOverlay.raycastTarget = false;
            darknessOverlay.rectTransform.anchorMin = new Vector2(0f, .14f);
            darknessOverlay.rectTransform.anchorMax = new Vector2(1f, .88f);
            darknessOverlay.rectTransform.offsetMin = Vector2.zero;
            darknessOverlay.rectTransform.offsetMax = Vector2.zero;

            GameObject top = CreatePanel(gameRoot.transform, new Vector2(.01f, .89f), new Vector2(.99f, .99f));
            roomTitle = CreateText(top.transform, "ROOM", 22, gold, new Vector2(.02f, .05f), new Vector2(.42f, .95f));
            scoreText = CreateText(top.transform, "SCORE", 22, cream, new Vector2(.45f, .05f), new Vector2(.70f, .95f));
            timeText = CreateText(top.transform, "00:00", 28, gold, new Vector2(.74f, .05f), new Vector2(.97f, .95f));

            GameObject bottom = CreatePanel(gameRoot.transform, new Vector2(.01f, .01f), new Vector2(.99f, .13f));
            targetText = CreateText(bottom.transform, "FIND", 18, cream, new Vector2(.02f, .05f), new Vector2(.80f, .95f));
            CreateButton(bottom.transform, "HINT", new Vector2(.82f, .15f), new Vector2(.98f, .85f), UseHint, out hintText);
        }

        private void BuildGameOverScreen()
        {
            gameOverRoot = CreateRoot("Over");

            Image shade = CreateImage(gameOverRoot.transform, new Color(.01f, .01f, .02f, .95f));
            Stretch(shade.rectTransform);

            GameObject box = CreatePanel(gameOverRoot.transform, new Vector2(.30f, .22f), new Vector2(.70f, .78f));
            CreateText(box.transform, "RUN OVER", 48, gold, new Vector2(.08f, .70f), new Vector2(.92f, .91f));

            Text result = CreateText(box.transform, string.Empty, 28, cream, new Vector2(.08f, .35f), new Vector2(.92f, .68f));
            result.name = "Result";

            CreateButton(box.transform, "PLAY AGAIN", new Vector2(.18f, .14f), new Vector2(.82f, .29f), StartRun, out _);
        }

        private void ShowMenu()
        {
            menuRoot.SetActive(true);
            gameRoot.SetActive(false);
            gameOverRoot.SetActive(false);
            running = false;
        }

        private void StartRun()
        {
            menuRoot.SetActive(false);
            gameOverRoot.SetActive(false);
            gameRoot.SetActive(true);

            round = 1;
            score = 0;
            combo = 0;
            hints = 3;

            BeginRound();
        }

        private void BeginRound()
        {
            running = true;
            foundTargets.Clear();
            wantedTargets.Clear();
            combo = 0;

            cryptic = round >= 3 && round % 3 == 0;
            darknessOverlay.gameObject.SetActive(round >= 4 && round % 4 == 0);

            bool tavern = round % 2 == 0;
            roomImage.sprite = CreateSprite(CreateRoomArt(tavern));
            roomTitle.text = "ROOM " + round + " — " + (tavern ? "SMUGGLER'S TAVERN" : "ALCHEMIST WORKSHOP");

            int count = Mathf.Clamp(3 + (round - 1) / 2, 3, 8);
            wantedTargets.AddRange(allTargets.OrderBy(_ => rng.Next()).Take(count));

            timeRemaining = Mathf.Max(18f, 42f - round) + 2f;

            BuildHotspots();
            RefreshTargets();
            UpdateHud();
        }

        private void BuildHotspots()
        {
            foreach (Transform child in roomImage.transform)
                Destroy(child.gameObject);

            hotspotImages.Clear();

            foreach (Target target in allTargets)
            {
                GameObject hotspot = new GameObject("Hit " + target.Name, typeof(RectTransform), typeof(Image), typeof(Button));
                hotspot.transform.SetParent(roomImage.transform, false);

                RectTransform rect = hotspot.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(target.Hotspot.x, 1f - target.Hotspot.y - target.Hotspot.height);
                rect.anchorMax = new Vector2(target.Hotspot.x + target.Hotspot.width, 1f - target.Hotspot.y);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                Image image = hotspot.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, .001f);

                Button button = hotspot.GetComponent<Button>();
                button.transition = Selectable.Transition.None;

                Target captured = target;
                button.onClick.AddListener(() => TapTarget(captured));

                hotspotImages[target.Name] = image;
            }
        }

        private void TapTarget(Target target)
        {
            if (!running)
                return;

            if (wantedTargets.Contains(target) && !foundTargets.Contains(target.Name))
            {
                foundTargets.Add(target.Name);
                combo++;
                lastFindTime = Time.time;
                score += 100 + Mathf.Max(0, combo - 1) * 20;
                RefreshTargets();

                if (foundTargets.Count == wantedTargets.Count)
                {
                    score += Mathf.CeilToInt(timeRemaining) * 5;
                    round++;
                    BeginRound();
                }
            }
            else
            {
                WrongTap();
            }
        }

        private void WrongTap()
        {
            if (!running)
                return;

            combo = 0;
            timeRemaining = Mathf.Max(0f, timeRemaining - 2.5f);
        }

        private void UseHint()
        {
            if (!running || hints <= 0)
                return;

            List<Target> remaining = wantedTargets.Where(t => !foundTargets.Contains(t.Name)).ToList();
            if (remaining.Count == 0)
                return;

            hints--;
            Target target = remaining[rng.Next(remaining.Count)];
            hotspotImages[target.Name].color = new Color(1f, .85f, .1f, .55f);
            Invoke(nameof(ClearHint), 1f);
            UpdateHud();
        }

        private void ClearHint()
        {
            foreach (Image image in hotspotImages.Values)
                image.color = new Color(1f, 1f, 1f, .001f);
        }

        private void EndRun()
        {
            running = false;

            int best = Mathf.Max(PlayerPrefs.GetInt("RelicRush.Best", 0), score);
            PlayerPrefs.SetInt("RelicRush.Best", best);
            PlayerPrefs.Save();

            gameRoot.SetActive(false);
            gameOverRoot.SetActive(true);

            Text result = gameOverRoot.GetComponentsInChildren<Text>(true).First(x => x.name == "Result");
            result.text = "FINAL SCORE\n" + score.ToString("N0") + "\n\nROOM " + round + "\nBEST " + best.ToString("N0");
        }

        private void RefreshTargets()
        {
            targetText.text = (cryptic ? "SOLVE: " : "FIND: ") +
                string.Join("  •  ", wantedTargets.Select(t =>
                    (foundTargets.Contains(t.Name) ? "[FOUND] " : string.Empty) +
                    (cryptic ? t.Clue : t.Name)));
        }

        private void UpdateHud()
        {
            if (!gameRoot.activeSelf)
                return;

            scoreText.text = "SCORE " + score.ToString("N0") + "  COMBO x" + (1f + Mathf.Max(0, combo - 1) * .18f).ToString("0.00");

            int seconds = Mathf.CeilToInt(timeRemaining);
            timeText.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
            hintText.text = "HINT x" + hints;
        }

        private GameObject CreateRoot(string name)
        {
            return CreateRect(name, canvas.transform, Vector2.zero, Vector2.one).gameObject;
        }

        private RectTransform CreateRect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private Sprite MakePixelSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return CreateSprite(texture);
        }

        private static Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100f);
        }

        private Image CreateImage(Transform parent, Color color)
        {
            GameObject gameObject = new GameObject("Image", typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);

            Image image = gameObject.GetComponent<Image>();
            image.sprite = pixelSprite;
            image.color = color;
            return image;
        }

        private GameObject CreatePanel(Transform parent, Vector2 min, Vector2 max)
        {
            Image border = CreateImage(parent, gold);
            border.rectTransform.anchorMin = min;
            border.rectTransform.anchorMax = max;
            border.rectTransform.offsetMin = Vector2.zero;
            border.rectTransform.offsetMax = Vector2.zero;

            Image inner = CreateImage(border.transform, panel);
            Stretch(inner.rectTransform);
            inner.rectTransform.offsetMin = new Vector2(3f, 3f);
            inner.rectTransform.offsetMax = new Vector2(-3f, -3f);
            return inner.gameObject;
        }

        private Text CreateText(Transform parent, string value, int size, Color color, Vector2 min, Vector2 max)
        {
            GameObject gameObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parent, false);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = gameObject.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 min, Vector2 max, Action action, out Text labelText)
        {
            Image image = CreateImage(parent, green);
            image.rectTransform.anchorMin = min;
            image.rectTransform.anchorMax = max;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => action());

            labelText = CreateText(image.transform, label, 19, cream, new Vector2(.03f, .05f), new Vector2(.97f, .95f));
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private Texture2D CreateRoomArt(bool tavern)
        {
            const int width = 320;
            const int height = 180;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Color32[] pixels = new Color32[width * height];

            Color32 outline = new Color32(19, 14, 18, 255);
            Color32 stone = new Color32(42, 38, 59, 255);
            Color32 stone2 = new Color32(62, 50, 74, 255);
            Color32 wood = new Color32(91, 53, 31, 255);
            Color32 wood2 = new Color32(125, 72, 38, 255);
            Color32 artGold = new Color32(211, 151, 47, 255);
            Color32 purple = new Color32(108, 56, 145, 255);
            Color32 blue = new Color32(46, 96, 131, 255);
            Color32 artGreen = new Color32(56, 109, 69, 255);
            Color32 red = new Color32(150, 56, 50, 255);
            Color32 artCream = new Color32(220, 198, 150, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = tavern ? new Color32(46, 31, 31, 255) : stone;
            }

            Action<int, int, int, int, Color32> fillRect = (x, y, w, h, color) =>
            {
                for (int yy = Mathf.Max(0, y); yy < Mathf.Min(height, y + h); yy++)
                {
                    for (int xx = Mathf.Max(0, x); xx < Mathf.Min(width, x + w); xx++)
                        pixels[yy * width + xx] = color;
                }
            };

            Action<int, int, int, int, Color32> outlinedRect = (x, y, w, h, color) =>
            {
                fillRect(x - 1, y - 1, w + 2, h + 2, outline);
                fillRect(x, y, w, h, color);
            };

            for (int y = 0; y < 120; y += 12)
            {
                for (int x = (y / 12 % 2) * 8; x < width; x += 24)
                {
                    fillRect(x, y, 22, 10, ((x + y) / 12) % 2 == 0 ? stone : stone2);
                    fillRect(x, y + 9, 22, 1, outline);
                }
            }

            fillRect(0, 120, width, 60, tavern ? new Color32(68, 40, 27, 255) : new Color32(49, 31, 33, 255));
            for (int x = 0; x < width; x += 18)
                fillRect(x, 120, 2, 60, outline);

            outlinedRect(125, 15, 70, 48, new Color32(25, 43, 62, 255));
            fillRect(129, 19, 62, 40, new Color32(42, 75, 99, 255));
            fillRect(159, 19, 3, 40, outline);
            fillRect(129, 38, 62, 3, outline);

            for (int shelfY = 64; shelfY < 116; shelfY += 23)
            {
                outlinedRect(15, shelfY, 115, 5, wood2);
                outlinedRect(205, shelfY, 100, 5, wood2);

                for (int i = 0; i < 9; i++)
                {
                    int x = 22 + i * 11;
                    int h = 8 + i % 4 * 3;
                    outlinedRect(x, shelfY - h, 6, h, i % 3 == 0 ? artGreen : i % 3 == 1 ? purple : blue);
                    outlinedRect(212 + i * 10, shelfY - 10 - i % 3 * 3, 6, 10 + i % 3 * 3, i % 2 == 0 ? purple : artGold);
                }
            }

            outlinedRect(68, 128, 186, 34, wood);
            fillRect(74, 134, 174, 22, wood2);
            outlinedRect(146, 119, 28, 19, new Color32(45, 45, 50, 255));
            fillRect(151, 113, 18, 7, new Color32(70, 63, 75, 255));
            fillRect(155, 109, 10, 4, purple);

            outlinedRect(28, 48, 13, 5, artGold);
            outlinedRect(89, 82, 13, 11, artCream);
            outlinedRect(224, 45, 7, 14, artGold);
            outlinedRect(162, 139, 11, 8, purple);
            outlinedRect(249, 94, 6, 18, artCream);
            outlinedRect(103, 149, 19, 5, artCream);
            outlinedRect(270, 141, 16, 4, new Color32(164, 168, 177, 255));
            outlinedRect(69, 143, 7, 7, artGold);
            outlinedRect(52, 108, 12, 10, red);
            outlinedRect(287, 86, 8, 12, artGold);
            outlinedRect(141, 100, 8, 7, artGold);
            outlinedRect(187, 97, 18, 11, new Color32(52, 45, 41, 255));
            outlinedRect(10, 86, 10, 18, artGold);
            outlinedRect(233, 149, 9, 8, red);
            outlinedRect(275, 31, 18, 12, artGold);

            for (int i = 0; i < 110; i++)
                fillRect((i * 47 + 13) % width, (i * 29 + 7) % 120, 1, 1, i % 2 == 0 ? new Color32(88, 76, 98, 255) : new Color32(35, 31, 48, 255));

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}
