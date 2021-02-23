using TMPro;
using UnityEngine;

/*  In charge of only making and removing elements in the HudMenu
    PartyController is in charge of handling the party list
    PartyWindowController is in charge of adding and removing elements from the party window
*/

namespace UiControllers.Party {
    public class PartyHudController : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI memberName;
        [SerializeField] private GameObject memberGrid;
        [SerializeField] private TextMeshProUGUI memberCount;
        [SerializeField] private GameObject kickButton;
        [SerializeField] private GameObject collapseButton;
    }
}