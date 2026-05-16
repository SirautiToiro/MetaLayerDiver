using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TreasuresOrDropsUI : MonoBehaviour
{
    [SerializeField] Image image;

    private TreasuresOrDrops treasuresOrDrops; // 対応しているTrerasuresOrDrops

    public void Init(PhysicalItemDataSO pConsumableData,TreasuresOrDrops td)
    {
        TreasuresOrDropsDataSO treasuresOrDropsData = pConsumableData.TreasuresOrDropsData;
        treasuresOrDrops = td;

        //表示すべき事項が少ないので、全て通常アイコンで表示
        SetIconSprite(pConsumableData.IconSprite);

        //画像のサイズのうち最大のものに合わせてサイズを拡大する
        int max = 1;
        if(pConsumableData.SizeX >=max)
        {
            max = pConsumableData.SizeX;
        }
        if (pConsumableData.SizeY >= max)
        {
            max = pConsumableData.SizeY;
        }

        Vector3 scale = this.transform.localScale;
        this.transform.localScale = new Vector3(scale.x * max, scale.y * max, scale.z);
    }

    private void SetIconSprite(Sprite iconSprite)
    {
        //ImageのpreserveAspectを使用しているのでアスペクトを維持しながら表示する
        image.sprite = iconSprite;
    }
}
