using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// プレイヤー自身のカードゾーン
/// </summary>
public class PlayerCardZone : EffectableCardZoneBase
{
    private Player player;

    //ハイライト時に表示するフレーム
    [SerializeField] private Canvas highlightFrameCanvas;

    //初期化
    public void Init(FieldManager _fieldManager,Player _player, CardEffectExecute _cardEffectExecute, HandManager _handManager)
    {
        fieldManager = _fieldManager;
        player = _player;
        cardEffectExecute = _cardEffectExecute;
        visualizeFlag = false;
        highlightFrameCanvas.enabled = false;
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
    public  override bool isEffectableToThis(TargetDefine target)
    {
        if (target.effectTarget == TargetDefine.EffectTarget.Self)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// カード効果の処理。
    /// ダメージ演出を一本化するために、全ての効果に対して使用。
    /// Sequenceを与えられて、それに順にエフェクトを作用させる。
    /// </summary>
    /// <param name="card">使用されるカード</param>
    /// <param name="sequence">カード効果処理が乗るSequence</param>
    public override void Set(Card card, Sequence sequence)
    {
        foreach (ActualEffect effect in card.effects)
        {
            if(!cardEffectExecute.Execute(effect, card.attributes, sequence, card.pos))
            {
                break;
            }
        }
    }

    /// <summary>
    /// 武器効果の処理。
    /// ダメージ演出を一本化するために、全ての効果に対して使用。上の物は使用しない。
    /// Sequenceを与えられて、それに順にエフェクトを作用させる。
    /// </summary>
    /// <param name="weapon">使用される武器</param>
    /// <param name="sequence">カード効果処理が乗るSequence</param>
    public override void Set(Weapon weapon, Sequence sequence)
    {
        foreach (ActualEffect effect in weapon.effects)
        {
            if(!cardEffectExecute.Execute(effect, weapon.attributes, sequence, 0))
            {
                break;
            }
        }
    }

    /// <summary>
    /// 消耗品の処理
    /// </summary>
    /// <param name="consumable"></param>
    /// <param name="sequence"></param>
    public override void Set(Consumable consumable, Sequence sequence)
    {
        foreach (ActualEffect effect in consumable.Effects)
        {
            var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Physics) };
            if (!cardEffectExecute.Execute(effect, attributes, sequence, 0))
            {
                break;
            }
        }
    }

    public override void VisualizeCardZoneOn()
    {
        visualizeFlag = true;
        highlightFrameCanvas.enabled = true;
    }
    public override void VisualizeCardZoneOff()
    {
        visualizeFlag = false;
        highlightFrameCanvas.enabled = false;
    }
}
