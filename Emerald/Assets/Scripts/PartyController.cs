using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Network = Emerald.Network;
using C = ClientPackets;
using Toggle = UnityEngine.UI.Toggle;

public class PartyController : MonoBehaviour {
    [SerializeField] private Toggle allowGroupToggle;
    [SerializeField] private TMP_InputField inputPlayerName;
    [SerializeField] private GameObject inviteWindow;
    [SerializeField] private TMP_Text inviteLabel;
    [SerializeField] private GameObject memberSlot;
    [SerializeField] private GameObject memberContainer;
    private readonly List<string> partyList = new List<string>();
    private readonly List<GameObject> memberSlots = new List<GameObject>();
    
    public string UserName { get; set; }

    public void ChangeAllowGroupValue() {
        Debug.Log(allowGroupToggle.isOn);
        Network.Enqueue(new C.SwitchGroup { AllowGroup = allowGroupToggle.isOn});
    }

    public void LeaveParty() {
        ClearPartyListAndMemberSlots();
        Network.Enqueue(new C.DelMember() {Name = UserName});
    }

    public void RemoveMemberFromParty() {
        Network.Enqueue(new C.DelMember { Name = inputPlayerName.text});
    }

    public void RemoveMemberFromParty(string playerName) {
        Debug.Log($"Should remove player: {playerName}");
        Network.Enqueue(new C.DelMember { Name = playerName});
    }

    public void RemoveMemberFromParty(int memberPosition) {
        Network.Enqueue(new C.DelMember() { Name = partyList[memberPosition]});
    }

    public void SendInviteToPlayer() {
        if (!RoomForMorePlayers()) return;
        if (!IsPartyLeader()) return;
        if (inputPlayerName.text.Length <= 3) return;
        Network.Enqueue(new C.AddMember() { Name = inputPlayerName.text });
        inputPlayerName.text = "";
    }

    public void ReplyToPartyInvite(bool response) {
        Network.Enqueue(new C.GroupInvite() { AcceptInvite = response });
        inviteWindow.SetActive(false);
    }

    public void ShowInviteWindow(string fromUser) {
        inviteLabel.text = $"Would you like to accept party invite from {fromUser}";
        inviteWindow.SetActive(true);
    }

    public void AddToPartyList(string newMember) {
        partyList.Add(newMember);
        RefreshPartyMenu();
    }


    public void RemoveFromPartyList(string member) {
        Debug.Log(member);
        partyList.Remove(member);
        if (partyList.Count == 1) {
            RemoveFromPartyList(UserName);
        }
        RefreshPartyMenu();
    }

    public void ClearPartyListAndMemberSlots() {
        ClearMemberSlots();
        partyList.Clear();
    }

    private void ClearMemberSlots() {
        for (int i = 0; i < memberSlots.Count; i++) {
            Destroy(memberSlots[i]);
        }
        memberSlots.Clear();
    }

    private void RefreshPartyMenu() {
        ClearMemberSlots();
        for (int i = 0; i < partyList.Count; i++) {
            Debug.Log(i);
            memberSlots.Add(Instantiate(memberSlot, memberContainer.transform));
            GameObject kickButton = memberSlots[i].transform.GetChild(1).gameObject;
            GameObject nameTextField = memberSlots[i].transform.GetChild(0).GetChild(1).gameObject;
            nameTextField.GetComponent<TextMeshProUGUI>().SetText(partyList[i]);
            kickButton.AddComponent<PlayerSlot>().Construct(this, partyList[i], kickButton, ShouldShowKickButton(i));
        }
    }

    private bool ShouldShowKickButton(int position) => position > 0 && partyList[0] == UserName;

    private bool IsPartyLeader() {
        return partyList.Count == 0 || UserName == partyList[0];
    }

    private bool RoomForMorePlayers() {
        return partyList.Count < 5; // Global party count?
    }
}

internal class PlayerSlot : MonoBehaviour {
    private PartyController partyController;
    private string playerName;
    private GameObject kickButton;

    public void Construct(PartyController partyController, string playerName, GameObject kickButton,
        bool shouldShowKickButton) {
        this.partyController = partyController;
        this.playerName = playerName;
        this.kickButton = kickButton;

        Debug.Log($"KickButton with {playerName} made");
        if(shouldShowKickButton)
            kickButton.GetComponent<Button>().onClick.AddListener(() 
                => partyController.RemoveMemberFromParty(playerName));
        else 
            kickButton.SetActive(false);
    }
}
