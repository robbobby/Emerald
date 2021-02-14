using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MirQuickCell : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    protected static GameSceneManager GameScene
    {
        get { return GameManager.GameScene; }
    }
    
    [SerializeField]
    Image IconImage;

    IQuickSlotItem item;
    public IQuickSlotItem Item
    {
        get { return item; }
        set
        {
            item = value;

            if (value == null)
                IconImage.color = Color.clear;
            else
            {
                IconImage.sprite = value.GetIcon();
                IconImage.color = Color.white;
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (GameScene.SelectedCell != null)
            Item = GameScene.SelectedCell;

        GameScene.SelectedCell = null;
        GameScene.PickedUpGold = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        DoAction();
    }

    public void DoAction()
    {
        if (item == null) return;
        item.DoAction();
    }
}
