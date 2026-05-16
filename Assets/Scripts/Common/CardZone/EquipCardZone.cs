using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipCardZone : MonoBehaviour, IPhysicalItemZone
{
    //配置される場所
    private IPhysicalItemHolder itemHolder;//どのカード設置場所に配置されているか
    public IPhysicalItemHolder ItemHolder
    {
        get { return itemHolder; }
        set { itemHolder = value; }
    }

    private int _posX;
    private int _posY;

    private PhysicalItemTypeDefine.PhysicalItemType _itemType; //このカードゾーンに配置されるアイテムの種類

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="holder">これを管理するIPhysicalItemHolder</param>
    /// <param name="posX">この装備場所の位置X</param>
    /// <param name="posY">この装備場所の位置X</param>
    /// <param name="itemType">配置されるべきアイテムの種類</param>
    public void Init(IPhysicalItemHolder holder,int posX,int posY, PhysicalItemTypeDefine.PhysicalItemType itemType)
    {
        itemHolder = holder;
        _posX = posX;
        _posY = posY;
        _itemType = itemType;
    }

    public Vector3 GetPosition()
    {
        return this.gameObject.transform.position;
    }

    public Transform GetTransform()
    {
        return this.gameObject.transform;
    }

    public (int x,int y) GetPos()
    {
        return (_posX, _posY);
    }

    public PhysicalItemTypeDefine.PhysicalItemType GetItemType()
    {
        return _itemType;
    }
}
