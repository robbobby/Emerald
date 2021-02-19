using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using C = ClientPackets;
using S = ServerPackets;

public class ShopController : MonoBehaviour {
    [SerializeField] private GameObject shopWindow;
    [SerializeField] private GameObject shopItemContainer;
    [SerializeField] private GameObject shopItem;
    private List<GameObject> shopItemContainers = new List<GameObject>();
    private int currentItemPage;
    
    
    private List<UserItem> goods = new List<UserItem>();

    public void SetNpcGoods(List<UserItem> shopItems) {
        shopWindow.SetActive(true);
        GameObject currentContainer = MakeNewSellingPage();
        for (int i = 0; i < shopItems.Count; i++){
            if( i != 0 && i % 10 == 0) {
                currentContainer = MakeNewSellingPage();
                currentContainer.SetActive(false);
            }
            GameObject thing = Instantiate(shopItem, currentContainer.transform);
            thing.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().SetText(shopItems[i].Name);
        }
    }

    public void HandleChangePage(int nextPage) {
        if (currentItemPage + nextPage >= 0 && currentItemPage + nextPage < shopItemContainers.Count) {
            SetCurrentPage(currentItemPage, currentItemPage += nextPage);
        }
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
