using UnityEngine;

namespace SpaceLoader.Core
{
    public static class PresentationTheme
    {
        public static readonly Color BackgroundNavy = new Color(0.07f, 0.09f, 0.14f);
        public static readonly Color PlatformSlate = new Color(0.23f, 0.28f, 0.41f);
        public static readonly Color PlatformHighlight = new Color(0.35f, 0.42f, 0.62f);
        public static readonly Color SpringGreen = new Color(0.38f, 0.97f, 0.58f);
        public static readonly Color DashBlue = new Color(0.29f, 0.76f, 1f);
        public static readonly Color SmashRed = new Color(1f, 0.37f, 0.34f);
        public static readonly Color HazardRed = new Color(1f, 0.29f, 0.31f);
        public static readonly Color GoalGold = new Color(1f, 0.86f, 0.28f);
        public static readonly Color BreakableAmber = new Color(0.82f, 0.61f, 0.23f);
        public static readonly Color ConveyorBlue = new Color(0.22f, 0.48f, 0.78f);
        public static readonly Color HudPanel = new Color(0.05f, 0.08f, 0.12f, 0.9f);
        public static readonly Color HudPanelSoft = new Color(0.08f, 0.11f, 0.17f, 0.8f);
        public static readonly Color HudText = new Color(0.95f, 0.98f, 1f);
        public static readonly Color HudSubtext = new Color(0.74f, 0.83f, 0.94f);
        public static readonly Color TutorialPanel = new Color(0.04f, 0.05f, 0.08f, 0.88f);
        public static readonly Color SuccessMint = new Color(0.73f, 1f, 0.85f);
        public static readonly Color FailureOrange = new Color(1f, 0.72f, 0.32f);

        public const float CameraSmoothTime = 0.22f;
        public const float CameraEdgePadding = 1.15f;
        public const float CameraObjectivePull = 3.6f;
        public const float CameraFocusZoneRadius = 4.5f;
        public const float CameraTransitionSizeSpeed = 4f;

        public const int RoomTitleFontSize = 28;
        public const int ActionLabelFontSize = 24;
        public const int ActionDescriptionFontSize = 18;
        public const int TutorialFontSize = 24;
        public const int BannerFontSize = 28;

        public static string FormatActionLabel(string actionName)
        {
            string normalized = string.IsNullOrWhiteSpace(actionName) ? "None" : actionName.Trim();
            return $"Selected Mechanic: {normalized}";
        }

        public static string FormatChargeLabel(string chargeSummary)
        {
            return string.IsNullOrWhiteSpace(chargeSummary) ? "Charges: --" : $"Charges: {chargeSummary.Trim()}";
        }

        public static string FormatInventoryLabel(string unlockedSummary)
        {
            return string.IsNullOrWhiteSpace(unlockedSummary) ? "Unlocked: none" : $"Unlocked: {unlockedSummary.Trim()}";
        }
    }
}
