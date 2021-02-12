using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MirQuickCell : MonoBehaviour, IPointerUpHandler
{
    protected static GameSceneManager GameScene
    {
        get { return GameManager.GameScene; }
    }

    [SerializeField]
    Image IconImage;

    private IQuickSlotItem item;
    public IQuickSlotItem Item
    {
        get { return item; }
        set
        {
            item = value;

            if (item != null)
            {
                IconImage.sprite = item.GetIcon();
                IconImage.color = Color.white;
            }
            else
                IconImage.color = Color.clear;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Up");
        if (GameScene.PickedUpGold || GameScene.SelectedCell == null) return;

        Item = GameScene.SelectedCell;
    }
}
