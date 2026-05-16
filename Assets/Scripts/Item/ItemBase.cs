using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// ドラッグでき、マウスカーソルを重ねると拡大される可能性があり、
/// 右クリックで画面が表示され、IItemManagerによって操作される
/// アイテムの抽象クラス
/// </summary>
public abstract class ItemBase : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private ICardZone currentZone;

    public ICardZone CurrentZone { 
        get { return currentZone; }
    }

    /////クリック操作系////
    //クリック可能かどうかのフラグ
    //図鑑などの表示ではオフ
    public bool clickableFlag { get; set; }

    //このアイテムを呼び出しているItemManager
    public IItemManager itemManager { get; set; }

    private const float pointerEnteringSize=1.2f;//ポインターが重なっている時に大きさを大きくする値

    public TierDefine tier { get; set; }//レア度

    public abstract ItemMover ItemMover { get;}

    public abstract IItemRightClick ItemRightClick { get; }

    private void Update()
    {
        if (clickableFlag)
        {//ドラッグ中処理
            ItemMover.UpdateItemMover();
        }
    }

    /// <summary>
    /// アイテムにマウスが重なった場合
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (clickableFlag)
        {//操作可能なら
            //カード拡大
            Vector3 scale = this.gameObject.transform.localScale;
            scale.x = pointerEnteringSize;
            scale.y = pointerEnteringSize;
            this.gameObject.transform.localScale = scale;
        }

    }

    /// <summary>
    /// アイテムからマウスが離れた場合
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (clickableFlag)
        {//操作可能なら
         //カードのサイズを戻す
            Vector3 scale = this.gameObject.transform.localScale;
            scale.x = 1.0f;
            scale.y = 1.0f;
            this.gameObject.transform.localScale = scale;
        }
    }

    /// <summary>
    /// 手札,デッキ調整画面、インベントリなどの元の位置に戻る
    /// </summary>
    /// <returns>その動きのTween</returns>
    public Tween BackToBasePos()
    {
        //カードゾーンのGameObjectを取得し、
        //その位置に移動する.
        //SetLinkで、Card消滅時に中断
        return ((RectTransform)this.gameObject.transform).DOMove(currentZone.GetPosition(), BattleConstants.CardWeaponMoveTime).
            SetLink(this.gameObject);
    }

    /// <summary>
    /// このオブジェクトの移動を終了し、終了地点へと移動させる
    /// </summary>
    public void CompleteDOTween()
    {
        ((RectTransform)this.gameObject.transform).DOComplete();
    }

    /// <summary>
    /// アイテムがクリックされたら
    /// アイテムに対応する情報をfieldManager.ShowDescription(///);に送る
    /// あるいは、Shift+Clickのクイック移動などの記述
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        ItemMover.ItemOnClicked(eventData);
    }

    /// <summary>
    /// アイテムが属性を持っているかを調べる
    /// </summary>
    /// <param name="attribute">検索する属性</param>
    /// <returns>持っているならtrue</returns>
    public abstract bool IsItemHasAttribute(AttributeDefine.Attribute attribute);

    /// <summary>
    /// アイテムがタグを持っているかを調べる
    /// </summary>
    /// <param name="cardTag">検索するタグ</param>
    /// <returns>持っているならtrue</returns>
    public abstract bool IsItemHasTag(CardTagDefine.CardTag cardTag);

    /// <summary>
    /// アイテムが所属するカードゾーンを設定する。
    /// 同時に、そのカードゾーンが親になる
    /// </summary>
    /// <param name="zone">設定するカードゾーン</param>
    public void SetCardZone(ICardZone zone)
    {
        currentZone = zone;
        //親に設定
        this.gameObject.transform.SetParent(zone.GetTransform());
    }
}
