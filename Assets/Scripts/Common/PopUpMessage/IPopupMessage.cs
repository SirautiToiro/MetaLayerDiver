using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ポップアップメッセージ(一時的に出現して選択を要求するウィンドウ)
/// の挙動のインターフェース
/// メッセージのテキストと、OK,Cancelのボタンがある
/// </summary>
public interface IPopupMessageController
{
    //OKボタンが押された時の挙動
    public void OnOKButtonPressed();

    //キャンセルボタンが押された時の挙動
    public void OnCancelButtonPressed();
}

/// <summary>
/// ポップアップメッセージのうち、トグルボタンを持つものの挙動
/// </summary>
public interface IPopupMessageControllerWithToggle : IPopupMessageController
{
    //トグルがオンであるときに起動する動作
    public void OnToggleOn();
}

/// <summary>
/// コストが残っているカードや武器があるときに表示されるポップアップのコントローラー
/// </summary>
public class CostRemainedPopupController : IPopupMessageControllerWithToggle
{
    //FieldManagerから呼び出される時に取得
    private FieldManager fieldManager;
    private PopupMessageWithToggleUI popupUI;
    private InputBlocker inputBlocker;

    public CostRemainedPopupController(FieldManager _fieldManager, PopupMessageWithToggleUI _popupUI, InputBlocker _inputBlocker)
    {
        string MessageText = "まだ使えるカードや武器が\r\n残っています\r\nターンを終了しますか？";

        fieldManager = _fieldManager;
        popupUI = _popupUI;
        inputBlocker = _inputBlocker;

        //メッセージを設定
        popupUI.SetText(MessageText);
        popupUI.SetToggleState(false);//トグルの状態を設定
        //それ以外の動きをブロック
        inputBlocker.InputBlockingUp();
    }

    public void OnCancelButtonPressed()
    {
        if(popupUI.IsToggleOn())OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊
        //(自身が消えて、他は何もしない)
    }

    public void OnOKButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊
        fieldManager.TurnEnd();//ターンを正常に終了
    }

    public void OnToggleOn()
    {
        //表示しないにチェックが入った
        SettingManager.IsShowCostRemainedPopup = false;
    }
}

/// <summary>
/// カード整理画面でアイテムを捨てる時に表示するポップアップのコントローラー
/// </summary>
public class CardDiscardPopupController : IPopupMessageControllerWithToggle
{
    private CardArrangeManager cardArrangeManager;

    private PopupMessageWithToggleUI popupUI;
    private InputBlocker inputBlocker;

    public CardDiscardPopupController(CardArrangeManager cardArrangeManager,
        PopupMessageWithToggleUI popupUI, InputBlocker inputBlocker)
    {
        this.cardArrangeManager = cardArrangeManager;
        this.inputBlocker = inputBlocker;
        this.popupUI = popupUI;

        popupUI.SetText($"アイテムを捨てますか?");

        popupUI.SetToggleState(false);//トグルの状態を設定
        //それ以外の動きをブロック
        inputBlocker.InputBlockingUp();
    }

    public void OnToggleOn()
    {
        //表示しないにチェックが入った
        SettingManager.IsShowCardDiscardPopup = false;
    }

    public void OnOKButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊

        cardArrangeManager.DiscardCard();
    }

    public void OnCancelButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊
        //(自身が消えて、他は何もしない)
    }
}

/// <summary>
/// アイテム整理画面でアイテムを捨てる時に表示するポップアップのコントローラー
/// </summary>
public class ItemDiscardPopupController : IPopupMessageControllerWithToggle
{
    private PhysicalItemArrangeManager physicalItemArrangeManager;

    private PopupMessageWithToggleUI popupUI;
    private InputBlocker inputBlocker;

    public ItemDiscardPopupController(PhysicalItemArrangeManager physicalItemArrangeManager,
        PopupMessageWithToggleUI popupUI, InputBlocker inputBlocker)
    {
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.inputBlocker = inputBlocker;
        this.popupUI = popupUI;

        popupUI.SetText($"アイテムを捨てますか?");

        popupUI.SetToggleState(false);//トグルの状態を設定
        //それ以外の動きをブロック
        inputBlocker.InputBlockingUp();
    }

    public void OnToggleOn()
    {
        //表示しないにチェックが入った
        SettingManager.IsShowCardDiscardPopup = false;
    }

    public void OnOKButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊

        physicalItemArrangeManager.DiscardItem();
    }

    public void OnCancelButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊
        //(自身が消えて、他は何もしない)
    }
}

/// <summary>
/// バトル終了時の画面を閉じるときに警告がでるポップアップのコントローラー
/// </summary>
public class EndBattlePopupController : IPopupMessageControllerWithToggle
{
    private InventoryAndRewardUIPage inventoryAndRewardUIPage;

    private PopupMessageWithToggleUI popupUI;
    private InputBlocker inputBlocker;

    public EndBattlePopupController(InventoryAndRewardUIPage inventoryAndRewardUIPage,
        PopupMessageWithToggleUI popupUI, InputBlocker inputBlocker)
    {
        this.inventoryAndRewardUIPage = inventoryAndRewardUIPage;
        this.inputBlocker = inputBlocker;
        this.popupUI = popupUI;

        popupUI.SetText($"バトルを終了しますか?\n取得しなかったアイテムは消失します");

        popupUI.SetToggleState(false);//トグルの状態を設定
        //それ以外の動きをブロック
        inputBlocker.InputBlockingUp();
    }

    public void OnToggleOn()
    {
        //表示しないにチェックが入った
        SettingManager.IsShowEndBattlePopup = false;
    }

    public void OnOKButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊

        inventoryAndRewardUIPage.PopCheckFlag = true;//強制的にPopされるようにする
        inventoryAndRewardUIPage.PopUIPage();//ページをPopして、バトル終了
    }

    public void OnCancelButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊
        //(自身が消えて、他は何もしない)
    }
}

/// <summary>
/// クエスト完了時の画面を閉じるときに警告がでるポップアップのコントローラー
/// </summary>
public class EndQuestRewardPopupController : IPopupMessageControllerWithToggle
{
    private InventoryAndRewardUIPage inventoryAndRewardUIPage;

    private PopupMessageWithToggleUI popupUI;
    private InputBlocker inputBlocker;

    public EndQuestRewardPopupController(InventoryAndRewardUIPage inventoryAndRewardUIPage,
        PopupMessageWithToggleUI popupUI, InputBlocker inputBlocker)
    {
        this.inventoryAndRewardUIPage = inventoryAndRewardUIPage;
        this.inputBlocker = inputBlocker;
        this.popupUI = popupUI;

        popupUI.SetText($"クエスト報酬画面を閉じますか？\n取得しなかったアイテムは消失します");

        popupUI.SetToggleState(false);//トグルの状態を設定
        //それ以外の動きをブロック
        inputBlocker.InputBlockingUp();
    }

    public void OnToggleOn()
    {
        //表示しないにチェックが入った
        SettingManager.IsShowQuestRewardPopup = false;
    }

    public void OnOKButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊

        inventoryAndRewardUIPage.PopCheckFlag = true;//強制的にPopされるようにする
        inventoryAndRewardUIPage.PopUIPage();//ページをPopして、クエスト報酬画面を終了
    }

    public void OnCancelButtonPressed()
    {
        if (popupUI.IsToggleOn()) OnToggleOn();
        inputBlocker.InputBlockingDown();

        GameObject.Destroy(popupUI.gameObject);//UIを破壊
        //(自身が消えて、他は何もしない)
    }
}


public class InvalidCardPopupController : IPopupMessageController
{
    private InventoryPanel inventoryPanel;
    private PopupMessageUI popupUI;
    private InputBlocker inputBlocker;

    /// <summary>
    /// デッキやバックパックのカードが不適切である状態に表示するポップアップ
    /// </summary>
    /// <param name="inventoryPanel"></param>
    /// <param name="popupUI"></param>
    /// <param name="inputBlocker"></param>
    /// <param name="isDeck">trueならデッキの不具合。falseならバックパックのカード</param>
    public InvalidCardPopupController(InventoryPanel inventoryPanel,
        PopupMessageUI popupUI, InputBlocker inputBlocker,bool isDeck)
    {
        this.inventoryPanel = inventoryPanel;
        this.inputBlocker = inputBlocker;
        this.popupUI = popupUI;
        if (isDeck)
        {
            popupUI.SetText("デッキ内のカード枚数が不適切、あるいは\n編成に条件のあるカードが含まれています。");
        }
        else
        {
            popupUI.SetText("バックパックのカードが多すぎます。");
        }

        //それ以外の動きをブロック
        inputBlocker.InputBlockingUp();
    }
    public void OnOKButtonPressed()
    {
        inputBlocker.InputBlockingDown();
        GameObject.Destroy(popupUI.gameObject);//UIを破壊
    }
    public void OnCancelButtonPressed()
    {
        //キャンセルボタンは存在しないので何もしない
    }
}
