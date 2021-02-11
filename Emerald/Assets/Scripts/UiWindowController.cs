using UnityEngine;
using UnityEngine.InputSystem;

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
    public InputController.UIActions UiInput;

    private void Awake() {
        UiInput = new InputController().UI;
        UiInput.Inventory.performed += inventoryCallback => InventoryWindowStateHandler();
        UiInput.Character.performed += characterCallback => CharacterWindowStateHandler();
        UiInput.Options.performed += optionsCallback => OptionWindowStateHandler();
        UiInput.Skills.performed += skillsCallback => SkillWindowStateHandler();
        UiInput.Guild.performed += guildCallback => GuildWindowStateHandler();
        UiInput.MiniMap.performed += miniMapCallback => MiniMapWindowStateHandler();
    }

    private void GuildWindowStateHandler() {
        GuildMenu.SetActive(!GuildMenu.activeSelf);
    }

    private void MiniMapWindowStateHandler() {
        MiniMap.SetActive(!MiniMap.activeSelf);
    }

    private void SkillWindowStateHandler() {
        SkillsMenu.SetActive(!SkillsMenu.activeSelf);
    }

    private void OptionWindowStateHandler() {
        OptionsMenu.SetActive(!OptionsMenu.activeSelf);
    }

    private void CharacterWindowStateHandler() {
        CharacterMenu.SetActive(!CharacterMenu.activeSelf);
    }

    private void InventoryWindowStateHandler() {
        InventoryMenu.SetActive(!InventoryMenu.activeSelf);
    }

    private void OnEnable() {
        UiInput.Enable();
    }

    private void OnDisable() {
        UiInput.Disable();
    }
}