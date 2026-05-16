using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードの持つ特殊な種類を定義(帰還時に消滅する「シャドウ」など。)
/// </summary>
[System.Serializable]
public class CardTagDefine
{
    //パラメータ
    [Header("タグの種類")]
    public CardTag cardTag;

    #region タグの定義
    public enum CardTag
    {
        Shadow,     //帰還時に消滅する
        Once,       //一回使うとそのバトル中は廃棄される
        Enbu,       //カード名を「演舞」として扱う
        NoLimit,    //デッキ内に何枚でも入れられる
        EnemyAll,   //敵全体に効果を及ぼす//カードの最初に説明を表示したいのでタグは残す
        Cost0,      //手札でのコストが0になる(カード効果で追加される)
        Temporary,  //使用時、ターン終了時に破棄される
        Cost0IfPsychoUsed,//念動カードをこのターン中使用していた場合、コストが0になる
        SystemAnyoneCard,//システム用:任意のカードを配置できる(RequireCardManagerで使用)
    }
    #endregion

    #region タグ説明
    //カードで表示される効果名.
    readonly public static Dictionary<CardTag, string> Dic_TagName = new Dictionary<CardTag, string>()
    {
        {CardTag.Shadow,
            "シャドウ"},
        {CardTag.Once,
            "一回"},
        {CardTag.Enbu,
            "演舞"},
        {CardTag.NoLimit,
            "デッキに何枚でも入れられる"},
        {CardTag.EnemyAll,
            "敵全体に作用"},
        {CardTag.Cost0,
            "コスト0"},
        {CardTag.Temporary,"一時的" },
        {CardTag.Cost0IfPsychoUsed,"念動に反応してコスト0" },
        {CardTag.SystemAnyoneCard,"任意のカードを配置" },
    };

#nullable enable
    //タグの詳細説明の際に表示される文字.説明しないときはnull
    readonly public static Dictionary<CardTag, string?> Dic_TagDescription = new Dictionary<CardTag, string?>()
    {
        {CardTag.Shadow,
            "ダンジョンから脱出する際に消滅する使い切りのカード"},
        {CardTag.Once,
            "一度使用するとバトル中は廃棄される"},
        {CardTag.Enbu,
            "カード名を『演舞』として扱う"},
        {CardTag.NoLimit,
            null},
        {CardTag.EnemyAll,
            null},
        {CardTag.Cost0,
            null},
        {CardTag.Temporary,
            "使用時、ターン終了時に破棄される"},
        {CardTag.Cost0IfPsychoUsed,
            "このターン中に念動カードを使用していたなら、このカードのコストを0にする"},
    };
#nullable disable
    #endregion
}
