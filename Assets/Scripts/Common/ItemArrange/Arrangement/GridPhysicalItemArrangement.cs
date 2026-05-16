using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// グリッドに沿って物理的アイテムを配置するクラス
/// </summary>
public class GridPhysicalItemArrangement : MonoBehaviour
{
    //アイテムサイズ取得のためのプレハブ
    [SerializeField] private GameObject physicalItem1_1Prefab;
    //生成のため
    [SerializeField] private Transform physicalItemParentTransform;

    //グリッドの基準となる外枠のオブジェクト。左上に座標中心を設定すること。
    [SerializeField] private Transform gridSquare;
    //アイテムを並べるためのマネージャー
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;

    //アイテムプレハブ生成のためのマネージャー
    [SerializeField] private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    //グリッドアイテムの配置場所
    //******左上の位置を記録する*********
    [SerializeField] private GridCardZone gridCardZone;

    private IGridPhysicalItemHolder gridPhysicalItemHolder;

    //生成し、確保されているアイテムのリスト
    private List<PhysicalItemGridPosNum> items;

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
    /// アイテムの情報をもとに初期化
    /// </summary>
    /// <param name="backpackItems">格納するアイテム</param>
    /// <param name="applyGridPhysicalItemHolder">保持するHolder</param>
    /// <param name="pArrange">Plefabになっているなどで、紐づける必要があるなら、必要</param>
    /// <param name="instantiateManager">Plefabになっているなどで、紐づける必要があるなら、必要</param>
    public void Init(List<PhysicalItemGridPosNumData> backpackItems,IGridPhysicalItemHolder applyGridPhysicalItemHolder,
        PhysicalItemArrangeManager pArrange = null,PhysicalItemInstantiateManager instantiateManager = null)
    {
        gridPhysicalItemHolder = applyGridPhysicalItemHolder;

        //必要なら初期化
        if(pArrange is not null)
        {
            physicalItemArrangeManager = pArrange;
        }
        if(instantiateManager is not null)
        {
            physicalItemInstantiateManager = instantiateManager;
        }

        DeleteItems();

        SetParams();

        //グリッドの初期化
        gridCardZone.Init(applyGridPhysicalItemHolder);

        items = new List<PhysicalItemGridPosNum>();

        foreach (var item in backpackItems)
        {
            //アイテムをインスタンス化する
            PhysicalItemBase itemData = InstantiatePhysicalItem(item);

            //リストに追加
            items.Add(new PhysicalItemGridPosNum(itemData, item.X, item.Y));
        }   
    }

    /// <summary>
    /// アイテムを1つだけ配置する。DiscardItemHolderで使用。
    /// </summary>
    /// <param name="item">配置するアイテム</param>
    /// <param name="gridPhysicalItemHolder">呼び出すHolder</param>
    /// <param name="x">アイテムのx座標</param>
    /// <param name="y">アイテムのx座標</param>
    public void Init(PhysicalItemBase item, IGridPhysicalItemHolder gridPhysicalItemHolder,int x,int y)
    {
        this.gridPhysicalItemHolder = gridPhysicalItemHolder;

        DeleteItems();

        SetParams();

        //配置したアイテムの位置をグリッドに設定
        Vector3? pos = GetGridWorldPos(x, y, item.ItemSizeX, item.ItemSizeY);
        //グリッド外に配置しようとしていたら失敗
        if (pos is null) return;

        //グリッドの初期化
        gridCardZone.Init(gridPhysicalItemHolder);

        items = new List<PhysicalItemGridPosNum>();

        //リストに追加
        items.Add(new PhysicalItemGridPosNum(item, x, y));

        item.SetCardZone(gridCardZone);//カードゾーンを設定

        item.transform.position = pos.Value;//アイテムの位置をグリッドに沿って決定
    }

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

    private void DeleteItems()
    {
        if (items != null && items.Count > 0)
        {//既にアイテムが配置されているなら削除
            foreach (var item in items)
            {
                //アイテムを削除
                //Destroy(item.Item.gameObject);

                if (item.Item.gameObject is not null)
                {
                    Destroy(item.Item.gameObject);
                }
            }
            items.Clear();
        }
    }

    /// <summary>
    /// 指定されたデータを元に、物理的アイテムをインスタンス化する。
    /// </summary>
    /// <param name="itemData">アイテムデータ</param>
    /// <returns>生成したアイテム</returns>
    public PhysicalItemBase InstantiatePhysicalItem(PhysicalItemGridPosNumData itemData)
    {
        Vector3? posWorld = GetGridWorldPos(itemData.X, itemData.Y,itemData.ItemData.SizeX, itemData.ItemData.SizeY);
        if (posWorld is null)
        {
            return null;
        }

        //生成
        PhysicalItemBase item = physicalItemInstantiateManager.InstantiatePhysicalItem(itemData.ItemData,posWorld.Value, physicalItemParentTransform);

        //初期化
        item.Init(itemData.ItemData,gridCardZone,physicalItemArrangeManager,itemData.Stack);

        //アイテム変形指示
        item.PutToHolder(gridPhysicalItemHolder);

        return item;
    }

    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧(データ)から
    /// 位置に該当するものを検索して削除
    /// </summary>
    /// <param name="posX">検索するX位置</param>
    /// <param name="posY">検索するY位置</param>
    /// <returns>削除したアイテムの実体</returns>
    public PhysicalItemBase RemoveItemFromList(int posX,int posY)
    {
        //アイテムの位置を検索
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].X == posX && items[i].Y == posY)
            {
                PhysicalItemBase physicalItemBase = items[i].Item;

                //見つかったので削除
                items.RemoveAt(i);
                return physicalItemBase;
            }
        }
        //見つからなかったので何もしない
        return null;
    }

    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧から
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
                items.RemoveAt(i);
                return;
            }
        }
    }


    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧(データ)から
    /// 位置に該当するものを検索して1スタックのみを取り出す
    /// </summary>
    /// <param name="posX">検索するX位置</param>
    /// <param name="posY">検索するY位置</param>
    /// <returns>削除したアイテムの実体</returns>
    public PhysicalItemBase RemoveOneItemFromList(int posX, int posY)
    {
        //アイテムの位置を検索
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].X == posX && items[i].Y == posY)
            {
                PhysicalItemBase physicalItemBase = items[i].Item;

                if (physicalItemBase.Stack > 1)
                {//スタック数が1より多いなら
                    //元のアイテムを削除
                    items.RemoveAt(i);

                    //そのアイテムのコピーをその場に残す
                    PhysicalItemBase itemCopy = InstantiatePhysicalItem(new PhysicalItemGridPosNumData(physicalItemBase.BaseItemData, posX, posY, physicalItemBase.Stack - 1));
                    //リストに追加
                    items.Add(new PhysicalItemGridPosNum(itemCopy, posX, posY));

                    //移動するアイテムはスタック数1にする
                    physicalItemBase.SetStack(1);
                    //アイテムの実体は削除せず、返す
                    return physicalItemBase;
                }
                else
                {//スタック数が1なら、そのアイテムを取り出す
                    //見つかったので削除
                    items.RemoveAt(i);
                    return physicalItemBase;
                }
            }
        }
        return null;
    }

    /*
    //一つのみのクイック移動の実装は難しいため、放棄
    public void RemoveOneItemFromList(PhysicalItemBase item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (ReferenceEquals(item, items[i].Item))
            {
                PhysicalItemBase physicalItemBase = items[i].Item;

                if (physicalItemBase.Stack > 1)
                {//スタック数が1より多いなら
                    int beforeX = items[i].X;
                    int beforeY = items[i].Y;
                    //元のアイテムを削除
                    items.RemoveAt(i);

                    //そのアイテムのコピーをその場に残す
                    PhysicalItemBase itemCopy = InstantiatePhysicalItem(new PhysicalItemGridPosNumData(physicalItemBase.BaseItemData, beforeX, beforeY, physicalItemBase.Stack - 1));
                    //リストに追加
                    items.Add(new PhysicalItemGridPosNum(itemCopy, beforeX, beforeY));

                    //移動するアイテムはスタック数1にする
                    physicalItemBase.SetStack(1);
                    //アイテムの実体は削除せず、返す
                    return;
                }
                else
                {//スタック数が1なら、そのアイテムを取り出す
                    //見つかったので削除
                    items.RemoveAt(i);
                    return;
                }
            }
        }
    }
    */

    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧から
    /// その中のItemを検索し、
    /// その位置を返す
    /// 存在しないならnullを返す
    /// </summary>
    /// <param name="itemApply">検索するItem</param>
    /// <returns>存在しないならnull,存在するならその座標</returns>
    public (int x, int y)? GetItemPos(PhysicalItemBase itemApply)
    {
        foreach (var item in items)
        {
            if (Object.ReferenceEquals(itemApply,item.Item))
            {//一致するitemが見つかった
                //その座標を返す
                return (item.X, item.Y);
            }
        }

        //何も見つからなかったのでnullを返す
        return null;
    }

    /// <summary>
    /// グリッド上のマウスのある位置にアイテムを配置する。
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public SetItemResult AddAndSetItem(PhysicalItemBase item)
    {
        var result = MousePosToGridPos(Input.mousePosition,item.ItemSizeX,item.ItemSizeY);
        if(result is null)
        {//グリッド外なら
            return new SetItemResult(false);
        }

        //グリッド内なら、座標に対してアイテム配置
        return AddAndSetItem(item, result.Value.Item1, result.Value.Item2);
    }

    /// <summary>
    /// (x,y)の位置に、データから生成して配置する
    /// </summary>
    /// <param name="item">生成する元のデータ</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public SetItemResult AddAndSetItem(PhysicalItemDataSO item,int x,int y)
    {
        PhysicalItemBase instantiatedItem = InstantiatePhysicalItem(new PhysicalItemGridPosNumData(item, x, y,1));

        return AddAndSetItem(instantiatedItem, x, y);
    }

    /// <summary>
    /// グリッド上の指定した位置にアイテムを配置する。
    /// アイテムの交換が発生したなら、その交換結果を返す。
    /// 交換しない場合も成否がクラスに入る。
    /// </summary>
    /// <param name="item">配置するアイテム</param>
    /// <param name="posX">配置する場所X</param>
    /// <param name="posY">配置する場所Y</param>
    /// <returns>アイテム配置結果のクラス</returns>
    public SetItemResult AddAndSetItem(PhysicalItemBase item,int posX,int posY)
    {
        //配置したアイテムの位置をグリッドに設定
        Vector3? pos = GetGridWorldPos(posX, posY,item.ItemSizeX,item.ItemSizeY);
        //グリッド外に配置しようとしていたら失敗
        if(pos is null)return new SetItemResult(false);

        int sizeX = item.ItemSizeX;
        int sizeY = item.ItemSizeY;

        PhysicalItemGridPosNum alternativeItem = null;

        bool itemStackFlag = false;//アイテムのスタックが重なるどうさを行うかどうか

        for (int i = 0; i < sizeX; i++)
        {//アイテムを配置したい全てのマスに対して
            for (int j = 0; j < sizeY; j++)
            {
                if (itemStackFlag) continue;//スタックが重なる動作をするなら、それ以外の動作は無視

                //配置したい位置をチェック
                //アイテムの交換が発生したなら、交換先を一時的に格納
                PhysicalItemGridPosNum ai2 = CheckEmpty(posX + i, posY + j).AlternativeItem;

                if(ai2 is not null)
                {
                    if(ai2.Item.ItemType==item.ItemType&&ai2.Item.SerialNum == item.SerialNum)
                    {//配置しようとしているアイテムと同じなら
                        //スタックを重ねる動作
                        itemStackFlag = true;
                        alternativeItem = ai2;
                        continue;
                    }

                    //アイテムが重なった
                    if (alternativeItem is null)
                    {//まだ代替アイテムが設定されていないなら
                        alternativeItem = ai2;
                    }
                    else
                    {//代替アイテムが既に設定されているなら
                        if (ReferenceEquals(alternativeItem, ai2))
                        {//同じアイテムに再度重なったなら
                            //継続
                            continue;
                        }

                        //アイテムのセットに失敗した
                        return new SetItemResult(false);
                    }
                }
            }
        }

        if (itemStackFlag)
        {//アイテムを重ねる動作をするなら
            int remainNum = 0;

            if(alternativeItem.Item.Stack + item.Stack > alternativeItem.Item.StackMax)
            {//スタック数が最大を超えるなら
                //残ったスタック数を計算
                remainNum = alternativeItem.Item.Stack + item.Stack - alternativeItem.Item.StackMax;
                //スタック数を最大にする
                alternativeItem.Item.SetStack(alternativeItem.Item.StackMax);

                //残っているスタックをitemに設定
                item.SetStack(remainNum);

                //itemを、手持ちのアイテムとして残す.位置は、仮に配置先の場所(必ず再度配置失敗)
                return new SetItemResult(true, new PhysicalItemGridPosNum(item, posX, posY));
            }
            else
            {//最大を超えず、単に配置するなら
                //配置先のアイテムのスタックを増加
                alternativeItem.Item.SetStack(alternativeItem.Item.Stack + item.Stack);

                //配置しようとしていたアイテムを削除
                Destroy(item.gameObject);

                //配置正常終了
                return new SetItemResult(true);
            }
        }
        else
        {//通常のアイテム配置
            //交換したアイテムをリストから取り除く
            items.Remove(alternativeItem);

            //配置可能なので、アイテムを配置する
            items.Add(new PhysicalItemGridPosNum(item, posX, posY));

            //アイテムの位置をグリッドに沿って決定
            item.gameObject.transform.position = pos.Value;

            //アイテムのカードゾーンを設定
            item.SetCardZone(gridCardZone);

            //配置処理が終わり、交換したアイテムを返す
            return new SetItemResult(true, alternativeItem);
        }
    }

    /// <summary>
    /// 指定された位置(1*1)にアイテムを配置できるかチェックする。
    /// 既にアイテムがあるならそのアイテムを返す。
    /// </summary>
    /// <param name="x">配置を確認するX座標</param>
    /// <param name="y">配置を確認するY座標</param>
    /// <returns>SetItemResultは、配置可能な時IsSuccessがtrue,
    /// 入れ替えが発生するときAlternativeItemにそのアイテム
    /// 今回は、isSuccessは必ずtrue</returns>
    private SetItemResult CheckEmpty(int x,int y)
    {
        foreach (var item in items)
        {//全ての配置されているアイテムについて
            int sizeX = item.Item.ItemSizeX;
            int sizeY = item.Item.ItemSizeY;
            for(int i=0; i<sizeX; i++)
            {//アイテムの配置されているすべてのマスに対して
                for(int j = 0; j < sizeY; j++)
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
    /// </summary>
    /// <param name="x">配置を確認するX座標</param>
    /// <param name="y">配置を確認するY座標</param>
    /// <param name="width">配置したいアイテムの幅</param>
    /// <param name="height">配置したいアイテムの高さ</param>
    /// <returns>設置結果を意味するSetItemResult</returns>
    private SetItemResult CheckEmpty(int x,int y,int width,int height)
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
    /// グリッド上の指定された位置に対応する
    /// アイテム原点のワールド座標を返す。
    /// </summary>
    /// <param name="x">グリッド上の位置X</param>
    /// <param name="y">グリッド上の位置Y</param>
    /// <param name="width">アイテムの幅</param>
    /// <param name="height">アイテムの高さ</param>
    /// <returns>位置(範囲外ならnull)</returns>
    private Vector3? GetGridWorldPos(int x,int y,int width,int height)
    {
        if(0<=x&&x+width-1< gridPhysicalItemHolder.GridWidth&&
            0<=y&& y+height-1<gridPhysicalItemHolder.GridHeight)
        {//アイテムの存在するすべてのマスについて範囲内なら
            //グリッド原点の座標
            Vector3 originPosLocal = new Vector3(gridOriginX, gridOriginY, 0f);
            //グリッド原点からの差分
            //アイテムの幅も影響する
            Vector3 deltaLocal = new Vector3(gridCellWidth * x + itemWidth*width / 2,
                -gridCellHeight * y - itemHeight*height / 2, 0f);

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

    /// <summary>
    /// マウスの位置をグリッド上の位置に変換する。
    /// アイテムの幅や高さが2以上の場合、その分補整する
    /// </summary>
    /// <param name="mousePos">マウススクリーン座標</param>
    /// <param name="width">マウスが動かしているアイテムの幅</param>
    /// <param name="height">マウスが動かしているアイテムの高さ</param>
    /// <returns>グリッド外ならnull</returns>
    private (int,int)? MousePosToGridPos(Vector2 mousePos,int width,int height)
    {
        // マウス位置をワールド座標に
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(mousePos);
        //マウス位置をグリッドのローカル座標に変換
        Vector3 mouseLocalPos = gridSquare.InverseTransformPoint(mouseWorldPoint);

        //マウス座標-アイテムサイズによりマウス座標が変化していることの補正
        int gridPosX = (int)Mathf.Floor((mouseLocalPos.x - gridCellWidth * (width - 1)/2) / gridCellWidth);
        int gridPosY = (int)Mathf.Floor((-gridCellHeight * (height - 1)/2 - 1*mouseLocalPos.y) / gridCellHeight);

        if (gridPosX<0 || gridPosX >= gridPhysicalItemHolder.GridWidth ||
           gridPosY < 0 || gridPosY >= gridPhysicalItemHolder.GridHeight)
        {//グリッド外なら
            return null;
        }
        else
        {//グリッド内ならその座標
            return (gridPosX, gridPosY);
        }
    }

    public List<PhysicalItemGridPosNumData> GetItems()
    {
        List<PhysicalItemGridPosNumData> data = new List<PhysicalItemGridPosNumData>();
        foreach(var item in items)
        {
            //アイテムの位置とスタック数を格納
            data.Add(new PhysicalItemGridPosNumData(item.Item.GetItemData(), item.X, item.Y, item.Item.Stack));
        }
        return data;
    }

    public List<PhysicalItemBase> GetItemInstances()
    {
        List<PhysicalItemBase> itemInstances = new List<PhysicalItemBase>();

        if(items.Count == 0)
        {
            return itemInstances;
        }

        foreach (var item in items)
        {
            //アイテムのインスタンスを格納
            itemInstances.Add(item.Item);
        }

        return itemInstances;
    }

    /// <summary>
    /// Holderが持っているアイテムの一覧から、
    /// 与えられたitemと同じアイテムを検索し、
    /// その一覧を返す
    /// 存在しないならnull
    /// </summary>
    /// <param name="item">同じアイテムを検索するアイテム</param>
    /// <returns>検索されたアイテムのリスト</returns>
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        List<PhysicalItemBase> result = new List<PhysicalItemBase>();
        //アイテムのリストから同じアイテムを検索
        foreach (var itemData in items)
        {
            if (itemData.Item.ItemType == item.ItemType&&itemData.Item.SerialNum == item.SerialNum)
            {//同じアイテムが見つかった
                result.Add(itemData.Item);
            }
        }
        if(result.Count > 0)
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
                if (result.AlternativeItem is null&&result.IsSuccess == true)
                {//空きスペースを発見した(交換が発生せず、配置も可能)
                    return (i, j);
                }
            }
        }
        //空きスペースが見つからなかった
        return null;
    }

    public GridCardZone GetGridCardZone()
    {
        return gridCardZone;
    }

    public void TestItemDump()
    {
        //テスト用にアイテムをダンプする
        foreach (var item in items)
        {
            Debug.Log($"Item: {item.Item}, Position: ({item.X}, {item.Y},Stack:{item.Item.Stack})");
        }
    }
}
