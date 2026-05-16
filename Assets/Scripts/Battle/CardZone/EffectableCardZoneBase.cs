using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// カード効果の発動対象として指定可能なCardZone
/// </summary>
public abstract class EffectableCardZoneBase : MonoBehaviour, ICardZone
{
    public FieldManager fieldManager { get; set; }

    public bool visualizeFlag { get; set; }

    //カード実行。Initで紐づけ
    public CardEffectExecute cardEffectExecute;

    public HandManager handManager;

    //
    //Initはそれぞれで異なる引数を持つので個々に実装
    //

    //そのカードがこのカードゾーンに干渉可能かを判定
    public abstract bool isEffectableToThis(Card card);
    //武器に対しても干渉可能かの判定を行う
    public abstract bool isEffectableToThis(Weapon weapon);
    public abstract bool isEffectableToThis(Consumable consumable);

    //アイテムが使用可能かの判定
    public bool isEffectableToThis(ItemBase item)
    {
        if(item is Weapon)
        {
            return isEffectableToThis((Weapon)item);
        }
        else if (item is Card){
            return isEffectableToThis((Card)item);
        }else if(item is Consumable)
        {
            return isEffectableToThis((Consumable)item);
        }
        return false;
    }

    //干渉判定を一元管理するための関数
    public abstract bool isEffectableToThis(TargetDefine target);

    //カードや武器の効果をカードゾーンにセットして発動させる
    //効果は、与えられたsequenceの上に乗る
    public abstract void Set(Card card,Sequence sequence);
    public abstract void Set(Weapon weapon, Sequence sequence);
    public abstract void Set(Consumable consumable, Sequence sequence);

    public abstract void VisualizeCardZoneOn();
    public abstract void VisualizeCardZoneOff();

    public Vector3 GetPosition()
    {
        return this.gameObject.transform.position;
    }

    public Transform GetTransform()
    {
        return this.gameObject.transform;
    }
}
