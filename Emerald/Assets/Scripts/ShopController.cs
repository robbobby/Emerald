using System.Collections.Generic;
using Aura2API;
using TMPro;
using UnityEngine;
using C = ClientPackets;
using Image = UnityEngine.UI.Image;
using S = ServerPackets;

public class ShopController : MonoBehaviour {
    [SerializeField] private GameObject shopWindow;
    [SerializeField] private GameObject shopItemContainer;
    [SerializeField] private GameObject shopItem;
    [SerializeField] private GameObject npcDialogue;
    [SerializeField] private TextMeshProUGUI shopPageText;
    private readonly List<GameObject> shopItemContainers = new List<GameObject>();
    private int currentItemPage;
    
    
    private List<UserItem> goods = new List<UserItem>();

    public void SetNpcGoods(List<UserItem> shopItems) {
        ClearContainers();
        shopWindow.SetActive(true);
        npcDialogue.SetActive(false);
        GameObject currentContainer = MakeNewSellingPage();
        for (int i = 0; i < shopItems.Count; i++) {
            if( i != 0 && i % 10 == 0) {
                currentContainer = MakeNewSellingPage();
                currentContainer.SetActive(false);
            }
            MakeItemAndPutInWindow(shopItems[i], currentContainer);
        }
        SetPageNumberText();
    }

    private void SetPageNumberText() {
        shopPageText.SetText($"{currentItemPage + 1}/{shopItemContainers.Count}");
    }

    private void ClearContainers() {
        for (int i = 0; i < shopItemContainers.Count; i++)
            shopItemContainers[i].Destroy();
        shopItemContainers.Clear();
        currentItemPage = 0;
    }

    private void MakeItemAndPutInWindow(UserItem shopItems, GameObject currentContainer) {
        GameObject thing = Instantiate(shopItem, currentContainer.transform);
        thing.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{shopItems.Info.Image}");
        thing.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().SetText(shopItems.Name);
    }

    public void HandleChangePage(int nextPage) {
        if (currentItemPage + nextPage < 0 || currentItemPage + nextPage >= shopItemContainers.Count) return;
        SetCurrentPage(currentItemPage, currentItemPage += nextPage);
        SetPageNumberText();
    }

    private void SetCurrentPage(int oldPage, int newPage) {
        Debug.Log($"Old page is: {oldPage}");
        Debug.Log($"New page is: {newPage}");
        shopItemContainers[oldPage].SetActive(false);
        shopItemContainers[newPage].SetActive(true);
    }

    private GameObject MakeNewSellingPage() {
        GameObject itemContainer = Instantiate(shopItemContainer, shopWindow.transform);
        shopItemContainers.Add(itemContainer);
        return itemContainer;
    }
}
