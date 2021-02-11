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
    private InputController.UIActions uiInput; // Not sure if static is the right approach for this

    private void Awake() {
        uiInput = new InputController().UI;
        uiInput.Inventory.performed += inventoryCallback => InventoryWindowStateHandler();
        uiInput.Character.performed += characterCallback => CharacterWindowStateHandler();
        uiInput.Options.performed += optionsCallback => OptionWindowStateHandler();
        uiInput.Skills.performed += skillsCallback => SkillWindowStateHandler();
        uiInput.Guild.performed += guildCallback => GuildWindowStateHandler();
        uiInput.MiniMap.performed += miniMapCallback => MiniMapWindowStateHandler();
    }

    private void GuildWindowStateHandler() => GuildMenu.SetActive(!GuildMenu.activeSelf);
    private void MiniMapWindowStateHandler() => MiniMap.SetActive(!MiniMap.activeSelf);
    private void SkillWindowStateHandler() => SkillsMenu.SetActive(!SkillsMenu.activeSelf);
    private void OptionWindowStateHandler()  => OptionsMenu.SetActive(!OptionsMenu.activeSelf);
    private void CharacterWindowStateHandler() => CharacterMenu.SetActive(!CharacterMenu.activeSelf);
    private void InventoryWindowStateHandler() => InventoryMenu.SetActive(!InventoryMenu.activeSelf);
    
    public void DisableControls() => uiInput.Disable();
    public void EnableControls() => uiInput.Enable();
}