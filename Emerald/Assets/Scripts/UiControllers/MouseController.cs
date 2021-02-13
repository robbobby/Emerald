using System;
using System.Collections;
using ClientPackets;
using UnityEngine;

public class MouseController : MonoBehaviour {
    private InputController.InteractionsActions mouseInput;
    [SerializeField] private GameObject gameManagerObject;
    private GameSceneManager gameManager;
    private bool startAttack;
    private String currentCoroutine;
    private CurrentAction currentAction;
    
    public void Awake() {
        gameManager = gameManagerObject.GetComponent<GameSceneManager>();
        mouseInput = new InputController().Interactions;
        mouseInput.ShiftLeftClick.performed += _ => HandleShiftLeftClick();
        mouseInput.LeftButtonUp.performed += _ => HandleLeftButtonUp();
        mouseInput.Tab.performed += _ => HandleTab();
        mouseInput.ShiftRelease.performed += _ => HandleShiftRelease();
        mouseInput.Enable();
    }

    private void HandleShiftRelease() {
        Debug.Log("Shift Up");
        if (currentAction == CurrentAction.ShiftLeftClick) {
            StopCoroutine(currentCoroutine);
            currentAction = CurrentAction.None;
        }
    }

    private void HandleTab() {
        gameManager.PickUpItem();
    }

    private void HandleLeftButtonUp() {
        Debug.Log("left Button Up");
        if(currentAction == CurrentAction.ShiftLeftClick) {
            StopCoroutine(currentCoroutine);
            currentAction = CurrentAction.None;
        }
        else
            gameManager.PlaceItemDown();
    }

    private void HandleShiftLeftClick() {
        currentCoroutine = nameof(ShiftLeftClickCoRoutine);  
        StartCoroutine(nameof(ShiftLeftClickCoRoutine));
    }

    private IEnumerator ShiftLeftClickCoRoutine() {
        currentAction = CurrentAction.ShiftLeftClick;
        while(true) {
            gameManager.HandleShiftLeftClick();
            yield return null;
        }
    }
    private enum CurrentAction {
        None,
        ShiftLeftClick,
        LeftClick,
    }
}

