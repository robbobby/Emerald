using System.Collections.Generic;
using Emerald.UiControllers;
using UnityEngine;

public class QuickSlotController : MonoBehaviour {
    private InputController.QuickSlotsActions quickSlotsActions;
    private IReplaceMe[] quickSlotsEquipped;
    void Start() {
        quickSlotsEquipped = new IReplaceMe[24];
        quickSlotsActions = new InputController().QuickSlots;
        quickSlotsActions.QuickSlot_F1.performed += callBack => QuickSlotHandler((int)QuickSlots.F1);
        quickSlotsActions.QuickSlot_F2.performed += callBack => QuickSlotHandler((int)QuickSlots.F2);
        quickSlotsActions.QuickSlot_F3.performed += callBack => QuickSlotHandler((int)QuickSlots.F3);
        quickSlotsActions.QuickSlot_F4.performed += callBack => QuickSlotHandler((int)QuickSlots.F4);
        quickSlotsActions.QuickSlot_F5.performed += callBack => QuickSlotHandler((int)QuickSlots.F5);
        quickSlotsActions.QuickSlot_F6.performed += callBack => QuickSlotHandler((int)QuickSlots.F6);
        quickSlotsActions.QuickSlot_S.performed += callBack => QuickSlotHandler((int)QuickSlots.S);
        quickSlotsActions.QuickSlot_D.performed += callBack => QuickSlotHandler((int)QuickSlots.D);
        quickSlotsActions.QuickSlot_Q.performed += callBack => QuickSlotHandler((int)QuickSlots.Q);
        quickSlotsActions.QuickSlot_Y.performed += callBack => QuickSlotHandler((int)QuickSlots.Y);
        quickSlotsActions.QuickSlot_B.performed += callBack => QuickSlotHandler((int)QuickSlots.B);
        quickSlotsActions.QuickSlot_H.performed += callBack => QuickSlotHandler((int)QuickSlots.H);
        quickSlotsActions.QuickSlot_1.performed += callBack => QuickSlotHandler((int)QuickSlots.ONE);
        quickSlotsActions.QuickSlot_2.performed += callBack => QuickSlotHandler((int)QuickSlots.TWO);
        quickSlotsActions.QuickSlot_3.performed += callBack => QuickSlotHandler((int)QuickSlots.THREE);
        quickSlotsActions.QuickSlot_4.performed += callBack => QuickSlotHandler((int)QuickSlots.FOUR);
        quickSlotsActions.QuickSlot_5.performed += callBack => QuickSlotHandler((int)QuickSlots.FIVE);
        quickSlotsActions.QuickSlot_6.performed += callBack => QuickSlotHandler((int)QuickSlots.SIX);
        quickSlotsActions.QuickSlot_7.performed += callBack => QuickSlotHandler((int)QuickSlots.SEVEN);
        quickSlotsActions.QuickSlot_8.performed += callBack => QuickSlotHandler((int)QuickSlots.EIGHT);
        quickSlotsActions.QuickSlot_9.performed += callBack => QuickSlotHandler((int)QuickSlots.NINE);
        quickSlotsActions.QuickSlot_0.performed += callBack => QuickSlotHandler((int)QuickSlots.TEN);
        quickSlotsActions.QuickSlot_minus.performed += callBack => QuickSlotHandler((int)QuickSlots.MINUS);
        quickSlotsActions.QuickSlot_equals.performed += callBack => QuickSlotHandler((int)QuickSlots.EQUALS);
    }

    private void QuickSlotHandler(int position) {
        if (quickSlotsEquipped[position] == null) return;
        quickSlotsEquipped[position].DoSomething();
    }
}

internal interface IReplaceMe {
    void DoSomething();
}
