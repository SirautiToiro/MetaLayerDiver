using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 敵が配置されているカードゾーン
/// </summary>
public class EnemyCardZone : EffectableCardZoneBase
{
    //敵の場所、0が近距離
    [SerializeField] private int enemyPos;
    //ハイライト時に表示するフレーム
    [SerializeField] private Canvas highlightFrameCanvas;

    //紐づいている敵
    private Enemy enemy;

    //初期化
    public void Init(FieldManager _fieldManager,Enemy _enemy,CardEffectExecute _cardEffectExecute,HandManager _handManager)
    {
        fieldManager=_fieldManager;
        enemy = _enemy;
        cardEffectExecute=_cardEffectExecute;
        visualizeFlag = false;
        highlightFrameCanvas.enabled = false;
        handManager = _handManager;
    }

    /// <summary>
    /// これまで参照していたEnemyを別のEnemyで上書きする。
    /// 新しいEnemyのオブジェクトを自身の位置に移動する
    /// NULLを入れることもある？
    /// </summary>
    /// <param name="enemy">上書きする敵</param>
    public void SetEnemy(Enemy _enemy)
    {
        enemy = _enemy;
        if(enemy != null)
        {//nullでないなら移動処理
         //enemyの既に動いでいる動作を終了
            ((RectTransform)_enemy.gameObject.transform).DOComplete();

            enemy.SetCardZone(this);//enemyの紐づけを移動
            enemy.gameObject.transform.position = this.transform.position;//enemyのgameobjectを移動
        }
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
        if (enemy == null)
        {//enemyが存在しないならfalse
            return false;
        }

        bool isEffectable = true;
        //全てのStateの計算結果がtrueなら
        fieldManager.SearchAndUsePlayerState<IStateEffectableToEnemy>(a => isEffectable&=a.IsEffectable(this,target));

        if (!isEffectable) return false;

        //状態異常による攻撃可能判定が終わったので、位置による攻撃可能判定を行う

        switch (enemyPos)
        {
            case 0://近距離の敵
                if (target.effectTarget == TargetDefine.EffectTarget.ShortRange ||
                    target.effectTarget == TargetDefine.EffectTarget.MediumRange ||
                    target.effectTarget == TargetDefine.EffectTarget.LongRange ||
                    target.effectTarget == TargetDefine.EffectTarget.EnemyAll)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case 1://中距離の敵
                if (target.effectTarget == TargetDefine.EffectTarget.MediumRange ||
                    target.effectTarget == TargetDefine.EffectTarget.LongRange ||
                    target.effectTarget == TargetDefine.EffectTarget.EnemyAll)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case 2://遠距離の敵
                if (target.effectTarget == TargetDefine.EffectTarget.LongRange ||
                    target.effectTarget == TargetDefine.EffectTarget.EnemyAll)
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }

    #region Set関数のオーバーロード
    /// <summary>
    /// EnemyCardZoneManagerが全体に対してカードを発動するときに使用する。
    /// また、ダメージ演出を一本化するために、全ての効果に対して使用。上の物は使用しない。
    /// Sequenceを与えられて、それに順にエフェクトを作用させる。
    /// </summary>
    /// <param name="card">使用されるカード</param>
    /// <param name="sequence">カード効果処理が乗るSequence</param>
    public override void Set(Card card,Sequence sequence)
    {
        if (enemy == null) return;//enemyがいないなら終了
        foreach (ActualEffect effect in card.effects)
        {
            if(!cardEffectExecute.Execute(effect, enemy, card.attributes, sequence, card.pos))
            {//判定に失敗したら以降の処理は中断
                break;
            }
        }
        sequence.AppendCallback(() =>
        {
            //カードの効果が全て終わった後に、敵の死亡判定を行う
            CheckDead(sequence);
        });
    }

    /// <summary>
    /// Enemyのみ。EnemyCardZoneManagerが全体に対してカードを発動するときに使用する。
    /// Sequenceを与えられて、それに順にエフェクトを作用させる。
    /// </summary>
    /// <param name="weapon">使用されるWeapon</param>
    /// <param name="sequence">エフェクトが乗るSequence</param>
    public override void Set(Weapon weapon,Sequence sequence)
    {
        if (enemy == null) return;//enemyがいないなら終了
        foreach (ActualEffect effect in weapon.effects)
        {
            if(!cardEffectExecute.Execute(effect, enemy, weapon.attributes, sequence, 0))
            {
                break;
            }
        }
        sequence.AppendCallback(() =>
        {
            //カードの効果が全て終わった後に、敵の死亡判定を行う
            CheckDead(sequence);
        });
    }

    public override void Set(Consumable consumable, Sequence sequence)
    {
        if (enemy == null) return;//enemyがいないなら終了
        foreach (ActualEffect effect in consumable.Effects)
        {
            var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Physics) };
            if (!cardEffectExecute.Execute(effect, enemy,attributes, sequence, 0))
            {
                break;
            }
        }
        sequence.AppendCallback(() =>
        {
            //カードの効果が全て終わった後に、敵の死亡判定を行う
            CheckDead(sequence);
        });
    }

    /// <summary>
    /// 効果一つの実行
    /// 敵全体に対してカード効果を使用するために作成
    /// </summary>
    /// <param name="effect">カード効果一つ</param>
    /// <param name="attributes">カードなどの最初の属性</param>
    /// <param name="sequence">エフェクトが乗るSequence</param>
    /// <param name="cardPos">カードの位置</param>
    /// <param name="useState">使用する状態効果</param>
    /// <returns>判定が成功したか</returns>
    public bool Set(ActualEffect effect,List<AttributeDefine> attributes,Sequence sequence,int cardPos)
    {
        if (enemy == null) return true;//enemyがいないなら終了
        return (cardEffectExecute.Execute(effect, enemy,attributes, sequence, 0));
    }
    #endregion

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

    /// <summary>
    /// enemyPosを取得
    /// </summary>
    /// <returns>enemyPos</returns>
    public int GetEnemyPos()
    {
        return enemyPos;
    }

    /// <summary>
    /// 敵の死亡判定を行い、sequenceに動作を乗せる
    /// </summary>
    /// <param name="sequence">死亡効果の発生するsequence</param>
    /// <returns>死亡していたらfalse</returns>
    public bool CheckDead(Sequence sequence)
    {
        if (enemy is null) return false;//既に死亡しているなら終了
        return enemy.CheckDead(sequence);
    }

    /// <summary>
    /// セットされているEnemyのインスタンスを返す
    /// </summary>
    /// <returns>Enemy</returns>
    public Enemy GetEnemy()
    {
        return (enemy == null) ? null : enemy;
    }
}
