using System;
using System.Collections.Generic;
using System.Linq;
using Aura2API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UiControllers.Party
{
    public class PartyWindowController : MonoBehaviour
    {
        private const int MEMBERS_PER_PAGE = 5;
        [SerializeField] private NewPartyController partyController;
        [SerializeField] private GameObject allowGroupToggle;
        [SerializeField] private GameObject memberSlot;
        [SerializeField] private GameObject memberPage;
        [SerializeField] private TextMeshProUGUI pageCountText;

        private List<GameObject> memberSlotList = new List<GameObject>();
        private List<GameObject> pages = new List<GameObject>();
        private int currentPage = 1;

        public void HandlePageTurn(int pageTurn)
        {
            Debug.Log(memberSlotList.Count);
            Debug.Log(memberSlotList.Count / MEMBERS_PER_PAGE);
            
            if (memberSlotList.Count < 5) return;
            if (currentPage + pageTurn <= 0 || currentPage + pageTurn > memberSlotList.Count / MEMBERS_PER_PAGE) return;
            currentPage += pageTurn;
            RefreshPartyMemberPage();
        }
        
        public void TEST_LOAD_MEMBERS() {
            for (int i = 0; i < 11; i++) 
                AddMember($"{i} member");
        }

        private void RefreshPartyMemberPage()
        {
            for (int i = 0; i < memberSlotList.Count; i++)
            {
                memberSlotList[i].SetActive(false);
            }
            int startPosition = (currentPage * 5) - MEMBERS_PER_PAGE;
            int finishPosition = currentPage * 5;
            for (int i = startPosition; i < finishPosition; i++)
            {
                Debug.Log(i);
                if (i >= memberSlotList.Count) return;
                memberSlotList[i].SetActive(true);
            }
        }

        private void SetPageText()
        {
            pageCountText.SetText($"{currentPage}/{pages.Count}");
        }

        public void AllowGroupToggle(Toggle allowGroup)
        {
            partyController.CmdAllowGroupChange(allowGroup);
        }

        public void OpenInviteWindow()
        {
            partyController.OpenInviteWindow();
        }

        public void OpenLeaveButtonWindow()
        {
            partyController.OpenLeaveWindow();
        }

        public void OpenRemoveMemberWindow()
        {
            // if (selectedMember.Length <= 3) return;
            partyController.OpenRemoveMemberWindow();
        }

        public void ClearMembers()
        {
            for (int i = 0; i < memberSlotList.Count; i++)
            {
                memberSlotList[i].Destroy();
            }
        }

        public void AddMember(string memberName)
        {
            GameObject newMember = Instantiate(memberSlot, memberPage.transform);
            newMember.SetActive(true);
            newMember.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(memberName);
            memberSlotList.Add(newMember);
            newMember.AddComponent<PlayerSlotListeners>()
                .Construct(partyController, memberName, newMember);
            RefreshPartyMemberPage();
        }
        
        public void RemoveMemberSlot(int index) {
            memberSlotList[index].Destroy();
            memberSlotList.RemoveAt(index);
            RefreshPartyMemberPage();
        }
    }

    internal class PlayerSlotListeners : MonoBehaviour
    {
        public void Construct(NewPartyController partyController, string playerName, GameObject memberSlot)
        {
            memberSlot.GetComponent<Button>().onClick.AddListener(() =>
            {
                partyController.currentSelectedMember = playerName;
            });
        }
    }
}