using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// 敵のPrefabに付属する管理スクリプト
/// </summary>
public class Enemy : MonoBehaviour
{
    private EnemyManager enemyManager;//インスタンス化する時に入れる
    private EnemyCardZone enemyCardZone;//配置されているCardZone

    [SerializeField] EnemyUI enemyUI;//UI

    //敵データ
    public EnemyDataSO baseEnemyData;//元となるSO敵のデータ
    private int gage;//現在のゲージ残り量
    private int hp;//HP
    private int maxHp;//最大HP
    private int shield;//盾
    private EnemyUniqueDefine.UniqueTag uniqueTag;

    //行動パターン系
    //0番目に初期行動、123番目にそれぞれのパターンの行動
    private List<List<ActionInTurn>> actions;//行動リストのリスト
    private int actionPattern;//現在の行動パターン,これが0の時、バトル開始時の初期行動を行う
    private int nextAction;//現在の行動パターンのリストのnextAction番目が次に行う行動
    private List<EnemyInitialEffectDefine> initialEffects;//敵の初期に発生する効果のリスト

    //実際に発生する行動。actionsから毎回ここにコピーして補整の後実行する
    private ActualNextAction actualNextAction;

    private EnemyRoutineDefine enemyRoutine;

    //IStateの状態異常のリスト
    private List<IState> statesNew;//TODO:全て整理後に名前を変える

    //敵のレア度
    public TierDefine.Tier Tier { get { return baseEnemyData.tier.tier; } }

    /// <summary>
    /// 敵の一ターンの行動について、補正後の値を付加して記録するクラス
    /// </summary>
    public class ActualNextAction
    {
        private List<ActualAction> actualActions;//1ターンで行う行動の全て

        public class ActualAction
        {
            public EnemyActionDefine actionDefine { get; set; }//敵行動の一つの種類と本来の値のクラス
            public int actualActionValue { get; set; }//補整などが加わって変動した値
        }

        //コンストラクタ
        public ActualNextAction(ActionInTurn actionInTurn)
        {
            actualActions =new List<ActualAction>();
            foreach(EnemyActionDefine enemyAction in actionInTurn.actionsInTurn)
            {
                actualActions.Add(new ActualAction()
                {
                    actionDefine = enemyAction,
                    actualActionValue=enemyAction.value,
                });
            }
        }

        /// <summary>
        /// n番目の1ターンでの行動を取得する
        /// </summary>
        /// <param name="n">行動の番号</param>
        /// <returns>番号に対応する行動</returns>
        public ActualAction GetActualAction(int n)
        {
            return actualActions[n];
        }

        public void SetActualActionValue(int n,int value)
        {
            this.actualActions[n].actualActionValue = value;
        }

        public int GetActualActionCount()
        {
            return actualActions.Count;
        }
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="enemyData">元となる敵情報のSO</param>
    /// <param name="enemyCardZone">配置される敵CardZone</param>
    /// <param name="_fieldManager">生成元のFieldManager</param>
    public void Init(EnemyDataSO enemyData,EnemyManager _enemyManager, EnemyActionToActionIcon _enemyActionToActionIcon, EnemyCardZone _enemyCardZone)
    {
        enemyManager = _enemyManager;
        enemyCardZone = _enemyCardZone;

        baseEnemyData = enemyData;

        enemyUI.Init(enemyData, _enemyActionToActionIcon, this);

        //数値初期化
        gage=enemyData.gage;
        hp=enemyData.hp;
        maxHp=enemyData.hp;
        shield = 0;

        //行動コピー
        actionPattern = 0;//行動パターンを初期行動に初期化
        nextAction=-1;//次の行動
        actions = new List<List<ActionInTurn>>();//初期化
        actions.Add(new List<ActionInTurn>(enemyData.initialAction));//リストに初期行動を追加
        actions.Add(new List<ActionInTurn>(enemyData.actions1));//リストに行動を追加
        actions.Add(new List<ActionInTurn>(enemyData.actions2));
        actions.Add(new List<ActionInTurn>(enemyData.actions3));
        enemyRoutine=enemyData.enemyRoutine;
        initialEffects = enemyData.enemyInitialEffects;

        uniqueTag = enemyData.uniqueTag.uniqueTag;

        enemyUI.SetShield(shield);//盾の表示を初期化   

        statesNew = new List<IState>();
    }

    /// <summary>
    /// 配置されているCardZoneを後から変更する
    /// </summary>
    /// <param name="_enemyCardZone">変更先のCardZone</param>
    public void SetCardZone(EnemyCardZone _enemyCardZone)
    {
        this.enemyCardZone = _enemyCardZone;
    }

    /// <summary>
    /// ダメージを敵が受ける
    /// マイナスは回復
    /// </summary>
    /// <param name="value">ダメージ量</param>
    /// <param name="isCalculateBlock">防御を計算するか</param>
    /// <param name="sequence">ダメージの後に発生する効果が乗るSequence</param>
    public void Damage(int value,bool isCalculateBlock,Sequence sequence)
    {
        if (isCalculateBlock) {
            if (shield >= value)//盾の方がダメージ量より多いなら
            {
                if (value < 0)
                {//回復処理
                    hp -= value;
                    if (hp > maxHp)
                    {
                        hp = maxHp;
                    }
                }
                else
                {
                    //盾だけ減らす
                    shield -= value;
                }
            }
            else
            {//盾を超えてダメージが入るなら
                hp -= (value - shield);
                shield = 0;

                if (hp > maxHp)
                {
                    hp = maxHp;
                }
            }
        }
        else
        {//防御を計算せずダメージ
            hp -= value;
            if (hp > maxHp)
            {
                hp = maxHp;
            }
        }

        enemyUI.SetShield(shield);
        enemyUI.SetEnemyHp(hp, maxHp);
        enemyUI.SetEnemyHpBar(hp,maxHp);
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

        //UIを変動
        enemyUI.SetShield(shield);
    }

    /// <summary>
    /// 盾の値を初期化する(基本的に0)
    /// </summary>
    public void ResetShield()
    {
        shield=0;
        //UIを変動
        enemyUI.SetShield(shield);
    }

    /// <summary>
    /// 敵の撃破時演出を行う
    /// </summary>
    public void Destroy()
    {
        enemyUI.DestroyEffect();   
    }

    /// <summary>
    /// enemyが死んでいるかを確認する
    /// 死んでいたら、撃破時演出を行う
    /// </summary>
    /// <param name="sequence">撃破時効果が乗るsequence.(撃破時演出は別の物)</param>
    /// <returns>死亡していたらfalse</returns>
    public bool CheckDead(Sequence sequence)
    {
        if (hp <= 0)
        {//死亡しているなら
            hp = 0;

            //死亡時効果の発生
            SearchAndUseState<IStateEnemyDefeat>(a => a.DefeatEffect(enemyManager,this));

            //死んでいるのでenemyManagerに削除してもらう
            enemyManager.DestroyEnemy(enemyCardZone.GetEnemyPos());

            return false;
        }
        else
        {
            return true;
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
        if (!AddExistState(state))
        {
            //現在所持しているstatesには存在しない
            //リストに加える
            statesNew.Add(state);

            //UIに加える
            enemyUI.AddState(state);
        }

        //次の行動が決まっているなら
        //召喚されたときは決まっていない
        if(actualNextAction is not null)
        {
            HoseiAction();//敵の行動に状態異常などによる補整をかける
                          //状態異常を受けた後は、Enemyの行動UIにも反映させる
            enemyUI.ShowNextAction(actualNextAction);//現在の行動を表示
        }
    }

    /// <summary>
    /// 敵の状態異常の値をvalueだけ減少させる
    /// stateは、既に敵についている状態異常
    /// valueがもとから0のものも、削除を行う
    /// </summary>
    /// <param name="state">既にEnemyについている状態異常</param>
    /// <param name="value">減少させる値</param>
    public void ConsumeState(IState state, int value)
    {
        //stateの値を減少させ、削除が必要なら削除する
        state.value -= value;

        //リストから削除
        if (state.value <= 0)
        {
            statesNew.Remove(state);

            //UIに反映(強制削除)
            enemyUI.ChangeStateValue(state,true);
        }
        else
        {
            //UIに反映
            enemyUI.ChangeStateValue(state);
        }
    }

    /// <summary>
    /// 既に存在する状態異常に対して、追加処理を行う
    /// 追加処理を行ったならtrueを返す
    /// </summary>
    /// <param name="state">状態異常クラス</param>
    /// <returns>追加処理を行ったならtrueを返す</returns>
    private bool AddExistState(IState state)
    {
        for (int i = 0; i < statesNew.Count; i++)
        {//既に状態異常が存在するかの確認
            if (statesNew[i] is IStateOffsetable)
            {//検索中の状態異常が相殺可能なら
                if (((IStateOffsetable)statesNew[i]).IsNeedsOffset(state))
                {//検索中の状態異常と相殺が必要なら
                    OffsetState(state, i);
                    return true;
                }
            }

            if (StateCompare.IsSameState(statesNew[i], state))
            {//既に状態異常が存在するなら
                statesNew[i].value += state.value;//状態異常のスタックを加算する

                //UIに反映
                enemyUI.ChangeStateValue(statesNew[i]);

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

        if (newValue == 0)
        {//値が0以下ならリストから削除するのみ
            //UIに反映
            enemyUI.ChangeStateValue(statesNew[pos]);
            statesNew.RemoveAt(pos);
        }
        else if (newValue > 0)
        {//値が0より大きいなら元のStateが残る
            //UIに反映
            enemyUI.ChangeStateValue(statesNew[pos]);
        }
        else
        {//値が0より小さいなら新しいStateが残る。
            //元の値の消去をUIに反映
            statesNew[pos].value = 0;
            enemyUI.ChangeStateValue(statesNew[pos]);
            statesNew.RemoveAt(pos);

            //相殺先のStateをセット
            state.value = -1 * newValue;
            statesNew.Add(state);
            enemyUI.AddState(state);//UIに反映
        }
    }

    /// <summary>
    /// ターン終了時に発生するStateの減少を発生させる。
    /// </summary>
    public void DecreaseState()
    {
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = statesNew.Count - 1; i >= 0; i--)
        {
            if (statesNew[i].TurnEndAdjust(this))
            {//値を変更したなら
                enemyUI.ChangeStateValue(statesNew[i]);
                if (statesNew[i].value <= 0)
                {//0以下なら削除
                    statesNew.RemoveAt(i);
                }
            }
        }
    }
    
    /// <summary>
    /// 状態異常を再読み込みする
    /// </summary>
    public void RefreshState()
    {
        for (int i = statesNew.Count - 1; i >= 0; i--)
        {
            enemyUI.ChangeStateValue(statesNew[i]);
        }
    }

    /// <summary>
    /// カウントを持つStateののカウントをリセットする
    /// </summary>
    public void ResetIStateCountPerTurn()
    {
        SearchAndUseState<IStateCountPerTurn>(a => {
            a.IStateCounter.ResetCount();
            enemyUI.ChangeStateCount(a, a.IStateCounter.GetCount());
        });
    }

    /// <summary>
    /// Enemyの所有する状態異常のリストからTに該当するものを検索し、
    /// デリゲートで指示したものを実行させる
    /// </summary>
    /// <typeparam name="T">検索する状態異常のタイプ</typeparam>
    /// <param name="useAction">a => a.UseA()のように、Tのメソッドを使用する指示</param>
    public void SearchAndUseState<T>(Action<T> useAction) where T : class, IState
    {
        for (int i = statesNew.Count - 1; i >= 0; i--)
        {
            if (statesNew[i] is T target && statesNew[i] is IStateEnemy)
            {
                useAction(target);
            }
        }
    }

    #endregion

    #region 行動決定系

    /// <summary>
    /// Enemyの次に行う行動を決定する
    /// すなわち、actionPatternとnextActionの値を適切に変動させる
    /// </summary>
    /// <param name="isFirstTurn">最初のターンならtrue</param>
    /// actionPatternは0の時戦闘開始時の行動を行う</returns>
    private void DecideNextAction(bool isFirstTurn)
    {
        int ap = actionPattern;//返すactionPattern
        int na = nextAction;//返すnextAction

        if (isFirstTurn&& actions[0].Count >=1)
        {//戦闘開始時の行動,戦闘開始時行動が設定されているなら
            ap = 0;
            na = 0;
        }
        else
        {//戦闘開始時ではない、あるいは戦闘開始時行動がない
            if (actionPattern == 0)
            {//前回が初期行動,あるいは初回だが初期行動がないなら次の行動はpattern1
                ap = 1;
                na = -1;
            }

            if (enemyRoutine.enemyRoutine == EnemyRoutineDefine.EnemyRoutine.Periodic)
            {//周期的なenemyRoutine
                na++;//次の行動へ
                if (actions[ap].Count <= na)
                {//配列の最後にいるなら
                    na = 0;//最初へ
                }
            }
            else if (enemyRoutine.enemyRoutine == EnemyRoutineDefine.EnemyRoutine.Random)
            {//ランダムなenemyRoutine
                na = UnityEngine.Random.Range(0, actions[ap].Count);//リストの中でランダムに決定
            }
        }
        //
        actionPattern = ap;
        nextAction = na;

        //actualNextActionに次の行動の情報を書き写す。
        actualNextAction = new ActualNextAction(actions[actionPattern][nextAction]);

        HoseiAction();//状態異常などによる補整をかける
    }

    /// <summary>
    /// actualNextActionに対して、状態異常などによる補正をかける
    /// </summary>
    public void HoseiAction()
    {
        int valueAfterEffect = 0;
        for (int i=0;i<actualNextAction.GetActualActionCount();i++)
        {
            //ひとつの行動と、その実行時の値を取り出す
            ActualNextAction.ActualAction actualAction = actualNextAction.GetActualAction(i);
            valueAfterEffect = actualAction.actionDefine.value;//本来の値を最初に使用

            //行動のタイプを取得
            EnemyActionDefine.EnemyActionType enemyActionType = EnemyActionDefine.GetActionType(actualAction.actionDefine.enemyAction);

            switch (enemyActionType)
            {
                case EnemyActionDefine.EnemyActionType.Block://防御
                    break;
                case EnemyActionDefine.EnemyActionType.Damage://攻撃
                    //状態異常による増減
                    SearchAndUseState<IStateEnemyDamage>(a => valueAfterEffect = a.AdjustDamage(this, valueAfterEffect));

                    break;
            }

            //actualNectActionに補正を反映
            actualNextAction.SetActualActionValue(i, valueAfterEffect);
        }

        //敵の行動を変更するものがあれば、全てを覆してそれを設定
        SearchAndUseState<IStateEnemyAction>(a => {
            //nullなら関係ない
            var tmp = a.ChangeEnemyAction(this);
            if (tmp != null)
            {
                actualNextAction = tmp;
            }
        });
    }

    /// <summary>
    /// Enemyの次に行う行動を更新する(決定、表示)
    /// </summary>
    /// <param name="isFirstTurn">最初のターンであるか</param>
    public void UpdateNextAction(bool isFirstTurn)
    {
        DecideNextAction(isFirstTurn);//次の行動を決定
        enemyUI.ShowNextAction(actualNextAction);//次の行動を表示
    }

    /// <summary>
    /// 敵の行動を実行する(ターン終了時の攻撃)
    /// n番目の行動だけを実行する。実行もとでは
    /// GetActualNextActionCount()を使用して数を数えて制御
    /// </summary>
    /// <param name="n">実行する行動の番号</param>
    public void ExecuteAction(int n)
    {
        //ひとつの行動と、その実行時の値を取り出す
        ActualNextAction.ActualAction actualAction = actualNextAction.GetActualAction(n);
        int actualValue = actualAction.actualActionValue;

        //行動によって発生するState
        IState useState = null;
        switch (actualAction.actionDefine.enemyAction)
        {
            case EnemyActionDefine.EnemyAction.Block://防御
                GetShield(actualValue);
                break;
            //攻撃行動
            case EnemyActionDefine.EnemyAction.DamagePhysics:
            case EnemyActionDefine.EnemyAction.DamagePsycho:
            case EnemyActionDefine.EnemyAction.DamageFaith:
            case EnemyActionDefine.EnemyAction.DamageEnergy:
            case EnemyActionDefine.EnemyAction.DamagePyro:
            case EnemyActionDefine.EnemyAction.DamageCreate:
            case EnemyActionDefine.EnemyAction.DamageMind:
                //ダメージを置き換えるような処理に対する対応
                bool changed = false;
                enemyManager.SearchAndUsePlayerState<IStatePlayerWhenAttacked>(a =>
                {
                    changed = a.ChangePlayerAttackedOperation(enemyManager.GetPlayer(), actualValue);
                });
                if(!changed)
                {
                    //変更がなければ普通のダメージ
                    //ダメージ量変更計算
                    enemyManager.SearchAndUsePlayerState<IStatePlayerDamagedValue>(a =>
                    {
                        actualValue = a.ChangeDamagedValue(enemyManager.GetPlayer(), actualValue);
                    });
                    enemyManager.DamageToPlayer(actualValue);
                }
                break;

            case EnemyActionDefine.EnemyAction.CauseDebuff:
            case EnemyActionDefine.EnemyAction.CauseBuff:
                useState = StateCopy.CopyInstanceAndSetState(actualAction.actionDefine.UseState, actualValue);
                enemyManager.StateToPlayer(useState);
                break;
            case EnemyActionDefine.EnemyAction.GetBuff:
            case EnemyActionDefine.EnemyAction.GetDebuff:
                
                useState = StateCopy.CopyInstanceAndSetState(actualAction.actionDefine.UseState, actualValue);
                GetState(useState);
                break;
            case EnemyActionDefine.EnemyAction.NotMove:
                break;//何もしない行動
            case EnemyActionDefine.EnemyAction.GetDebuffAll:
            case EnemyActionDefine.EnemyAction.GetBuffAll:
                enemyManager.StateToEnemyAll(actualAction.actionDefine.UseState.GetType(), actualValue);
                break;
        }
    }

    /// <summary>
    /// Enemyが次にいくつのActionを予定しているかを取得する
    /// </summary>
    /// <returns>次に予定されているActionの個数</returns>
    public int GetActualNextActionCount()
    {
        return actualNextAction.GetActualActionCount();
    }

    /// <summary>
    /// EnemyのActualNextActionのなかのn番目の行動を取得する
    /// </summary>
    /// <param name="n">リスト上の位置</param>
    /// <returns>行動</returns>
    public ActualNextAction.ActualAction GetEnemyActualAction(int n)
    {
        return actualNextAction.GetActualAction(n);
    }

    /// <summary>
    /// 敵の行動パターンの番号を設定する
    /// 変更直後はnextActionが0になる(その行動パターンの中で最初の行動)
    /// </summary>
    /// <param name="patNum">敵の行動パターンの番号</param>
    public void SetActionPattern(int patNum)
    {
        actionPattern = patNum;
        nextAction = 0;

        //actualNextActionに次の行動の情報を書き写す。
        actualNextAction = new ActualNextAction(actions[actionPattern][nextAction]);

        HoseiAction();//状態異常などによる補整をかける

        enemyUI.ShowNextAction(actualNextAction);//次の行動を表示
    }
    #endregion

#region エフェクト系

    /// <summary>
    /// Enemyの行動による効果を受けた時のエフェクト効果を発生させる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        enemyUI.Effect(actionType, enemyAction, value);
    }

    /// <summary>
    /// Card効果によるエフェクトを発生させる
    /// </summary>
    /// <param name="effectType">カード効果の種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        enemyUI.Effect(effectType, attribute, value);
    }

    /// <summary>
    /// Enemyに発生しているエフェクト全てに削除演出を行う
    /// </summary>
    public void DestroyEffects()
    {
        enemyUI.DestroyEffects();
    }

#endregion

    /// <summary>
    /// バトル開始時の効果を持っている場合、それを発生させる。
    /// </summary>
    public void InitialEffectExecute()
    {
        foreach(EnemyInitialEffectDefine effect in initialEffects)
        {
            IState getState = null;
            switch (effect.Initial)
            {
                case EnemyInitialEffectDefine.InitialEffect.GetEffect:
                    getState = effect.State;
                    getState.value = effect.Value;
                    GetState(getState);
                    break;
            }
        }
    }

    /// <summary>
    /// このEnemyのuniqueTagを返す。
    /// </summary>
    /// <returns>このEnemyのuniqueTag</returns>
    public EnemyUniqueDefine.UniqueTag GetUniqueTag()
    {
        return uniqueTag;
    }

    /// <summary>
    /// enemyの画像をnum番目に変更する
    /// </summary>
    /// <param name="num">変更する画像の番号</param>
    public void PicChange(int num)
    {
        enemyUI.SetEnemySprite(num);
    }

    public void NameChange(string str)
    {
        enemyUI.SetEnemyName(str);
    }

    /// <summary>
    /// 付属しているEnemyCardZoneの位置を返す
    /// </summary>
    /// <returns>Enemyの位置</returns>
    public int GetPos()
    {
        return enemyCardZone.GetEnemyPos();
    }

    public EnemyManager GetEnemyManager()
    {
        return enemyManager;
    }
}
