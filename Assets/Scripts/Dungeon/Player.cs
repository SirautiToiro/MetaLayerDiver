using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

/// <summary>
/// プレイヤーステータスの管理
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private DungeonManager dungeonManager;

    private PlayerUI playerUI;

    //プレイヤーステータス
    private int hp;//Hp
    private int mental;//精神力
    private int maxHp;//Hp最大値
    private int maxMental;//精神力最大値
    private int shield;//盾

    //IStateの状態異常のリスト
    private List<IState> statesNew;//TODO:全て整理後に名前を変える

    private PlayerBattleManager playerBattleManager;

    private bool inBattleFlag;//バトル中かどうかのフラグ

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="initialHp">Hp。最初は最大値で始まる</param>
    /// <param name="initialMental">精神力。最初は最大値で始まる</param>
    public void Init(int initialHp,int initialMental)
    {
        hp = initialHp;
        maxHp = initialHp;
        mental = initialMental;
        maxMental = initialMental;

        shield = 0;//盾は最初は0
        
        statesNew = new List<IState>();

        playerUI = null;

        inBattleFlag = false;//バトル中ではない
    }

    /// <summary>
    /// バトル開始時の設定を行う
    /// </summary>
    /// <param name="_playerUI">設定するPlayerUI</param>
    /// <param name="_playerBattleManager">設定するplayerBattleManager</param>
    public void SetBattle(PlayerUI _playerUI, PlayerBattleManager _playerBattleManager)
    {
        playerBattleManager = _playerBattleManager;

        playerUI = _playerUI;
        playerUI.Init(hp, maxHp,shield);//初期化

        inBattleFlag = true;//バトル中フラグを立てる
    }

    /// <summary>
    /// バトル終了時の処理を行う
    /// </summary>
    public void EndBattle()
    {
        playerBattleManager = null;
        playerUI = null;

        inBattleFlag = false;
    }

    /// <summary>
    /// 盾を獲得する。値が負の場合はその分減らし、最小で0
    /// </summary>
    /// <param name="value">盾の値</param>
    public void GetShield(int value)
    {
        shield += value;
        if (shield <= 0)
        {
            shield = 0;
        }

        //バトル中UIがあるなら、それを変動
        if(playerUI != null)
        {
            playerUI.SetShield(shield);
        }

    }

    #region 状態異常系

    /// <summary>
    /// バフデバフをプレイヤーに加える
    /// statesに追加する
    /// 既に存在する場合、スタック加算
    /// </summary>
    /// <param name="state">状態異常クラス</param>
    public void GetState(IState state)
    {
        //リストに存在しているならそこに追加
        //リストに存在し、永久の状態変化なら追加もなし
        if (!AddExistState(state))
        {
            //現在所持しているstatesには存在しない
            //リストに加える
            statesNew.Add(state);

            //バトル中UIがあるなら、それに追加
            if (playerUI != null)
            {
                playerUI.AddState(state);
            }
        }

        //状態異常が更新されたので、武器とカードを更新
        playerBattleManager.RefreshCard();
        playerBattleManager.RefreshWeapon();
    }

    /// <summary>
    /// プレイヤーの状態異常の値をvalueだけ減少させる
    /// stateは、既にPlayerについている状態異常
    /// </summary>
    /// <param name="state">既にPlayerについている状態異常</param>
    /// <param name="value">減少させる値</param>
    public void ConsumeState(IState state, int value)
    {
        if(state is IStateInGear gState)
        {//装備の状態異常なら、装備から変動させる
            playerBattleManager.ConsumeGearState(gState, value);
            return;
        }

        //stateの値を減少させ、削除が必要なら削除する
        state.value -= value;

        if(playerUI != null)
        {//UIがあるなら、UIに反映
            playerUI.ChangeStateValue(state);
        }

        //リストから削除
        if (state.value <= 0)
        {
            statesNew.Remove(state);
        }

        //状態異常が更新されたので、武器とカードを更新
        playerBattleManager.RefreshCard();
        playerBattleManager.RefreshWeapon();
    }

    /// <summary>
    /// カウントするステータスのカウントを変動させる
    /// </summary>
    /// <param name="cState">カウントを持つState</param>
    public void DecreaseCountOfCountState(IStateHasCount cState)
    {
        if (cState is IStateInGear)
        {//装備の状態異常なら、装備から変動させる
            playerBattleManager.DecreaseCountOfCountStateInGear(cState);
            return;
        }

        //ターンごとにカウントするステータスなら
        //カウントを減少させる
        cState.IStateCounter.CountNum();

        if(playerUI != null)
        {//UIがあるなら、UIに反映
            playerUI.ChangeStateCount(cState, cState.IStateCounter.GetCount());
        }

        //表示リセット
        playerBattleManager.RefreshCard();
        playerBattleManager.RefreshWeapon();
    }

    /// <summary>
    /// ターンの開始時に、ターンごとにカウントのある状態異常の値をリセットする
    /// </summary>
    public void ResetIStateCountPerTurn()
    {
        playerBattleManager.ResetIStateInGearCountPerTurn();

        SearchAndUseState<IStateCountPerTurn>(a => {
            a.IStateCounter.ResetCount();
            if(playerUI != null)
            {//UIがあるなら、UIに反映
                playerUI.ChangeStateCount(a, a.IStateCounter.GetCount());
            }
        });
    }

    /// <summary>
    /// 既に存在する状態異常に対して、追加処理を行う
    /// 追加処理を行ったならtrueを返す
    /// 永久のステータスならなら追加処理を行わない
    /// </summary>
    /// <param name="state">状態異常クラス</param>
    /// <returns>追加処理を行ったならtrueを返す</returns>
    private bool AddExistState(IState state)
    {
        for (int i = 0; i < statesNew.Count; i++)
        {//既に状態異常が存在するかの確認
            if(statesNew[i] is IStateOffsetable)
            {//検索中の状態異常が相殺可能なら
                if (((IStateOffsetable)statesNew[i]).IsNeedsOffset(state))
                {//検索中の状態異常と相殺が必要なら
                    OffsetState(state, i);
                    return true;
                }
            }

            if (StateCompare.IsSameState(statesNew[i], state))
            {//既に状態異常が存在するなら
                //永久のステータスなら追加処理を行わない
                if (state is StateContinueTypeEternalBase) return true;

                statesNew[i].value += state.value;//状態異常のスタックを加算する

                if(playerUI != null)
                {//UIがあるなら、UIに反映
                    playerUI.ChangeStateValue(statesNew[i]);
                }

                if (statesNew[i].value <= 0)
                {//値が0以下ならリストから削除する
                    statesNew.RemoveAt(i);
                }

                return true;//終了
            }
        }

        //既存のStateに追加しなかった
        return false;
    }

    /// <summary>
    /// 配列のpos番目のStateを引数のstateの値(value)で相殺する
    /// posの値をstateの値で引き、量が多く残った方を残す。
    /// </summary>
    /// <param name="state">相殺する新しいState</param>
    /// <param name="pos">相殺されるStateのstates内での位置</param>
    private void OffsetState(IState state, int pos)
    {
        statesNew[pos].value -= state.value;
        //相殺の結果残った値、これがマイナスなら相殺先のstateが残る
        int newValue = statesNew[pos].value;

        if(newValue == 0)
        {//値が0以下ならリストから削除するのみ
            //UIに反映
            if(playerUI != null)
            {//UIがあるなら、UIに反映
                playerUI.ChangeStateValue(statesNew[pos]);
            }
            statesNew.RemoveAt(pos);
        }else if(newValue > 0)
        {//値が0より大きいなら元のStateが残る
            if(playerUI != null)
            {//UIがあるなら、元の値の消去をUIに反映
                playerUI.ChangeStateValue(statesNew[pos]);
            }
        }
        else
        {//値が0より小さいなら新しいStateが残る。
            //元の値の消去をUIに反映
            statesNew[pos].value = 0;
            if(playerUI != null)
            {
                playerUI.ChangeStateValue(statesNew[pos]);
            }
            statesNew.RemoveAt(pos);

            //相殺先のStateをセット
            state.value = -1*newValue;
            statesNew.Add(state);
            if(playerUI != null)
            {
                playerUI.AddState(state);//UIに反映
            }
        }
    }

    /// <summary>
    /// ターン終了時に発生するStateの減少を発生させる。
    /// </summary>
    public void DecreaseState()
    {
        playerBattleManager.DecreaseStateInGear();

        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = statesNew.Count - 1; i >= 0; i--)
        {
            if (statesNew[i].TurnEndAdjust())
            {//値を変更したなら
                if (playerUI != null)
                {
                    playerUI.ChangeStateValue(statesNew[i]);

                }
                if (statesNew[i].value <= 0)
                {//0以下なら削除
                    statesNew.RemoveAt(i);
                }
            }
        }
    }

    public void DeleteAllState()
    {
        statesNew = new List<IState>();
    }

    /// <summary>
    /// プレイヤーの状態異常を削除する
    /// </summary>
    /// <param name="state">削除する状態異常</param>
    public void DeleteState(IState state)
    {
        state.value = 0;//値を0にして、UIに反映
        if(playerUI != null)
        {//UIがあるなら、UIに反映
            playerUI.DeleteState(state);
        }

        statesNew.Remove(state);//リストから削除

        //状態異常が更新されたので、武器とカードを更新
        playerBattleManager.RefreshCard();
        playerBattleManager.RefreshWeapon();
    }


    /// <summary>
    /// プレイヤーの所有する状態異常のリストからTに該当するものを検索し、
    /// デリゲートで指示したものを実行させる
    /// TがIStateHasPriorityなら、その中で最も優先度の高いものを使用する
    /// TODO:しっかりテストしたほうが良いかも？
    /// </summary>
    /// <typeparam name="T">検索する状態異常のタイプ</typeparam>
    /// <param name="useAction">a => a.UseA()のように、Tのメソッドを使用する指示</param>
    public void SearchAndUseState<T>(Action<T> useAction) where T:class,IState
    {
        List<IState> targetStateList = new List<IState>();
        foreach(IState state in statesNew)
        {//全部コピー
            targetStateList.Add(state);
        }
        //装備の状態異常も加えて使用する
        targetStateList.Add(playerBattleManager.GetGearState());

        if(typeof(T).GetInterfaces().Contains(typeof(IStateHasPriority)))
        {//優先度のあるIStateなら、その中で最大のものから順番に使用する
            
            List<int>[] targetStatePosArray = new List<int>[5];//優先度は5段階(0-4)まである
            for(int i = 0; i < 5; i++)
            {
                targetStatePosArray[i] = new List<int>();//リストを初期化
            }

            for (int i = targetStateList.Count - 1; i >= 0; i--)
            {
                if (targetStateList[i] is T target && targetStateList[i] is IStatePlayer)
                {//条件に当てはまるStateの位置をリストにして、最大の優先度を記録する
                    IStateHasPriority isHasPriority = targetStateList[i] as IStateHasPriority;

                    targetStatePosArray[isHasPriority.Priority].Add(i);//優先度のリストに追加
                }
            }

            //実行すべきIStateがリストに追加された.あるいは何も追加されていない

            for(int i = 4; i >= 0; i--)
            {
                foreach (int pos in targetStatePosArray[i])
                {//優先度の高い順に実行
                    T target = targetStateList[pos] as T;
                    useAction(target);
                }
            }
        }
        else
        {
            for (int i = targetStateList.Count - 1; i >= 0; i--)
            {
                if (targetStateList[i] is T target && targetStateList[i] is IStatePlayer)
                {
                    useAction(target);
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 盾の値を初期化する(基本的に0)
    /// </summary>
    public void ResetShield()
    {
        shield = 0;
        //UIを変動
        playerUI.SetShield(shield);
    }

    /// <summary>
    /// プレイヤーがダメージを受ける
    /// マイナスは回復
    /// </summary>
    /// <param name="value">ダメージの値</param>
    /// <returns>プレイヤーの残りHP</returns>
    public int Damage(int value)
    {
        if (value < 0)
        {//マイナスなら回復
            hp += -value;//マイナスを取る
            if (hp > maxHp)//最大値を超えないようにする
            {
                hp = maxHp;
            }

            //UI反映
            if (inBattleFlag)
            {
                playerUI.SetPlayerHp(hp, maxHp);
                playerUI.SetPlayerHpBar(hp, maxHp);
            }
            else
            {
                dungeonManager.UpdateHp(hp);
            }
            return hp;
        }

        if (shield >= value)//盾の方がダメージ量より多いなら
        {
            //盾だけ減らす
            shield -= value;
        }
        else
        {//盾を超えてダメージが入るなら
            //HPも減らす
            hp -= (value-shield);
            shield = 0;
            if (hp <= 0)//0以下なら0に
            {
                hp = 0;
            }
            else if (hp > maxHp)
            {
                hp = maxHp;
            }
        }

        //UIを更新
        if (inBattleFlag)
        {
            playerUI.SetPlayerHp(hp, maxHp);
            playerUI.SetPlayerHpBar(hp, maxHp);
            playerUI.SetShield(shield);
        }
        else
        {
            dungeonManager.UpdateHp(hp);
        }

        return hp;
    }

    /// <summary>
    /// プレイヤーが攻撃された際のエフェクト効果を発生させる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        playerUI.Effect(actionType, enemyAction, value);
    }

    /// <summary>
    /// Card効果によるエフェクトを発生させる
    /// </summary>
    /// <param name="effectType">カード効果の種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        playerUI.Effect(effectType, attribute, value);
    }

    /// <summary>
    /// プレイヤーに発生しているエフェクト全てに削除演出を行う
    /// </summary>
    public void DestroyEffects()
    {
        playerUI.DestroyEffects();
    }

    /// <summary>
    /// PlayerのHPを取得する
    /// </summary>
    /// <returns>PlayerのHP</returns>
    public int GetHp()
    {
        return hp;
    }

    public int GetHpMax()
    {
        return maxHp;
    }

    public PlayerBattleManager GetPlayerBattleManager()
    {
        return playerBattleManager;
    }
}
