using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttributeDefine;

/// <summary>
/// IStateの比較(一致判定を行う)
/// 属性を持つIState(IStateHasAttribute)
/// なら、その属性も含めて判定
/// </summary>
public static class StateCompare
{
    public static bool IsSameState(IState state1,IState state2)
    {
        if(state1.GetType() != state2.GetType())
        {//型が違うならfalse
            return false;
        }

        if(state1 is IStateHasAttribute&&state2 is IStateHasAttribute){
            //属性を持っているなら
            IStateHasAttribute state1a = state1 as IStateHasAttribute;
            IStateHasAttribute state2a = state2 as IStateHasAttribute;

            if(state1a.stateAttribute != state2a.stateAttribute)
            {//属性が異なるならtrue
                return false;
            }
        }

        //否定条件がないならtrue
        return true;
    }
}

public static class StateCopy
{
    /// <summary>
    /// 状態効果を情報からコピーして値を設定する
    /// </summary>
    /// <param name="state">コピー元の状態効果</param>
    /// <param name="value">状態効果にセットされる値</param>
    /// <returns>コピーされ、生成された状態効果</returns>
    public static IState CopyInstanceAndSetState(IState state, int value)
    {
        IState newState = (IState)Activator.CreateInstance(state.GetType());
        newState.value = value;//値を設定
        if (state is IStateHasAttribute)
        {
            //属性を持っているなら、属性もコピー
            IStateHasAttribute stateHasAttribute = (IStateHasAttribute)state;
            ((IStateHasAttribute)newState).stateAttribute = stateHasAttribute.stateAttribute;
        }

        return newState;
    }
}

/// <summary>
/// 攻撃力Down
/// </summary>
public class StateAttackDown : StateContinueTypeDecreaseBase,
    IStateCardDamage, IStateEnemyDamage, IStateWeaponDamage, IStateOffsetable, IStateFellow/*test*/
{
    public StateAttackDown() : base()
    {

    }

    public int AdjustCardDamage(Card card, int damage)
    {
        return damage - value;
    }

    public int AdjustDamage(Enemy enemy, int damage)
    {
        return damage - value;
    }

    public int AdjustWeaponDamage(Weapon weapon, int damage)
    {
        return damage - value;
    }

    public bool IsNeedsOffset(IState state)
    {
        //AttackUpと相殺
        if (state is StateAttackUp) return true;
        else return false;
    }
}

/// <summary>
/// 攻撃力Up
/// </summary>
public class StateAttackUp : StateContinueTypeDecreaseBase,
    IStateCardDamage, IStateEnemyDamage,IStateWeaponDamage, IStateOffsetable, IStateFellow/*test*/
{
    public StateAttackUp() : base()
    {

    }

    public int AdjustCardDamage(Card card, int damage)
    {
        return damage + value;
    }

    public int AdjustDamage(Enemy enemy, int damage)
    {
        return damage + value;
    }

    public int AdjustWeaponDamage(Weapon weapon, int damage)
    {
        return damage + value;
    }

    public bool IsNeedsOffset(IState state)
    {
        //AttackUpと相殺
        if (state is StateAttackDown) return true;
        else return false;
    }
}

/// <summary>
/// 属性攻撃力UP
/// </summary>
public class StateAttackUpByAttribute: StateContinueTypeConstantBase,
    IStateCardDamage, IStateWeaponDamage, IStateHasAttribute, IStateOffsetable
{
    [SerializeField] public AttributeDefine.Attribute attribute;

    public AttributeDefine.Attribute stateAttribute { get { return attribute; } set { attribute = value; } }

    public StateAttackUpByAttribute() : base()
    {

    }

    public int AdjustCardDamage(Card card, int damage)
    {
        if (card.IsItemHasAttribute(stateAttribute))
        {//カードが属性を持っているなら
            card.AddUsingStateNew(this, value);
            return damage + value;
        }
        else
        {
            return damage;
        }
    }

    public int AdjustWeaponDamage(Weapon weapon,int damage)
    {
        if (weapon.IsItemHasAttribute(stateAttribute))
        {
            weapon.AddUsingStateNew(this, value);
            return damage + value;
        }
        else
        {
            return damage;
        }
    }

    public bool IsNeedsOffset(IState state)
    {
        //同じ属性の属性Downと相殺
        if (state is StateAttackDownByAttribute)
        {
            StateAttackDownByAttribute stateA = state as StateAttackDownByAttribute;
            if(stateAttribute == stateA.stateAttribute)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// 属性攻撃力DOWN
/// </summary>
public class StateAttackDownByAttribute : StateContinueTypeConstantBase,
    IStateCardDamage, IStateWeaponDamage, IStateHasAttribute, IStateOffsetable
{
    [SerializeField] public AttributeDefine.Attribute attribute;

    public AttributeDefine.Attribute stateAttribute { get { return attribute; } set { attribute = value; } }

    public StateAttackDownByAttribute() : base()
    {
    }

    public int AdjustCardDamage(Card card, int damage)
    {
        if (card.IsItemHasAttribute(stateAttribute))
        {//カードが属性を持っているなら
            card.AddUsingStateNew(this, value);
            return damage - value;
        }
        else
        {
            return damage;
        }
    }

    public int AdjustWeaponDamage(Weapon weapon, int damage)
    {
        if (weapon.IsItemHasAttribute(stateAttribute))
        {
            weapon.AddUsingStateNew(this, value);
            return damage - value;
        }
        else
        {
            return damage;
        }
    }

    public bool IsNeedsOffset(IState state)
    {
        //同じ属性の属性Upと相殺
        if (state is StateAttackUpByAttribute)
        {
            StateAttackUpByAttribute stateA = state as StateAttackUpByAttribute;
            if (stateAttribute == stateA.stateAttribute)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// 視野狭窄状態。敵前列にしか攻撃できない
/// </summary>
public class StateTunnelVision : StateContinueTypeDecreaseBase, IStateEffectableToEnemy
{
    public StateTunnelVision() : base()
    {
    }

    public bool IsEffectable(EnemyCardZone enemyCardZone, TargetDefine target)
    {
        if(enemyCardZone.GetEnemyPos()==0&& (target.effectTarget == TargetDefine.EffectTarget.ShortRange ||
                    target.effectTarget == TargetDefine.EffectTarget.MediumRange ||
                    target.effectTarget == TargetDefine.EffectTarget.LongRange ||
                    target.effectTarget == TargetDefine.EffectTarget.EnemyAll)){
            return true;
        }

        return false;
    }
}

/// <summary>
/// 盲目状態。敵に攻撃できない
/// </summary>
public class StateBlindness : StateContinueTypeDecreaseBase, IStateEffectableToEnemy
{
    public StateBlindness() : base()
    {
    }

    public bool IsEffectable(EnemyCardZone enemyCardZone, TargetDefine target)
    {
        return false;
    }
}

/// <summary>
/// 属性を参照したカードのコストダウン
/// </summary>
public class StateCostDownByAttribute : StateContinueTypeConstantBase, IStateCardCost,
    IStateHasAttribute
{
    public StateCostDownByAttribute() : base()
    {
        Priority = 1;//最も優先度が低い
    }

    public int Priority { get; }

    [SerializeField] public AttributeDefine.Attribute attribute;

    public AttributeDefine.Attribute stateAttribute { get { return attribute; } set { attribute = value; } }

    public int AdjustCardCost(Card card, int cost)
    {
        if (card.IsItemHasAttribute(stateAttribute)&&cost>0)
        {//Cardが属性を持っており、コストが0以上なら
            card.AddUsingStateNew(this, 1);
            return cost -1;
        }
        else
        {
            return cost;
        }
    }
}

/// <summary>
/// 武器のコストダウン
/// </summary>
public class StateWeaponCostDown : StateContinueTypeConstantBase, IStateWeaponCost
{
    public StateWeaponCostDown() : base()
    {
    }

    public int AdjustWeaponCost(Weapon weapon, int cost)
    {
        if (cost > 0)
        {//コストが0以上なら
            weapon.AddUsingStateNew(this, 1);
            return cost-1;
        }
        else
        {
            return cost;
        }
    }
}

/// <summary>
/// 消費、獲得可能な祈り
/// </summary>
public class StatePlay : StateContinueTypeConstantBase, IStateConsumable
{
    public StatePlay() : base()
    {
    }

    public bool IsHaveEnoughState(int _value)
    {
        if (value >= _value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RecordConsumeState(Card card, int _value)
    {
        if (value >= _value)
        {//必要な量のStateを持っているなら使用を記録
            card.AddUsingStateNew(this, _value);
        }
        else
        {
        }
    }

    public void RecordConsumeState(Weapon weapon, int _value)
    {
        if (value >= _value)
        {//必要な量のStateを持っているなら使用を記録
            weapon.AddUsingStateNew(this, _value);
        }
        else
        {
        }
    }
}

/// <summary>
/// アイドル。倒された時に敵全体に攻撃力上昇
/// </summary>
public class StateIdol : StateContinueTypeEternalBase, IStateEnemyDefeat
{
    public StateIdol() : base()
    {
    }

    public void DefeatEffect(EnemyManager manager,Enemy enemy)
    {
        //敵全体に攻撃力UP2
        manager.StateToEnemyAllExceptThis(typeof(StateAttackUp), 2,enemy.GetPos());

        //これを除いた敵全体にバフエフェクト
        manager.EnemyAllEffectExceptThis(EnemyActionDefine.EnemyActionType.BuffAll, EnemyActionDefine.EnemyAction.GetBuffAll
            , 2, enemy.GetPos());
    }
}

/// <summary>
/// 剣先.巨大な剣の先端部分。破壊すると剣が短くなる
/// </summary>
public class StateBBSwordFront : StateContinueTypeEternalBase, IStateEnemyDefeat
{
    public StateBBSwordFront() : base()
    {
    }

    public void DefeatEffect(EnemyManager manager, Enemy enemy)
    {
        manager.BBSwordFrontDead();
    }
}

/// <summary>
/// 剣中.巨大な剣の中間部分。破壊すると剣が短くなる
/// </summary>
public class StateBBSwordMiddle : StateContinueTypeEternalBase, IStateEnemyDefeat
{
    public StateBBSwordMiddle() : base()
    {
    }

    public void DefeatEffect(EnemyManager manager, Enemy enemy)
    {
        manager.BBSwordMiddleDead();
    }
}

/// <summary>
/// 従属。他の全ての従属でないEnemyが倒されると死亡.あるだけ
/// </summary>
public class StateDependent : StateContinueTypeEternalBase,IStateEnemy
{
    public StateDependent() : base()
    {
    }
}

/// <summary>
/// 回避。敵の攻撃を回避する
/// ダメージ演出も変化
/// </summary>
public class StateAvoid : StateContinueTypeConstantBase, IStateEnemyWhenAttacked,
    IStatePlayerWhenAttacked, IStateFellow//TODO
{
    public int Priority { get; }

    public StateAvoid() : base()
    {
        Priority = 3;//優先度が最も高い
    }

    public SpecialAttackEffectDefine.SpecialEffect ChangePlayerAttackedEffect(SpecialAttackEffectDefine.SpecialEffect nowEffect)
    {
        //回避を返す.ダメージ演出は行わない
        return SpecialAttackEffectDefine.SpecialEffect.Avoid;
    }

    public bool ChangePlayerAttackedOperation(Player player, int damage)
    {
        player.ConsumeState(this, 1);
        //回避したので何もしない
        return true;
    }
    public SpecialAttackEffectDefine.SpecialEffect ChangeEnemyAttackedOperation(SpecialAttackEffectDefine.SpecialEffect nowEffect,Enemy enemy)
    {
        enemy.ConsumeState(this,1);
        //回避を返す.ダメージ演出は行わない
        return SpecialAttackEffectDefine.SpecialEffect.Avoid;
    }
}

/// <summary>
/// 防御力Down
/// </summary>
public class StateDefenseDown : StateContinueTypeDecreaseBase,
    IStatePlayerDamagedValue, IStateEnemyDamagedValue, IStateFellow
{
    public StateDefenseDown() : base()
    {
        Priority = 3;//優先度が最も高い(倍率系より先に発生)
    }

    public int Priority { get; }

    public int ChangeDamagedValue(Player player, int damage)
    {
        return damage+value;
    }

    public int ChangeDamagedValue(Enemy enemy, int damage, List<AttributeDefine> attributes)
    {
        return damage+value;
    }
}

/// <summary>
/// 陽の構え
/// </summary>
public class StateEnbuYang : StateContinueTypeEternalBase, IStatePlayer, IStateConsumable
{
    public StateEnbuYang() : base()
    {
    }

    public bool IsHaveEnoughState(int _value)
    {
        return true;//Eternalなので常に十分な量を持っているとみなす
    }

    public void RecordConsumeState(Card card, int _value)
    {
        card.AddUsingStateNew(this, 1);
    }

    public void RecordConsumeState(Weapon weapon, int _value)
    {
        weapon.AddUsingStateNew(this, 1);
    }
}

/// <summary>
/// 陰の構え
/// </summary>
public class StateEnbuYin : StateContinueTypeEternalBase, IStatePlayer, IStateConsumable
{
    public StateEnbuYin() : base()
    {
    }

    public bool IsHaveEnoughState(int _value)
    {
        return true;//Eternalなので常に十分な量を持っているとみなす
    }

    public void RecordConsumeState(Card card, int _value)
    {
        card.AddUsingStateNew(this, 1);
    }

    public void RecordConsumeState(Weapon weapon, int _value)
    {
        weapon.AddUsingStateNew(this, 1);
    }
}

/// <summary>
/// 多感。ターンの最初に使用する精神カードのコストを0にする
/// </summary>
public class StateTakan : StateContinueTypeEternalBase, IStateCardCost, IStateCountPerTurn
{
    public StateTakan() : base()
    {
        IStateCounter = new StateDecreaseCounter(1);
        Priority = 3;//優先度が最も高い
    }

    public IStateCounter IStateCounter { get; set; }

    public int Priority { get; }

    public int AdjustCardCost(Card card, int cost)
    {
        if(IStateCounter.IsCountRemaining()&&card.IsItemHasAttribute(AttributeDefine.Attribute.Mind))
        {//カウントが残っているなら,精神属性のカードなら
            card.AddUsingStateNew(this, 1);
            //コストを0にする
            return 0;
        }
        else
        {
            return cost;
        }
    }
}

/// <summary>
/// 属性攻撃力UPをターン開始時に解除する
/// </summary>
public class StateRemoveAttributeUp : StateContinueTypeEternalBase,
    IStateHasAttribute, IStatePlayerTurnStart
{
    public StateRemoveAttributeUp() : base()
    {
    }

    [SerializeField] public AttributeDefine.Attribute attribute;

    public AttributeDefine.Attribute stateAttribute { get { return attribute; } set { attribute = value; } }

    public void TurnStartEffect(Player player)
    {
        player.GetPlayerBattleManager().SetTurnStartEvent(
            (CardEffectExecute cardEffectExecute,Sequence sequence,Player player1) =>
        {
            //ターン開始時に効果を発動。自身を消す。
            player1.ConsumeState(this,this.value);
            //対応する属性UPを消滅
            player1.SearchAndUseState<StateAttackUpByAttribute>(s =>
            {
                if (s.stateAttribute == stateAttribute)
                {
                    player1.ConsumeState(s, s.value);
                    return;
                }
            });
        });
    }
}

/// <summary>
/// ターン開始時にブロックを付与する
/// </summary>
public class StateBlockOnTurnStart : StateContinueTypeConstantBase, IStatePlayerTurnStart
{
    public StateBlockOnTurnStart() : base()
    {
    }

    public void TurnStartEffect(Player player)
    {
        player.GetPlayerBattleManager().SetTurnStartEvent(
            (CardEffectExecute cardEffectExecute, Sequence sequence, Player player1) =>
        {
            var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Physics) };

            //ターン開始時に効果を発動する効果を記録する場所に記録する。
            //ブロック効果
            cardEffectExecute.Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.Block, value), value, null),
                attributes, sequence, 0);
            
            //自身を消す。
            player1.ConsumeState(this, this.value);
        });
    }
}


/// <summary>
/// 同名カードを2枚使用したときにシールドを生成する
/// </summary>
public class StateDanceBoots : StateContinueTypeEternalBase, IStateAfterUsingCard,
    IStateCountPerTurn,IStateInGear
{
    //コンストラクターに値があるとSubclassSelectorAttributeでエラーが出るので、
    //シリアライズされた値を使用する
    //->と思ったが、シリアライズされた値も、ScriptableObjectでは使用できない？
    //生成時に設定すること。
    public StateDanceBoots() : base()
    {
        //同名カードが2枚使用された時にカウントが減る
        IStateCounter = new StateDecreaseCounter(1);
    }

    public IStateCounter IStateCounter { get; set; }

    public Gear Gear { get; set; }

    public void AfterUsingCard(CardEffectExecute cardEffectExecute, Sequence sequence, Card card, Player player)
    {
        if(cardEffectExecute.GetDataPerTurn().IsSomeCardUsedNTimes(2))
        {//同名カードが2枚使用されたなら
            if (IStateCounter.IsCountRemaining())
            {//カウントが残っているなら
             //ダンスブーツの効果を発動
             //ブロック3
                var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Physics) };

                cardEffectExecute.Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.Block, value), value, null),
                    attributes, sequence, 0);

                //カウントを減らす
                Gear.DecreaseCountOfCountState(this);
            }
        }
    }

    public IStateInGear Clone(Gear gear)
    {
        StateDanceBoots clone = new StateDanceBoots();
        clone.value = this.value;
        clone.Gear = gear;
        return clone;
    }
}

/// <summary>
/// 装備アイテムが、バトル中の状態異常を持っていない状態
/// </summary>
public class StateNone : StateContinueTypeEternalBase, IStateInGear
{
    public StateNone() : base()
    {
    }

    public Gear Gear { get; set; }

    public IStateInGear Clone(Gear gear)
    {
        StateNone clone = new StateNone();
        clone.value = this.value;
        clone.Gear = gear;
        return clone;
    }
}

public class StateBurn : StateContinueTypeConstantBase, IStatePlayerTurnEnd, IStateEnemyTurnEnd
{
    public StateBurn() : base()
    {
    }
    public void TurnEndEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Player player)
    {
        var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Pyro) };

        //ターン終了時にダメージを与える
        cardEffectExecute.Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.SelfDamage, value), value, null),
            attributes, sequence, 0);
        sequence.AppendCallback(() =>
        {
            //ダメージ演出が終わった後に、状態異常を消す
            player.ConsumeState(this, this.value);
        });
    }

    public void TurnEndEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Enemy enemy)
    {
        var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Pyro) };
        //ターン終了時にダメージを与える
        cardEffectExecute.Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.Damage, value),value, null)
            , enemy,attributes, sequence, 0);
        sequence.AppendCallback(() =>
        {
            //ダメージ演出が終わった後に、状態異常を消す
            enemy.ConsumeState(this, this.value);
        });
    }
}

public class StateStan : StateContinueTypeDecreaseBase, IStateEnemyAction,IStatePlayerTurnStart
{
    public StateStan() : base()
    {
    }
    public Enemy.ActualNextAction ChangeEnemyAction(Enemy enemy)
    {
        List<EnemyActionDefine> notMoveAction = new List<EnemyActionDefine>();
        notMoveAction.Add(new EnemyActionDefine
        {
            enemyAction = EnemyActionDefine.EnemyAction.NotMove,
            value = 0
        });

        //スタン状態なら、敵の行動を無効化する
        //何もしない行動をセットする。
        Enemy.ActualNextAction nextAction = new Enemy.ActualNextAction(
            new ActionInTurn()
            {
                actionsInTurn = notMoveAction
            }
            );

        return nextAction;
    }

    //エネルギーを0まで減らす
    public void TurnStartEffect(Player player)
    {
        FieldManager fieldManager = player.GetPlayerBattleManager().GetFieldManager();
        fieldManager.UseEnergy(fieldManager.GEtEnergy());
    }
}

public class StateAsyura : StateContinueTypeDecreaseBase, IStateAfterUsingWeapon
{
    public StateAsyura() : base()
    {
    }

    public void AfterUsingWeapon(CardEffectExecute cardEffectExecute, Sequence sequence, Weapon weapon,Player player)
    {
        var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Faith) };

        //1の祈りを追加する
        cardEffectExecute.Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.GetStateBuff, 1), 1, new StatePlay()),
                    attributes, sequence, 0);

        //カウントを減らす
        player.ConsumeState(this, 1);
    }

    public override bool TurnEndAdjust()
    {
        value = 0;
        return true;
    }
}

/// <summary>
/// 敵がダメージを受けるとスタンを得る
/// ターン中1回のカウントを持つ
/// </summary>
public class StateCasting : StateContinueTypeEternalBase, IStateEnemyDamaged,IStateCountPerTurn
{
    public IStateCounter IStateCounter { get; set; }

    public StateCasting() : base() {
        IStateCounter = new StateDecreaseCounter(1);
    }

    public void OnDamagedEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Enemy enemy)
    {
        //敵がすでにスタン状態なら終了
        if(!IStateCounter.IsCountRemaining())
        {
            return;
        }

        var attributes = new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Physics) };

        //ダメージを受けるとスタンを受ける
        cardEffectExecute.Execute(
            new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.CauseState, 1), 1, new StateStan()),
            enemy,
            attributes,
            sequence,
            0
            );

        //カウントを減らす
        IStateCounter.CountNum();
        enemy.RefreshState();
    }
}

/// <summary>
/// ある属性のダメージが二倍
/// </summary>
public class StateAttributeWeakness : StateContinueTypeEternalBase, IStateEnemyDamagedValue, IStateHasAttribute
{
    public StateAttributeWeakness() : base() {
        Priority = 1;//優先度が最も低い(倍率なので最後に計算)
    }

    public int Priority { get; }

    [SerializeField] public AttributeDefine.Attribute attribute;

    public AttributeDefine.Attribute stateAttribute { get { return attribute; } set { attribute = value; } }

    public int ChangeDamagedValue(Enemy enemy, int damage, List<AttributeDefine> attributes)
    {
        foreach (AttributeDefine attribute in attributes) {
            if(attribute.attribute == stateAttribute)
            {//属性が一つでも同じなら
                return damage * 2;//ダメージ二倍
            }
        }

        return damage;//それ以外はそのまま
    }
}

/// <summary>
/// ターン終了時に最大までステラビットを召喚する
/// </summary>
public class StateCreateStellabit : StateContinueTypeEternalBase, IStateEnemyTurnEnd
{
    public StateCreateStellabit() : base() { }

    public void TurnEndEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Enemy enemy)
    {
        EnemyManager enemyManager = enemy.GetEnemyManager();
        //限界までステラビットを召喚する
        for(int i = 0; i < 2; i++)
        {
            enemyManager.SummonEnemy(sequence,
                EnemySummonManager.SummonedEnemyType.StellaBit,
                EnemySummonManager.SummonPolicy.Back);
        }
    }
}

/// <summary>
/// フォーリンスター。4ターン後にvalue*5のダメージを出す。
/// valueは味方のステラビットが破壊されるたび減少
/// </summary>
public class StateFallenStar : StateContinueTypeEternalBase,IStateEnemyAction,
    IStateCountInBattle
{
    //経過ターンを数えるカウンター
    public IStateCounter TurnCounter { get; set; }

    public IStateCounter IStateCounter { get; set; }

    public StateFallenStar():base() {
        IStateCounter = new StateDecreaseCounter(8);
        TurnCounter = new StateDecreaseCounter(4);
    }

    /// <summary>
    /// ターン終了時にフォーリンスターを発動する
    /// カウントを減らす
    /// </summary>
    /// <param name="enemy">敵のターン終了補正はenemyが必要</param>
    /// <returns>baseと同じ</returns>
    public override bool TurnEndAdjust(Enemy enemy)
    {
        //すでに効果が発動した後なら
        if (!TurnCounter.IsCountRemaining())
        {
            //状態異常を消費
            enemy.ConsumeState(this, this.value);
        }

        TurnCounter.CountNum();
        return base.TurnEndAdjust(enemy);
    }

    public Enemy.ActualNextAction ChangeEnemyAction(Enemy enemy)
    {
        //ターンがまだならnull(変更しない)
        if (TurnCounter.IsCountRemaining()) return null;

        //必殺技のフォーリンスターに行動を変更
        List<EnemyActionDefine> action = new List<EnemyActionDefine>();
        action.Add(new EnemyActionDefine
        {
            enemyAction = EnemyActionDefine.EnemyAction.DamageEnergy,
            //カウントの五倍
            value = IStateCounter.GetCount() * 5
        });

        Enemy.ActualNextAction nextAction = new Enemy.ActualNextAction(
            new ActionInTurn()
            {
                actionsInTurn = action
            }
            );

        return nextAction;
    }
}

public class StateStellaBit : StateContinueTypeEternalBase, IStateEnemyDefeat
{
    public StateStellaBit():base() { }

    public void DefeatEffect(EnemyManager manager, Enemy enemy)
    {
        manager.ActionToEnemyAllExceptThis(e =>
        {
            bool flag = false;
            //フォーリンスターのカウントがあるならそれを減らす
            //ターンではなく、ダメージ量のカウント
            e.SearchAndUseState<StateFallenStar>(a =>
            {
                a.IStateCounter.CountNum();
                flag = true;
            });

            //状態異常の再読み込み
            if(flag) e.RefreshState();
        },
        //自身の位置
        enemy.GetPos()
        );
    }
}

/// <summary>
/// ターン開始時に自身に 値分のブロックを与える
/// 値はカウンタとして保持され、毎ターン最大値に回復(意味なし)
/// </summary>
public class StateSnowBlessing : StateContinueTypeConstantBase, IStateEnemyTurnStart
{
    public StateSnowBlessing(): base() {
        
    }

    public void TurnStartEffect(CardEffectExecute cardEffectExecute, Sequence sequence, Enemy enemy)
    {
        cardEffectExecute.Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.BlockToEnemy, value), value, null),
            enemy,
            new List<AttributeDefine>() { new AttributeDefine(AttributeDefine.Attribute.Physics) },
            sequence,
            0);
    }
}
