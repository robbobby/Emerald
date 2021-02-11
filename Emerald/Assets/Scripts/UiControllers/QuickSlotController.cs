using System.Collections.Generic;
using Emerald.UiControllers;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotController : MonoBehaviour {
    private InputController.QuickSlotsActions quickSlotsActions;
    private IReplaceMe[] quickSlotsEquipped;

    private void Start() {
        quickSlotsEquipped = new IReplaceMe[24];
        quickSlotsActions = new InputController().QuickSlots;
        quickSlotsActions.QuickSlot_F1.performed += callBack => QuickSlotHandler((int)QuickSlot.F1);
        quickSlotsActions.QuickSlot_F2.performed += callBack => QuickSlotHandler((int)QuickSlot.F2);
        quickSlotsActions.QuickSlot_F3.performed += callBack => QuickSlotHandler((int)QuickSlot.F3);
        quickSlotsActions.QuickSlot_F4.performed += callBack => QuickSlotHandler((int)QuickSlot.F4);
        quickSlotsActions.QuickSlot_F5.performed += callBack => QuickSlotHandler((int)QuickSlot.F5);
        quickSlotsActions.QuickSlot_F6.performed += callBack => QuickSlotHandler((int)QuickSlot.F6);
        quickSlotsActions.QuickSlot_S.performed += callBack => QuickSlotHandler((int)QuickSlot.S);
        quickSlotsActions.QuickSlot_D.performed += callBack => QuickSlotHandler((int)QuickSlot.D);
        quickSlotsActions.QuickSlot_Q.performed += callBack => QuickSlotHandler((int)QuickSlot.Q);
        quickSlotsActions.QuickSlot_Y.performed += callBack => QuickSlotHandler((int)QuickSlot.Y);
        quickSlotsActions.QuickSlot_B.performed += callBack => QuickSlotHandler((int)QuickSlot.B);
        quickSlotsActions.QuickSlot_H.performed += callBack => QuickSlotHandler((int)QuickSlot.H);
        quickSlotsActions.QuickSlot_1.performed += callBack => QuickSlotHandler((int)QuickSlot.ONE);
        quickSlotsActions.QuickSlot_2.performed += callBack => QuickSlotHandler((int)QuickSlot.TWO);
        quickSlotsActions.QuickSlot_3.performed += callBack => QuickSlotHandler((int)QuickSlot.THREE);
        quickSlotsActions.QuickSlot_4.performed += callBack => QuickSlotHandler((int)QuickSlot.FOUR);
        quickSlotsActions.QuickSlot_5.performed += callBack => QuickSlotHandler((int)QuickSlot.FIVE);
        quickSlotsActions.QuickSlot_6.performed += callBack => QuickSlotHandler((int)QuickSlot.SIX);
        quickSlotsActions.QuickSlot_7.performed += callBack => QuickSlotHandler((int)QuickSlot.SEVEN);
        quickSlotsActions.QuickSlot_8.performed += callBack => QuickSlotHandler((int)QuickSlot.EIGHT);
        quickSlotsActions.QuickSlot_9.performed += callBack => QuickSlotHandler((int)QuickSlot.NINE);
        quickSlotsActions.QuickSlot_0.performed += callBack => QuickSlotHandler((int)QuickSlot.TEN);
        quickSlotsActions.QuickSlot_minus.performed += callBack => QuickSlotHandler((int)QuickSlot.MINUS);
        quickSlotsActions.QuickSlot_equals.performed += callBack => QuickSlotHandler((int)QuickSlot.EQUALS);
    }

    private void QuickSlotHandler(int position) {
        Debug.Log($"Position {position} clicked");
        if (quickSlotsEquipped[position] == null) return;
        quickSlotsEquipped[position].DoSomething();
    }

    private void setQuickSlot(QuickSlot position, IReplaceMe newItem) {
        quickSlotsEquipped[(int) position] = newItem;
    }

    private void removeFromQuickSlot(QuickSlot position) {
        quickSlotsEquipped[(int)position] = null;
    }

    public InputActionMap GetQuickSlotActions() => quickSlotsActions.Get();
    public void EnableControls() => quickSlotsActions.Enable();
    public void DisableControls() => quickSlotsActions.Disable();
    public bool enabled => quickSlotsActions.enabled;
}

internal interface IReplaceMe {
    void DoSomething();
}
