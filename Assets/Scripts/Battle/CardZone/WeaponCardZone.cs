using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeaponCardZone : EffectableCardZoneBase
{
    private Weapon weapon;//紐づいている武器

    public void Init(FieldManager _fieldManager,Weapon _weapon, CardEffectExecute _cardEffectExecute,HandManager _handManager)
    {
        fieldManager = _fieldManager;
        weapon = _weapon;
        cardEffectExecute = _cardEffectExecute;
        visualizeFlag = false;
        //TODO ハイライトフレーム
        handManager = _handManager;
    }

    public override bool isEffectableToThis(Card card)
    {
        return isEffectableToThis(card.effectTarget);
    }

    public override bool isEffectableToThis(Weapon weapon)
    {
        return isEffectableToThis(weapon.effectTarget);
    }
    public override bool isEffectableToThis(Consumable consumable)
    {
        return isEffectableToThis(consumable.effectTarget);
    }

    public override bool isEffectableToThis(TargetDefine target)
    {
        if (target.effectTarget == TargetDefine.EffectTarget.Weapon)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Set(Card card,Sequence sequence)
    {

    }

    public override void Set(Weapon weapon, Sequence sequence)
    {

    }
    public override void Set(Consumable consumable, Sequence sequence)
    {

    }

    public override void VisualizeCardZoneOn()
    {
        visualizeFlag = true;
        Debug.Log("WeaponVisualizeOn");
    }
    public override void VisualizeCardZoneOff()
    {
        visualizeFlag = false;
        Debug.Log("WeaponVisualizeOn");
    }
}
