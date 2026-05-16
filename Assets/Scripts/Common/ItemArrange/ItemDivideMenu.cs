using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDivideMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemDivideNumText; //分割するアイテムの最大個数、分割数を表示するテキスト

    [SerializeField] private Slider slider;

    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;

    private int itemMaxNum;
    private PhysicalItemBase targetItem; //分割対象のアイテム

    public void Update()
    {
        //エスケープキーが押された場合閉じる
        //右クリックで閉じる
        if ((Input.GetKey(KeyCode.Escape) || Input.GetMouseButton(1)))
        {
            CanselButton();
        }
    }

    public void Init(PhysicalItemBase item)
    {
        itemMaxNum = item.Stack; //分割するアイテムの個数を取得
        slider.maxValue = itemMaxNum - 1; //スライダーの最大値はアイテムの個数-1(必ず分割するため)
        slider.value = 1;//最初は1つ分割
        targetItem = item;

        if(itemMaxNum <= 2)
        {
            //1つしか分割できない場合、スライダーを非表示
            slider.gameObject.SetActive(false);
        }
        else
        {
            //スライダーを表示
            slider.gameObject.SetActive(true);
        }

        //分割数を表示する
        itemDivideNumText.text = $"{slider.value} / {itemMaxNum}";
    }

    public void OnValueChanged()
    {
        //スライダーの値が変わったときに分割数を表示する
        itemDivideNumText.text = $"{slider.value} / {itemMaxNum}";
    }

    public void CanselButton()
    {
        physicalItemArrangeManager.CloseDivideMenu(); //アイテム分割メニューを閉じる
    }

    public void DicideButton()
    {
        physicalItemArrangeManager.DivideItem(targetItem,(int)slider.value);
    }
}
