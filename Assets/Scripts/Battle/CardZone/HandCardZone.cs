using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手札カードゾーン
/// </summary>
class HandCardZone : MonoBehaviour, ICardZone
{
    [SerializeField] private RectTransform rectTransform;//このCardZoneのrectTransform

    //位置を変更する
    public void SetPos(float x,float y)
    {
        rectTransform.anchoredPosition = new Vector3(x,y,0);
    } 

    //ローカル座標を取得する
    public Vector3 GetLocalPos()
    {
        return this.gameObject.GetComponent<RectTransform>().anchoredPosition;
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
