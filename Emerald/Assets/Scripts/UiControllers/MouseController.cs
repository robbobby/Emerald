using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiControllers {
    public class MouseController : MonoBehaviour {
        private InputController.InteractionsActions sceneInteractions;
        [SerializeField] private GameObject gameManagerObject;
        private GameSceneManager gameManager;
        private bool startAttack;
        private String currentCoroutine;
        private CurrentAction currentAction;
    
        public void Awake() {
            gameManager = gameManagerObject.GetComponent<GameSceneManager>();
            sceneInteractions = new InputController().Interactions;
            sceneInteractions.ShiftLeftClick.performed += _ => HandleShiftAndClickAttack();
            sceneInteractions.LeftClickReleased.performed += _ => HandleLeftMouseButtonRelease();
            sceneInteractions.TabPress.performed += _ => HandlePickUpItem();
            sceneInteractions.ShiftReleased.performed += _ => HandleShiftRelease();
            sceneInteractions.ShiftPress.performed += _ => HandleShiftAttack();
            sceneInteractions.LeftClick.performed += _ => HandleLeftClick();
            sceneInteractions.RightClick.performed += _ => HandleRightClick();
            sceneInteractions.RightClickReleased.performed += _ => HandleRightClickReleased();
            sceneInteractions.Enable();
        }

        private void HandleRightClick() {
            Debug.Log("Right Click pressed");
            StopCurrentCoRoutine();
            Debug.Log("HandleRightClick Stopping");
            currentCoroutine = nameof(RunCoroutine);
            StartCoroutine(nameof(RunCoroutine));
        }

        private void HandleRightClickReleased() {
            if(currentAction == CurrentAction.Run)
                StopCurrentCoRoutine();
        }

        private void HandleLeftClick() {
            if (IsHoldingShift()) return;
            Debug.Log("Left button clicked");
            StopCurrentCoRoutine();
            currentAction = CurrentAction.LeftClick;
            currentCoroutine = nameof(LeftClickCoroutine);
            StartCoroutine(nameof(LeftClickCoroutine));
        }

        private void HandlePickUpItem() {
            Debug.Log("Tab Pressed");
            gameManager.PickUpItem();
        }

        private void HandleShiftAttack() {
            Debug.Log("Shift Pressed");
            if (Input.GetMouseButtonDown(0)) {
                StopCurrentCoRoutine();
                currentCoroutine = nameof(ShiftAttackCoroutine);
                StartCoroutine(nameof(ShiftAttackCoroutine));
            }
        }


        private void HandleShiftRelease() {
            Debug.Log("Shift released");
            if (IsAttackingWithShift()) {
                StopCurrentCoRoutine();
                currentAction = CurrentAction.None;
            }

            if (Input.GetMouseButtonDown(0)) 
                HandleLeftClick();
        }

        private void HandleShiftAndClickAttack() {
            Debug.Log("Shift left click pressed");
            StopCurrentCoRoutine();
            currentCoroutine = nameof(ShiftLeftClickCoRoutine);
            StartCoroutine(nameof(ShiftLeftClickCoRoutine));
        }

        private void HandleLeftMouseButtonRelease() {
            Debug.Log("left Button Up");
             if(IsHoldingShift()) {
                 HandleShiftAttack();
             } 
             else if (currentAction == CurrentAction.ShiftClickAttack) {
                 StopCurrentCoRoutine();
             }
             else
                gameManager.PlaceItemDown();
        }

        private IEnumerator LeftClickCoroutine() {
            while(true) {
                gameManager.HandleLeftMouseButtonDown();
                yield return null;
            }
        }

        private IEnumerator ShiftLeftClickCoRoutine() {
            currentAction = CurrentAction.ShiftClickAttack;
            while(true) {
                gameManager.HandleHoldingShift();
                yield return null;
            }
        }

        private IEnumerator ShiftAttackCoroutine() {
            currentAction = CurrentAction.ShiftAttack;
            bool hasShiftTarget = true;
            while (hasShiftTarget) {
                hasShiftTarget = gameManager.ShiftAttackTarget();
                yield return null;
            }
        }

        private bool IsAttackingWithShift() => currentAction == CurrentAction.ShiftClickAttack || currentAction == CurrentAction.ShiftAttack;

        private IEnumerator RunCoroutine() {
            Debug.Log("Before the run coroutine");
            currentAction = CurrentAction.Run;
            while (true) {
                gameManager.StartRunAction();
                yield return null;
            }
        }

        private void StopCurrentCoRoutine() {
            Debug.Log($"Stopping {currentCoroutine}");
            StopCoroutine(currentCoroutine);
            currentAction = CurrentAction.None;
        }

        private bool IsHoldingShift() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        private enum CurrentAction {
            None,
            ShiftClickAttack,
            ShiftAttack,
            LeftClick,
            Run,
        }
    }
}

