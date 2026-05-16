using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OverlappedCardzoneGetter
{
    private Camera mainCamera; // 所属しているシーンのカメラ。コンストラクタで取得

    private ParentManagerInDragging parentManagerInDragging; // ドラッグ時に親を最前に操作する機能

    //範囲制限をする場所を操作する場合に、その範囲(TODO:使用しないことになった？)
    private List<GameObject> viewportGameObjectList;

    /// <summary>
    /// コンストラクタ。
    /// viewportGameObjectListはオプションで、nullを許容する。
    /// </summary>
    /// <param name="c">シーンのメインカメラ</param>
    /// <param name="pmd">親操作のクラス</param>
    /// <param name="vgo">範囲制限のgameObject</param>
    public OverlappedCardzoneGetter(Camera c, ParentManagerInDragging pmd, List<GameObject> vgo = null)
    {
        mainCamera = c;
        parentManagerInDragging = pmd;
        viewportGameObjectList = vgo;
    }

#nullable enable

    /// <summary>
    /// ドラッグ中のアイテムが重なっているICardZoneを取得する。
    /// </summary>
    /// <typeparam name="T">ICardZoneを継承した型。重なっているこれを持つものを探す</typeparam>
    /// <param name="draggingItem">ドラッグしているItemBase</param>
    /// <returns>発見されたICardZone.null許容</returns>
    public T? GetOverlappedCardZone<T>(ItemBase draggingItem)where T: class,ICardZone
    {
        //マウスのスクリーン座標を取得する
        // マウス位置を取得
        Vector2 tapPos = Input.mousePosition;
        Vector3 mouseScreenPoint = new Vector3(tapPos.x, tapPos.y, 10);

        // メインカメラから上記で取得した座標に向けてRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPoint);

        

        List<T> hitZoneList = new List<T>();

        //ドラッグ中のカードがScrollに従って配置される場合、さらにscrollViewにも重なっていなければならない
        List<GameObject> scrollViewList = new List<GameObject>();

        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, 10.0f))
        {
            if (!hit.collider)
            {//コライダーのないオブジェクトなら
                continue;
            }

            //コライダーからオブジェクトを取得
            var hitObj = hit.collider.gameObject;

            // 当たったオブジェクトがドラッグ中のアイテムと同一ならスキップ
            if (draggingItem != null && hitObj == draggingItem.gameObject)
            {
                continue;
            }

            if (hitObj == parentManagerInDragging.GetParentGameObject())
            {//カード一時配置用のオブジェクトでも良くない
                continue;
            }

            

            /*
            //範囲制限は行わないこととする
            if(viewportGameObjectList != null)
            {
                foreach (GameObject ob in viewportGameObjectList)
                {//Viewportに接触しているなら
                    if (hitObj == ob)
                    {//viewportの内側のオブジェクトに接触している
                        hitOnViewportFlag = true;
                    }
                }
            }
            else
            {//範囲内制限をしない場合は常にtrue
                hitOnViewportFlag = true;
            }*/

            //ScrollViewの当たり判定をもつオブジェクトにアタッチされているものがあるか
            var hitScrollView = hitObj.GetComponent<ScrollViewColliderTarget>();
            if(hitScrollView != null)
            {//ScrollViewなら、カードがScrollに従って配置される場合に必要
                scrollViewList.Add(hitObj);
                continue;
            }

            // オブジェクトが対象のCardZoneなら、次へ
            var hitZone = hitObj.GetComponent<T>();
            if (hitZone != null)
            {
                

                hitZoneList.Add(hitZone);
                continue;
            }
        }

        //viewportの内側のオブジェクトに接触しているかのフラグ
        bool hitOnViewportFlag = false;
        if(draggingItem is Card)
        {//viewportの内側のオブジェクトに接触しているかを確認する
            hitOnViewportFlag = false;
        }
        else
        {//viewportの内側のオブジェクトに接触しているかを確認しない
            hitOnViewportFlag = true;
        }

        //PlaceCardZoneが対応するScrollViewに重なっているかの確認
        foreach (var hitZone in hitZoneList)
        {
            foreach (GameObject scrollView in scrollViewList)
            {
                if (hitZone is PlaceCardZone placeCardZone &&
                    ReferenceEquals(placeCardZone.CardHolder.GetScrollViewObject(),scrollView))
                {//PlaceCardZoneが対応するScrollViewに重なっているなら
                    //判定成功
                    hitOnViewportFlag = true;
                }
            }
        }

        if (hitZoneList.Count > 0 && hitOnViewportFlag)
        {
            //重なっているCardZoneが複数ある場合は、最も近いものを返す
            Vector3 mouseWorldPoint = mainCamera.ScreenToWorldPoint(mouseScreenPoint);

            float minDistance = float.MaxValue;
            T? targetZone = null;

            foreach (var zone in hitZoneList)
            {
                // ドラッグ中のアイテムと重なっているICardZoneの距離を計算
                float distance = Vector3.Distance(mouseWorldPoint, zone.GetPosition());
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetZone = zone; // 最も近いICardZoneを更新
                }
            }

            return targetZone; // 最も近いICardZoneを返す
        }
        else
        {
            return null; // 重なっているICardZoneがない場合はnullを返す
        }
    }

#nullable disable
}
