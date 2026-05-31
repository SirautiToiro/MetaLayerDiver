using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

/// <summary>
/// CardのPrefabに付属する管理スクリプト
/// </summary>
public class Card : ItemBase
{
    [SerializeField] private CardUI cardUI = null;//カードUI表示クラス

    //元になったデータ
    private CardDataSO baseCardData;

    //カードデータ
    public List<ActualEffect> effects;    //実際のカード効果のリスト
    public List<AttributeDefine> attributes;  //カード属性のリスト
    public int cost;//カードコスト
    public TargetDefine effectTarget;//カードの効果対象
    public List<CardTagDefine> tags;

    public List<AttributeDefine> actualAttributes;  //補正後のカード属性のリスト
    public int actualCost;//補正後のカードコスト
    public TargetDefine actualEffectTarget;//補正後の効果対象
    public int serialNum { get; set; }

    //カードのステータス変動によって使用され、カードを使用すると消費されるPlayerの状態異常
    //(IState,int)のタプルになっていて、intには、使用後の減少量が入る
    //IStateはプレイヤーのものを参照する
    public List<(IState,int)> usingStateNew { get; set; }

    public int pos { get; set; }//手札での位置

    //移動機能、アイテム右クリック機能
    private ItemMover _itemMover;
    public override ItemMover ItemMover { get { return _itemMover; } }
    private ShowDescriptionWhenRightClick _itemRightClick;
    public override IItemRightClick ItemRightClick { get { return _itemRightClick; } }

    //カードプレイ中かどうか
    private bool executingFlag;

    private HandManager handManager = null;

    public CardSpecialEffectDefine.SpecialEffect specialEffect
    {
        get
        {
            return baseCardData.specialEffect.specialEffect;
        }
    }

    /// <summary>
    /// 初期化処理。バトル中からの初期化
    /// </summary>
    /// <param name="cardData"></param>
    /// <param name="cardZone"></param>
    /// <param name="_itemManager"></param>
    /// <param name="_pos"></param>
    /// <param name="handManager"></param>
    public void Init(CardDataSO cardData, ICardZone cardZone, IItemManager _itemManager, int _pos,HandManager _handManager)
    {
        //手札内にいるので、handManagerをセット
        handManager = _handManager;

        //移動可能な初期化を行う
        Init(cardData, cardZone, _itemManager, _pos);
    }

    /// <summary>
    /// 初期化処理.移動可能
    /// </summary>
    /// <param name="cardData">カード情報の元となるSO</param>
    /// <param name="cardZone">配置されるCardZone</param>
    /// <param name="_iItemManager">生成元のIItemManager</param>
    /// /// <param name="_handPos">手札やデッキでの位置</param>
    public void Init(CardDataSO cardData,ICardZone cardZone,IItemManager itemManager,int _pos)
    {
        clickableFlag = true;

        _itemMover = new ItemMover(this);//アイテム移動機能をセット
        _itemRightClick = new ShowDescriptionWhenRightClick();//右クリックメニューをセット

        this.itemManager = itemManager;

        SetCardZone(cardZone);//カードゾーンにセット

        DataSet(cardData);//データをセット

        pos = _pos;

        if (handManager != null)
        {
            cardUI.Init(cardData, this, effects,handManager);
        }else{
            cardUI.Init(cardData, this, effects);
        }

        RefreshCard();//カード情報を補正後に更新//バトル中のみ

        executingFlag = false;
    }

    /// <summary>
    /// 初期化処理
    /// clickableFlagをfalseにすることでカードは一切動かないようになる
    /// </summary>
    /// <param name="cardData">カード情報の元となるSO</param>
    public void Init(CardDataSO cardData)
    {
        clickableFlag=false;

        _itemMover = new ItemMover(this);//アイテムムーバーをセット
        _itemRightClick = new ShowDescriptionWhenRightClick();//右クリックメニューをセット

        DataSet(cardData);//データをセット

        cardUI.Init(cardData, this, effects);

        executingFlag = true;//動かないので更新もない
    }

    /// <summary>
    /// 初期化処理で使用。
    /// CardDataSOをもとに設定する
    /// </summary>
    /// <param name="cardData">元になるデータ</param>
    private void DataSet(CardDataSO cardData)
    {
        baseCardData = cardData;

        //効果リストを移動
        //本来の値はまだ変動していないので初期状態
        foreach (var e in cardData.effectList)
        {
            effects.Add(new ActualEffect(e.Effect, e.Effect.value,e.UseState));
        }

        //データをコピー
        for (int i = 0; i < cardData.attributeList.Count; i++)
        {
            attributes.Add(cardData.attributeList[i]);
            actualAttributes.Add(cardData.attributeList[i]);
        }
        cost = cardData.cost;
        actualCost = cost;
        effectTarget = cardData.targetDefine;
        actualEffectTarget = cardData.targetDefine;

        //タグコピー
        for (int i = 0; i < cardData.tagList.Count; i++)
        {
            tags.Add(cardData.tagList[i]);
        }
        serialNum = cardData.serialNum;
        tier = cardData.tier;

        usingStateNew = new List<(IState, int)>();
    }

    /// <summary>
    /// カードUI情報を再読み込みする
    /// カード効果を補正後の値にする
    /// </summary>
    public void RefreshCard()
    {
        if (executingFlag) return;//カード効果実行中は更新しない

        actualCost = cost;
        actualEffectTarget = effectTarget;
        actualAttributes=new List<AttributeDefine>(attributes);
        //補正後の値を初期化

        //使用している状態異常初期化
        usingStateNew = new List<(IState, int)>();

        //それぞれの効果について補整を適用する
        int valueAfterEffect;
        for (int i=0;i<effects.Count;i++)
        {
            valueAfterEffect = effects[i].effect.value;//本来の値を最初に使用

            //攻撃効果だったら
            //攻撃力変動による変化
            if (CardEffectDefine.GetCardEffectType(effects[i].effect.cardEffect) == CardEffectDefine.CardEffectType.Damage)
            {
                //ストラテジーパターン使用の状態異常計算
                //Playerの状態異常リストからジェネリック型で検索し、効果を実行させる
                SearchAndUsePlayerState<IStateCardDamage>(a => valueAfterEffect = a.AdjustCardDamage(this, valueAfterEffect));


                //0以下なら0に
                if(valueAfterEffect <= 0)valueAfterEffect = 0;
            }
            else if(effects[i].effect.cardEffect == CardEffectDefine.CardEffect.UseStateConstant||
                effects[i].effect.cardEffect == CardEffectDefine.CardEffect.UseStateEternal)
            {//状態効果を使用するなら
                if(effects[i].UseState is IStateConsumable)
                {//消費可能効果が設定されているなら
                    //Type t = effects[i].UseState.GetType();
                    SearchAndUsePlayerState <IStateConsumable> (a =>
                    {
                        if(a.GetType() == effects[i].UseState.GetType())
                        {//同じタイプの状態異常を見つけたら
                            //消費を記録する
                            a.RecordConsumeState(this, effects[i].actualEffectValue);
                        }
                    });
                }
            }
            else if(effects[i].effect.cardEffect == CardEffectDefine.CardEffect.UseAllPlay)
            {//すべての祈りを消費する効果
                SearchAndUsePlayerState<StatePlay>(a =>
                {
                    a.RecordConsumeState(this,a.value);
                });
            }
                //補正後の値を記録
                effects[i].actualEffectValue = valueAfterEffect;
        }

        //Cost変動(それぞれ、Costが1以上なら)

        //タグによる変動
        if (IsItemHasTag(CardTagDefine.CardTag.Cost0) && actualCost > 0)
        {//コスト0
            actualCost = 0;
        }
        if(IsItemHasTag(CardTagDefine.CardTag.Cost0IfPsychoUsed) && actualCost > 0)
        {//精神攻撃を使用していたらコスト0
            if(handManager is not null && handManager.GetDataPerTurn().IsCardOfAttributeUsed(AttributeDefine.Attribute.Psycho))
            {
                actualCost = 0;
            }
        }

        //状態異常によるCost変動
        SearchAndUsePlayerState<IStateCardCost>(a => actualCost = a.AdjustCardCost(this, actualCost));


        if (actualCost < 0)
        {//0以上にする
            actualCost = 0;
        }

        cardUI.RefreshCard(actualCost,effects, actualAttributes, actualEffectTarget,tags);
    }

    /// <summary>
    /// プレイヤーの所有する状態異常のリストからTに該当するものを検索し、
    /// デリゲートで指示したものを実行させる
    /// </summary>
    /// <typeparam name="T">検索する状態異常のタイプ</typeparam>
    /// <param name="useAction">a => a.UseA()のように、Tのメソッドを使用する指示</param>
    private void SearchAndUsePlayerState<T>(Action<T> useAction) where T : class, IState
    {
        if (itemManager is FieldManager field)
        {//バトル中なら
            field.SearchAndUsePlayerState<T>(useAction);
        }
    }

    /// <summary>
    /// このカードが与えられた属性を持っているかを検索
    /// </summary>
    /// <param name="attribute">検索する属性</param>
    /// <returns>属性を持っているならtrueを返す</returns>
    public override bool IsItemHasAttribute(AttributeDefine.Attribute attribute)
    {
        foreach(AttributeDefine at in actualAttributes)
        {
            if(at.attribute == attribute)
            {//見つかったら
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// このカードが与えられたカード効果を持っているかを検索
    /// </summary>
    /// <param name="cardEffect">検索する効果</param>
    /// <returns>効果を持っているならtrueを返す</returns>
    public bool IsCardHasEffect(CardEffectDefine.CardEffect cardEffect)
    {
        foreach(ActualEffect ae in effects)
        {
            if(ae.effect.cardEffect == cardEffect)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 与えられたカード効果をこのカードが持っているならその値を返す
    /// </summary>
    /// <param name="cardEffect">検索する効果</param>
    /// <returns>カード効果を持っているならその値。ないなら-1</returns>
    public int GetActualValueOfEffect(CardEffectDefine.CardEffect cardEffect)
    {
        foreach (ActualEffect ae in effects)
        {
            if (ae.effect.cardEffect == cardEffect)
            {
                return ae.actualEffectValue;
            }
        }

        return -1;
    }

    /// <summary>
    /// このカードが与えられたタグを持っているかを検索
    /// </summary>
    /// <param name="cardTag">検索するタグ</param>
    /// <returns>タグを持っているならtrueを返す</returns>
    public override bool IsItemHasTag(CardTagDefine.CardTag cardTag)
    {
        foreach (CardTagDefine tg in tags)
        {
            if (tg.cardTag == cardTag)
            {//見つかったら
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// カードに、(何らかの効果で)タグを追加する
    /// </summary>
    /// <param name="cardTag">追加するタグ</param>
    public void AddTag(CardTagDefine cardTag)
    {
        tags.Add(cardTag);
        RefreshCard();
    }

    /// <summary>
    /// 状態異常を使用することができるたび追加
    /// 減少する値を記録する
    /// Playerから参照
    /// </summary>
    /// <param name="state">Playerの状態異常</param>
    /// <param name="value">減少する値</param>
    public void AddUsingStateNew(IState state,int value)
    {
        foreach((IState,int) touple in usingStateNew)
        {
            if(StateCompare.IsSameState(touple.Item1,state))
            {//既に使用していることが記録されているIStateなら、処理をスキップ
                return;
            }
        }

        usingStateNew.Add((state, value));
    }

    /// <summary>
    /// このカードが実行中状態であることを更新する
    /// 実行中状態ならカードのリフレッシュが行われない
    /// </summary>
    /// <param name="b">trueなら実行中状態</param>
    public void SetExecuting(bool b)
    {
        executingFlag = b;
    }

    public CardDataSO GetBaseCardData()
    {
        return baseCardData;
    }
}
