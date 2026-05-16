using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// カード、武器、アイテムの右クリックで出る詳細画面を表示する
/// </summary>
public class DescriptionPanel : MonoBehaviour
{
    [SerializeField] EffectTagToText effectTagToText;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform cardPlace;
    [SerializeField] private Transform weaponPlace;
    [SerializeField] private Transform stateDescriptionParent;
    [SerializeField] private Transform tierIconParent;
    [SerializeField] private Transform itemTypeParent;
    [SerializeField] private TextMeshProUGUI miniEffectText;
    [SerializeField] LayoutGroup miniEffectLayoutGroup;
    [SerializeField] ContentSizeFitter miniEffectContentSizeFitter;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] LayoutGroup nameLayoutGroup;
    [SerializeField] ContentSizeFitter nameContentSizeFitter;
    [SerializeField] private TextMeshProUGUI otherDescriptionText;//スタック数などの説明

    [SerializeField] private TextMeshProUGUI flavorText;//換金アイテム、ドロップ品のフレーバーテキスト
    [SerializeField] ContentSizeFitter flavorContentSizeFitter;

    private FieldManager fieldManager;
    private ItemArrangeManager itemArrangeManager;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject consumablePrefab;
    [SerializeField] private GameObject treasuresOrDropsPrefab;
    [SerializeField] private GameObject gearPrefab;
    [SerializeField] private GameObject stateDescriptionPrefab;
    [SerializeField] private GameObject tagDescriptionPrefab;
    [SerializeField] private GameObject tierIconPrefab;
    [SerializeField] private GameObject itemTypePrefab;

    [SerializeField] private float cardSize;

    //インスタンス化した後に格納
    private GameObject descriptedObj;//このパネルで生成したオブジェクト
    private GameObject[] stateDescriptionObjs;
    private TierIcon tierIcon;
    private ItemTypeIcon itemTypeIcon;

    // このインスタンスが何によって呼び出されたか
    private Caller instanceCaller;

    //一度に表示できる詳細の上限は3
    private const int descriptionMax = 3;

    /// <summary>
    /// このインスタンスが何によって呼び出されたか
    /// </summary>
    private enum Caller
    {
        FieldManager,
        ItemArrangeManager,
    }

    public void Update()
    {
        //エスケープキーが押された場合閉じる
        //右クリックで閉じる
        if (canvas.enabled == true&&(Input.GetKey(KeyCode.Escape)||Input.GetMouseButton(1)))
        {
            ClosePanel();
        }
    }

    /// <summary>
    /// 初期化
    /// バトル中に呼び出された時
    /// </summary>
    /// <param name="_fieldManager">fieldManagerから呼び出されている。それを取得</param>
    public void Init(FieldManager _fieldManager)
    {
        instanceCaller=Caller.FieldManager;//呼出元をFieldManagerに設定

        fieldManager = _fieldManager;
        canvas.enabled = false;
        stateDescriptionObjs = new GameObject[3];//3個までしか説明は表示しない

        tierIcon = InstantiateTierIcon();
        itemTypeIcon = InstantiateItemType();

        //武器の効果説明オフ
        miniEffectText.gameObject.SetActive(false);
        //武器の名前説明表示オフ
        nameText.gameObject.SetActive(false);
        //その他の説明テキストを非表示にする
        otherDescriptionText.gameObject.SetActive(false);
        flavorText.gameObject.SetActive(false);//フレーバーテキストも非表示にする
    }

    /// <summary>
    /// 初期化
    /// アイテム整理画面で呼び出された時
    /// </summary>
    /// <param name="itemArrangeManager">itemArrangeManagerから呼び出されている。それを取得。</param>
    public void Init(ItemArrangeManager _itemArrangeManager)
    {
        instanceCaller = Caller.ItemArrangeManager;//呼出元をItemArrangeManagerに設定

        itemArrangeManager = _itemArrangeManager;
        canvas.enabled = false;
        stateDescriptionObjs = new GameObject[3];//3個までしか説明は表示しない

        tierIcon = InstantiateTierIcon();
        itemTypeIcon = InstantiateItemType();

        //武器の効果説明オフ
        miniEffectText.gameObject.SetActive(false);
        //武器の名前説明表示オフ
        nameText.gameObject.SetActive(false);
        //その他の説明テキストを非表示にする
        otherDescriptionText.gameObject.SetActive(false);
        flavorText.gameObject.SetActive(false);//フレーバーテキストも非表示にする
    }

    /// <summary>
    /// パネルを表示
    /// カードの情報を表示する
    /// </summary>
    /// <param name="serialNum">表示するカードのシリアル番号</param>
    public void OpenPanel(int serialNum)
    {
        canvas.enabled = true;

        Card card = MakeCard(serialNum);

        //タグと状態異常の説明
        ShowStateDescripiton(card.tags, card.effects);

        //レア度の表示
        tierIcon.SetSprite(card.tier.tier);

        //タイプ設定(カードである)
        itemTypeIcon.SetSprite(card);
    }

    public void OpenPanel(PhysicalItemDataSO pItemData)
    {
        //パネル可視化
        canvas.enabled = true;

        //タイプ設定
        itemTypeIcon.SetSprite(pItemData.PhysicalItemType.itemType);

        switch (pItemData.PhysicalItemType.itemType)
        {

            case PhysicalItemTypeDefine.PhysicalItemType.Weapon:
                WeaponDataSO weaponData = pItemData.WeaponData;

                //武器の場合は、武器の詳細を表示
                Weapon weapon = MakeWeapon(pItemData);

                //タグと状態異常の説明
                ShowStateDescripiton(weapon.tags, weapon.effects);

                //レア度の表示
                tierIcon.SetSprite(weapon.tier.tier);

                //武器の効果説明
                miniEffectText.gameObject.SetActive(true);
                miniEffectText.text = effectTagToText.ConvertToText(weapon.effects, weapon.tags);
                //レイアウトグループが更新されないので手動更新
                miniEffectLayoutGroup.CalculateLayoutInputHorizontal();
                miniEffectLayoutGroup.CalculateLayoutInputVertical();
                miniEffectLayoutGroup.SetLayoutHorizontal();
                miniEffectLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                miniEffectContentSizeFitter.SetLayoutHorizontal();
                miniEffectContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(miniEffectContentSizeFitter.GetComponent<RectTransform>());

                //武器の名前説明表示
                nameText.gameObject.SetActive(true);
                //名前を書く
                nameText.text = pItemData.ItemName;
                //レイアウトグループが更新されないので手動更新
                nameLayoutGroup.CalculateLayoutInputHorizontal();
                nameLayoutGroup.CalculateLayoutInputVertical();
                nameLayoutGroup.SetLayoutHorizontal();
                nameLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                nameContentSizeFitter.SetLayoutHorizontal();
                nameContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(nameContentSizeFitter.GetComponent<RectTransform>());

                //その他の説明テキストを表示
                otherDescriptionText.gameObject.SetActive(true);
                string otherText = "";
                otherText += $"最大スタック：{pItemData.StackMax}\n";

                int attributeLength = weaponData.attributeList.Count;
                int count = 1;

                foreach (AttributeDefine attributeDefine in weaponData.attributeList)
                {
                    otherText += AttributeDefine.Dic_AttributeName[attributeDefine.attribute];

                    //最後のみ・を入れない
                    if (count < attributeLength)
                    {
                        otherText += "・";
                    }
                    count++;
                }

                otherText += "/";
                otherText += TargetDefine.Dic_EffectTarget[weaponData.targetDefine.effectTarget];

                otherDescriptionText.text = otherText;

                break;
            case PhysicalItemTypeDefine.PhysicalItemType.Consumables:
                ConsumablesDataSO consumableData = pItemData.ConsumablesData;

                Consumable consumable = MakeConsumable(pItemData);

                //レア度の表示
                tierIcon.SetSprite(consumable.tier.tier);

                //タグと状態異常の説明
                ShowStateDescripiton(new List<CardTagDefine>(), consumable.Effects);

                //消耗品の効果説明
                miniEffectText.gameObject.SetActive(true);
                //タグは存在しないので空のリストを渡す
                miniEffectText.text = effectTagToText.ConvertToText(consumable.Effects, new List<CardTagDefine>());
                //レイアウトグループが更新されないので手動更新
                miniEffectLayoutGroup.CalculateLayoutInputHorizontal();
                miniEffectLayoutGroup.CalculateLayoutInputVertical();
                miniEffectLayoutGroup.SetLayoutHorizontal();
                miniEffectLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                miniEffectContentSizeFitter.SetLayoutHorizontal();
                miniEffectContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(miniEffectContentSizeFitter.GetComponent<RectTransform>());

                //消耗品の名前説明表示
                nameText.gameObject.SetActive(true);
                //名前を書く
                nameText.text = pItemData.ItemName;
                //レイアウトグループが更新されないので手動更新
                nameLayoutGroup.CalculateLayoutInputHorizontal();
                nameLayoutGroup.CalculateLayoutInputVertical();
                nameLayoutGroup.SetLayoutHorizontal();
                nameLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                nameContentSizeFitter.SetLayoutHorizontal();
                nameContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(nameContentSizeFitter.GetComponent<RectTransform>());

                //その他の説明テキストを表示
                otherDescriptionText.gameObject.SetActive(true);
                string otherTextC = "";
                otherTextC += $"最大スタック：{pItemData.StackMax}\n";

                if (consumableData.UsableOnMap)
                {
                    otherTextC += "いつでも";
                }
                else
                {
                    otherTextC += "戦闘中";
                }
                otherTextC += "/";
                otherTextC += TargetDefine.Dic_EffectTarget[consumableData.TargetDefine.effectTarget];

                otherDescriptionText.text = otherTextC;
                break;

            case PhysicalItemTypeDefine.PhysicalItemType.Treasures:
            case PhysicalItemTypeDefine.PhysicalItemType.Drops:
                //換金アイテムやドロップ品の場合
                TreasuresOrDropsDataSO treasuresOrDropsData = pItemData.TreasuresOrDropsData;
                TreasuresOrDrops treasuresOrDrops = MakeTreasuresOrDrops(pItemData);

                //レア度の表示
                tierIcon.SetSprite(treasuresOrDrops.tier.tier);

                //アイテムの名前説明表示
                nameText.gameObject.SetActive(true);
                //名前を書く
                nameText.text = pItemData.ItemName;
                //レイアウトグループが更新されないので手動更新
                nameLayoutGroup.CalculateLayoutInputHorizontal();
                nameLayoutGroup.CalculateLayoutInputVertical();
                nameLayoutGroup.SetLayoutHorizontal();
                nameLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                nameContentSizeFitter.SetLayoutHorizontal();
                nameContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(nameContentSizeFitter.GetComponent<RectTransform>());

                //フレーバーテキストを表示
                flavorText.gameObject.SetActive(true);
                flavorText.text = treasuresOrDropsData.FlavorText;
                flavorContentSizeFitter.SetLayoutHorizontal();
                flavorContentSizeFitter.SetLayoutVertical();
                LayoutRebuilder.ForceRebuildLayoutImmediate(flavorContentSizeFitter.GetComponent<RectTransform>());

                //その他の説明テキストを表示
                otherDescriptionText.gameObject.SetActive(true);
                string otherTextT = "";
                otherTextT += $"最大スタック：{pItemData.StackMax}\n";

                //TODO:売値表示

                otherDescriptionText.text = otherTextT;

                break;
            case PhysicalItemTypeDefine.PhysicalItemType.Gear:
                //装備品の場合
                GearDataSO gearData = pItemData.GearData;
                Gear gear = MakeGear(pItemData);

                //レア度の表示
                tierIcon.SetSprite(gear.tier.tier);

                //アイテムの名前説明表示
                nameText.gameObject.SetActive(true);
                //名前を書く
                nameText.text = pItemData.ItemName;
                //レイアウトグループが更新されないので手動更新
                nameLayoutGroup.CalculateLayoutInputHorizontal();
                nameLayoutGroup.CalculateLayoutInputVertical();
                nameLayoutGroup.SetLayoutHorizontal();
                nameLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                nameContentSizeFitter.SetLayoutHorizontal();
                nameContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(nameContentSizeFitter.GetComponent<RectTransform>());


                //効果説明
                miniEffectText.gameObject.SetActive(true);

                miniEffectText.text = gearData.description;
                //レイアウトグループが更新されないので手動更新
                miniEffectLayoutGroup.CalculateLayoutInputHorizontal();
                miniEffectLayoutGroup.CalculateLayoutInputVertical();
                miniEffectLayoutGroup.SetLayoutHorizontal();
                miniEffectLayoutGroup.SetLayoutVertical();
                //contentSizeFitterも更新
                //縦と横のサイズ計算
                miniEffectContentSizeFitter.SetLayoutHorizontal();
                miniEffectContentSizeFitter.SetLayoutVertical();
                //レイアウトを即時更新
                LayoutRebuilder.ForceRebuildLayoutImmediate(miniEffectContentSizeFitter.GetComponent<RectTransform>());

                //その他の説明テキストを表示
                otherDescriptionText.gameObject.SetActive(true);
                string otherTextG = "";
                otherTextG += $"最大スタック：{pItemData.StackMax}\n";

                //TODO:売値表示

                otherDescriptionText.text = otherTextG;


                break;
            default:
                Debug.LogError("Unknown Physical Item Type: " + pItemData.PhysicalItemType.itemType);
                return;
        }

    }

    /// <summary>
    /// タグと状態異常の説明を表示する
    /// </summary>
    /// <param name="tags">タグ</param>
    /// <param name="aEffects">状態異常</param>
    private void ShowStateDescripiton(List<CardTagDefine> tags, List<ActualEffect> aEffects)
    {
        int descriptionNum = 0;

        //タグの中の説明必要なものを説明
        foreach (CardTagDefine tag in tags)
        {
            if (descriptionNum > descriptionMax)
            {
                Debug.Log("ERROR:Too Much Description");
                return;
            }
#nullable enable
            string? str = CardTagDefine.Dic_TagDescription[tag.cardTag];
            //説明不要の場合nullを得る
            if (str != null)
            {
                stateDescriptionObjs[descriptionNum] = Instantiate(tagDescriptionPrefab, stateDescriptionParent.position, Quaternion.identity, stateDescriptionParent);
                TagDescription tagDescription = stateDescriptionObjs[descriptionNum].GetComponent<TagDescription>();
                tagDescription.Init(tag.cardTag, str);

                descriptionNum++;
            }
#nullable disable
        }

        //カード効果のうち、説明が必要なものの説明
        foreach (ActualEffect aEffect in aEffects)
        {
            if (descriptionNum > descriptionMax)
            {
                Debug.Log("ERROR:Too Much Description");
                return;
            }

            List<string> descriptionList = CardEffectDefine.GetOtherDescription(aEffect);
            if (descriptionList is null) continue;

            foreach (string str in descriptionList)
            {
                if (descriptionNum > descriptionMax)
                {
                    Debug.Log("ERROR:Too Much Description");
                    return;
                }

                //説明。タグと同じ表示を使う。
                stateDescriptionObjs[descriptionNum] = Instantiate(tagDescriptionPrefab, stateDescriptionParent.position, Quaternion.identity, stateDescriptionParent);
                TagDescription tagDescription = stateDescriptionObjs[descriptionNum].GetComponent<TagDescription>();
                tagDescription.Init(null,str);
                descriptionNum++;
            }
        }


        //状態異常の説明
        List<IState> usedStates = new List<IState>();

        foreach (ActualEffect aEffect in aEffects)
        {
            //カードの持つ効果ごとに見ていって、状態異常を使用しているなら
            //descriptionNumを一つ増やし、説明を生成する
            IState usedState = CardEffectDefine.GetUsedState(aEffect);
            
            if(usedState is null)
            {//状態異常を使用していないならスルー
                continue;
            }

            bool usedFlag = false;
            foreach (IState state in usedStates)
            {
                if (state.GetType() == usedState.GetType()&&
                    (usedState is not IStateHasAttribute||((IStateHasAttribute)state).stateAttribute == ((IStateHasAttribute)usedState).stateAttribute))
                {//すでに説明している状態異常ならスルー(属性を持つなら、その一致も判定)
                    usedFlag = true;
                    break;
                }
            }

            if(!usedFlag)
            {
                usedStates.Add(usedState);
            }
        }
        //状態異常の表示
        foreach(IState usedState in usedStates)
        {
            if (descriptionNum > descriptionMax)
            {
                Debug.Log("ERROR:Too Much Description");
                return;
            }

            if (usedState != null)
            {
                stateDescriptionObjs[descriptionNum] = Instantiate(stateDescriptionPrefab, stateDescriptionParent.position, Quaternion.identity, stateDescriptionParent);
                StateDescription stateDescription = stateDescriptionObjs[descriptionNum].GetComponent<StateDescription>();
                stateDescription.Init(usedState);

                descriptionNum++;
            }
        }

    }

    /// <summary>
    /// このパネルを閉じる
    /// </summary>
    public void ClosePanel()
    {
        canvas.enabled = false;

        //生成した効果説明とカードを削除
        GameObject.Destroy(descriptedObj);
        foreach (Transform n in stateDescriptionParent)
        {
            GameObject.Destroy(n.gameObject);
        }

        //武器の効果説明オフ
        miniEffectText.gameObject.SetActive(false);
        //武器の名前説明表示オフ
        nameText.gameObject.SetActive(false);
        //その他の説明テキストを非表示にする
        otherDescriptionText.gameObject.SetActive(false);
        flavorText.gameObject.SetActive(false);//フレーバーテキストも非表示にする

        //正しい呼出元に対して、処理終了を通知
        switch (instanceCaller)
        {
            case Caller.FieldManager:
                fieldManager.CloseCardDescription();
                break;
            case Caller.ItemArrangeManager:
                itemArrangeManager.CloseDescription();
                break;
        }
        
    }


    /// <summary>
    /// 表示するカードをインスタンス化し、
    /// そのCardを返す
    /// </summary>
    /// <param name="serialNum">インスタンス化するカードのシリアル番号</param>
    /// <returns>生成したカードに紐づいたCard</returns>
    private Card MakeCard(int serialNum)
    {
        descriptedObj = Instantiate(cardPrefab, cardPlace.position, Quaternion.identity,cardPlace);
        Card card = descriptedObj.GetComponent<Card>();
        card.Init(PlayerCardData.GetCardDataFromSerialNum(serialNum));//Card初期化,引数をSOのみにするとcardは動かない
        //サイズ変更
        Vector3 scale =new Vector3(cardSize, cardSize, cardSize);
        descriptedObj.transform.localScale = scale;

        return card;
    }

    /// <summary>
    /// 表示する武器をインスタンス化し、
    /// そのWeaponを返す
    /// </summary>
    /// <param name="pWeaponData">インスタンス化する武器のデータ</param>
    /// <returns>紐づいたWeapon</returns>
    private Weapon MakeWeapon(PhysicalItemDataSO pWeaponData)
    {
        //インスタンス化
        //親はweaponPlace
        descriptedObj = Instantiate(weaponPrefab, weaponPlace.position, Quaternion.identity, weaponPlace);
        Weapon weapon = descriptedObj.GetComponent<Weapon>();
        weapon.Init(pWeaponData);

        return weapon;
    }

    private Consumable MakeConsumable(PhysicalItemDataSO pConsumableData)
    {
        //インスタンス化
        //親はweaponPlace
        descriptedObj = Instantiate(consumablePrefab, weaponPlace.position, Quaternion.identity, weaponPlace);
        Consumable consumable = descriptedObj.GetComponent<Consumable>();
        consumable.Init(pConsumableData);
        return consumable;
    }

    private TreasuresOrDrops MakeTreasuresOrDrops(PhysicalItemDataSO pTreasuresOrDropsData)
    {
        //インスタンス化
        //親はweaponPlace
        descriptedObj = Instantiate(treasuresOrDropsPrefab, weaponPlace.position, Quaternion.identity, weaponPlace);
        TreasuresOrDrops treasuresOrDrops = descriptedObj.GetComponent<TreasuresOrDrops>();
        treasuresOrDrops.Init(pTreasuresOrDropsData);
        return treasuresOrDrops;
    }

    private Gear MakeGear(PhysicalItemDataSO pGearData)
    {
        //インスタンス化
        //親はweaponPlace
        descriptedObj = Instantiate(gearPrefab, weaponPlace.position, Quaternion.identity, weaponPlace);
        Gear gear = descriptedObj.GetComponent<Gear>();
        gear.Init(pGearData);
        return gear;
    }

    private TierIcon InstantiateTierIcon()
    {
        GameObject tierIconObj = Instantiate(tierIconPrefab, tierIconParent.position, Quaternion.identity, tierIconParent);
        return tierIconObj.GetComponent<TierIcon>();
    }

    private ItemTypeIcon InstantiateItemType()
    {
        GameObject obj = Instantiate(itemTypePrefab, itemTypeParent.position, Quaternion.identity, itemTypeParent);
        return obj.GetComponent<ItemTypeIcon>();
    }
}
