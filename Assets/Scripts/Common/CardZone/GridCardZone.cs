using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PhysicalItemがGrid上に配置されるときの配置場所
/// </summary>
public class GridCardZone : MonoBehaviour,IPhysicalItemZone
{
    //配置される場所
    private IPhysicalItemHolder itemHolder;//どのカード設置場所に配置されているか
    public IPhysicalItemHolder ItemHolder
    {
        get { return itemHolder; }
        set { itemHolder = value; }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="holder">これを管理するIPhysicalItemHolder</param>
    public void Init(IPhysicalItemHolder holder)
    {
        itemHolder = holder;
    }

    public Vector3 GetPosition()
    {
        return this.gameObject.transform.position;
    }

    public Transform GetTransform()
    {
        return this.gameObject.transform;
    }
}
