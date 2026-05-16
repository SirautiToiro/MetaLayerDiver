using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 敵の攻撃時などのアニメーションを扱うクラス
/// </summary>
public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] PlayerBattleManager playerBattleManager;

    /// <summary>
    /// 敵の行動を実行する
    /// sequenceを受け取って、敵の行動アニメーションを実行する
    /// </summary>
    /// <param name="enemies">行動する敵のリスト</param>
    /// <param name="sequence">行動を実行するSequence</param>
    public void EnemyActionAnimation(Enemy[] enemies,Sequence sequence)
    {
        //DOTweenを使用するためのSequenceを作成
        Sequence actionSequence = sequence;

        int actionNum = 0;
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;//Enemyがいないなら終了

            //行動の回数を取得して、それぞれに行動させる
            actionNum = enemy.GetActualNextActionCount();
            for (int i = 0; i < actionNum; i++)
            {
                int actionNumber = i;
                //コールバックとして行動を設定
                //Enmeyが攻撃するアニメーション
                //上下運動
                actionSequence.Append(enemy.gameObject.transform.DOLocalMoveY(BattleConstants.EnemyActionJumpHeight, BattleConstants.EnemyActionJumpTime))
                    .Append(enemy.gameObject.transform.DOLocalMoveY(BattleConstants.EnemyActionJumpHeight*-1, BattleConstants.EnemyActionJumpTime))
                    .SetRelative(true)
                    .AppendCallback(() => {//コールバック
                        SetActionEffect(enemy, actionNumber);
                        //敵の行動を実行
                        enemy.ExecuteAction(actionNumber);
                    })
                    .SetLink(enemy.gameObject);
                ;
            }
        }

        //演出が終了するまで待つ
        actionSequence.AppendInterval(BattleConstants.EffectDisAppearTime);

        //全部終了したら処理を次に進める
        actionSequence.OnComplete(() =>
        {
            enemyManager.TurnEndAfterEnemiesAction();
        });
    }

    /// <summary>
    /// 敵の行動に対応したエフェクトを各所にセットする
    /// </summary>
    /// <param name="enemy">行動を起こしている敵</param>
    /// <param name="n">敵の行動の番号</param>
    public void SetActionEffect(Enemy enemy,int n)
    {
        //敵の行動を取得
        Enemy.ActualNextAction.ActualAction actualAction = enemy.GetEnemyActualAction(n);

        //与えられた行動のタイプを取得する
        EnemyActionDefine.EnemyActionType enemyActionType = EnemyActionDefine.GetActionType(actualAction.actionDefine.enemyAction);

        //行動のタイプによってエフェクトの画像が変わる
        //攻撃行動なら、属性によって画像が変わる
        switch (enemyActionType)
        {
            //Damageは属性によってエフェクトを変える
            case EnemyActionDefine.EnemyActionType.Damage:
                //playerに攻撃されるエフェクト

                //特殊なエフェクトの判断
                SpecialAttackEffectDefine.SpecialEffect se = SpecialAttackEffectDefine.SpecialEffect.NoChange;
                playerBattleManager.SearchAndUsePlayerState<IStatePlayerWhenAttacked>(a =>se = a.ChangePlayerAttackedEffect(se));

                switch (se)
                {
                    case SpecialAttackEffectDefine.SpecialEffect.NoChange:
                        //通常のダメージ演出
                        playerBattleManager.PlayerEffect(enemyActionType, actualAction.actionDefine.enemyAction, actualAction.actualActionValue);
                        break;
                    case SpecialAttackEffectDefine.SpecialEffect.Avoid:
                        //Playerが回避を持っているならエフェクトを回避エフェクトに
                        playerBattleManager.PlayerEffect(EnemyActionDefine.EnemyActionType.Avoided, actualAction.actionDefine.enemyAction, actualAction.actualActionValue);
                        break;
                }
                break;
            //敵自身のみに効果を与えるエフェクト
            case EnemyActionDefine.EnemyActionType.Block:
            case EnemyActionDefine.EnemyActionType.BuffSelf:
            case EnemyActionDefine.EnemyActionType.EnemyHeal:
                //敵単体にエフェクト
                enemy.Effect(enemyActionType, actualAction.actionDefine.enemyAction, actualAction.actualActionValue);

                break;
            //敵全体に効果を与えるエフェクト
            case EnemyActionDefine.EnemyActionType.BuffAll:
                //敵全体にエフェクト
                enemyManager.EnemyAllEffect(enemyActionType, actualAction.actionDefine.enemyAction, actualAction.actualActionValue);

                break;
            //プレイヤー単体に効果を与えるエフェクト
            case EnemyActionDefine.EnemyActionType.Debuff:
            case EnemyActionDefine.EnemyActionType.PlayerHeal:
                //playerに攻撃されるエフェクト
                playerBattleManager.PlayerEffect(enemyActionType, actualAction.actionDefine.enemyAction, actualAction.actualActionValue);

                break;
        }
    }

    /// <summary>
    /// Enemyの出現時のアニメーション
    /// </summary>
    /// <param name="enemies">動かすEnemyの配列</param>
    public void EnemyAppearAnimation(Enemy[] enemies)
    {
        //DOTweenを使用するためのSequenceを作成
        Sequence actionSequence = DOTween.Sequence();

        int i = 0;
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;//Enemyがいないなら終了

            //Enemyを少し上に動かしておく
            Vector3 pos = enemy.gameObject.transform.localPosition;
            pos.y += BattleConstants.CharacterAppearHeight;
            enemy.gameObject.transform.localPosition = pos;

            //最初だけラグがない
            if (i == 0)
            {
                //Enemyが出現するアニメーション(ラグなし)
                actionSequence
                    .Join(enemy.gameObject.transform.DOLocalMoveY(-1 * BattleConstants.CharacterAppearHeight, BattleConstants.CharacterAppearTime))
                    .SetRelative(true)
                    .SetLink(enemy.gameObject);
            }
            else
            {
                //Enemyが出現するアニメーション(ラグあり)
                actionSequence
                    .Join(enemy.gameObject.transform.DOLocalMoveY(-1 * BattleConstants.CharacterAppearHeight, BattleConstants.CharacterAppearTime)
                    .SetDelay(BattleConstants.CharacterAppearTime * 0.5f))
                    .SetRelative(true)
                    .SetLink(enemy.gameObject);
            }
            i++;
        }
    }
}
