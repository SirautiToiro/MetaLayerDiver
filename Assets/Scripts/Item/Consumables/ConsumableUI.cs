using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConsumableUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI costText;

    [SerializeField] Image image;

    private Consumable consumable;//対応しているConsumable

    /// <summary>
    /// UI初期化処理
    /// </summary>
    /// <param name="pConsumableData">元データ</param>
    /// <param name="consumable">対応するクラス</param>
    public void Init(PhysicalItemDataSO pConsumableData,Consumable consumable)
    {
        ConsumablesDataSO consumableData = pConsumableData.ConsumablesData;

        this.consumable = consumable;

        SetCost(consumableData.Cost);

        if (pConsumableData.SizeX == 1 && pConsumableData.SizeY == 1)
        {
            //1*1のアイテムなので通常のアイコン
            SetIconSprite(pConsumableData.IconSprite);
        }
        else
        {//1*1以外のサイズのアイテムなので小アイコン
            SetIconSprite(pConsumableData.MiniIconSprite);
        }
    }


    /// <summary>
    /// コスト表示の設定
    /// </summary>
    /// <param name="cost">消費アイテムの使用コスト</param>
    private void SetCost(int cost)
    {
        costText.text = cost.ToString();
    }

    private void SetIconSprite(Sprite iconSprite)
    {
        image.sprite = iconSprite;
    }
}
