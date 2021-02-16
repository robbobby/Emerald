using System.Collections.Generic;
using Emerald.UiControllers;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
    
    [SerializeField] private MirQuickCell[] quickSlots;
    
    private InputController.ChatActions chatActions;
    private InputController.UIActions uiInput; // Not sure if static is the right approach for this
    private InputController.QuickSlotsActions quickSlotsActions;
    private int[] chatSizes = new int[4] { 0, 120, 165, 250 };
    private byte toggleSize = 2;
    private List<GameObject> priorityWindowCloseList;
    private List<GameObject> activeWindows;
    private bool hasActiveWindows;

    /* TODO: Add UiPartyWindow collapse menu */
    /* TODO: Escape button closing windows, by priority? */
    /* TODO: Make windows draggable */
    private void Awake()
    {
        uiInput = new InputController().UI;
        //quickSlotsEquipped = new IQuickSlotItem[24];
        quickSlotsActions = new InputController().QuickSlots;
        chatActions = new InputController().Chat;
        chatActions.Newaction.performed += _ => ToggleChat();

        // Window Action Handlers //
        uiInput.Inventory.performed += _ => InventoryWindowStateHandler();
        uiInput.Character.performed += _ => CharacterWindowStateHandler();
        uiInput.Options.performed += _ => OptionWindowStateHandler();
        uiInput.Skills.performed += _ => SkillWindowStateHandler();
        uiInput.Guild.performed += _ => GuildWindowStateHandler();
        uiInput.MiniMap.performed += _ => MiniMapWindowStateHandler();
        uiInput.Party.performed += _ => PartyWindowStateHandler();
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
        SetPartyInputFieldListeners();
        priorityWindowCloseList = new List<GameObject>() {optionsMenu, gfxMenu, soundsSettingsMenu, gameSettingsMenu, partyInviteWindow, guildSendInviteWindow}; // add guild invite to this list
    }

    private void HandleEscapePress() {
        Debug.Log("HandlingEscape");
        // TODO: Should Cancel holding item if the player is holding an item with cursor?
        // TODO: Add flag for priorityWindowCloseList so it doesn't loop through none of them are open
        for (int i = 0; i > priorityWindowCloseList.Count; i++) {
            switch (priorityWindowCloseList[i].activeSelf) {
                case true when priorityWindowCloseList[i].name == guildSendInviteWindow.name:
                    guildSendInviteWindow.SetActive(false);
                    return;
                case true when priorityWindowCloseList[i].name == partyInviteWindow.name:
                    partyWindow.GetComponent<PartyController>().ReplyToPartyInvite(false);
                    priorityWindowCloseList[i].SetActive(false);
                    return;
                case true:
                    priorityWindowCloseList[i].SetActive(false);
                    return;
            }
        }
    }

    public void PartyWindowStateHandler() {
        partyWindow.SetActive(!partyWindow.activeSelf);
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

    private void StartQuickSlotAction(int position)
    {        
        quickSlots[position].DoAction();
    }

    public InputActionMap GetQuickSlotActions() => quickSlotsActions.Get();
    public void EnableControls()
    {
        quickSlotsActions.Enable();
        uiInput.Enable();
    }

    public void DisableControls()
    {
        quickSlotsActions.Disable();
        uiInput.Disable();
    }

    public bool IsQuickSlotsEnabled => quickSlotsActions.enabled;
    public bool IsWindowControlsEnabled => uiInput.enabled;
    public void GuildWindowStateHandler() {
        guildMenu.SetActive(!guildMenu.activeSelf);
        // CheckAndAddToActiveWindows(guildMenu);
    }

    public void SkillWindowStateHandler() {
        skillsMenu.SetActive(!skillsMenu.activeSelf);
        // CheckAndAddToActiveWindows(skillsMenu);
    }

    public void OptionWindowStateHandler() {
        optionsMenu.SetActive(!optionsMenu.activeSelf);
        // CheckAndAddToActiveWindows(optionsMenu);
    }

    public void CharacterWindowStateHandler() {
        characterMenu.SetActive(!characterMenu.activeSelf);
        // CheckAndAddToActiveWindows(characterMenu);
    }

    public void InventoryWindowStateHandler() {
        inventoryMenu.SetActive(!inventoryMenu.activeSelf);
        // CheckAndAddToActiveWindows(inventoryMenu);
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

    // private bool AddToActiveWindows(GameObject window) {
    //     activeWindows.Add(window);
    //     return true;
    // }
    //
    // private bool RemoveFromActiveWindows(GameObject window) {
    //     activeWindows.Remove(window);
    //     return activeWindows.Count > 0;
    // }
    //
    // private void CheckAndAddToActiveWindows(GameObject window) {
    //     hasActiveWindows = window.activeSelf ? AddToActiveWindows(window) : RemoveFromActiveWindows(window);
    // }
    
    private void SetPartyInputFieldListeners() {
        TMP_InputField partyInputField = partyWindow.transform.GetChild(5).GetChild(2).gameObject.GetComponent<TMP_InputField>();
        Debug.Log(partyInputField);
        partyInputField.onSelect.AddListener(delegate(string arg0) { DisableControls(); });
        partyInputField.onDeselect.AddListener(delegate(string arg0) { EnableControls(); });
    }
}

public interface IQuickSlotItem
{
    void DoAction();
    Sprite GetIcon();
    [CanBeNull] MirQuickCell QuickCell { get; set; }
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit(PointerEventData eventData);
}