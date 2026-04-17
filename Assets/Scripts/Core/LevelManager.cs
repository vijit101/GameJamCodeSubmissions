using System.Collections;
using SpaceLoader.Environment;
using SpaceLoader.Mechanics;
using SpaceLoader.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceLoader.Core
{
    [DisallowMultipleComponent]
    public sealed class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        private Camera sceneCamera;
        private CameraFollow cameraFollow;
        private UIManager uiManager;
        private GameplayFeedbackService feedbackService;
        private PlayerController player;
        private Rigidbody2D playerRb;
        private Transform levelRoot;
        private RoomDefinition[] roomDefinitions;
        private int currentLevelIndex;
        private bool isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            roomDefinitions = CreateRoomDefinitions();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            BuildLevel(currentLevelIndex);
        }

        private void Update()
        {
            if (!Debug.isDebugBuild)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R) && !isTransitioning)
            {
                RestartLevel();
            }
        }

        public void RestartLevel()
        {
            BuildLevel(currentLevelIndex);
        }

        public void CompleteRoom(Vector2 goalPosition)
        {
            if (!isTransitioning)
            {
                StartCoroutine(CompleteRoomRoutine(goalPosition));
            }
        }

        private IEnumerator CompleteRoomRoutine(Vector2 goalPosition)
        {
            isTransitioning = true;

            RoomDefinition room = roomDefinitions[currentLevelIndex];
            feedbackService?.PlayRoomComplete(goalPosition, room.AccentColor);

            yield return new WaitForSeconds(0.55f);

            if (currentLevelIndex < roomDefinitions.Length - 1)
            {
                currentLevelIndex++;
                BuildLevel(currentLevelIndex);
            }
            else
            {
                currentLevelIndex = 0;
                BuildLevel(currentLevelIndex);
                uiManager?.ShowBanner("Prototype Clear", PresentationTheme.GoalGold, 1.4f);
                uiManager?.SetTutorialMessage("All five rooms cleared. Press Space with purpose.");
            }

            isTransitioning = false;
        }

        private void BuildLevel(int levelIndex)
        {
            EnsureSystems();
            DestroyExistingLevel();

            RoomDefinition room = roomDefinitions[levelIndex];

            levelRoot = new GameObject($"Room_{levelIndex + 1}").transform;
            levelRoot.SetParent(transform, false);

            CreateBackdrop(levelRoot, room);
            CreateRoomBounds(levelRoot, room);
            SpawnPlayer(room.SpawnPoint);

            switch (levelIndex)
            {
                case 0:
                    BuildDashRoom(levelRoot);
                    break;
                case 1:
                    BuildSpringRoom(levelRoot);
                    break;
                case 2:
                    BuildSmashRoom(levelRoot);
                    break;
                case 3:
                    BuildSwapRoom(levelRoot);
                    break;
                case 4:
                    BuildFinalRoom(levelRoot);
                    break;
            }

            player.ResetActionInventory();
            uiManager.SetRoomTitle(room.Title, room.AccentColor);
            uiManager.SetTutorialMessage(room.Tutorial);
            cameraFollow.SetTarget(player.transform);
            cameraFollow.ConfigureRoom(room.CameraBounds, room.ObjectivePoint, room.OrthographicSize);
        }

        private void EnsureSystems()
        {
            EnsureCamera();
            EnsureFeedback();
            EnsureUi();
        }

        private void EnsureCamera()
        {
            if (sceneCamera == null)
            {
                sceneCamera = Camera.main;

                if (sceneCamera == null)
                {
                    GameObject cameraObject = new GameObject("Main Camera");
                    cameraObject.tag = "MainCamera";
                    sceneCamera = cameraObject.AddComponent<Camera>();
                    cameraObject.AddComponent<AudioListener>();
                }
            }

            sceneCamera.orthographic = true;
            sceneCamera.backgroundColor = PresentationTheme.BackgroundNavy;

            if (!sceneCamera.TryGetComponent(out cameraFollow))
            {
                cameraFollow = sceneCamera.gameObject.AddComponent<CameraFollow>();
            }
        }

        private void EnsureFeedback()
        {
            if (!TryGetComponent(out feedbackService))
            {
                feedbackService = gameObject.AddComponent<GameplayFeedbackService>();
            }

            feedbackService.SetCamera(cameraFollow);
        }

        private void EnsureUi()
        {
            if (uiManager != null)
            {
                return;
            }

            Font font = TryGetBuiltInFont();

            GameObject canvasObject = new GameObject("HUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            uiManager = canvasObject.AddComponent<UIManager>();

            Image roomPanel = CreatePanel("RoomTitlePanel", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(340f, 58f), PresentationTheme.HudPanel);
            Text roomTitle = CreateText("RoomTitle", roomPanel.transform, font, Vector2.zero, new Vector2(300f, 40f), TextAnchor.MiddleCenter, PresentationTheme.RoomTitleFontSize, PresentationTheme.HudText);

            Image actionPanel = CreatePanel("ActionPanel", canvas.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(372f, 128f), PresentationTheme.HudPanelSoft);
            Text actionLabel = CreateText("ActionLabel", actionPanel.transform, font, new Vector2(18f, -14f), new Vector2(320f, 28f), TextAnchor.UpperLeft, PresentationTheme.ActionLabelFontSize, PresentationTheme.HudText);
            Text actionDescription = CreateText("ActionDescription", actionPanel.transform, font, new Vector2(18f, -46f), new Vector2(332f, 70f), TextAnchor.UpperLeft, PresentationTheme.ActionDescriptionFontSize, PresentationTheme.HudSubtext);

            Image tutorialPanel = CreatePanel("TutorialPanel", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(640f, 90f), PresentationTheme.TutorialPanel);
            Text tutorialText = CreateText("TutorialText", tutorialPanel.transform, font, new Vector2(0f, 0f), new Vector2(580f, 56f), TextAnchor.MiddleCenter, PresentationTheme.TutorialFontSize, PresentationTheme.HudText);

            Image bannerPanel = CreatePanel("BannerPanel", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(280f, 48f), PresentationTheme.GoalGold);
            Text bannerText = CreateText("BannerText", bannerPanel.transform, font, Vector2.zero, new Vector2(240f, 34f), TextAnchor.MiddleCenter, PresentationTheme.BannerFontSize, PresentationTheme.HudText);
            bannerPanel.gameObject.SetActive(false);

            uiManager.BindWidgets(roomTitle, actionLabel, actionDescription, tutorialText, bannerText, roomPanel, actionPanel, tutorialPanel, bannerPanel);
        }

        private void DestroyExistingLevel()
        {
            if (levelRoot != null)
            {
                Destroy(levelRoot.gameObject);
                levelRoot = null;
            }
        }

        private void SpawnPlayer(Vector2 spawnPoint)
        {
            if (player == null)
            {
                GameObject playerObject = new GameObject("Player");
                SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
                renderer.sprite = GeneratedSpriteLibrary.CircleSprite;
                renderer.color = Color.white;
                renderer.sortingOrder = 20;

                CircleCollider2D collider = playerObject.AddComponent<CircleCollider2D>();
                collider.radius = 0.48f;

                playerRb = playerObject.AddComponent<Rigidbody2D>();
                playerRb.mass = 1f;
                playerRb.gravityScale = 3f;
                playerRb.linearDamping = 0.55f;
                playerRb.angularDamping = 0.45f;
                playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                player = playerObject.AddComponent<PlayerController>();
            }

            player.transform.SetPositionAndRotation(spawnPoint, Quaternion.identity);
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        private void BuildDashRoom(Transform parent)
        {
            CreateSolid(parent, "StartPlatform", new Vector2(-7.2f, -3.9f), new Vector2(4.5f, 1f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "LandingPlatform", new Vector2(2.9f, -4.35f), new Vector2(10.4f, 1f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "GoalPlatform", new Vector2(8.75f, -4.35f), new Vector2(2.7f, 1f), PresentationTheme.PlatformSlate);
            CreatePickup<ActionDash>(parent, new Vector2(-7.2f, -2.8f), PresentationTheme.DashBlue);
            CreateGoal(parent, new Vector2(9.4f, -2.95f), new Vector2(1.1f, 2.2f));
        }

        private void BuildSpringRoom(Transform parent)
        {
            CreateSolid(parent, "StartPlatform", new Vector2(-7.4f, -3.9f), new Vector2(4.5f, 1f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "MiddleLedge", new Vector2(-1.1f, -1.3f), new Vector2(3.6f, 1f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "HighLedge", new Vector2(6.5f, 1.4f), new Vector2(5f, 1f), PresentationTheme.PlatformSlate);
            CreatePickup<ActionSpring>(parent, new Vector2(-7.4f, -2.8f), PresentationTheme.SpringGreen);
            CreateGoal(parent, new Vector2(8.6f, 2.8f), new Vector2(1.1f, 2.4f));
        }

        private void BuildSmashRoom(Transform parent)
        {
            CreateSolid(parent, "StartPlatform", new Vector2(-5.2f, 4.1f), new Vector2(4.2f, 1f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "SetupRunway", new Vector2(-1.35f, 4.1f), new Vector2(3.4f, 1f), PresentationTheme.PlatformSlate);
            CreateBreakable(parent, new Vector2(3.1f, 1.15f), new Vector2(2.4f, 0.6f));
            CreateBreakable(parent, new Vector2(3.1f, -0.55f), new Vector2(2.4f, 0.6f));
            CreateSolid(parent, "GoalFloor", new Vector2(3.95f, -4.15f), new Vector2(5.4f, 0.8f), PresentationTheme.PlatformSlate);

            CreateHazard(parent, new Vector2(-0.9f, 0.1f), new Vector2(3.6f, 1f));

            CreateWall(parent, new Vector2(-6.8f, -0.2f), new Vector2(0.8f, 10f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(6.8f, -0.2f), new Vector2(0.8f, 10f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(1.45f, 1.5f), new Vector2(0.8f, 4.5f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(5.1f, 0.15f), new Vector2(0.8f, 5.6f), PresentationTheme.PlatformSlate);

            CreatePickup<ActionSmash>(parent, new Vector2(-5.2f, 5.15f), PresentationTheme.SmashRed);
            CreateGoal(parent, new Vector2(5.75f, -2.85f), new Vector2(1.1f, 2.3f));
        }

        private void BuildSwapRoom(Transform parent)
        {
            CreateSolid(parent, "StartPlatform", new Vector2(-8.1f, -3.9f), new Vector2(4.2f, 1f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "DashPickupLedge", new Vector2(-1.35f, -0.9f), new Vector2(3.4f, 0.8f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "DashRunway", new Vector2(1.9f, -0.95f), new Vector2(2.2f, 0.7f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(3.55f, -1.1f), new Vector2(0.8f, 3.8f), PresentationTheme.PlatformSlate);

            CreateSolid(parent, "GoalPlatform", new Vector2(6.75f, -2.25f), new Vector2(3.4f, 0.9f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "GoalBackstop", new Vector2(8.5f, -0.95f), new Vector2(0.8f, 3.6f), PresentationTheme.PlatformSlate);
            CreateHazard(parent, new Vector2(0.35f, -4.2f), new Vector2(8.8f, 0.45f));

            CreatePickup<ActionSpring>(parent, new Vector2(-7.2f, -2.8f), PresentationTheme.SpringGreen);
            CreatePickup<ActionDash>(parent, new Vector2(-1.35f, 0.15f), PresentationTheme.DashBlue);
            CreateGoal(parent, new Vector2(7.95f, -0.95f), new Vector2(1.1f, 2.4f));
        }

        private void BuildFinalRoom(Transform parent)
        {
            CreateSolid(parent, "StartPlatform", new Vector2(-11.1f, -3.9f), new Vector2(3.8f, 1f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(-8.65f, -2.55f), new Vector2(0.8f, 3f), PresentationTheme.PlatformSlate);

            CreateSolid(parent, "SpringCatch", new Vector2(-6.8f, -2.15f), new Vector2(2.6f, 0.7f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "UpperRunway", new Vector2(-3.35f, -1.15f), new Vector2(4.2f, 0.8f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "SmashSetup", new Vector2(2.7f, -1.15f), new Vector2(2.8f, 0.8f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(4.55f, -0.7f), new Vector2(0.8f, 1.4f), PresentationTheme.PlatformSlate);

            CreateSolid(parent, "LowerCatch", new Vector2(2.7f, -4.2f), new Vector2(3.6f, 0.9f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "FinalJumpPad", new Vector2(6.35f, -4.2f), new Vector2(2.8f, 0.9f), PresentationTheme.PlatformSlate);
            CreateHazard(parent, new Vector2(8.65f, -4.2f), new Vector2(1.4f, 0.45f));
            CreateWall(parent, new Vector2(8.85f, -3.05f), new Vector2(0.8f, 2f), PresentationTheme.PlatformSlate);
            CreateSolid(parent, "GoalLedge", new Vector2(10.75f, -2.35f), new Vector2(3.2f, 0.9f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(12.95f, -1.15f), new Vector2(0.8f, 4.4f), PresentationTheme.PlatformSlate);

            CreatePickup<ActionSpring>(parent, new Vector2(-10.45f, -2.8f), PresentationTheme.SpringGreen);
            CreatePickup<ActionDash>(parent, new Vector2(-3.35f, -0.2f), PresentationTheme.DashBlue);
            CreatePickup<ActionSmash>(parent, new Vector2(2.7f, -0.2f), PresentationTheme.SmashRed);
            CreatePickup<ActionSpring>(parent, new Vector2(6.35f, -3.25f), PresentationTheme.SpringGreen);

            CreateGoal(parent, new Vector2(11.75f, -1f), new Vector2(1.1f, 2.4f));
        }

        private void CreateBackdrop(Transform parent, RoomDefinition room)
        {
            CreateVisual(parent, "Backdrop", room.RoomCenter, room.RoomSize + new Vector2(2f, 2f), PresentationTheme.BackgroundNavy, 0);
        }

        private void CreateRoomBounds(Transform parent, RoomDefinition room)
        {
            Rect bounds = room.CameraBounds;
            CreateWall(parent, new Vector2(bounds.xMin - 0.5f, room.RoomCenter.y), new Vector2(1f, room.RoomSize.y + 2f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(bounds.xMax + 0.5f, room.RoomCenter.y), new Vector2(1f, room.RoomSize.y + 2f), PresentationTheme.PlatformSlate);
            CreateWall(parent, new Vector2(room.RoomCenter.x, bounds.yMax + 0.5f), new Vector2(room.RoomSize.x + 3f, 1f), PresentationTheme.PlatformSlate);
            CreateHazard(parent, new Vector2(room.RoomCenter.x, bounds.yMin - 0.35f), new Vector2(room.RoomSize.x + 3f, 0.8f));
        }

        private GameObject CreateSolid(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject block = CreateVisual(parent, name, position, size, color, 4);
            block.AddComponent<BoxCollider2D>().size = Vector2.one;

            GameObject highlight = CreateVisual(block.transform, "Highlight", new Vector2(0f, 0.38f), new Vector2(1f, 0.12f), PresentationTheme.PlatformHighlight, 5);
            highlight.GetComponent<SpriteRenderer>().sprite = GeneratedSpriteLibrary.SquareSprite;
            return block;
        }

        private void CreateWall(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            CreateSolid(parent, "Wall", position, size, color);
        }

        private GameObject CreateBreakable(Transform parent, Vector2 position, Vector2 size)
        {
            GameObject block = CreateSolid(parent, "Breakable", position, size, PresentationTheme.BreakableAmber);
            block.AddComponent<BreakableBlock>();

            CreateVisual(block.transform, "CrackLineA", new Vector2(-0.18f, 0.05f), new Vector2(0.1f, 0.7f), new Color(0.38f, 0.2f, 0.08f, 0.8f), 6);
            CreateVisual(block.transform, "CrackLineB", new Vector2(0.12f, -0.05f), new Vector2(0.1f, 0.8f), new Color(0.38f, 0.2f, 0.08f, 0.8f), 6);
            return block;
        }

        private GameObject CreateHazard(Transform parent, Vector2 position, Vector2 size)
        {
            GameObject hazard = CreateVisual(parent, "Hazard", position, size, PresentationTheme.HazardRed, 6);
            BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            hazard.AddComponent<Hazard>();

            for (int i = -2; i <= 2; i++)
            {
                CreateVisual(
                    hazard.transform,
                    $"Stripe_{i}",
                    new Vector2(i * 0.22f, 0f),
                    new Vector2(0.08f, 1.1f),
                    new Color(0.45f, 0.03f, 0.03f, 0.85f),
                    7);
            }

            return hazard;
        }

        private void CreateGoal(Transform parent, Vector2 position, Vector2 size)
        {
            GameObject goal = CreateVisual(parent, "Goal", position, size, PresentationTheme.GoalGold, 8);
            goal.transform.localScale = new Vector3(size.x * 1.12f, size.y * 1.18f, 1f);
            BoxCollider2D collider = goal.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            goal.AddComponent<GoalDoor>();

            GameObject glow = CreateVisual(goal.transform, "GoalGlow", Vector2.zero, new Vector2(1.5f, 1.24f), new Color(PresentationTheme.GoalGold.r, PresentationTheme.GoalGold.g, PresentationTheme.GoalGold.b, 0.22f), 7);
            glow.GetComponent<SpriteRenderer>().sprite = GeneratedSpriteLibrary.CircleSprite;
            glow.AddComponent<PresentationMotion>();

            CreateVisual(goal.transform, "GoalBeacon", new Vector2(0f, 1.8f), new Vector2(0.28f, 1.8f), new Color(1f, 0.98f, 0.72f, 0.95f), 9);
            CreateVisual(goal.transform, "GoalMarker", new Vector2(0f, 3f), new Vector2(1.25f, 0.5f), new Color(1f, 0.93f, 0.4f, 1f), 10);
        }

        private void CreatePickup<TAction>(Transform parent, Vector2 position, Color color) where TAction : MonoBehaviour, IActionParameter
        {
            GameObject pickup = CreateVisual(parent, typeof(TAction).Name, position, new Vector2(0.72f, 0.72f), color, 9);
            pickup.GetComponent<SpriteRenderer>().sprite = GeneratedSpriteLibrary.CircleSprite;
            pickup.AddComponent<CircleCollider2D>().isTrigger = true;
            pickup.AddComponent<TAction>();
            pickup.AddComponent<ParameterPickup>();
            pickup.AddComponent<PresentationMotion>();

            GameObject glow = CreateVisual(pickup.transform, "Glow", Vector2.zero, new Vector2(1.18f, 1.18f), new Color(color.r, color.g, color.b, 0.18f), 8);
            glow.GetComponent<SpriteRenderer>().sprite = GeneratedSpriteLibrary.CircleSprite;
            glow.AddComponent<PresentationMotion>();

            CreateActionSymbol(pickup.transform, typeof(TAction), color);
        }

        private void CreateActionSymbol(Transform parent, System.Type actionType, Color color)
        {
            Color symbolColor = Color.Lerp(color, Color.white, 0.5f);

            if (actionType == typeof(ActionDash))
            {
                CreateVisual(parent, "DashBody", new Vector2(-0.05f, 0f), new Vector2(0.34f, 0.08f), symbolColor, 10);
                CreateVisual(parent, "DashHead", new Vector2(0.14f, 0f), new Vector2(0.12f, 0.18f), symbolColor, 10);
            }
            else if (actionType == typeof(ActionSpring))
            {
                CreateVisual(parent, "SpringStem", new Vector2(0f, -0.06f), new Vector2(0.08f, 0.26f), symbolColor, 10);
                CreateVisual(parent, "SpringHead", new Vector2(0f, 0.1f), new Vector2(0.22f, 0.12f), symbolColor, 10);
            }
            else if (actionType == typeof(ActionSmash))
            {
                CreateVisual(parent, "SmashStem", new Vector2(0f, 0.04f), new Vector2(0.08f, 0.24f), symbolColor, 10);
                CreateVisual(parent, "SmashHead", new Vector2(0f, -0.12f), new Vector2(0.22f, 0.12f), symbolColor, 10);
            }
        }

        private void DecorateConveyor(Transform conveyor)
        {
            for (int i = -2; i <= 2; i++)
            {
                CreateVisual(
                    conveyor,
                    $"ConveyorStripe_{i}",
                    new Vector2(i * 0.18f, 0f),
                    new Vector2(0.08f, 1f),
                    new Color(0.72f, 0.92f, 1f, 0.75f),
                    7);
            }
        }

        private static GameObject CreateVisual(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.SquareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return visual;
        }

        private static Image CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = panelObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Font font,
            Vector2 anchoredPosition,
            Vector2 size,
            TextAnchor alignment,
            int fontSize,
            Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            bool leftAligned = alignment == TextAnchor.UpperLeft || alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.LowerLeft;
            bool topAligned = alignment == TextAnchor.UpperLeft || alignment == TextAnchor.UpperCenter || alignment == TextAnchor.UpperRight;

            rectTransform.anchorMin = new Vector2(leftAligned ? 0f : 0.5f, topAligned ? 1f : 0.5f);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.pivot = new Vector2(leftAligned ? 0f : 0.5f, topAligned ? 1f : 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
            outline.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private static Font TryGetBuiltInFont()
        {
            Font font = null;

            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                // Fallback below.
            }

            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static RoomDefinition[] CreateRoomDefinitions()
        {
            return new[]
            {
                new RoomDefinition(
                    "Room 1  Dash",
                    "Grab the blue orb.\nDash across and land on the lower platform.",
                    new Vector2(-9.8f, -2.8f),
                    new Vector2(0f, -1.2f),
                    new Vector2(24f, 12f),
                    new Vector2(9.4f, -2.95f),
                    5.1f,
                    PresentationTheme.DashBlue),
                new RoomDefinition(
                    "Room 2  Spring",
                    "Grab the green orb.\nUse Space to reach the high ledge.",
                    new Vector2(-9.8f, -2.8f),
                    new Vector2(0f, -0.8f),
                    new Vector2(24f, 12f),
                    new Vector2(8.6f, 2.8f),
                    5.2f,
                    PresentationTheme.SpringGreen),
                new RoomDefinition(
                    "Room 3  Smash",
                    "Carry the red orb to the weak floor stack.\nSmash through both floors to reach the goal door.",
                    new Vector2(-5.2f, 5.15f),
                    new Vector2(0.2f, -0.35f),
                    new Vector2(20f, 13f),
                    new Vector2(5.75f, -2.85f),
                    5.4f,
                    PresentationTheme.SmashRed),
                new RoomDefinition(
                    "Room 4  Swap",
                    "Grab Spring first.\nUse it to reach Dash, then dash to the goal platform.",
                    new Vector2(-10.2f, -2.8f),
                    new Vector2(0.4f, -1.35f),
                    new Vector2(24f, 12f),
                    new Vector2(7.95f, -0.95f),
                    5.2f,
                    PresentationTheme.DashBlue),
                new RoomDefinition(
                    "Room 5  Final Run",
                    "Spring to the upper runway.\nDash across, drop to the lower lane, grab Spring again, then clear the last blocker.",
                    new Vector2(-11.2f, -2.8f),
                    new Vector2(0.3f, -1.9f),
                    new Vector2(26f, 13f),
                    new Vector2(11.75f, -1f),
                    5.5f,
                    PresentationTheme.GoalGold)
            };
        }
    }
}
