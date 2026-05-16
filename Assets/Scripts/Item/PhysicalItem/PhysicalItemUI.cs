using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysicalItemUI : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] GameObject stackNumObject;
    [SerializeField] TextMeshProUGUI stackNumText;
    [SerializeField] GameObject stackMiniNumObject;
    [SerializeField] TextMeshProUGUI stackMiniNumText;

    private PhysicalItemBase mainItem;

    /// <summary>
    /// 見た目の初期化処理
    /// </summary>
    /// <param name="itemData">アイテムのデータ</param>
    public void Init(PhysicalItemDataSO itemData,int stackNum,PhysicalItemBase item)
    {
        mainItem = item;

        itemImage.sprite=itemData.IconSprite;
        SetStack(stackNum);
    }

    public void ChangeItemSize(int sizeX, int sizeY)
    {
        //スタック数表示の変更
        if (sizeX == 1 && sizeY == 1)
        {//ミニサイズへの変更なら
            if (mainItem is not PhysicalItem1_1)
            {//1*1以外のアイテムかを一応チェック
                stackNumObject.SetActive(false);
                if (stackMiniNumObject is not null) stackMiniNumObject.SetActive(true);
            }
        }
        else
        {//ミニサイズから戻すなら
            if (mainItem is not PhysicalItem1_1)
            {//1*1以外のアイテムかを一応チェック
                stackNumObject.SetActive(true);
                if (stackMiniNumObject is not null) stackMiniNumObject.SetActive(false);
            }
        }

        ((RectTransform)this.gameObject.transform).sizeDelta =
            new Vector2(sizeX * ItemArrangeConstants.ItemCellSize, sizeY * ItemArrangeConstants.ItemCellSize);
    }

    public void ChangeIcon(Sprite iconSprite)
    {
        itemImage.sprite = iconSprite;
    }

    public void SetStack(int stackNum)
    {
        if (stackNum == 0)
        {
            stackNumText.text = stackNum.ToString();
            stackNumObject.SetActive(false);
            if (mainItem is not PhysicalItem1_1)
            {//1*1のアイテムでないなら
                stackMiniNumText.text = stackNum.ToString();
                stackMiniNumObject.SetActive(false);
            }
        }
        else
        {
            stackNumText.text = stackNum.ToString();

            if (mainItem is not PhysicalItem1_1)
            {//1*1のアイテムでないなら
                stackMiniNumText.text = stackNum.ToString();
                stackMiniNumObject.SetActive(false);
            }
        }
    }
}
