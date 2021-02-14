using Emerald.UiControllers;
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
    [SerializeField] private TMP_InputField chatBar;
    private InputController.ChatActions chatActions;
    private InputController.UIActions uiInput; // Not sure if static is the right approach for this
    private InputController.QuickSlotsActions quickSlotsActions;
    [SerializeField] private MirQuickCell[] quickSlots;
    private int[] chatSizes = new int[4] { 0, 120, 165, 250 };
    private byte toggleSize = 2;


    private void Awake()
    {
        uiInput = new InputController().UI;
        //quickSlotsEquipped = new IQuickSlotItem[24];
        quickSlotsActions = new InputController().QuickSlots;
        chatActions = new InputController().Chat;
        chatActions.Newaction.performed += _ => ToggleChat();

        // Window Action Handlers //
        uiInput.Inventory.performed += inventoryCallback => InventoryWindowStateHandler();
        uiInput.Character.performed += characterCallback => CharacterWindowStateHandler();
        uiInput.Options.performed += optionsCallback => OptionWindowStateHandler();
        uiInput.Skills.performed += skillsCallback => SkillWindowStateHandler();
        uiInput.Guild.performed += guildCallback => GuildWindowStateHandler();
        uiInput.MiniMap.performed += miniMapCallback => MiniMapWindowStateHandler();

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
    public bool IsWindiwControlsEnabled => uiInput.enabled;
    private void GuildWindowStateHandler() => guildMenu.SetActive(!guildMenu.activeSelf);
    private void SkillWindowStateHandler() => skillsMenu.SetActive(!skillsMenu.activeSelf);
    private void OptionWindowStateHandler() => optionsMenu.SetActive(!optionsMenu.activeSelf);
    private void CharacterWindowStateHandler() => characterMenu.SetActive(!characterMenu.activeSelf);
    private void InventoryWindowStateHandler() => inventoryMenu.SetActive(!inventoryMenu.activeSelf);

    public void MiniMapWindowStateHandler()
    {
        miniMap.SetActive(!miniMap.activeSelf);
        int yRotation = miniMap.activeSelf ? 0 : 180;
        miniMapToggleButton.transform.rotation = new Quaternion(
            miniMap.transform.rotation.x,
            yRotation,
            miniMap.transform.rotation.z,
            miniMap.transform.rotation.w);
    }
}

public interface IQuickSlotItem
{
    void DoAction();
    Sprite GetIcon();
    MirQuickCell QuickCell { get; set; }
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit(PointerEventData eventData);
}