using System;
using System.Collections.Generic;
using SpaceLoader.UI;
using UnityEngine;

namespace SpaceLoader.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        private sealed class ActionInventoryEntry
        {
            public ActionInventoryEntry(IActionParameter action, int charges)
            {
                Action = action;
                Charges = charges;
            }

            public IActionParameter Action { get; }
            public int Charges { get; set; }
        }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float moveAcceleration = 35f;

        private readonly List<ActionInventoryEntry> unlockedActions = new List<ActionInventoryEntry>();
        private Rigidbody2D rb;
        private IActionParameter currentAction;
        private float horizontalInput;
        private float horizontalControlLockTimer;
        private int selectedActionIndex = -1;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            UpdateActionText();
        }

        private void Update()
        {
            horizontalInput = ResolveHorizontalInput();
            horizontalControlLockTimer = Mathf.Max(0f, horizontalControlLockTimer - Time.deltaTime);
            HandleActionCycling();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryUseSelectedAction();
            }
        }

        private void FixedUpdate()
        {
            if (horizontalControlLockTimer > 0f)
            {
                return;
            }

            float targetVelocityX = horizontalInput * moveSpeed;
            float nextVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, moveAcceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(nextVelocityX, rb.linearVelocity.y);
        }

        public void SetActionParameter(IActionParameter newAction)
        {
            if (newAction == null)
            {
                ClearSelectedAction();
                return;
            }

            Type actionType = newAction.GetType();
            int existingIndex = FindUnlockedActionIndex(actionType);

            if (existingIndex >= 0)
            {
                unlockedActions[existingIndex].Charges = MechanicChargeConfig.DefaultChargesPerPickup;
                SetSelectedAction(existingIndex, true);
                return;
            }

            IActionParameter ownedAction = ResolveOwnedAction(actionType);

            if (ownedAction == null)
            {
                return;
            }

            unlockedActions.Add(new ActionInventoryEntry(ownedAction, MechanicChargeConfig.DefaultChargesPerPickup));
            SetSelectedAction(unlockedActions.Count - 1, true);
        }

        public void LockHorizontalControl(float duration)
        {
            horizontalControlLockTimer = Mathf.Max(horizontalControlLockTimer, duration);
        }

        public void ResetActionInventory()
        {
            unlockedActions.Clear();
            ClearSelectedAction();
        }

        private void HandleActionCycling()
        {
            if (unlockedActions.Count <= 1)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int nextIndex = FindNextSelectableActionIndex(-1);

                if (nextIndex >= 0)
                {
                    SetSelectedAction(nextIndex, true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int nextIndex = FindNextSelectableActionIndex(1);

                if (nextIndex >= 0)
                {
                    SetSelectedAction(nextIndex, true);
                }
            }
        }

        private void ClearSelectedAction()
        {
            currentAction = null;
            selectedActionIndex = -1;
            UpdateActionText();
        }

        private void SetSelectedAction(int index, bool playFeedback)
        {
            if (index < 0 || index >= unlockedActions.Count)
            {
                ClearSelectedAction();
                return;
            }

            selectedActionIndex = index;
            currentAction = unlockedActions[index].Action;
            UpdateActionText();

            if (playFeedback)
            {
                GameplayFeedbackService.Instance?.PlayActionSwap(
                    currentAction.GetActionName(),
                    currentAction.GetActionColor(),
                    unlockedActions[index].Charges,
                    MechanicChargeConfig.DefaultChargesPerPickup);
            }
        }

        private int FindUnlockedActionIndex(Type actionType)
        {
            for (int i = 0; i < unlockedActions.Count; i++)
            {
                if (unlockedActions[i].Action != null && unlockedActions[i].Action.GetType() == actionType)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindNextSelectableActionIndex(int direction)
        {
            if (unlockedActions.Count == 0)
            {
                return -1;
            }

            int currentIndex = selectedActionIndex < 0 ? (direction > 0 ? -1 : 0) : selectedActionIndex;

            for (int stepCount = 0; stepCount < unlockedActions.Count; stepCount++)
            {
                currentIndex = (currentIndex + direction + unlockedActions.Count) % unlockedActions.Count;

                if (unlockedActions[currentIndex].Charges > 0 && unlockedActions[currentIndex].Action != null)
                {
                    return currentIndex;
                }
            }

            return -1;
        }

        private ActionInventoryEntry GetSelectedEntry()
        {
            return selectedActionIndex >= 0 && selectedActionIndex < unlockedActions.Count
                ? unlockedActions[selectedActionIndex]
                : null;
        }

        private IActionParameter ResolveOwnedAction(Type actionType)
        {
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component is IActionParameter actionParameter && component.GetType() == actionType)
                {
                    return actionParameter;
                }
            }

            if (!typeof(MonoBehaviour).IsAssignableFrom(actionType))
            {
                return null;
            }

            MonoBehaviour createdComponent = gameObject.AddComponent(actionType) as MonoBehaviour;
            return createdComponent as IActionParameter;
        }

        private void TryUseSelectedAction()
        {
            ActionInventoryEntry selectedEntry = GetSelectedEntry();

            if (selectedEntry == null || selectedEntry.Action == null)
            {
                GameplayFeedbackService.Instance?.PlayMechanicFail("Collect an orb first");
                UpdateActionText();
                return;
            }

            if (selectedEntry.Charges <= 0)
            {
                GameplayFeedbackService.Instance?.PlayOutOfCharges(selectedEntry.Action.GetActionName(), selectedEntry.Action.GetActionColor());
                SelectNextAvailableActionOrClear(1, false);
                return;
            }

            if (!selectedEntry.Action.TryExecute(rb))
            {
                GameplayFeedbackService.Instance?.PlayMechanicFail("No valid use");
                UpdateActionText();
                return;
            }

            selectedEntry.Charges = Mathf.Max(0, selectedEntry.Charges - 1);
            currentAction = selectedEntry.Action;

            GameplayFeedbackService.Instance?.PlayMechanicUse(transform.position, currentAction.GetActionColor());

            if (selectedEntry.Charges > 0)
            {
                GameplayFeedbackService.Instance?.PlayChargeConsumed(
                    currentAction.GetActionName(),
                    selectedEntry.Charges,
                    MechanicChargeConfig.DefaultChargesPerPickup,
                    currentAction.GetActionColor());
                UpdateActionText();
                return;
            }

            GameplayFeedbackService.Instance?.PlayOutOfCharges(currentAction.GetActionName(), currentAction.GetActionColor());
            SelectNextAvailableActionOrClear(1, false);
        }

        private static float ResolveHorizontalInput()
        {
            bool moveLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
            bool moveRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);

            if (moveLeft == moveRight)
            {
                return 0f;
            }

            return moveRight ? 1f : -1f;
        }

        private void SelectNextAvailableActionOrClear(int direction, bool playFeedback)
        {
            int nextIndex = FindNextSelectableActionIndex(direction);

            if (nextIndex >= 0 && nextIndex != selectedActionIndex)
            {
                SetSelectedAction(nextIndex, playFeedback);
                return;
            }

            currentAction = null;
            selectedActionIndex = -1;
            UpdateActionText();
        }

        private void UpdateActionText()
        {
            ActionInventoryEntry selectedEntry = GetSelectedEntry();
            string actionName = selectedEntry?.Action != null ? selectedEntry.Action.GetActionName() : null;
            string actionDescription = selectedEntry?.Action != null ? selectedEntry.Action.GetActionDescription() : null;
            Color actionColor = selectedEntry?.Action != null ? selectedEntry.Action.GetActionColor() : PresentationTheme.HudSubtext;

            UIManager.Instance?.SetCurrentAction(
                actionName,
                actionDescription,
                actionColor);
            UIManager.Instance?.SetMechanicStatus(BuildChargeSummary(selectedEntry), BuildUnlockedSummary());
        }

        private string BuildChargeSummary(ActionInventoryEntry selectedEntry)
        {
            if (selectedEntry == null || selectedEntry.Action == null)
            {
                return null;
            }

            return $"{selectedEntry.Charges}/{MechanicChargeConfig.DefaultChargesPerPickup}";
        }

        private string BuildUnlockedSummary()
        {
            if (unlockedActions.Count == 0)
            {
                return null;
            }

            List<string> labels = new List<string>(unlockedActions.Count);

            for (int i = 0; i < unlockedActions.Count; i++)
            {
                ActionInventoryEntry entry = unlockedActions[i];

                if (entry.Action == null)
                {
                    continue;
                }

                string label = $"{entry.Action.GetActionName()} {entry.Charges}/{MechanicChargeConfig.DefaultChargesPerPickup}";

                if (i == selectedActionIndex)
                {
                    label += " [selected]";
                }

                labels.Add(label);
            }

            return labels.Count > 0 ? string.Join(", ", labels) : null;
        }
    }
}
