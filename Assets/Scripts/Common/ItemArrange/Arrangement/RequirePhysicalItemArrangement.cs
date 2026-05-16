using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequirePhysicalItemArrangement : MonoBehaviour
{
    //アイテムサイズ取得のためのプレハブ
    [SerializeField] private GameObject physicalItem1_1Prefab;
    //生成のため
    [SerializeField] private Transform physicalItemParentTransform;
    //仮想のアイテムの配置場所
    [SerializeField] private Transform virtualPhysicalItemParentTransform;

    //グリッドの基準となる外枠のオブジェクト。左上に座標中心を設定すること。
    [SerializeField] private Transform gridSquare;
    //アイテムを並べるためのマネージャー
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;

    //アイテムプレハブ生成のためのマネージャー
    [SerializeField] private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    //アイテム要求のスタック数表示のためのPrefab
    [SerializeField] private GameObject requireItemStackPrefab;

    [SerializeField] private Transform requireItemStackParentTransform;

    //グリッドアイテムの配置場所
    //******左上の位置を記録する*********
    [SerializeField] private GridCardZone gridCardZone;

    private IGridPhysicalItemHolder gridPhysicalItemHolder;

    //生成し、確保されているアイテムのリスト
    private List<PhysicalItemStackAndRequire> items;

    //ある一つのアイテム、に対応するためのアイテムデータ(システム上のワイルドカード)
    [SerializeField] private PhysicalItemDataSO anyoneItemData;

    //位置計算のための値
    private float gridWidth;
    private float gridHeight;
    private float gridOriginX;
    private float gridOriginY;
    private float gridCellWidth;
    private float gridCellHeight;
    //位置計算用(アイテム)
    private float itemWidth;
    private float itemHeight;

    /// <summary>
    /// PhysicalItemGridPosNumを継承し、要求される仮想のアイテムと
    /// 実際に配置される実態であるPhysicalItemStackAndRequire.Itemを両方持つクラス。
    /// Itemは要求されるスタック数を最大値として配置する
    /// </summary>
    private class PhysicalItemStackAndRequire : PhysicalItemGridPosNum
    {
        public PhysicalItemBase VirtualItem;//要求される架空のアイテム。スタック情報などを持つ。

        public int VirtualStack;//要求されるスタック

        public RequireItemStack RequireItemStack;//スタック数表示

        /// <summary>
        /// 最初は実体を配置しない。仮想のアイテムの情報のみ.
        /// </summary>
        /// <param name="virtualData">仮想のアイテム</param>
        /// <param name="x">配置されるX座標</param>
        /// <param name="y">配置されるY座標</param>
        /// <param name="arrangement">呼び出し元。生成メソッドを使用するために.</param>
        public PhysicalItemStackAndRequire(PhysicalItemGridPosNumData virtualData, int x, int y,RequirePhysicalItemArrangement arrangement) : base(null, x, y)
        {
            this.VirtualItem = arrangement.InstantiateVirtualPhysicalItem(virtualData);
            this.VirtualStack = virtualData.Stack;
            this.RequireItemStack = arrangement.InstantiateRequireItemStack(virtualData);
            RequireItemStack.SetVirtualStack(virtualData.Stack);
            RequireItemStack.SetRealStack(0);//最初は配置は0
        }
    }



    /// <summary>
    /// 要求するアイテムの情報をもとに初期化
    /// 最初はアイテムを配置しない
    /// </summary>
    /// <param name="requiredItems">要求するアイテム</param>
    /// <param name="applyGridPhysicalItemHolder">保持するHolder</param>
    /// <param name="pArrange">Plefabになっているなどで、紐づける必要があるなら、必要</param>
    /// <param name="instantiateManager">Plefabになっているなどで、紐づける必要があるなら、必要</param>
    public void Init(List<PhysicalItemGridPosNumData> requiredItems, IGridPhysicalItemHolder applyGridPhysicalItemHolder,
        PhysicalItemArrangeManager pArrange = null, PhysicalItemInstantiateManager instantiateManager = null)
    {
        this.gridPhysicalItemHolder = applyGridPhysicalItemHolder;

        //必要なら初期化
        if (pArrange is not null)
        {
            physicalItemArrangeManager = pArrange;
        }
        if (instantiateManager is not null)
        {
            physicalItemInstantiateManager = instantiateManager;
        }

        DeleteItems();

        SetParams();

        //グリッドの初期化
        gridCardZone.Init(applyGridPhysicalItemHolder);

        items = new List<PhysicalItemStackAndRequire>();

        //仮想のアイテムの初期化
        foreach (var itemData in requiredItems)
        {
            items.Add(new PhysicalItemStackAndRequire(itemData, itemData.X, itemData.Y, this));
        }
    }

    private void DeleteItems()
    {
        if (items != null && items.Count > 0)
        {//既にアイテムが配置されているなら削除
            foreach (var item in items)
            {
                //紐づいているアイテムを削除
                if(item.VirtualItem.gameObject is not null)
                {
                    Destroy(item.VirtualItem.gameObject);
                }
                
                if(item.Item is not null)
                {
                    if(item.Item.gameObject is not null)
                    {
                        Destroy(item.Item.gameObject);
                    }
                }
                if(item.RequireItemStack.gameObject is not null)
                {
                    Destroy(item.RequireItemStack.gameObject);
                }
            }
            items.Clear();
        }
    }

    /// <summary>
    /// グリッド上のマウスのある位置にアイテムを配置する。
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public SetItemResult AddAndSetItem(PhysicalItemBase item)
    {
        var result = MousePosToGridPos(Input.mousePosition, item.ItemSizeX, item.ItemSizeY);
        if (result is null)
        {//グリッド外なら
            return new SetItemResult(false);
        }

        //グリッド内なら、座標に対してアイテム配置
        return AddAndSetItem(item, result.Value.Item1, result.Value.Item2);
    }

    /// <summary>
    /// グリッド上の指定した位置には配置しない。
    /// これが確保しているすべてのアイテムと配置場所を見て、配置可能な場所に配置する。
    /// 配置するのはただ一か所のみ。配置する場所が複数ある場合は最初に見つけた場所に配置する。
    /// アイテムの交換は発生しない。要求されるスタック数を最大値として配置する
    /// 交換しない場合も成否がクラスに入る。
    /// 要求を満たす場所がない場合失敗する。
    /// </summary>
    /// <param name="item">配置するアイテム</param>
    /// <param name="posX">配置する場所X(使用しない)</param>
    /// <param name="posY">配置する場所Y(使用しない)</param>
    /// <returns>アイテム配置結果のクラス</returns>
    public SetItemResult AddAndSetItem(PhysicalItemBase item, int posX, int posY)
    {
        //配置したアイテムの位置をグリッドに設定
        Vector3? pos = GetGridWorldPos(posX, posY, item.ItemSizeX, item.ItemSizeY);
        //グリッド外に配置しようとしていたら失敗
        if (pos is null) return new SetItemResult(false);
        //以降このposは使用しない。

        int sizeX = item.ItemSizeX;
        int sizeY = item.ItemSizeY;

        Vector3? targetPos = null;
        PhysicalItemStackAndRequire targetStack = null;
        //すべての配置要求アイテム場所に対して
        //一か所のみ
        foreach (PhysicalItemStackAndRequire itemStack in items)
        {
            if (IsSameItem(itemStack.VirtualItem, item)) 
            {//アイテムが一致するものを見つけた
                //同じ場所に別のアイテムが存在するなら配置しない
                //上限まで入っているなら配置しない
                if(itemStack.Item is not null && (!IsSameItem(itemStack.Item, item) || itemStack.Item.Stack >= itemStack.VirtualStack))
                {
                    continue;
                }

                //空いている場所が見つかった
                //その場所に配置するため、座標を記録
                targetPos = itemStack.VirtualItem.transform.position;
                targetStack = itemStack;
                break;
            }
        }

        if (targetPos is null) 
        {//配置場所が見つからなかったので配置失敗
            return new SetItemResult(false);
        }

        //アイテムのスタック数は要求されるスタック数を最大値として配置する
        if (targetStack.Item is not null) 
        {//既に存在する
            if (item.Stack + targetStack.Item.Stack > targetStack.VirtualStack)
            {//スタック数が最大(要求される値)を超えるなら
                //残ったスタック数を計算
                int remainNum = item.Stack + targetStack.Item.Stack-targetStack.VirtualStack;

                //配置されているスタック数を最大にする
                targetStack.Item.SetStack(targetStack.VirtualStack);
                //残っているスタックをItemに設定
                item.SetStack(remainNum);

                //配置されているスタック数を表示
                targetStack.RequireItemStack.SetRealStack(targetStack.Item.Stack);

                //itemを、手持ちのアイテムとして残す.位置は、仮に配置先の場所(必ず再度配置失敗)
                return new SetItemResult(true, new PhysicalItemGridPosNum(item, posX, posY));
            }
            else
            {//最大を超えない
                //スタック数を増やす
                targetStack.Item.SetStack(targetStack.Item.Stack+item.Stack);

                //配置しようとしていたアイテムを削除
                Destroy(item.gameObject);

                //配置されているスタック数を表示
                targetStack.RequireItemStack.SetRealStack(targetStack.Item.Stack);

                //配置正常終了
                return new SetItemResult(true);
            }
        }
        else
        {//新規配置
            if (item.Stack > targetStack.VirtualStack)
            {//最大(要求される値)を超えるなら
                //アイテムを生成し、配置。
                //スタック数は要求の値。
                PhysicalItemBase newItem = InstantiateRealPhysicalItem(
                    new PhysicalItemGridPosNumData(item.BaseItemData, posX, posY, targetStack.VirtualStack),
                    targetStack.VirtualStack,
                    targetPos.Value);
                targetStack.Item = newItem;

                //カードゾーンを設定
                newItem.SetCardZone(gridCardZone);

                //残された手持ちのアイテムのスタック数を変更
                item.SetStack(item.Stack - targetStack.VirtualStack);

                //配置されているスタック数を表示
                targetStack.RequireItemStack.SetRealStack(targetStack.Item.Stack);

                //itemを、手持ちのアイテムとして残す.位置は、仮に配置先の場所(必ず再度配置失敗)
                return new SetItemResult(true, new PhysicalItemGridPosNum(item, posX, posY));
            }
            else
            {//最大を超えない
                targetStack.Item = item;
                //アイテムの位置を重ねるように決定
                item.gameObject.transform.position = targetPos.Value;
                //アイテムのカードゾーンを設定
                item.SetCardZone(gridCardZone);

                //配置されているスタック数を表示
                targetStack.RequireItemStack.SetRealStack(targetStack.Item.Stack);

                //配置正常終了.アイテムの交換も発生しない
                return new SetItemResult(true);
            }
        }
    }

    /// <summary>
    /// RequirePhysicalItemArrangementが持っているアイテムの一覧(データ)から
    /// 位置に該当するものを検索して削除
    /// </summary>
    /// <param name="posX">検索するX位置</param>
    /// <param name="posY">検索するY位置</param>
    /// <returns>削除したアイテムの実体</returns>
    public PhysicalItemBase RemoveItemFromList(int posX, int posY)
    {
        //アイテムの位置を検索
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].X == posX && items[i].Y == posY)
            {
                PhysicalItemBase physicalItemBase = items[i].Item;

                //見つかったので削除
                items[i].RequireItemStack.SetRealStack(0);//表示するスタック数を変更
                items[i].Item = null;
                
                return physicalItemBase;
            }
        }
        //見つからなかったので何もしない
        return null;
    }

    /// <summary>
    /// RequirePhysicalItemArrangementが持っているアイテムの一覧から
    /// 与えられたitemに該当するものを検索して削除
    /// なければスキップ
    /// </summary>
    /// <param name="item">検索して削除するitem</param>
    public void RemoveItemFromList(PhysicalItemBase item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (ReferenceEquals(item, items[i].Item))
            {
                items[i].RequireItemStack.SetRealStack(0);//表示するスタック数を変更
                items[i].Item = null;
                return;
            }
        }
    }

    /// <summary>
    /// 同じとみなせるアイテムであればTrue
    /// </summary>
    /// <param name="baseItem">比較元の、VirtualItem</param>
    /// <param name="itemIn">新しく配置する、比較するアイテム</param>
    /// <returns>同じとみなせるアイテムであればTrue</returns>
    public bool IsSameItem(PhysicalItemBase baseItem, PhysicalItemBase itemIn)
    {
        if (baseItem.SerialNum == anyoneItemData.SerialNum && baseItem.ItemType == anyoneItemData.PhysicalItemType.itemType)
        {//ワイルドカード
            return true;
        }
        else if (baseItem.SerialNum == itemIn.SerialNum && baseItem.ItemType == itemIn.ItemType)
        {//アイテムが一致
            return true;
        }
        else
        {//一致せず
            return false;
        }
    }

    /// <summary>
    /// 指定されたデータを元に、仮想の物理的アイテムをインスタンス化する。
    /// 仮想の物理アイテムは半透明になっており、触ることはできない。
    /// </summary>
    /// <param name="itemData">アイテムデータ</param>
    /// <returns>生成したアイテム</returns>
    public PhysicalItemBase InstantiateVirtualPhysicalItem(PhysicalItemGridPosNumData itemData)
    {
        Vector3? posWorld = GetGridWorldPos(itemData.X, itemData.Y, itemData.ItemData.SizeX, itemData.ItemData.SizeY);
        if (posWorld is null)
        {
            return null;
        }

        //生成
        PhysicalItemBase item = physicalItemInstantiateManager.InstantiatePhysicalItem(itemData.ItemData, posWorld.Value, virtualPhysicalItemParentTransform);

        //初期化//判定を消去
        item.Init(itemData.ItemData);

        //アイテム変形指示
        item.PutToHolder(gridPhysicalItemHolder);

        return item;
    }

    /// <summary>
    /// 実際に配置されているアイテムをインスタンス化する。
    /// 選択も有効化。
    /// </summary>
    /// <param name="itemData">アイテムデータ</param>
    /// <returns>生成したアイテム</returns>
    public PhysicalItemBase InstantiateRealPhysicalItem(PhysicalItemGridPosNumData itemData,int stack,Vector3 posWorld)
    {
        //生成
        PhysicalItemBase item = physicalItemInstantiateManager.InstantiatePhysicalItem(itemData.ItemData, posWorld, physicalItemParentTransform);

        //初期化
        item.Init(itemData.ItemData,gridCardZone,physicalItemArrangeManager,stack);

        //アイテム変形指示
        item.PutToHolder(gridPhysicalItemHolder);

        return item;
    }

    /// <summary>
    /// 指定されたデータをもとに、スタック数表示用のプレハブをインスタンス化する。
    /// XY座標をもとに、アイテムサイズを考慮しながら配置。
    /// </summary>
    /// <param name="itemData"></param>
    /// <returns></returns>
    public RequireItemStack InstantiateRequireItemStack(PhysicalItemGridPosNumData itemData)
    {
        //スタック表示用のプレハブは1*1.アイテムサイズが1*1でない場合、その分だけずれて表示。
        Vector3? posWorld = GetGridWorldPos(itemData.X+ itemData.ItemData.SizeX-1, itemData.Y + itemData.ItemData.SizeY - 1, 1,1);
        if (posWorld is null)
        {
            return null;
        }

        RequireItemStack stack = Instantiate(requireItemStackPrefab, posWorld.Value, Quaternion.identity,
            requireItemStackParentTransform).GetComponent<RequireItemStack>();

        return stack;
    }

    /// <summary>
    /// (x,y)の大きさのアイテムが配置可能な場所を返す
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        //グリッド上の空きスペースを探す
        for (int j = 0; j < gridPhysicalItemHolder.GridHeight - y + 1; j++)
        {//X方向に空きスペースを探す
            for (int i = 0; i < gridPhysicalItemHolder.GridWidth - x + 1; i++)
            {//Y方向に空きスペースを探す
                var result = CheckEmpty(i, j, x, y);
                if (result.AlternativeItem is null && result.IsSuccess == true)
                {//空きスペースを発見した(交換が発生せず、配置も可能)
                    return (i, j);
                }
            }
        }
        //空きスペースが見つからなかった
        return null;
    }

    /// <summary>
    /// 指定された位置(1*1)にアイテムを配置できるかチェックする。
    /// 既にアイテムがあるならそのアイテムを返す。
    /// RealItemに対して。
    /// </summary>
    /// <param name="x">配置を確認するX座標</param>
    /// <param name="y">配置を確認するY座標</param>
    /// <returns>SetItemResultは、配置可能な時IsSuccessがtrue,
    /// 入れ替えが発生するときAlternativeItemにそのアイテム
    /// 今回は、isSuccessは必ずtrue</returns>
    private SetItemResult CheckEmpty(int x, int y)
    {
        foreach (var item in items)
        {//全ての配置されているアイテムについて

            if (item.Item is null) continue;//アイテムが配置されていないならスキップ

            int sizeX = item.Item.ItemSizeX;
            int sizeY = item.Item.ItemSizeY;
            for (int i = 0; i < sizeX; i++)
            {//アイテムの配置されているすべてのマスに対して
                for (int j = 0; j < sizeY; j++)
                {
                    //接触を判定
                    if (item.X + i == x && item.Y + j == y)
                    {//配置したい位置にアイテムがある
                        //そのアイテムを返す
                        return new SetItemResult(true, item);
                    }
                }
            }
        }

        //重なりが発生しなかった
        //配置可能なので、成功を返す
        return new SetItemResult(true);
    }

    /// <summary>
    /// (x,y)の位置に(width,height)の大きさのアイテムを置くときに配置可能か、
    /// 交換が発生するか、を判定する
    /// RealItemに対して。
    /// </summary>
    /// <param name="x">配置を確認するX座標</param>
    /// <param name="y">配置を確認するY座標</param>
    /// <param name="width">配置したいアイテムの幅</param>
    /// <param name="height">配置したいアイテムの高さ</param>
    /// <returns>設置結果を意味するSetItemResult</returns>
    private SetItemResult CheckEmpty(int x, int y, int width, int height)
    {
        PhysicalItemGridPosNum alternativeItem = null;

        for (int i = 0; i < width; i++)
        {//アイテムを配置したい全てのマスに対して
            for (int j = 0; j < height; j++)
            {
                //配置したい位置をチェック
                //アイテムの交換が発生したなら、交換先を一時的に格納
                PhysicalItemGridPosNum ai2 = CheckEmpty(x + i, y + j).AlternativeItem;

                if (ai2 is not null)
                {
                    //アイテムが重なった
                    if (alternativeItem is null)
                    {//まだ代替アイテムが設定されていないなら
                        alternativeItem = ai2;
                    }
                    else
                    {//代替アイテムが既に設定されているなら
                        //アイテムのセットに失敗した
                        return new SetItemResult(false);
                    }
                }
            }
        }

        //配置可能
        return new SetItemResult(true, alternativeItem);
    }

    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧から
    /// その中のRealItemを検索し、
    /// その位置を返す
    /// 存在しないならnullを返す
    /// </summary>
    /// <param name="itemApply">検索するItem</param>
    /// <returns>存在しないならnull,存在するならその座標</returns>
    public (int x, int y)? GetItemPos(PhysicalItemBase itemApply)
    {
        foreach (var item in items)
        {
            if (Object.ReferenceEquals(itemApply, item.Item))
            {//一致するitemが見つかった
                //その座標を返す
                return (item.X, item.Y);
            }
        }

        //何も見つからなかったのでnullを返す
        return null;
    }

    /// <summary>
    /// Holderが持っているアイテムの一覧から、
    /// 与えられたitemと同じアイテムを検索し、
    /// その一覧を返す
    /// 存在しないならnull
    /// RealItemに対して。
    /// </summary>
    /// <param name="item">同じアイテムを検索するアイテム</param>
    /// <returns>検索されたアイテムのリスト</returns>
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        List<PhysicalItemBase> result = new List<PhysicalItemBase>();
        //アイテムのリストから同じアイテムを検索
        foreach (var itemData in items)
        {
            if (itemData.Item is null) continue;//アイテムが配置されていないならスキップ
            if (itemData.Item.ItemType == item.ItemType && itemData.Item.SerialNum == item.SerialNum)
            {//同じアイテムが見つかった
                result.Add(itemData.Item);
            }
        }
        if (result.Count > 0)
        {//同じアイテムが見つかった
            return result;
        }
        else
        {
            //見つからなかったので空リストを返す
            return result;
        }
    }

    /// <summary>
    /// Holderが持っているアイテムの一覧から、
    /// 与えられたitemと同じアイテムを検索し、
    /// その一覧をスタック最大値を付加して返す
    /// 存在しないならnull
    /// RealItemに対して。
    /// </summary>
    /// <param name="item">同じアイテムを検索するアイテム</param>
    /// <returns>検索されたアイテムのリストとそのスタック最大値</returns>
    public List<(PhysicalItemBase,int)> SearchSameItemsAndStackMax(PhysicalItemBase item)
    {
        List<(PhysicalItemBase,int)> result = new List<(PhysicalItemBase,int)>();
        //アイテムのリストから同じアイテムを検索
        foreach (var itemData in items)
        {
            if (itemData.Item is null) continue;//アイテムが配置されていないならスキップ
            if (itemData.Item.ItemType == item.ItemType && itemData.Item.SerialNum == item.SerialNum)
            {//同じアイテムが見つかった
                result.Add((itemData.Item,itemData.VirtualStack));
            }
        }
        if (result.Count > 0)
        {//同じアイテムが見つかった
            return result;
        }
        else
        {
            //見つからなかったのでnullを返す
            return null;
        }
    }

    #region 座標計算

    private void SetParams()
    {
        //グリッドの幅と高さを取得
        gridWidth = ((RectTransform)gridSquare).rect.width;
        gridHeight = ((RectTransform)gridSquare).rect.height;
        gridOriginX = gridSquare.localPosition.x;
        gridOriginY = gridSquare.localPosition.y;
        gridCellWidth = gridWidth / gridPhysicalItemHolder.GridWidth;
        gridCellHeight = gridHeight / gridPhysicalItemHolder.GridHeight;

        //アイテムの幅と高さを取得
        itemWidth = ((RectTransform)physicalItem1_1Prefab.transform).sizeDelta.x;
        itemHeight = ((RectTransform)physicalItem1_1Prefab.transform).sizeDelta.y;

    }

    /// <summary>
    /// マウスの位置をグリッド上の位置に変換する。
    /// アイテムの幅や高さが2以上の場合、その分補整する
    /// </summary>
    /// <param name="mousePos">マウススクリーン座標</param>
    /// <param name="width">マウスが動かしているアイテムの幅</param>
    /// <param name="height">マウスが動かしているアイテムの高さ</param>
    /// <returns>グリッド外ならnull</returns>
    private (int, int)? MousePosToGridPos(Vector2 mousePos, int width, int height)
    {
        // マウス位置をワールド座標に
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(mousePos);
        //マウス位置をグリッドのローカル座標に変換
        Vector3 mouseLocalPos = gridSquare.InverseTransformPoint(mouseWorldPoint);

        //マウス座標-アイテムサイズによりマウス座標が変化していることの補正
        int gridPosX = (int)Mathf.Floor((mouseLocalPos.x - gridCellWidth * (width - 1) / 2) / gridCellWidth);
        int gridPosY = (int)Mathf.Floor((-gridCellHeight * (height - 1) / 2 - 1 * mouseLocalPos.y) / gridCellHeight);

        if (gridPosX < 0 || gridPosX >= gridPhysicalItemHolder.GridWidth ||
           gridPosY < 0 || gridPosY >= gridPhysicalItemHolder.GridHeight)
        {//グリッド外なら
            return null;
        }
        else
        {//グリッド内ならその座標
            return (gridPosX, gridPosY);
        }
    }

    /// <summary>
    /// グリッド上の指定された位置に対応する
    /// アイテム原点のワールド座標を返す。
    /// </summary>
    /// <param name="x">グリッド上の位置X</param>
    /// <param name="y">グリッド上の位置Y</param>
    /// <param name="width">アイテムの幅</param>
    /// <param name="height">アイテムの高さ</param>
    /// <returns>位置(範囲外ならnull)</returns>
    private Vector3? GetGridWorldPos(int x, int y, int width, int height)
    {
        if (0 <= x && x + width - 1 < gridPhysicalItemHolder.GridWidth &&
            0 <= y && y + height - 1 < gridPhysicalItemHolder.GridHeight)
        {//アイテムの存在するすべてのマスについて範囲内なら
            //グリッド原点の座標
            Vector3 originPosLocal = new Vector3(gridOriginX, gridOriginY, 0f);
            //グリッド原点からの差分
            //アイテムの幅も影響する
            Vector3 deltaLocal = new Vector3(gridCellWidth * x + itemWidth * width / 2,
                -gridCellHeight * y - itemHeight * height / 2, 0f);

            //ローカル座標をワールド座標に変換
            //親はphysicalItemParentTransform
            Vector3 posWorld = physicalItemParentTransform.TransformPoint(originPosLocal + deltaLocal);

            return posWorld;
        }
        else
        {//範囲外なら
            return null;
        }
    }

    #endregion

    public List<PhysicalItemBase> GetItemsInstances()
    {
        List<PhysicalItemBase> result = new List<PhysicalItemBase>();

        foreach (var item in items)
        {
            if (item.Item is not null)
            {
                result.Add(item.Item);
            }
        }

        return result;
    }

    /// <summary>
    /// 配置可能な個数を返す
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int PuttableNum(PhysicalItemBase item)
    {
        Vector3? targetPos = null;
        PhysicalItemStackAndRequire targetStack = null;
        int remain = 0;
        //すべての配置要求アイテム場所に対して
        //一か所のみ
        foreach (PhysicalItemStackAndRequire itemStack in items)
        {
            if (IsSameItem(itemStack.VirtualItem, item))
            {//アイテムが一致するものを見つけた
                //同じ場所に別のアイテムが存在するなら配置しない
                //上限まで入っているなら配置しない
                if (itemStack.Item is not null && (!IsSameItem(itemStack.Item, item) || itemStack.Item.Stack >= itemStack.VirtualStack))
                {
                    continue;
                }
                //空いている場所が見つかった
                //その場所に配置するため、座標を記録
                targetPos = itemStack.VirtualItem.transform.position;
                targetStack = itemStack;
                remain = itemStack.VirtualStack - (itemStack.Item is not null ? itemStack.Item.Stack : 0);
                break;
            }
        }

        if (targetPos is null)
        {//配置場所が見つからなかったので配置しない
            return 0;
        }

        return remain;
    }

    /// <summary>
    /// 配置要求と実際の配置を両方更新する。
    /// </summary>
    public void RefreshStack()
    {
        foreach(var item in items)
        {
            if (item.Item is not null)
            {
                item.RequireItemStack.SetRealStack(item.Item.Stack);
            }
            else
            {
                item.RequireItemStack.SetRealStack(0);
            }

            item.RequireItemStack.SetVirtualStack(item.VirtualStack);
        }
    }

    public bool IsRequireCompleted()
    {
        foreach (var item in items)
        {
            if (item.Item is null) return false;

            if (item.VirtualStack != item.Item.Stack)
            {
                return false;
            }
        }
        return true;
    }

    public void DeleteRealItems()
    {
        foreach(var item in items)
        {
            if (item.Item is not null)
            {
                Destroy(item.Item.gameObject);
                item.RequireItemStack.SetRealStack(0);
                item.Item = null;
            }
        }
    }

    public void TestItemDump()
    {
        //テスト用にアイテムをダンプする
        foreach (var item in items)
        {
            if (item.Item is not null) {
                Debug.Log($"Item: {item.Item}, Position: {item.X}, {item.Y},Stack:{item.Item.Stack}\n" +
                    $"VirtualItem:{item.VirtualItem},VirtualStack{item.VirtualStack}");
            }
            else
            {
                Debug.Log($"Item: null, Position: {item.X}, {item.Y}\n" +
                    $"VirtualItem:{item.VirtualItem},VirtualStack{item.VirtualStack}");
            }
            
        }
    }

}