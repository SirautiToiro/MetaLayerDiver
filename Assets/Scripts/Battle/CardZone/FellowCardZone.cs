using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class FellowCardZone : EffectableCardZoneBase
{
    public void Init(FieldManager _fieldManager, CardEffectExecute _cardEffectExecute)
    {
        fieldManager = _fieldManager;
        cardEffectExecute = _cardEffectExecute;
        visualizeFlag = false;
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
        if (target.effectTarget == TargetDefine.EffectTarget.Fellow)
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
        Debug.Log("FellowVisualizeOn");
    }
    public override void VisualizeCardZoneOff()
    {
        visualizeFlag = false;
        Debug.Log("FellowVisualizeOff");
    }
}
