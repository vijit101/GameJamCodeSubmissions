using SpaceLoader.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceLoader.UI
{
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private Text actionText;
        [SerializeField] private Text actionDescriptionText;
        [SerializeField] private Text roomTitleText;
        [SerializeField] private Text tutorialText;
        [SerializeField] private Text bannerText;
        [SerializeField] private Image actionPanel;
        [SerializeField] private Image roomTitlePanel;
        [SerializeField] private Image tutorialPanel;
        [SerializeField] private Image bannerPanel;

        private string currentActionName;
        private string currentActionDescription;
        private string currentChargeSummary;
        private string currentUnlockedSummary;
        private Color currentAccentColor;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SetCurrentAction(null, null, PresentationTheme.HudSubtext);
            SetMechanicStatus(null, null);
            SetRoomTitle(string.Empty, PresentationTheme.HudPanelSoft);
            SetTutorialMessage("Use Arrow Keys to move.\nPress Space to use the selected mechanic.");
            HideBannerImmediate();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetCurrentAction(string actionName, string description, Color accentColor)
        {
            currentActionName = actionName;
            currentActionDescription = description;
            currentAccentColor = accentColor;
            RefreshActionPanel();
        }

        public void SetMechanicStatus(string chargeSummary, string unlockedSummary)
        {
            currentChargeSummary = chargeSummary;
            currentUnlockedSummary = unlockedSummary;
            RefreshActionPanel();
        }

        public void SetMechanicCharges(string chargeSummary)
        {
            currentChargeSummary = chargeSummary;
            RefreshActionPanel();
        }

        public void SetUnlockedMechanics(string unlockedSummary)
        {
            currentUnlockedSummary = unlockedSummary;
            RefreshActionPanel();
        }

        private void RefreshActionPanel()
        {
            if (actionText == null)
            {
                return;
            }

            actionText.text = PresentationTheme.FormatActionLabel(currentActionName);
            actionText.color = PresentationTheme.HudText;

            if (actionDescriptionText != null)
            {
                actionDescriptionText.text = BuildActionDetails();
                actionDescriptionText.color = PresentationTheme.HudSubtext;
            }

            if (actionPanel != null)
            {
                Color accent = currentActionName == null ? PresentationTheme.HudSubtext : currentAccentColor;
                actionPanel.color = Color.Lerp(PresentationTheme.HudPanelSoft, accent, 0.18f);
                actionPanel.color = new Color(actionPanel.color.r, actionPanel.color.g, actionPanel.color.b, 0.9f);
            }
        }

        public void SetRoomTitle(string title, Color accentColor)
        {
            if (roomTitleText != null)
            {
                roomTitleText.text = string.IsNullOrWhiteSpace(title) ? string.Empty : title;
                roomTitleText.color = PresentationTheme.HudText;
            }

            if (roomTitlePanel != null)
            {
                roomTitlePanel.color = Color.Lerp(PresentationTheme.HudPanel, accentColor, 0.15f);
                roomTitlePanel.color = new Color(roomTitlePanel.color.r, roomTitlePanel.color.g, roomTitlePanel.color.b, 0.92f);
            }
        }

        public void SetTutorialMessage(string message)
        {
            if (tutorialText != null)
            {
                tutorialText.text = message;
                tutorialText.color = PresentationTheme.HudText;
            }
        }

        public void ShowBanner(string message, Color accentColor, float duration)
        {
            if (bannerText == null || bannerPanel == null)
            {
                return;
            }

            StopAllCoroutines();
            bannerText.text = message;
            bannerText.color = PresentationTheme.HudText;
            bannerPanel.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.9f);
            bannerPanel.gameObject.SetActive(true);
            StartCoroutine(HideBannerAfter(duration));
        }

        public void PulseAction(Color accentColor)
        {
            if (actionPanel == null)
            {
                return;
            }

            StopCoroutine(nameof(PulseActionRoutine));
            StartCoroutine(PulseActionRoutine(accentColor));
        }

        public void BindWidgets(
            Text roomTitle,
            Text actionLabel,
            Text actionDescription,
            Text tutorialLabel,
            Text bannerLabel,
            Image roomPanelImage,
            Image actionPanelImage,
            Image tutorialPanelImage,
            Image bannerPanelImage)
        {
            roomTitleText = roomTitle;
            actionText = actionLabel;
            actionDescriptionText = actionDescription;
            tutorialText = tutorialLabel;
            bannerText = bannerLabel;
            roomTitlePanel = roomPanelImage;
            actionPanel = actionPanelImage;
            tutorialPanel = tutorialPanelImage;
            bannerPanel = bannerPanelImage;

            SetCurrentAction(null, null, PresentationTheme.HudSubtext);
            SetMechanicStatus(null, null);
            SetRoomTitle(string.Empty, PresentationTheme.HudPanelSoft);
            SetTutorialMessage("Use Arrow Keys to move.\nPress Space to use the selected mechanic.");
            HideBannerImmediate();
        }

        private System.Collections.IEnumerator HideBannerAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            HideBannerImmediate();
        }

        private void HideBannerImmediate()
        {
            if (bannerPanel != null)
            {
                bannerPanel.gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator PulseActionRoutine(Color accentColor)
        {
            if (actionPanel == null)
            {
                yield break;
            }

            Color original = actionPanel.color;
            Color boosted = Color.Lerp(original, accentColor, 0.45f);
            boosted.a = 0.96f;
            float elapsed = 0f;

            while (elapsed < 0.18f)
            {
                elapsed += Time.deltaTime;
                actionPanel.color = Color.Lerp(boosted, original, elapsed / 0.18f);
                yield return null;
            }

            actionPanel.color = original;
        }

        private string BuildActionDetails()
        {
            string description = string.IsNullOrWhiteSpace(currentActionDescription)
                ? "Collect an orb to unlock mechanics. Up/Down switches."
                : currentActionDescription.Trim();

            return string.Join(
                "\n",
                description,
                PresentationTheme.FormatChargeLabel(currentChargeSummary),
                PresentationTheme.FormatInventoryLabel(currentUnlockedSummary));
        }
    }
}
