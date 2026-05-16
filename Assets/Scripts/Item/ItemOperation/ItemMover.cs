using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// アイテムの移動動作を処理するクラス
/// </summary>
public class ItemMover
{
    private ItemBase _item;//紐づいているアイテム

    private bool movingFlag = false;//移動中かどうかのフラグ

    private bool onlyRightClickFlag;//右クリックのみのフラグ

    /// <summary>
    /// コンストラクタ。
    /// 第二引数にtrueを指定すると、右クリックのみの動作を許可する。
    /// </summary>
    /// <param name="item">紐づくアイテム</param>
    /// <param name="onlyRightClickFlag">trueで、右クリック動作のみを許可</param>
    public ItemMover(ItemBase item, bool onlyRightClickFlag = false)
    {
        _item = item;
        movingFlag = false;
        this.onlyRightClickFlag = onlyRightClickFlag;
    }

    /// <summary>
    /// アイテムの移動中処理
    /// </summary>
    public void UpdateItemMover()
    {
        if (movingFlag)
        {//移動中なら
            // マウス位置を取得
            Vector2 tapPos = Input.mousePosition;
            Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(tapPos);

            Move(mouseWorldPoint);//アイテムを動かす

            //アイテムの呼出元によって異なる処理
            _item.itemManager.UpdateItemDragging();
        }
    }

    /// <summary>
    /// アイテム交換での移動処理
    /// 移動を開始するだけ
    /// </summary>
    public void StartSwitchMoving()
    {
        movingFlag = true;
    }

    /// <summary>
    /// アイテムがクリックされた時の処理
    /// 右クリックで詳細表示
    /// 左クリックでドラッグ開始
    /// Shift+左クリックでクイック移動
    /// </summary>
    /// <param name="eventData"></param>
    public void ItemOnClicked(PointerEventData eventData)
    {
        if (_item.clickableFlag)
        {//操作可能なら
            if (eventData.button == PointerEventData.InputButton.Right)
            {//右クリックだった場合
                if (movingFlag) return;//移動中なら何もしない
                _item.ItemRightClick.OnRightClick(_item);//アイテムの右クリック動作を実行
            }
            else if ((eventData.button == PointerEventData.InputButton.Left)
                && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                &&!onlyRightClickFlag)
            {//Shift+Clickだった場合クイック移動
                if (movingFlag) return;//移動中なら何もしない
                _item.itemManager.QuickMove(_item);
            }
            else if (eventData.button == PointerEventData.InputButton.Left &&
                !movingFlag && !onlyRightClickFlag)
            {//左クリックかつ移動していない状態だった場合、移動開始
                movingFlag = true;
                _item.itemManager.StartDragging(_item);//アイテムマネージャーにドラッグ開始を通知
            }
            else if (eventData.button == PointerEventData.InputButton.Left && movingFlag
                && !onlyRightClickFlag)
            {//左クリックで移動中なら、移動を終了
                bool result = _item.itemManager.EndDragging();//アイテムマネージャーにドラッグ終了を通知
                if (result)
                {//ドラッグ終了に成功したなら、移動フラグをfalseに
                    movingFlag = false;
                }
            }
        }
    }

    /// <summary>
    /// アイテム分割などが発生した際に、
    /// そのアイテムの移動を発生させる
    /// </summary>
    public void StartDraggingForcibly()
    {
        movingFlag = true;
    }

    /// <summary>
    /// 移動の強制終了
    /// </summary>
    public void EndDraggingForcibly()
    {
        movingFlag = false;
    }

    /// <summary>
    /// posの位置にアイテムを動かす
    /// </summary>
    /// <param name="pos">移動先の位置</param>
    private void Move(Vector2 pos)
    {
        ((RectTransform)_item.gameObject.transform).position = pos;
    }

    public bool TestGetMovingFlag()
    {
        return movingFlag;
    }
}
