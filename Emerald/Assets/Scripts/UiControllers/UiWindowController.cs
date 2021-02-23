using System.Collections.Generic;
using Emerald.UiControllers;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UiControllers
{
    public class UiWindowController : MonoBehaviour
    {
        [SerializeField] private GameObject[] chatWindowDisplay = new GameObject[3];
        [SerializeField] private GameObject[] chatWindowsToHide = new GameObject[3];
        [SerializeField] private GameObject miniMapToggleButton;
        [SerializeField] private GameObject gfxMenu;
        [SerializeField] private GameObject soundsSettingsMenu;
        [SerializeField] private GameObject gameSettingsMenu;
        [SerializeField] private GameObject characterMenu;
        [SerializeField] private GameObject inventoryMenu;
        [SerializeField] private GameObject skillsMenu;
        [SerializeField] private GameObject guildMenu;
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private GameObject miniMap;
        [SerializeField] private GameObject partyWindow;
        [SerializeField] private TMP_InputField chatBar;
        [SerializeField] private GameObject partyInviteWindow;
        [SerializeField] private GameObject guildReceiveInviteWindow;
        [SerializeField] private GameObject guildSendInviteWindow;
        public bool IsPopUpActive { get; set; } = false;
        
        [SerializeField] private MirQuickCell[] quickSlots;
    
        private InputController.ChatActions chatActions;
        private InputController.UIActions uiInput; // Not sure if static is the right approach for this
        private InputController.QuickSlotsActions quickSlotsActions;
        private int[] chatSizes = new int[4] { 0, 120, 165, 250 };
        private byte toggleSize = 2;
        private List<GameObject> priorityWindowCloseList;
        private List<GameObject> activeWindows;
        private byte priorityWindowCount = 0;

        /* TODO: Add UiPartyWindow collapse menu */
        /* TODO: Escape button closing windows, by priority? */
        /* TODO: Make windows draggable */
        private void Awake() {
            activeWindows = new List<GameObject>();
            uiInput = new InputController().UI;
            //quickSlotsEquipped = new IQuickSlotItem[24];
            quickSlotsActions = new InputController().QuickSlots;
            chatActions = new InputController().Chat;
            chatActions.Newaction.performed += _ => ToggleChat();

            // Window Action Handlers //
            uiInput.Options.performed += _ => PriorityWindowStateHandler(optionsMenu);
            uiInput.Inventory.performed += _ => WindowStateHandler(inventoryMenu);
            uiInput.Character.performed += _ => WindowStateHandler(characterMenu);
            uiInput.Skills.performed += _ => WindowStateHandler(skillsMenu);
            uiInput.Guild.performed += _ => WindowStateHandler(guildMenu);
            uiInput.Party.performed += _ => WindowStateHandler(partyWindow);
            uiInput.MiniMap.performed += _ => MiniMapWindowStateHandler();
            uiInput.Escape.performed += _ => HandleEscapePress();

            // QuickSlot Action Handlers //
            quickSlotsActions.QuickSlot_F1.performed += callBack => StartQuickSlotAction((int)QuickSlot.F1);
            quickSlotsActions.QuickSlot_F2.performed += callBack => StartQuickSlotAction((int)QuickSlot.F2);
            quickSlotsActions.QuickSlot_F3.performed += callBack => StartQuickSlotAction((int)QuickSlot.F3);
            quickSlotsActions.QuickSlot_F4.performed += callBack => StartQuickSlotAction((int)QuickSlot.F4);
            quickSlotsActions.QuickSlot_F5.performed += callBack => StartQuickSlotAction((int)QuickSlot.F5);
            quickSlotsActions.QuickSlot_F6.performed += callBack => StartQuickSlotAction((int)QuickSlot.F6);
            quickSlotsActions.QuickSlot_S.performed += callBack => StartQuickSlotAction((int)QuickSlot.S);
            quickSlotsActions.QuickSlot_D.performed += callBack => StartQuickSlotAction((int)QuickSlot.D);
            quickSlotsActions.QuickSlot_Q.performed += callBack => StartQuickSlotAction((int)QuickSlot.Q);
            quickSlotsActions.QuickSlot_Y.performed += callBack => StartQuickSlotAction((int)QuickSlot.Y);
            quickSlotsActions.QuickSlot_B.performed += callBack => StartQuickSlotAction((int)QuickSlot.B);
            quickSlotsActions.QuickSlot_H.performed += callBack => StartQuickSlotAction((int)QuickSlot.H);
            quickSlotsActions.QuickSlot_1.performed += callBack => StartQuickSlotAction((int)QuickSlot.ONE);
            quickSlotsActions.QuickSlot_2.performed += callBack => StartQuickSlotAction((int)QuickSlot.TWO);
            quickSlotsActions.QuickSlot_3.performed += callBack => StartQuickSlotAction((int)QuickSlot.THREE);
            quickSlotsActions.QuickSlot_4.performed += callBack => StartQuickSlotAction((int)QuickSlot.FOUR);
            quickSlotsActions.QuickSlot_5.performed += callBack => StartQuickSlotAction((int)QuickSlot.FIVE);
            quickSlotsActions.QuickSlot_6.performed += callBack => StartQuickSlotAction((int)QuickSlot.SIX);
            quickSlotsActions.QuickSlot_7.performed += callBack => StartQuickSlotAction((int)QuickSlot.SEVEN);
            quickSlotsActions.QuickSlot_8.performed += callBack => StartQuickSlotAction((int)QuickSlot.EIGHT);
            quickSlotsActions.QuickSlot_9.performed += callBack => StartQuickSlotAction((int)QuickSlot.NINE);
            quickSlotsActions.QuickSlot_0.performed += callBack => StartQuickSlotAction((int)QuickSlot.TEN);
            quickSlotsActions.QuickSlot_minus.performed += callBack => StartQuickSlotAction((int)QuickSlot.MINUS);
            quickSlotsActions.QuickSlot_equals.performed += callBack => StartQuickSlotAction((int)QuickSlot.EQUALS);
            chatActions.Enable();
            EnableControls();
            // SetPartyInputFieldListeners();
            priorityWindowCloseList = new List<GameObject>() {gfxMenu, optionsMenu, soundsSettingsMenu, gameSettingsMenu}; // add guild invite to this list
        }

        #region UI_HANDLERS
        private void HandleEscapePress()
        {
            if (IsPopUpActive) return;
            
            // TODO: Should Cancel holding item if the player is holding an item with cursor?
            if(priorityWindowCount > 0) {
                for (int i = 0; i < priorityWindowCloseList.Count; i++) {
                    switch (priorityWindowCloseList[i].activeSelf) {
                        /* TODO: Add custom case for popup windows */
                        case true when priorityWindowCloseList[i].name == guildSendInviteWindow.name:
                            PriorityWindowStateHandler(priorityWindowCloseList[i]);
                            return;
                        case true when priorityWindowCloseList[i].name == partyInviteWindow.name:
                            partyWindow.GetComponent<PartyController>().ReplyToPartyInvite(false);
                            priorityWindowCount--;
                            return;
                        case true:
                            PriorityWindowStateHandler(priorityWindowCloseList[i]);
                            return;
                    }
                }
            }

            if (activeWindows.Count > 0) {
                WindowStateHandler(activeWindows[activeWindows.Count - 1]);
                return;
            }
            PriorityWindowStateHandler(optionsMenu); // No other windows open, open the options menu
        }

        private void WindowStateHandler(GameObject window) {
            window.SetActive(!window.activeSelf);
            if (window.activeSelf) {
                activeWindows.Add(window);
            } else {
                activeWindows.Remove(window);
            }
        }

        private void PriorityWindowStateHandler(GameObject window) {
            window.SetActive(!window.activeSelf);
            if (window.activeSelf) {
                priorityWindowCount++;
            } else {
                priorityWindowCount--;
            }
        }
    
        public void ToggleChatWindowHeight()
        {
            toggleSize++;
            if (toggleSize == 4)
            {
                ShowChatWindow(false);
                toggleSize = 0;
            }
            else
            {
                ShowChatWindow();
                for (int i = 0; i < chatWindowDisplay.Length; i++)
                {
                    chatWindowDisplay[i].transform.GetComponent<RectTransform>()
                        .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, chatSizes[toggleSize]);
                }
            }
        }

        private void ShowChatWindow(bool shouldHide = true)
        {
            for (int i = 0; i < chatWindowsToHide.Length; i++)
            {
                chatWindowsToHide[i].SetActive(shouldHide);
                chatWindowDisplay[i].SetActive(shouldHide);
            }
        }

        private void ToggleChat()
        {
            if (chatBar.gameObject.activeSelf)
            {
                EnableControls();
                if (chatBar.text.Trim().Length > 0)
                    GameSceneManager.SendUserMessagePackage(chatBar.text);
                chatBar.gameObject.SetActive(false);
                chatBar.text = string.Empty;
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                DisableControls();
                chatBar.gameObject.SetActive(true);
                chatBar.Select();
            }
        }

        public void MiniMapWindowStateHandler()
        {
            miniMap.SetActive(!miniMap.activeSelf);
            int newRotationY = miniMap.activeSelf ? 0 : 180;
            var rotation = miniMap.transform.rotation;
            miniMapToggleButton.transform.rotation = new Quaternion(
                rotation.x,
                newRotationY,
                rotation.z,
                rotation.w);
        }
    
        public void DisableControls()
        {
            quickSlotsActions.Disable();
            uiInput.Disable();
        }
    
        public void EnableControls()
        {
            quickSlotsActions.Enable();
            uiInput.Enable();
        }
    
        // private void SetPartyInputFieldListeners() {
        // TMP_InputField partyInputField = partyWindow.transform.GetChild(5).GetChild(2).gameObject.GetComponent<TMP_InputField>();
        // partyInputField.onSelect.AddListener(delegate(string arg0) { DisableControls(); });
        // partyInputField.onDeselect.AddListener(delegate(string arg0) { EnableControls(); });
        
        // }
        #endregion

        #region QuickSlots
    
        private void StartQuickSlotAction(int position)
        {        
            quickSlots[position].DoAction();
        }

        public InputActionMap GetQuickSlotActions() => quickSlotsActions.Get();

        public bool IsWindowControlsEnabled => uiInput.enabled;

        public bool IsQuickSlotsEnabled => quickSlotsActions.enabled;
        #endregion
    }

    public interface IPopUpWindow {
        void AddToPopUpWindowList();
        void ClosePopUp();
    }


    public interface IQuickSlotItem
    {
        void DoAction();
        Sprite GetIcon();
        [CanBeNull] MirQuickCell QuickCell { get; set; }
        void OnPointerEnter(PointerEventData eventData);
        void OnPointerExit(PointerEventData eventData);
    }
}