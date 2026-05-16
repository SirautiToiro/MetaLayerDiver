using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 敵のカードゾーンを管理する。敵全体にカードを使用するときなどに使用
/// </summary>
public class EnemyCardZoneManager : MonoBehaviour
{
    [SerializeField] EnemyCardZone[] enemyCardZones;//敵のカードゾーン

    /// <summary>
    /// 全てのEnemyCardZoneを可視化する/しない
    /// </summary>
    /// <param name="b">trueで可視化</param>
    public void VisualizeAllEnemyCardZone(bool b)
    {
        if (b)
        {
            foreach (EnemyCardZone zone in enemyCardZones)
            {
                zone.VisualizeCardZoneOn();
            }
        }
        else
        {
            foreach (EnemyCardZone zone in enemyCardZones)
            {
                zone.VisualizeCardZoneOff();
            }
        }
        
    }

    /// <summary>
    /// 敵全体にカードを使用する
    /// 呼出元からSequenceを与えられる。
    /// </summary>
    /// <param name="card">使用するカード</param>
    /// <param name="sequence">カード効果処理が乗るSequence</param>
    public void SetAllEnemyCardZone(Card card,Sequence sequence)
    {
        if (card.IsItemHasTag(CardTagDefine.CardTag.EnemyAll))
        {//全体に作用する、のタグを持っているなら、敵1体ずつに効果を発揮する
            bool isSelf = false;

            foreach (ActualEffect effect in card.effects)
            {
                if (effect.effect.cardEffect == CardEffectDefine.CardEffect.NextEffectIsSelf)
                {//全体攻撃中にNextEffectIsSelfがあると、次の一つの効果を自身だけに適用する
                 //実際には、敵の先頭だけに適用する
                    isSelf = true;
                    continue;
                }

                if (isSelf)
                {//NextEffectIsSelfの次なら
                    enemyCardZones[0].Set(effect, card.attributes, sequence, card.pos);
                }
                else
                {
                    for (int i = enemyCardZones.Length - 1; i >= 0; i--)
                    {
                        enemyCardZones[i].Set(effect, card.attributes, sequence, card.pos);
                    }
                    isSelf = false;
                }
            }

            sequence.AppendCallback(() =>
            {
                //カードの効果が全て終わった後に、敵の死亡判定を行う
                for (int i = enemyCardZones.Length - 1; i >= 0; i--)
                {//死亡の判定
                 //敵がいるかの判定
                    if (enemyCardZones[i].GetEnemy() == null) continue;

                    enemyCardZones[i].CheckDead(sequence);
                }
            });
        }
        else
        {//ランダムな敵に発生する効果などは、一旦最初の敵だけに効果を与える(CardEffectExecuteでランダム化する)
            enemyCardZones[0].Set(card, sequence);
        }
        
    }

    /// <summary>
    /// 敵全体に武器を使用する
    /// </summary>
    /// <param name="card">使用するカード</param>
    /// <param name="sequence">カード効果処理が乗るSequence</param>
    public void SetAllEnemyCardZone(Weapon weapon, Sequence sequence)
    {
        bool isSelf = false;

        foreach (ActualEffect effect in weapon.effects)
        {
            if (effect.effect.cardEffect == CardEffectDefine.CardEffect.NextEffectIsSelf)
            {//全体攻撃中にNextEffectIsSelfがあると、次の一つの効果を自身だけに適用する
                //実際には、敵の先頭だけに適用する
                isSelf = true;
                continue;
            }

            if (isSelf)
            {//NextEffectIsSelfの次なら
                enemyCardZones[0].Set(effect, weapon.attributes, sequence, 0);
            }
            else
            {
                for (int i = enemyCardZones.Length - 1; i >= 0; i--)
                {
                    enemyCardZones[i].Set(effect, weapon.attributes, sequence, 0);
                }
                isSelf = false;
            }
        }

        sequence.AppendCallback(() =>
        {//カード効果演出が全て終わった後に死亡判定
            for (int i = enemyCardZones.Length - 1; i >= 0; i--)
            {//死亡の判定
             //敵がいるかの判定
                if (enemyCardZones[i].GetEnemy() == null) continue;

                enemyCardZones[i].CheckDead(sequence);
            }
        });
    }
}
