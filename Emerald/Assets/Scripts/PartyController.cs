using System;
using ServerPackets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Network = Emerald.Network;
using C = ClientPackets;

public class PartyController : MonoBehaviour {
    [SerializeField] private Toggle allowGroupToggle;
    [SerializeField] private TMP_InputField newPlayerName;

    

    public void ChangeAllowGroupValue() {
        Debug.Log(allowGroupToggle.isOn);
        Network.Enqueue(new C.SwitchGroup { AllowGroup = allowGroupToggle.isOn});
    }

    public void SendInviteToPlayer() {
        if (!RoomForMorePlayers()) return;
        if (!IsPartyLeader()) return;
        if (newPlayerName.text.Length <= 3) return;
        Network.Enqueue(new C.AddMember() { Name = newPlayerName.text });
        newPlayerName.text = "";
    }

    private bool RoomForMorePlayers() {
        return true;
    }

    private bool IsPartyLeader() {
        return true;
    }
}