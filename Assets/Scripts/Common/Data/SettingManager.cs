using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各種設定を記録しているクラス
/// シングルトン
/// </summary>
public class SettingManager : SingletonMonoBehaviour<SettingManager>
{
    //////ユーザーから変更可能//////
    //コストが残っている時にその表示をするポップアップを表示するか
    public static bool IsShowCostRemainedPopup { get; set; }

    //アイテムやカードを捨てる時にポップアップを表示するか
    public static bool IsShowItemDiscardPopup { get; set; }

    public static bool IsShowCardDiscardPopup { get; set; }

    //バトル終了時のアイテムドロップ終了画面の終了にポップアップを表示するか
    public static bool IsShowEndBattlePopup { get; set; }

    //クエスト終了時の報酬画面のポップアップを表示するか
    public static bool IsShowQuestRewardPopup { get; set; }

    //////ENDユーザーから変更可能END//////

    //////ユーザーから変更不能//////
    
    public static bool IsCanEnterDungeon { get; set; } = true;//ダンジョンに入れるかどうか。これがfalseのときは封印されている


    //////ENDユーザーから変更不能END//////



    private void Awake()
    {
        //シングルトンの処理
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //シングルトン処理終了

        //TODO:セッティング画面作る

        var data = SaveDataManager.LoadSettingData();
        if (data != null)
        {
            IsShowCostRemainedPopup = data.IsShowCostRemainedPopup;
            IsShowItemDiscardPopup = data.IsShowItemDiscardPopup;
            IsShowCardDiscardPopup = data.IsShowCardDiscardPopup;
            IsShowEndBattlePopup = data.IsShowEndBattlePopup;
            IsShowQuestRewardPopup = data.IsShowQuestRewardPopup;
            IsCanEnterDungeon = data.IsCanEnterDungeon;
        }

        if (TestFlags.Instance.useTestDataFlag)
        {//デフォルト値での初期化
            IsShowCostRemainedPopup = true;
            IsShowItemDiscardPopup = true;
            IsShowCardDiscardPopup = true;
            IsShowEndBattlePopup = true;
            IsShowQuestRewardPopup = true;
            IsCanEnterDungeon = false;
        }
    }
}
