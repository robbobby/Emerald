using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*  In charge of only making and removing elements in the HudMenu
    PartyController is in charge of handling the party list
    PartyWindowController is in charge of adding and removing elements from the party window
*/

namespace UiControllers.Party {
    public class PartyHudController : MonoBehaviour {
        
        [SerializeField] private NewPartyController partyController;
        [SerializeField] private TextMeshProUGUI memberName;
        [SerializeField] private GameObject memberGrid;
        [SerializeField] private TextMeshProUGUI memberCount;
        [SerializeField] private GameObject kickButton;
        [SerializeField] private GameObject collapseButton;
        [SerializeField] private GameObject memberSlot;
        private List<GameObject> memberSlotList = new List<GameObject>();

        public void AddMember(string memberName)
        {
            GameObject newMember = Instantiate(memberSlot, memberGrid.transform);
            newMember.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(memberName);
            memberSlotList.Add(newMember);
            newMember.AddComponent<KickButtonListener>().Construct(partyController, memberName, newMember.transform.GetChild(3).gameObject);
        }
        
        
    }
    internal class KickButtonListener : MonoBehaviour
    {
        public void Construct(NewPartyController partyController, string playerName, GameObject kickButton)
        {
            // Set kick button //
            kickButton.GetComponent<Button>().onClick.AddListener(()
                => partyController.OpenRemoveMemberWindow(playerName));
            kickButton.SetActive(true);
        }
    }
}