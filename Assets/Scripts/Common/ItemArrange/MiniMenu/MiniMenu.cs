using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物理アイテム操作画面で右クリックすると表示されるメニュー
/// </summary>
public class MiniMenu : MonoBehaviour
{
    [SerializeField] RectTransform thisRectTransform;
    [SerializeField] GameObject miniMenuTipPrefab;
    [SerializeField] LayoutGroup layoutGroup; //ボタンの配置を制御するLayoutGroup

    private PhysicalItemArrangeManager physicalItemArrangeManager;

    private PhysicalItemBase targetItem; //メニューが表示されているアイテム

    private Camera mainCamera;

    public void Update()
    {
        //エスケープキーが押された場合自身を削除
        //右クリックで削除
        if ((Input.GetKey(KeyCode.Escape) || Input.GetMouseButton(1)))
        {
            CloseMenu();
        }

        //左クリックで削除(自身をクリックしていない場合)
        if (Input.GetMouseButtonDown(0))
        {
            //自身をクリックしていない場合閉じる
            if (!RectTransformUtility.RectangleContainsScreenPoint(thisRectTransform, Input.mousePosition,mainCamera))
            {
                CloseMenu();
            }
        }
    }

    public void Init(PhysicalItemArrangeManager physicalItemArrangeManager,List<MiniMenuTipDefine.MiniMenuTipType> buttonTypeList, PhysicalItemBase item,Camera camera)
    {
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        mainCamera = camera;

        targetItem = item;

        foreach (MiniMenuTipDefine.MiniMenuTipType type in buttonTypeList)
        {
            //miniMenuのボタンを生成
            InstantiateMiniMenuTip(type);
        }
    }

    /// <summary>
    /// アイテムの分割。
    /// miniMenuTipから呼出。
    /// </summary>
    public void DivideItem()
    {
        CloseMenu();//ミニメニューを閉じる
        physicalItemArrangeManager.ShowDivideMenu(targetItem);//分割メニューを表示する
    }

    /// <summary>
    /// アイテムの詳細の表示。
    /// miniMenuTipから呼出。
    /// </summary>
    public void DescribeItem()
    {
        CloseMenu();//ミニメニューを閉じる
        //アイテムの詳細を表示
        physicalItemArrangeManager.ShowDescription(targetItem.BaseItemData);
    }

    /// <summary>
    /// アイテムの使用。
    /// miniMenuTipから呼出。
    /// </summary>
    public void UseItem()
    {
        CloseMenu();//ミニメニューを閉じる
        physicalItemArrangeManager.UseItem(targetItem);
    }

    /// <summary>
    /// miniMenuのボタンの一つを生成し、初期化
    /// </summary>
    /// <param name="type">ボタンのタイプ</param>
    private void InstantiateMiniMenuTip(MiniMenuTipDefine.MiniMenuTipType type)
    {
        
        MiniMenuTip menuTip = Instantiate(miniMenuTipPrefab, gameObject.transform.position,
            Quaternion.identity,
            gameObject.transform.transform).GetComponent<MiniMenuTip>();

        menuTip.Init(this,type);//MiniMenuTipの初期化
    }

    /// <summary>
    /// メニューを閉じる
    /// </summary>
    private void CloseMenu()
    {
        physicalItemArrangeManager.CloseMiniMenu();//呼出元に終了を通知

        Destroy(this.gameObject);//自身を削除
    }
}
