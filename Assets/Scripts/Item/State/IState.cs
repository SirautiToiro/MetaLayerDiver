using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////一覧////////
///継続条件抽象クラス.必ずこれのうちどれかを実装する
/// StateContinueTypeDecreaseBase: ターン終了時減少する
/// StateContinueTypeConstantBase: ターン終了時減少しない
/// StateContinueTypeEternalBase: 不変

///***プレイヤー状態異常***
///
/// IStateCardDamage        :カードダメージ増減
/// IStateCardCost          :カードコスト増減
/// IStateWeaponCost        :武器コスト増減
/// IStateWeaponDamage      :武器ダメージ増減
/// IStateEffectableToEnemy :攻撃可能な敵の変動
/// IStateConsumable        :カードによって消費可能
/// IStatePlayerWhenAttacked : プレイヤー被ダメージ時効果変更
/// IStatePlayerDamagedValue : プレイヤーダメージ量変更
/// IStatePlayerTurnStart    : プレイヤーターン開始時効果変更
/// IStateAfterUsingCard     :カード使用後効果
/// IStateInGear             :装備に使用される状態異常
/// IStatePlayerTurnEnd      :プレイヤーターン終了時効果
/// IStateAfterUsingWeapon  ：武器使用後効果

///***敵状態異常***
/// IStateEnemyDamage       :敵ダメージ増減
/// IStateEnemyDefeat       :敵撃破時効果
/// IStateEnemyWhenAttacked  :敵ダメージ時ダメージ効果変更
/// IStateEnemyDamagedValue  :敵ダメージ時ダメージ量変更
/// IStateEnemyTurnEnd       :敵ターン終了時効果
/// IStateEnemyTurnStart    :敵ターン開始時効果
/// IStateEnemyAction        :敵行動に変化
/// IStateEnemyDamaged      :敵ダメージ後効果

///***仲間状態異常
///

///***全般***
/// IStateOffsetable        :他の状態異常によって相殺可能
/// IStateHasAttribute      :属性を持っている状態異常
/// IStateHasPriority       :優先度を持っている状態異常
/// IStateHasCount          :何らかのものをカウントする状態異常
/// IStateCountPerTurn      :1ターンで使用できる回数が決まっている状態異常
/// IStateCountInBattle     :バトル中に使用できる回数が決まっている


/// ツリー：
/// IState
/// ┠StateContinueTypeBase
///     ┠ StateContinueTypeDecreaseBase :普通の減少を行うものは
///     ┠ StateContinueTypeConstantBase :どれかを必ず継承
///     ┠ StateContinueTypeEternalBase  :
///     
///  効果を持つもの、付属先別
/// ┠IStatePlayer
///     ┠ IStateCardDamage
///     ┠ IStateCardCost
///     ┠ IStateWeaponCost
///     ┠ IStateWeaponDamage
///     ┠ IStateEffectableToEnemy
///     ┠ IStateConsumable
///     ┠ IStatePlayerWhenAttacked
///     ┠ IStatePlayerDamagedValue
///     ┠ IStatePlayerTurnStart
///     ┠ IStateAfterUsingCard
///     ┠ IStateInGear
///     ┠ IStatePlayerTurnEnd
///     ┠ IStateAfterUsingWeapon
/// ┠IStateEnemy
///     ┠ IStateEnemyDamage
///     ┠ IStateEnemyDefeat
///     ┠ IStateEnemyWhenAttacked
///     ┠ IStateEnemyDamagedValue
///     ┠ IStateEnemyTurnEnd
///     ┠ IStateEnemyTurnStart
///     ┠ IStateEnemyAction
///     ┠ IStateEnemyDamaged
/// ┠IStateFellow
/// 
/// その他の機能を持つもの
/// ┠IStateOffsetable
/// ┠IStateHasAttribute
/// ┠IStateHasPriority
/// ┠IStateHasCount 
///     ┠IStateCountPerTurn
///     ┠IStateCountInBattle 


















/// <summary>
/// 状態異常全般が属するインターフェース
/// </summary>
public interface IState
{
    int value { get; set; }//状態異常の値

    /// <summary>
    /// ターン終了時に増減させる処理
    /// </summary>
    /// <param name="player">StateがついているPlayer</param>
    /// <returns>値を変更したならtrue</returns>
    bool TurnEndAdjust();
    /// <summary>
    /// ターン終了時に増減させる処理
    /// </summary>
    /// <param name="enemy">StateがついているEnemy</param>
    /// <returns>値を変更したならtrue</returns>
    bool TurnEndAdjust(Enemy enemy);
}

/// <summary>
/// 状態異常の継続タイプを表すインターフェース
/// </summary>
public abstract class StateContinueTypeBase:IState
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public StateContinueTypeBase()
    {
        //値はここではセットしない
    }

    public int value { get; set; }

    /// <summary>
    /// ターン終了時に増減させる処理
    /// </summary>
    /// <param name="player">StateがついているPlayer</param>
    /// <returns>値を変更したならtrue</returns>
    abstract public bool TurnEndAdjust();
    /// <summary>
    /// ターン終了時に増減させる処理
    /// </summary>
    /// <param name="enemy">StateがついているEnemy</param>
    /// <returns>値を変更したならtrue</returns>
    abstract public bool TurnEndAdjust(Enemy enemy);
}

/// <summary>
/// ターン終了時に1ずつ減少するもの
/// </summary>
public class StateContinueTypeDecreaseBase: StateContinueTypeBase
{
    public StateContinueTypeDecreaseBase() : base()
    {

    }

    public override bool TurnEndAdjust()
    {
        value--;
        return true;
    }
    public override bool TurnEndAdjust(Enemy enemy)
    {
        value--;
        return true;
    }
}

/// <summary>
/// ターン終了時に減少しないもの
/// </summary>
public class StateContinueTypeConstantBase : StateContinueTypeBase
{
    public StateContinueTypeConstantBase() : base()
    {

    }
    public override bool TurnEndAdjust()
    {
        return false;
    }
    public override bool TurnEndAdjust(Enemy enemy)
    {
        return false;
    }
}

/// <summary>
/// ターン終了時に減少せず、固有の値を持たず不変なもの
/// </summary>
public class StateContinueTypeEternalBase : StateContinueTypeBase
{
    public StateContinueTypeEternalBase() : base()
    {

    }

    public override bool TurnEndAdjust()
    {
        return false;
    }
    public override bool TurnEndAdjust(Enemy enemy)
    {
        return false;
    }
}

/// <summary>
/// 状態異常であり、プレイヤーに設定されるもの
/// </summary>
public interface IStatePlayer : IState
{
    
}

/// <summary>
/// 状態異常であり、敵に設定されるもの
/// </summary>
public interface IStateEnemy : IState
{

}

/// <summary>
/// 状態異常であり、仲間に設定されるもの
/// </summary>
public interface IStateFellow : IState
{

}

#region プレイヤー状態異常定義
/// <summary>
/// カードのダメージの増減を行う状態異常
/// </summary>
public interface IStateCardDamage: IStatePlayer
{
    /// <summary>
    /// Cardからdamageの値を受け取り、その値を
    /// 状態異常によって調整した値を返す。
    /// </summary>
    /// <param name="card">状態異常適用対象のCard</param>
    /// <param name="damage">元のダメージ</param>
    /// <returns>適用後のダメージ</returns>
    int AdjustCardDamage(Card card, int damage);
}

/// <summary>
/// カードのコストの増減を行う状態異常
/// </summary>
public interface IStateCardCost : IStatePlayer, IStateHasPriority
{
    /// <summary>
    /// Cardからcostの値を受け取り、
    /// その値を状態異常によって調整した値を返す
    /// </summary>
    /// <param name="card">状態異常適用対象のCard</param>
    /// <param name="cost">元のコスト</param>
    /// <returns>適用後のコスト</returns>
    int AdjustCardCost(Card card, int cost);
}

/// <summary>
/// 武器コストの増減を行う状態異常
/// </summary>
public interface IStateWeaponCost : IStatePlayer
{
    int AdjustWeaponCost(Weapon weapon, int cost);
}

public interface IStateWeaponDamage : IStatePlayer
{
    int AdjustWeaponDamage(Weapon weapon, int damage);
}

/// <summary>
/// 攻撃可能な敵を変動させる状態異常
/// </summary>
public interface IStateEffectableToEnemy : IStatePlayer
{
    /// <summary>
    /// EnemyCardZone内で使用され、
    /// targetにカード使用が可能かを判定する
    /// </summary>
    /// <param name="enemyCardZone">状態異常適用対象のEnemyCardZone</param>
    /// <param name="targetDefine">判定するCardの攻撃範囲</param>
    /// <returns>trueなら攻撃可能</returns>
    bool IsEffectable(EnemyCardZone enemyCardZone,TargetDefine target);
}

/// <summary>
/// プレイヤーがダメージを受けるタイミングに、ダメージ効果を変更する状態異常
/// </summary>
public interface IStatePlayerWhenAttacked : IStatePlayer, IStateHasPriority
{
    //ダメージ演出の際に、演出の変更を示す
    SpecialAttackEffectDefine.SpecialEffect ChangePlayerAttackedEffect(SpecialAttackEffectDefine.SpecialEffect nowEffect);

    /// <summary>
    /// ダメージ計算の際に、計算の変更を示す。
    /// EnemyからPlayerへの攻撃の際(ExecuteAction)に使用
    /// </summary>
    /// <param name="player">ダメージを受けるPlayer</param>
    /// <param name="damage">そのダメージ量</param>
    /// <returns>計算の変更が起こったならtrue</returns>
    bool ChangePlayerAttackedOperation(Player player,int damage);
}

/// <summary>
/// プレイヤーがダメージを受けるタイミングに、敵の攻撃のダメージ量を変更する状態異常
/// </summary>
public interface IStatePlayerDamagedValue : IStatePlayer
{
    /// <summary>
    /// ダメージ量の変化を示す
    /// </summary>
    /// <param name="player">Player</param>
    /// <param name="damage">変更前のDamage</param>
    /// <returns>変更後のDamage</returns>
    int ChangeDamagedValue(Player player, int damage);
}

/// <summary>
/// カードによって消費されうる状態異常
/// </summary>
public interface IStateConsumable : IStatePlayer
{

    /// <summary>
    /// 消費できるかの判定をする際に使用。
    /// 十分な状態異常の量があるかを計測
    /// </summary>
    /// <param name="value">判定を行う状態異常の量</param>
    /// <returns>trueなら消費可能</returns>
    bool IsHaveEnoughState(int value);

    /// <summary>
    /// 消費できるなら、Card内に消費する状態異常を記録する
    /// </summary>
    /// <param name="card">適用対象のCard</param>
    /// <param name="_value">必要な量</param>
    void RecordConsumeState(Card card, int _value);

    /// <summary>
    /// 消費できるなら、Weapon内に消費する状態異常を記録する
    /// </summary>
    /// <param name="card">適用対象のWeapon</param>
    /// <param name="_value">必要な量</param>
    void RecordConsumeState(Weapon weapon, int _value);
}

public interface IStatePlayerTurnStart : IStatePlayer
{
    /// <summary>
    /// ターン開始時に処理を行う
    /// </summary>
    /// <param name="player"></param>
    void TurnStartEffect(Player player);
}

public interface IStateAfterUsingCard : IStatePlayer
{
    /// <summary>
    /// カード使用後に処理を行う
    /// 処理はSequence上で、CardEffectExecuteから実行される
    /// </summary>
    /// <param name="cardEffectExecute">カード効果処理クラス</param>
    /// <param name="sequence">カードを使用したときのSequence</param>
    /// <param name="card">使用したカード</param>
    /// /// <param name="player">使用するプレイヤー</param>
    void AfterUsingCard(CardEffectExecute cardEffectExecute,Sequence sequence, Card card, Player player);
}

public interface IStateAfterUsingWeapon : IStatePlayer
{
    /// <summary>
    /// 武器使用後に処理を行う
    /// 処理はSequence上で、CardEffectExecuteから実行される
    /// </summary>
    /// <param name="cardEffectExecute">効果処理クラス</param>
    /// <param name="sequence">武器を使用したときのSequence</param>
    /// <param name="weapon">使用した武器</param>
    /// <param name="player">使用するプレイヤー</param>
    void AfterUsingWeapon(CardEffectExecute cardEffectExecute, Sequence sequence, Weapon weapon, Player player);
}

/// <summary>
/// 装備で使用される状態異常
/// これのデータをコピーしてバトル中に使用する
/// </summary>
public interface IStateInGear : IStatePlayer
{
    public Gear Gear { get; set; } //この状態異常が紐づくGear

    public IStateInGear Clone(Gear gear); //クローンを作成する.それはGearに紐づく
}

public interface IStatePlayerTurnEnd : IStatePlayer
{
    /// <summary>
    /// ターン終了時に処理を行う
    /// </summary>
    /// <param name="player">Player</param>
    void TurnEndEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Player player);
}
#endregion

#region 敵状態異常定義

/// <summary>
/// 敵のダメージの増減を操作する状態異常
/// </summary>
public interface IStateEnemyDamage : IStateEnemy
{
    /// <summary>
    /// Enemyからdamageの値を受け取り、その値を
    /// 状態異常によって調整した値を返す。
    /// </summary>
    /// <param name="card">状態異常適用対象のCard</param>
    /// <param name="damage">元のダメージ</param>
    /// <returns>適用後のダメージ</returns>
    int AdjustDamage(Enemy enemy, int damage);
}

/// <summary>
/// 敵が倒れるタイミングに効果を持つ状態異常
/// </summary>
public interface IStateEnemyDefeat : IStateEnemy
{
    /// <summary>
    /// 死亡時効果の処理をEnemyManagerに依頼する
    /// </summary>
    /// <param name="manager">処理を行うもの</param>
    /// <param name="enemy">死亡したEnemy</param>
    void DefeatEffect(EnemyManager manager, Enemy enemy);
}

/// <summary>
/// 敵がダメージを受けるタイミングに、ダメージ効果を変更する状態異常
/// </summary>
public interface IStateEnemyWhenAttacked : IStateEnemy
{
    SpecialAttackEffectDefine.SpecialEffect ChangeEnemyAttackedOperation(SpecialAttackEffectDefine.SpecialEffect nowEffect,Enemy enemy);
}

/// <summary>
/// プレイヤーからダメージを受けるタイミングに、敵の受けるダメージ量を変更する状態異常
/// </summary>
public interface IStateEnemyDamagedValue : IStateEnemy, IStateHasPriority
{
    /// <summary>
    /// ダメージ量の変化を示す
    /// </summary>
    /// <param name="enemy">Player</param>
    /// <param name="damage">変更前のDamage</param>
    /// <param name="attributes">ダメージの属性</param>
    /// <returns>変更後のDamage</returns>
    int ChangeDamagedValue(Enemy enemy, int damage, List<AttributeDefine> attributes);
}

public interface IStateEnemyTurnEnd : IStateEnemy
{
    /// <summary>
    /// ターン終了時に処理を行う
    /// </summary>
    /// <param name="cardEffectExecute">カード実行クラス</param>
    /// <param name="sequence">効果が乗るSequence</param>
    /// <param name="enemy">対象の敵</param>
    void TurnEndEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Enemy enemy);
}

public interface IStateEnemyTurnStart : IStateEnemy
{
    /// <summary>
    /// ターン開始時に処理を行う
    /// </summary>
    /// <param name="cardEffectExecute">カード実行クラス</param>
    /// <param name="sequence">効果が乗るSequence</param>
    /// <param name="enemy">対象の敵</param>
    void TurnStartEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Enemy enemy);
}
public interface IStateEnemyAction : IStateEnemy
{
    /// <summary>
    /// 敵の行動を変更する処理
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns>変更後の敵の行動</returns>
    public Enemy.ActualNextAction ChangeEnemyAction(Enemy enemy);
}

/// <summary>
/// 敵のダメージ後に発生する効果
/// </summary>
public interface IStateEnemyDamaged : IStateEnemy
{
    /// <summary>
    /// 敵のダメージの後に発生する効果
    /// </summary>
    /// <param name="cardEffectExecute">効果処理クラス</param>
    /// <param name="sequence">効果の乗るSequence</param>
    /// <param name="enemy">攻撃を受けた敵</param>
    public void OnDamagedEffect(CardEffectExecute cardEffectExecute, Sequence sequence,Enemy enemy);
}
#endregion

#region 仲間状態異常定義
#endregion

#region 全般状態異常定義
/// <summary>
/// 他の状態異常と相殺されるもの
/// </summary>
public interface IStateOffsetable: IState
{
    /// <summary>
    /// 状態異常を与えられて、
    /// 相殺が必要な物かを判定する
    /// </summary>
    /// <param name="stateType">判定する状態異常</param>
    /// <returns>相殺が必要ならtrue</returns>
    bool IsNeedsOffset(IState state);
}

/// <summary>
/// 属性を持つ状態異常
/// </summary>
public interface IStateHasAttribute : IState
{
    //内部に属性を持っており、コンストラクタで設定する
    public AttributeDefine.Attribute stateAttribute { set; get; }
}

/// <summary>
/// 優先度を持つ状態異常.優先度は最も数字の大きいものから順に適用される
/// </summary>
public interface IStateHasPriority : IState
{
    //優先度は5段階(0-4).大きいほど優先度が高い
    public int Priority { get; }
}

//何らかのカウントを持つ状態異常
public interface IStateHasCount : IState {
    public IStateCounter IStateCounter { get; set; }
}

/// <summary>
/// 1ターンで使用できる回数が決まっている状態異常。
/// ターンの開始時に数がリセットされる
/// </summary>
public interface IStateCountPerTurn : IStateHasCount
{
}

/// <summary>
/// バトル中共通するカウントを持つ状態異常
/// カウントがリセットされない
/// </summary>
public interface IStateCountInBattle : IStateHasCount
{
}

#endregion

#region 状態異常内部で使用するクラス

/// <summary>
/// 状態異常のカウントを行うクラス
/// カードなどに使用されたStateとして登録されると、使用時に変動する
/// </summary>
public interface IStateCounter
{
    int Count { get; }

    int CountMax { get; }

    /// <summary>
    /// カウンターをリセットする(残っている数を上限にする)
    /// </summary>
    public void ResetCount();

    /// <summary>
    /// カウントを変動させる
    /// </summary>
    public void CountNum();

    /// <summary>
    /// カウントが残っているかを判定する
    /// </summary>
    /// <returns>カウントが残っているか</returns>
    public bool IsCountRemaining();

    public int GetCount();
}

/// <summary>
/// 状態異常のカウントを行うクラス
/// ターン開始時に最大値にリセットされ、
/// 使用されるごとに値が減少する
/// 
/// </summary>
public class StateDecreaseCounter : IStateCounter
{
    public int Count { get { return count; } }

    public int CountMax { get { return countMax; } }

    private int count;
    private int countMax;

    public StateDecreaseCounter(int _countMax)
    {
        countMax = _countMax;
        count = countMax;
    }

    /// <summary>
    /// カウンターをリセットする(残っている数を上限にする)
    /// </summary>
    public void ResetCount()
    {
        count = countMax;
    }

    /// <summary>
    /// カウントを減少させる
    /// </summary>
    public void CountNum()
    {
        if(count>0)count--;
    }

    /// <summary>
    /// カウントが残っているかを判定する
    /// </summary>
    /// <returns>カウントが残っているか</returns>
    public bool IsCountRemaining()
    {
        if (count > 0) return true;
        else return false;
    }

    public int GetCount()
    {
        return count;
    }
}

/// <summary>
/// 状態異常のカウントを行うクラス
/// ターン開始時に0にリセットされ、
/// 使用されるごとに値が増加する
/// </summary>
public class StateIncreaseCounter : IStateCounter
{
    public int Count { get { return count; } }

    public int CountMax { get { return countMax; } }

    private int count;
    private int countMax;

    public StateIncreaseCounter(int _countMax)
    {
        countMax = _countMax;
        count = 0;
    }

    /// <summary>
    /// カウンターをリセットする(残っている数を上限にする)
    /// </summary>
    public void ResetCount()
    {
        count = 0;
    }

    /// <summary>
    /// カウントを増加させる
    /// </summary>
    public void CountNum()
    {
        if (count<countMax) count--;
    }

    /// <summary>
    /// カウントが残っているかを判定する
    /// </summary>
    /// <returns>カウントが残っているか</returns>
    public bool IsCountRemaining()
    {
        if (count < countMax) return true;
        else return false;
    }

    public int GetCount()
    {
        return count;
    }
}

#endregion