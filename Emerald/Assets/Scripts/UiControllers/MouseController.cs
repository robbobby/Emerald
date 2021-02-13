using System;
using System.Collections;
using ClientPackets;
using UnityEngine;

public class MouseController : MonoBehaviour {
    private InputController.MouseActions mouseInput;
    [SerializeField] private GameObject gameManagerObject;
    private GameSceneManager gameManager;
    private bool startAttack;
    public void Awake() {
        gameManager = gameManagerObject.GetComponent<GameSceneManager>();
        mouseInput = new InputController().Mouse;
        mouseInput.ShiftLeftClick.performed += _ => HandleShiftLeftClick();
        mouseInput.LeftButtonUp.performed += _ => HandleLeftButtonUp();
        mouseInput.Enable();
    }

    private void HandleLeftButtonUp() {
        StopCoroutine(nameof(ShiftLeftClickCoRoutine));
    }

    private void HandleShiftLeftClick() {
        Debug.Log($"left click pressed {startAttack}");
        StartCoroutine(nameof(ShiftLeftClickCoRoutine));
    }

    private IEnumerator ShiftLeftClickCoRoutine() {
        while(true) {
            gameManager.HandleShiftLeftClick();
            yield return null;
        }
    }
}