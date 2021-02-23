using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using C = ClientPackets;
using Network = Emerald.Network;

namespace UiControllers.Party {
    public class NewPartyController : MonoBehaviour {
        [SerializeField] private PartyHudController partyHudController;
        [SerializeField] private PartyWindowController partyWindowController;
        [SerializeField] private MirMessageBox messageBox;
        [SerializeField] private GameObject inviteWindow;
        [SerializeField] private GameObject invitationNoticeIcon;
        private readonly List<string> partyList = new List<string>();
        [SerializeField] public string currentSelectedMember;
        public object removeMemberWindow;
        public string UserName { get; internal set; } = "";

        public void RpcReceiveInvite(string fromUser) {
            messageBox.Show($"{fromUser} has invites you to join their group", okbutton: true, cancelbutton: true);
            messageBox.Cancel += () => CmdReplyToInvite(false);
            messageBox.OK += () => CmdReplyToInvite(true);
        }

        public void OpenInviteWindow()
        {
            if (!IsPartyLeader()) return;
            messageBox.Show("Invite member: ", true, true, true, "Invite member:");
            messageBox.OK += SendInviteToPlayer;
        }

        private void SendInviteToPlayer()
        {
            string playerToInvite = messageBox.InputField.GetComponent<TMP_InputField>().text;
            if (playerToInvite.Length < 3) return;
            if (playerToInvite == UserName) return;
            CmdSendInviteToPlayer(playerToInvite);
        }

        public void OpenRemoveMemberWindow()
        {
            if (!IsPartyLeader() || currentSelectedMember.Length < 3) return;
            messageBox.Show($"Are you sure you want to remove {currentSelectedMember}?", true, true);
            messageBox.OK += () => CmdRemoveMemberFromParty(currentSelectedMember);
            messageBox.Cancel += () => currentSelectedMember = String.Empty;
        }

        public void OpenLeaveWindow()
        {
            if (partyList.Count <= 0) return;
            messageBox.Show("Are you sure you want to leave your group?", true, true);
            messageBox.OK += CmdLeaveParty;
        }

        private bool IsPartyLeader()
        {
            return partyList.Count == 0 || UserName == partyList[0];
        }

        private void CmdLeaveParty() // This can't be the way to do this?
        {
            Network.Enqueue(new C.SwitchAllowGroup() {AllowGroup = false});
            Network.Enqueue(new C.SwitchAllowGroup() {AllowGroup = true});
        }

        internal void CmdAllowGroupChange(bool isAllowingGroup) =>
            Network.Enqueue(new C.SwitchAllowGroup() {AllowGroup = isAllowingGroup});
        
        private void CmdRemoveMemberFromParty(string memberName) =>
            Network.Enqueue(new C.DeleteMemberFromGroup() {Name = memberName});

        private  void CmdSendInviteToPlayer(string memberName) =>
            Network.Enqueue(new C.AddMemberToGroup() {Name = memberName});
        
        private void CmdReplyToInvite(bool response) =>
            Network.Enqueue(new C.RespondeToGroupInvite() {AcceptInvite = response});

        public void RpcDeleteGroup()
        {
            Debug.Log("In the RPC Delete Group Method");
            partyList.Clear();
            partyWindowController.ClearMembers();
        }

        public void RpcDeleteMember(string memberName)
        {
            Debug.Log("Deleting member");
            int index = partyList.IndexOf(memberName);
            Debug.Log(partyList[index]);
            partyList.RemoveAt(index);
            partyWindowController.RemoveMemberSlot(index);
        }

        public void RpcAddNewMember(string memberName)
        {
            partyList.Add(memberName);
            partyWindowController.AddMember(memberName);
        }
    }
}