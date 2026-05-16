using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードの効果種類を定義
/// </summary>
[System.Serializable]
public class CardEffectDefine
{
    //パラメータ
    [Header("効果の種類")]
    public CardEffect cardEffect;
    [Header("効果値")]
    public int value;

    public CardEffectDefine(CardEffect effect, int value)
    {
        cardEffect = effect;
        this.value = value;
    }

    #region 効果種類の定義
    public enum CardEffect
    {
        /// <summary>
        /// 必ず上から更新すること
        /// </summary>
        Damage,             //ダメージ
        Block,              //ブロック付与

        SelfHeal,          //自分を回復する
        SelfDamage,        //自分にダメージを与える

        RandomXDamage3Times, //ランダムに3回、敵にダメージを与える

        GainStamina,        //元気を得る
        LoseStamina,        //元気を失う

        CauseState,         //状態異常を与える(状態異常の詳細はCardEffectClass.useState)
        GetStateBuff,       //良い状態異常を得る
        GetStateDebuff,     //悪い状態異常を得る(良い悪いは、演出に影響)
        UseStateConstant,   //判定。状態異常を消費する(Constant).消費する数を決められる
        UseStateEternal,    //判定。状態異常を消費する(Eternal).消費するのは必ずすべて

        DisCard,            //手札をすべて捨てる
        Draw,               //手札を引く
        FrameDraw,          //1枚引き、それが火炎属性のカードならCost0のタグを付与する

        GetShadowBoxingInHand,//手札に「シャドーボクシング」を加える

        IsShuffleNum,       //判定。シャッフルした回数が値以上なら
        Cost0DrawedCard,    //このカードで引いたカードのコストを0にする

        NextEffectIsSelf,   //全体攻撃カードのみで使用する。次のカード効果は自信に対して発生する。

        GetTakan,           //多感を得る

        GetBarricade,       //バリケード(ターン開始時ブロック)を得る

        IsLongRangeEnemy,   //判定。遠距離の敵なら
        UseEnergy,          //判定。エネルギー(AP)を消費する

        GetDragonSet1,      //竜化初級の効果(カードを手札に加える)
        GetDragonSet2,      //竜化中級の効果(カードを手札に加える)

        GetMysticKnifeInHandXPieces, //手札に「幻想ナイフ」を加える(X枚)

        PureDamage,        //ダメージ(防御無視)

        DrawRandomCreateCard, //生成属性のカードをランダムにX枚引く(存在しないなら引かない)

        IsShortRangeEnemy,  //判定。近距離の敵なら

        UseAllPlay,     //判定。祈りをすべて消費する。消費成功なら次に進む。
        GetWeaponCostDownAccordingToConsumedPlay, //祈りを消費した数に応じて、武器のコスト減少を獲得

        EnbuTurn,         //演舞・ターン。このターン中に使用した演舞カードの枚数ぶんAPを回復

        BlockToEnemy,        //敵にブロックを与える(敵の行動で使用)

        _MAX,               //最後
    }
    #endregion

    //効果タイプの定義
    public enum CardEffectType
    {
        Damage, //ダメージ
        Block,  //防御
        Debuff, //デバフ
        Buff,   //バフ
        Heal,   //回復
        Other,  //その他
        Error,  //エラー
        Avoided,//カード効果が回避された場合
    }

    /// <summary>
    /// Cardの効果タイプを取得する
    /// </summary>
    /// <param name="effect">カード効果</param>
    /// <returns>対応する効果タイプ</returns>
    public static CardEffectType GetCardEffectType(CardEffect effect)
    {
        switch (effect)
        {
            //ダメージ
            case CardEffect.Damage:
            case CardEffect.SelfDamage:
            case CardEffect.RandomXDamage3Times:
            case CardEffect.PureDamage:
                return CardEffectType.Damage;

                //ブロック
            case CardEffect.Block:
            case CardEffect.BlockToEnemy:
                return CardEffectType.Block;

                //デバフ
            case CardEffect.LoseStamina:
            case CardEffect.CauseState:
            case CardEffect.GetStateDebuff:
                return CardEffectType.Debuff;

                //バフ行為
            case CardEffect.GetTakan:
            case CardEffect.GetBarricade:
            case CardEffect.GetStateBuff:
            case CardEffect.GetWeaponCostDownAccordingToConsumedPlay:
                return CardEffectType.Buff;

            //回復
            case CardEffect.SelfHeal:
            case CardEffect.GainStamina:
                return CardEffectType.Heal;

            //その他(演出なし)
            case CardEffect.DisCard:
            case CardEffect.Draw:
            case CardEffect.FrameDraw:
            case CardEffect.GetShadowBoxingInHand:
            case CardEffect.IsShuffleNum:
            case CardEffect.Cost0DrawedCard:
            case CardEffect.NextEffectIsSelf:
            case CardEffect.UseStateConstant:
            case CardEffect.UseStateEternal:
            case CardEffect.IsLongRangeEnemy:
            case CardEffect.IsShortRangeEnemy:
            case CardEffect.UseEnergy:
            case CardEffect.GetDragonSet1:
            case CardEffect.GetDragonSet2:
            case CardEffect.GetMysticKnifeInHandXPieces:
            case CardEffect.DrawRandomCreateCard:
            case CardEffect.UseAllPlay:
            case CardEffect.EnbuTurn:
                return CardEffectType.Other;

            default : return CardEffectType.Error;
        }
    }

    /// <summary>
    /// それぞれのカード効果が使用する状態異常を返す
    /// 状態異常を使用しないならnull
    /// カードの詳細表記に使用
    /// TODO:全てのカードが正常に登録されているかをチェック
    /// </summary>
    /// <param name="effect">検索するカード効果</param>
    /// <returns>カード効果に対応する状態効果。ないならnull</returns>
    public static IState GetUsedState(ActualEffect effect)
    {
        switch (effect.effect.cardEffect)
        {
            //状態異常を使用するカード効果なら、それをそのまま返す
            case CardEffect.CauseState:
            case CardEffect.GetStateBuff:
            case CardEffect.GetStateDebuff:
            case CardEffect.UseStateConstant:
            case CardEffect.UseStateEternal:
                return effect.UseState;

            //個別に使用するもの
            case CardEffect.GetTakan:
                return new StateTakan();
            case CardEffect.GetBarricade:
                return new StateBlockOnTurnStart();
            case CardEffect.UseAllPlay:
                return new StatePlay();
            case CardEffect.GetWeaponCostDownAccordingToConsumedPlay:
                return new StateWeaponCostDown();

            default: return null;
        }
    }

    /// <summary>
    /// 複雑な説明が必要な場合に、それを返す。
    /// 複数の枠にまたがって説明する可能性もあるため、Listで返す
    /// </summary>
    /// <param name="effect">検索するカード効果</param>
    /// <returns>説明のリスト</returns>
    public static List<string> GetOtherDescription(ActualEffect effect)
    {
        switch (effect.effect.cardEffect)
        {
            case CardEffect.GetDragonSet1:
                return new List<string>()
                {
                    "竜鱗守護：ブロック5、火炎ダメージ上昇2を得る"
                };
            case CardEffect.GetDragonSet2:
                return new List<string>()
                {
                    "竜鱗守護：ブロック5、火炎ダメージ上昇2を得る",
                    "火炎爪：ダメージ5、火傷2を与える"
                };
            case CardEffect.GetMysticKnifeInHandXPieces:
                return new List<string>()
                {
                    "幻想ナイフ：一時的な攻撃カード。4ダメージ。" +
                    "念動属性のカードを使用したとき、コストが0になる"
                };
            case CardEffect.IsLongRangeEnemy:
            case CardEffect.IsShortRangeEnemy:
                return new List<string>()
                {
                    "距離：前の敵から近距離、中距離、遠距離と呼ぶ"
                };
            case CardEffect.UseEnergy:
            case CardEffect.EnbuTurn:
                return new List<string>()
                {
                    "AP：カードや武器を使用するために消費するコスト"
                };

            default: return null;
        }
    }

    #region 効果説明
    //カードで表示される効果名. string.Formatに入れて使用
    readonly public static Dictionary<CardEffect, string> Dic_EffectName = new Dictionary<CardEffect, string>()
    {
        {CardEffect.Damage,
            "ダメージ {0}"},

        {CardEffect.Block,
            "ブロック {0}"},

        //状態を使用するカードは引数が1つ多い
        {CardEffect.CauseState,
            "{0}の{1}を与える"},
        {CardEffect.GetStateBuff,
            "{0}の{1}を得る"},
        {CardEffect.GetStateDebuff,
            "{0}の{1}を得る"},
        {CardEffect.UseStateConstant,
            "{0}の{1}を消費："},
        {CardEffect.UseStateEternal,
            "{0}を消費："},

        {CardEffect.DisCard,
            "手札を全て捨てる"},
        {CardEffect.Draw,
            "カードを{0}枚引く"},

        {CardEffect.FrameDraw,
            "1枚引き、それが火炎属性ならコストを0にする" },

        {CardEffect.GetShadowBoxingInHand,
            "「シャドーボクシング」を手札に加える"
        },
        {CardEffect.IsShuffleNum,
            "シャッフルした回数が{0}回以上なら(現在{1})：" },
        {CardEffect.Cost0DrawedCard,
            "そのコストを0にする" },
        {CardEffect.NextEffectIsSelf,""},
        {CardEffect.GetTakan,
            "毎ターン最初に使用する精神カードのコストを0にする効果を得る"},
        {CardEffect.GetBarricade,
            "次のターン開始時にブロック{0}" },
        {CardEffect.SelfHeal,
            "自分を{0}回復する" },
        {CardEffect.SelfDamage,
            "自身に{0}ダメージ" },
        {CardEffect.RandomXDamage3Times,
            "ランダムな敵に{0}ダメージを与える。3回行う" },
        {CardEffect.GainStamina,"元気を{0}回復する" },
        {CardEffect.LoseStamina,"元気を{0}失う" },
        {CardEffect.IsLongRangeEnemy,
            "遠距離の敵が対象なら：" },
        {CardEffect.IsShortRangeEnemy,
            "近距離の敵が対象なら：" },
        {CardEffect.UseEnergy,"{0}のAPをさらに消費:" },
        {CardEffect.GetDragonSet1,"「竜鱗守護」を手札に加える"},
        {CardEffect.GetDragonSet2,"「竜鱗守護」、「火炎爪」を手札に加える"},
        {CardEffect.GetMysticKnifeInHandXPieces,"「幻想ナイフ」を手札に{0}枚加える"},
        {CardEffect.PureDamage,
            "防御を無視してダメージ {0}"},
        {CardEffect.DrawRandomCreateCard,"デッキからランダムに{0}枚の生成属性のカードを手札に加える" },
        {CardEffect.UseAllPlay,"祈りをすべて消費する：" },
        {CardEffect.GetWeaponCostDownAccordingToConsumedPlay,"消費した祈り分の武器コスト減少を得る" },
        {CardEffect.EnbuTurn,"このターン中に使用した演舞カードの枚数分、APを回復する" },
        {CardEffect.BlockToEnemy,"敵に{0}のブロックを与える" },
    };
    #endregion
}
