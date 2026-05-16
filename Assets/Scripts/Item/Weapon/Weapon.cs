using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;


/// <summary>
/// 武器のPrefabに付属する管理スクリプト
/// </summary>
public class Weapon : ItemBase
{
    [SerializeField] private WeaponUI weaponUI;
    [SerializeField] private WeaponQuickDescription weaponQuickDescriptionRight;
    [SerializeField] private WeaponQuickDescription weaponQuickDescriptionLeft;

    //武器データ
    private WeaponDataSO baseWeaponData;//元となるScriptableObject側の武器データ
    public WeaponDataSO BaseWeaponData { get { return baseWeaponData; } }//元となるScriptableObject側の武器データ
    
    //物理アイテムとしてのデータ
    private PhysicalItemDataSO basePlItemData;//物理アイテムとしてのデータ
    public PhysicalItemDataSO BasePlItemData { get { return basePlItemData; } }

    public List<ActualEffect> effects;    //武器効果のリスト.効果リストの先頭が武器の主要な能力で、その値がUIに表示される
    public List<AttributeDefine> attributes;  //武器属性のリスト
    public int cost { get; set; }//武器コスト
    public int actualCost;//補正後のカードコスト
    public TargetDefine effectTarget;//武器の効果対象
    public int serialNum;
    public List<CardTagDefine> tags;//武器の特殊タグのリスト
    private int pos;//バトル画面のどの位置に配置されているか

    //武器のステータス変動によって使用され、武器を使用すると消費されるPlayerの状態異常
    //(IState,int)のタプルになっていて、intには、使用後の減少量が入る
    //IStateはプレイヤーのものを参照する
    public List<(IState, int)> usingStateNew { get; set; }

    private ItemMover _itemMover = null;
    public override ItemMover ItemMover { get{ return _itemMover; } }

    private ShowDescriptionWhenRightClick _itemRightClick;
    public override IItemRightClick ItemRightClick { get { return _itemRightClick; } }

    private bool executingFlag;

    /// <summary>
    /// 初期化処理.バトル中
    /// </summary>
    /// <param name="pWeaponData">武器情報の元となる物理アイテムのSO</param>
    /// <param name="cardZone">配置されるCardZone</param>
    /// <param name="_itemManager">生成元のIItemManager</param>
    /// <param name="pos">配置されたWeaponCardZoneの位置</param>
    public void Init(PhysicalItemDataSO pWeaponData,ICardZone cardZone, IItemManager _itemManager, int _pos)
    {
        WeaponDataSO weaponData = pWeaponData.WeaponData;//物理アイテムから武器データを取得
        basePlItemData = pWeaponData;

        clickableFlag = true;

        itemManager = _itemManager;

        _itemMover = new ItemMover(this);
        _itemRightClick = new ShowDescriptionWhenRightClick();//右クリックで説明を表示する機能をセット

        SetCardZone(cardZone);//カードゾーンにセット

        //効果リストを移動
        //本来の値はまだ変動していないので初期状態
        foreach (var e in weaponData.effectList)
        {
            effects.Add(new ActualEffect(e.Effect, e.Effect.value,e.UseState));
        }

        //データをコピー
        for (int i = 0; i < weaponData.attributeList.Count; i++)
        {
            attributes.Add(weaponData.attributeList[i]);
        }
        cost = weaponData.cost;
        actualCost = cost;
        effectTarget = weaponData.targetDefine;
        serialNum = pWeaponData.SerialNum;
        baseWeaponData = weaponData;

        //タグコピー
        for (int i = 0; i < weaponData.tagList.Count; i++)
        {
            tags.Add(weaponData.tagList[i]);
        }
        pos = _pos;
        tier= pWeaponData.Tier;

        weaponUI.Init(pWeaponData, this);
        weaponQuickDescriptionRight.SetText(this);
        weaponQuickDescriptionLeft.SetText(this);

        usingStateNew = new List<(IState, int)>();

        RefreshWeapon();//補正後に更新

        executingFlag = false;
    }

    /// <summary>
    /// 初期化処理
    /// clickableFlagをfalseにすることでカードは一切動かないようになる
    /// </summary>
    /// <param name="pWeaponData">武器情報の元となるSO</param>
    public void Init(PhysicalItemDataSO pWeaponData)
    {
        WeaponDataSO weaponData = pWeaponData.WeaponData;//物理アイテムから武器データを取得
        basePlItemData = pWeaponData;

        clickableFlag = false;

        _itemMover = new ItemMover(this);//アイテムムーバーをセット
        _itemRightClick = new ShowDescriptionWhenRightClick();//右クリックで説明を表示する機能をセット

        //効果リストを移動
        //本来の値はまだ変動していないので初期状態
        foreach (var e in weaponData.effectList)
        {
            effects.Add(new ActualEffect(e.Effect, e.Effect.value, e.UseState));
        }

        attributes = weaponData.attributeList;
        cost = weaponData.cost;
        actualCost = cost;
        effectTarget = weaponData.targetDefine;
        serialNum = pWeaponData.SerialNum;
        baseWeaponData = weaponData;

        //タグコピー
        for (int i = 0; i < weaponData.tagList.Count; i++)
        {
            tags.Add(weaponData.tagList[i]);
        }
        pos = 0;
        tier = pWeaponData.Tier;

        usingStateNew = new List<(IState, int)>();

        weaponUI.Init(pWeaponData, this);

        //カーソルを重ねると出る説明も表示しない
        weaponQuickDescriptionLeft.DescriptionOff();
        weaponQuickDescriptionRight.DescriptionOff();

        executingFlag = true;//動かない
    }

    /// <summary>
    /// 武器UI情報を再読み込みする
    /// </summary>
    public void RefreshWeapon()
    {
        if (executingFlag) return;

        //それぞれの効果について補整を適用する
        actualCost = cost;

        //使用している状態異常初期化
        usingStateNew = new List<(IState, int)>();

        int valueAfterEffect;
        for (int i = 0; i < effects.Count; i++)
        {
            valueAfterEffect = effects[i].effect.value;

            //攻撃効果だったら
            //攻撃力変動による変化
            if (CardEffectDefine.GetCardEffectType(effects[i].effect.cardEffect) == CardEffectDefine.CardEffectType.Damage)
            {
                //ストラテジーパターン使用の状態異常計算
                //Playerの状態異常リストからジェネリック型で検索し、効果を実行させる
                SearchAndUsePlayerState<IStateWeaponDamage>(a => valueAfterEffect = a.AdjustWeaponDamage(this, valueAfterEffect));

                //0以下なら0に
                if (valueAfterEffect <= 0) valueAfterEffect = 0;
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
        //状態異常による変動
        SearchAndUsePlayerState<IStateWeaponCost>(a => actualCost = a.AdjustWeaponCost(this, actualCost));

        weaponUI.RefreshWeapon(actualCost,effects);
        //詳細説明のテキストを更新
        if (IsRightPos())
        {
            weaponQuickDescriptionLeft.SetText(this);
        }
        else
        {
            weaponQuickDescriptionRight.SetText(this);
        }
        
    }


    #region 武器の移動、選択

    /// <summary>
    /// 武器にマウスが重なった場合
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (clickableFlag)
        {//操作可能なら
            base.OnPointerEnter(eventData);//元の動作

            //加えて詳細説明を見せる
            if (IsRightPos())
            {
                weaponQuickDescriptionLeft.DescriptionOn();
            }
            else
            {
                weaponQuickDescriptionRight.DescriptionOn();
            }
        }
    }

    /// <summary>
    /// 武器からマウスが離れた場合
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (clickableFlag)
        {//操作可能なら

            base.OnPointerExit(eventData);//元の動作

            //詳細説明をオフ
            if (IsRightPos())
            {
                weaponQuickDescriptionLeft.DescriptionOff();
            }
            else
            {
                weaponQuickDescriptionRight.DescriptionOff();
            }
        }
    }
    #endregion

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
    /// この武器が与えられた属性を持っているかを検索
    /// </summary>
    /// <param name="attribute">検索する属性</param>
    /// <returns>属性を持っているならtrueを返す</returns>
    public override bool IsItemHasAttribute(AttributeDefine.Attribute attribute)
    {
        foreach (AttributeDefine at in attributes)
        {
            if (at.attribute == attribute)
            {//見つかったら
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// この武器が与えられたタグを持っているかを検索
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

    private bool IsRightPos()
    {
        if (pos % 2 == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 状態異常を使用することができるたび追加
    /// 減少する値を記録する
    /// Playerから参照
    /// </summary>
    /// <param name="state">Playerの状態異常</param>
    /// <param name="value">減少する値</param>
    public void AddUsingStateNew(IState state, int value)
    {
        foreach ((IState, int) touple in usingStateNew)
        {
            if (StateCompare.IsSameState(touple.Item1, state))
            {//既に使用していることが記録されているIStateなら、処理をスキップ
                return;
            }
        }

        usingStateNew.Add((state, value));
    }

    /// <summary>
    /// 武器に、(何らかの効果で)タグを追加する
    /// </summary>
    /// <param name="cardTag">追加するタグ</param>
    public void AddTag(CardTagDefine cardTag)
    {
        tags.Add(cardTag);
        RefreshWeapon();
    }

    /// <summary>
    /// 武器のタグを削除する
    /// </summary>
    /// <param name="cardTag">削除するタグ</param>
    public void RemoveTag(CardTagDefine cardTag)
    {
        for (int i = 0; i < tags.Count; i++)
        {
            if(tags[i].cardTag == cardTag.cardTag) { tags.RemoveAt(i); break; }
        }
    }

    /// <summary>
    /// この武器が実行中状態であることを更新する
    /// 実行中状態なら武器のリフレッシュが行われない
    /// </summary>
    /// <param name="b">trueなら実行中状態</param>
    public void SetExecuting(bool b)
    {
        executingFlag = b;
    }
}
