using Emerald.UiControllers;
using ServerPackets;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Network = Emerald.Network;

public class UiWindowController : MonoBehaviour {
    public GameObject GfxMenu;
    public GameObject SoundsSettingsMenu;
    public GameObject GameSettingsMenu;
    public GameObject CharacterMenu;
    public GameObject InventoryMenu;
    public GameObject SkillsMenu;
    public GameObject GuildMenu;
    public GameObject OptionsMenu;
    public GameObject MiniMap;
    public TMP_InputField ChatBar;
    private InputController.ChatActions chatActions;
    private InputController.UIActions uiInput; // Not sure if static is the right approach for this
    private InputController.QuickSlotsActions quickSlotsActions;
    private IQuickSlotItem[] quickSlotsEquipped;
    

    private void Awake() {
        uiInput = new InputController().UI;
        quickSlotsEquipped = new IQuickSlotItem[24];
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
    }

    private void ToggleChat() {
        if (ChatBar.gameObject.activeSelf) {
            EnableControls();
            if (ChatBar.text.Trim().Length > 0)
                GameSceneManager.SendUserMessagePackage(ChatBar.text);
            ChatBar.gameObject.SetActive(false);
            ChatBar.text = string.Empty;
            EventSystem.current.SetSelectedGameObject(null);
        } else {
            DisableControls();
            ChatBar.gameObject.SetActive(true);
            ChatBar.Select();
        }
    }

    private void StartQuickSlotAction(int position) {
        Debug.Log($"Position {position} pressed");
        if (quickSlotsEquipped[position] == null) return;
        quickSlotsEquipped[position].DoAction();
    }

    private void SetQuickSlot(QuickSlot position, IQuickSlotItem newItem) {
        quickSlotsEquipped[(int) position] = newItem;
    }

    private void RemoveFromQuickSlot(QuickSlot position) {
        quickSlotsEquipped[(int)position] = null;
    }

    public InputActionMap GetQuickSlotActions() => quickSlotsActions.Get();
    public void EnableControls() {
        quickSlotsActions.Enable();
        uiInput.Enable();
    }
    
    public void DisableControls() {
        quickSlotsActions.Disable();
        uiInput.Disable();
    }
    public bool IsQuickSlotsEnabled => quickSlotsActions.enabled;
    public bool IsWindiwControlsEnabled => uiInput.enabled;
    private void GuildWindowStateHandler() => GuildMenu.SetActive(!GuildMenu.activeSelf);
    private void MiniMapWindowStateHandler() => MiniMap.SetActive(!MiniMap.activeSelf);
    private void SkillWindowStateHandler() => SkillsMenu.SetActive(!SkillsMenu.activeSelf);
    private void OptionWindowStateHandler()  => OptionsMenu.SetActive(!OptionsMenu.activeSelf);
    private void CharacterWindowStateHandler() => CharacterMenu.SetActive(!CharacterMenu.activeSelf);
    private void InventoryWindowStateHandler() => InventoryMenu.SetActive(!InventoryMenu.activeSelf);
}

internal interface IQuickSlotItem {
    void DoAction();
}