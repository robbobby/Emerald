using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XNode;
using Button = UnityEngine.UI.Button;
using Network = Emerald.Network;
using C = ClientPackets;
using Toggle = UnityEngine.UI.Toggle;

public class PartyController : MonoBehaviour, IPopUpWindow {
    [SerializeField] private Toggle allowGroupToggle;
    [SerializeField] private TMP_InputField inputPlayerName;
    [SerializeField] private GameObject inviteWindow;
    [SerializeField] private TMP_Text inviteLabel;
    [SerializeField] private GameObject memberSlot;
    [SerializeField] private GameObject memberContainer;
    [SerializeField] private GameObject uiMemberContainer;
    [SerializeField] private GameObject uiMemberSlot;
    [SerializeField] private GameObject uiCollapsePartyButton;
    [SerializeField] private TextMeshProUGUI pageCountText;
    [SerializeField] private GameObject groupPage;
    [SerializeField] private GameObject receiveInviteWindow;
    [SerializeField] private GameObject deleteMemberWindow;
    [SerializeField] private GameObject invitationNoticeIcon;
    private int currentPage = 0;
    private readonly List<string> partyList = new List<string>();
    private readonly List<GameObject> memberSlots = new List<GameObject>();
    private readonly List<GameObject> pages = new List<GameObject>();
    private readonly List<GameObject> uiMemberSlots = new List<GameObject>();
    private string currentSelectedMember;
    
    /* TODO: Checks before sending package */
    /* TODO: Optomise, only delete member when deleted, don't remake the full list*/
    /* TODO: Packet receiver for initial allow group value or find where this is already sent
        Careful with the ChangeAllowGroupValue and recursive loop.*/

    public void HandlePageTurn(int pageTurn) {
        Debug.Log($"current page + pageTurn is {currentPage + 1}");
        Debug.Log(currentPage + pageTurn >= pages.Count);
        if (currentPage + pageTurn < 0 || currentPage + pageTurn >= pages.Count) return;
        pages[currentPage].SetActive(false);
        currentPage+= pageTurn;
        pages[currentPage].SetActive(true);
        SetPageText();
    }

    private void SetPageText() {
        pageCountText.SetText($"{currentPage + 1}/{pages.Count}");
    }

    public void TEST_FILL_GROUP() {
        for (int i = 0; i < 20; i++)
            AddToPartyList($"{i} member");        
    }
    
    public string UserName { get; set; }
    
    public void AllowGroupChange() {
        Network.Enqueue(new C.SwitchGroup { AllowGroup = allowGroupToggle.isOn});
    }

    public void LeaveParty() {
        ClearPartyListAndMemberSlots();
        Network.Enqueue(new C.DelMember() {Name = UserName});
    }
    
    public void ConfirmRemovePlayerFromParty() {}

    public void RemoveMemberFromParty() {
        Network.Enqueue(new C.DelMember { Name = inputPlayerName.text});
    }

    public void RemoveMemberFromParty(string playerName) {
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
        receiveInviteWindow.SetActive(false);
    }

    public void ReceiveInvite(string fromPlayer) {
        receiveInviteWindow.transform.GetChild(2).GetComponent<TextMeshProUGUI>()
            .SetText($"{fromPlayer} has invited you to join their group");
        receiveInviteWindow.SetActive(true);
    }

    public void ShowInviteWindow(string fromUser) {
        receiveInviteWindow.transform.GetChild(2).GetComponent<TextMeshProUGUI>()
            .SetText($"{fromUser} has invited you to join their group");
        invitationNoticeIcon.SetActive(true);
    }

    public void AddToPartyList(string newMember) {
        partyList.Add(newMember);
        RefreshPartyMenu();
    }


    public void RemoveFromPartyList(string member) {
        Debug.Log("RemoveFromPartyList");
        partyList.Remove(member);
        if (partyList.Count == 1) {
            RemoveFromPartyList(UserName);
        }
        RefreshPartyMenu();
    }

    public void ClearPartyListAndMemberSlots() {
        ClearMemberSlots();
        partyList.Clear();
        SetUiCollapseButtonActive();
    }

    private void ClearMemberSlots() {
        if(memberSlots.Count > 0)
            for (int i = 0; i < memberSlots.Count; i++) {
                Destroy(memberSlots[i]);
            // Destroy(uiMemberSlots[i]);
        }
        
        for(int i = 0; i < pages.Count; i++)
            Destroy(pages[i]);
        pages.Clear();
        memberSlots.Clear();
        uiMemberSlots.Clear();
    }

    private void RefreshPartyMenu() {
        ClearMemberSlots();
        GameObject currentContainer = SetNewPartyPage();
        currentContainer.SetActive(true);
        for (int i = 0; i < partyList.Count; i++) {
            if (i != 0 && i % 5 == 0) {
                currentContainer = SetNewPartyPage();
            }
            SetPartyMemberSlots(memberSlot, currentContainer, memberSlots, i);
        }
        SetUiCollapseButtonActive();
        SetPageText();
    }

    private GameObject SetNewPartyPage() {
        GameObject page = Instantiate(groupPage, memberContainer.transform);
        page.SetActive(false);
        pages.Add(page);
        return page;
    }

    private void SetUiCollapseButtonActive() {
        uiCollapsePartyButton.SetActive(partyList.Count > 0);
    }

    private void SetPartyMemberSlots(GameObject slot, GameObject parent, List<GameObject> slotList, int position) {
        slotList.Add(Instantiate(slot, parent.transform));
        GameObject kickButton = slotList[position].transform.GetChild(1).gameObject;
        GameObject nameTextField = slotList[position].transform.GetChild(0).GetChild(1).gameObject;
        nameTextField.GetComponent<TextMeshProUGUI>().SetText(partyList[position]);
        kickButton.AddComponent<PlayerSlot>().Construct(this, partyList[position], kickButton, ShouldShowKickButton(position));
    }

    private bool ShouldShowKickButton(int position) => position > 0 && partyList[0] == UserName;

    private bool IsPartyLeader() {
        return partyList.Count == 0 || UserName == partyList[0];
    }

    private bool RoomForMorePlayers() {
        return partyList.Count < 5; // Global party count?
    }

    public void AddToPopUpWindowList() {
        UiWindowController.AddToPopUpList(this);
    }

    public void ClosePopUp() {
        ReplyToPartyInvite(false);
        UiWindowController.RemoveFromPopUpList(this);
    }
}

internal class PlayerSlot : MonoBehaviour {

    public void Construct(PartyController partyController, string playerName, GameObject kickButton,
        bool shouldShowKickButton) {

        if(shouldShowKickButton)
            kickButton.GetComponent<Button>().onClick.AddListener(() 
                => partyController.RemoveMemberFromParty(playerName));
        else 
            kickButton.SetActive(false);
    }
}
