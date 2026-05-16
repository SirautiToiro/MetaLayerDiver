using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// インベントリ中などのカードが単に配置のみされる場所の定義.
/// ScrollViewに配置されるカードのみの場所
/// </summary>
public class PlaceCardZone : MonoBehaviour, ICardZone
{
    //配置される場所
    private ICardHolder cardHolder;//どのカード設置場所に配置されているか
    public ICardHolder CardHolder { get { return cardHolder; } 
        set { cardHolder = value;} 
    }

    //ScrollView内での位置
    private int pos;
    public int Pos { get { return pos; } set { pos = value; } }

    public void Init(ICardHolder _holder,int _pos)
    {
        pos = _pos;
        cardHolder = _holder;
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
