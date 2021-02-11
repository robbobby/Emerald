using System;
using UnityEngine;

public class UiWindowController2 : MonoBehaviour {
    public GameObject GfxMenu;
    public GameObject SoundsSettingsMenu;
    public GameObject GameSettingsMenu;
    public GameObject CharacterMenu;
    public GameObject InventoryMenu;
    public GameObject SkillsMenu;
    public GameObject GuildMenu;
    public GameObject OptionsMenu;
    public GameObject MiniMap;

    private KeyCode lastPressedKey;
    private bool newEventTrigger;

    public void Update() {
        if (!Input.anyKeyDown || !newEventTrigger) return;
        HandleKeyDown();
        newEventTrigger = false;
    }

    public void OnGUI() {
        if (!Input.anyKeyDown || Event.current.keyCode == KeyCode.None) return;
        lastPressedKey = Event.current.keyCode;
        newEventTrigger = Event.current.isKey;
    }

    private void HandleKeyDown() {
        Debug.Log(lastPressedKey);
        switch (lastPressedKey) {
            case KeyCode.None:
                break;
            case KeyCode.F9:
                InventoryMenu.SetActive(!InventoryMenu.activeSelf);
                break;
            case KeyCode.F10:
                CharacterMenu.SetActive(!CharacterMenu.activeSelf);
                break;
            case KeyCode.F11:
                SkillsMenu.SetActive(!SkillsMenu.activeSelf);
                break;
            case KeyCode.F12:
                OptionsMenu.SetActive(!OptionsMenu.activeSelf);
                break;
            case KeyCode.C:
                CharacterMenu.SetActive(!CharacterMenu.activeSelf);
                break;
            case KeyCode.G:
                GuildMenu.SetActive(!GuildMenu.activeSelf);
                break;
            case KeyCode.I:
                InventoryMenu.SetActive(!InventoryMenu.activeSelf);
                break;
            case KeyCode.V:
                MiniMap.SetActive(!MiniMap.activeSelf);
                break;
            case KeyCode.K:
                SkillsMenu.SetActive(!SkillsMenu.activeSelf);
                // Rank
                break;
            case KeyCode.M:
                // Mail
                break;
            case KeyCode.P:
                // Group
                break;
            case KeyCode.Q:
                // Quest
                break;
            case KeyCode.R:
                // Mount
                break;
            case KeyCode.Y:
                // Game shop
                break;
            default:
                Debug.Log("Something just went wrong with the UI Controller");
                throw new ArgumentOutOfRangeException();
        }
    }
}