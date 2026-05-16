using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手札や武器などのカードをドラッグしている時に呼出、
/// draggingParentTransformに、ドラッグしているオブジェクトの親を設定し、
/// 一番前に表示されるようにする
/// </summary>
public class ParentManagerInDragging : MonoBehaviour
{
    //ドラッグ中はペアレントをこの場所に設定する
    //[SerializeField] Transform draggingParentTransform;
    [SerializeField] PlaceCardZone parentCardZone;

    //ドラッグしているオブジェクトがどこにあるべきものか
    //private Transform defaultTransform;

    //ドラッグしているアイテムの元あった位置
    private ICardZone defaultZone;

    //ドラッグしているアイテム
    private ItemBase draggingItem;

    //ドラッグしているオブジェクトのGameObject
    //private GameObject draggingObject;

    public void SetParent(ItemBase item)
    {
        draggingItem = item;
        defaultZone = item.CurrentZone;
        item.SetCardZone(parentCardZone);
    }

    /// <summary>
    /// ドラッグしていたオブジェクト
    /// を元の場所に戻す
    /// </summary>
    public void ReturnToBaseParent()
    {
        draggingItem.SetCardZone(defaultZone);
    }


    /// <summary>
    /// ドラッグしていたオブジェクトの所属しているCardZoneを変更する
    /// </summary>
    /// <param name="cardZone">変更先のCardZone</param>
    public void ChangeParent(ICardZone cardZone)
    {
        draggingItem.SetCardZone(cardZone);
    }

    /// <summary>
    /// 親のCardZoneのGameObjectを取得
    /// </summary>
    /// <returns> 親のCardZoneのGameObject</returns>
    public GameObject GetParentGameObject()
    {
        return parentCardZone.gameObject;
    }

    public PlaceCardZone GetParentCardZone()
    {
        return parentCardZone;
    }
}
