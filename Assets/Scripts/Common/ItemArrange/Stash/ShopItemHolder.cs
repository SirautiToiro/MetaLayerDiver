using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 店画面で使用するIPhysicalItemHolder。IShopManagerから与えられたアイテムを、与えられた値段で販売する
/// ShopItemHolderにはドラッグで配置ができない。PhysicalItemArrangeManager.EndDragging()に記述
/// </summary>
public class ShopItemHolder : MonoBehaviour, IGridPhysicalItemHolder
{
    private IShopManager shopManager;
    private PhysicalItemArrangeManager physicalItemArrangeManager;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    private ShopPriceRate shopPriceRate;

    //天秤屋の実装に伴い追加
    private bool isMoveAllItem = false;//アイテムを全て移動させるかどうか。trueのときはスタック数に関わらず全て移動させる。falseのときは1スタックだけ移動させる

    /// <summary>
    /// PhysicalItemGridPosNumDataでの初期化。座標を最初から与える
    /// </summary>
    /// <param name="shopDataItems">初期化に使用するアイテムデータのリスト</param>
    /// <param name="physicalItemArrangeManager">アイテム配置マネージャー</param>
    /// <param name="shopManager">ショップマネージャー</param>
    /// <param name="instantiateManager">アイテム生成マネージャー</param>
    /// <param name="rate">ショップの価格レート</param>
    public void Init(List<PhysicalItemGridPosNumData> shopDataItems,PhysicalItemArrangeManager physicalItemArrangeManager,
        IShopManager shopManager,PhysicalItemInstantiateManager instantiateManager, ShopPriceRate rate, bool isMoveAllItem = false)
    {
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.shopManager = shopManager;
        this.shopPriceRate = rate;
        this.isMoveAllItem = isMoveAllItem;
        gridPhysicalItemArrangement.Init(shopDataItems, this,physicalItemArrangeManager,instantiateManager);
    }

    /// <summary>
    /// PhysicalItemDataSOでの初期化。座標を与えない
    /// </summary>
    /// <param name="itemDataList">初期化に使用するアイテムデータのリスト</param>
    /// <param name="physicalItemArrangeManager">アイテム配置マネージャー</param>
    /// <param name="shopManager">ショップマネージャー</param>
    /// <param name="instantiateManager">アイテム生成マネージャー</param>
    /// <param name="rate">ショップの価格レート</param>
    public void Init(List<PhysicalItemDataSO> itemDataList, PhysicalItemArrangeManager physicalItemArrangeManager,
        IShopManager shopManager, PhysicalItemInstantiateManager instantiateManager, ShopPriceRate rate,bool isMoveAllItem=false)
    {
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.shopManager = shopManager;
        this.shopPriceRate = rate;
        this.isMoveAllItem = isMoveAllItem;
        gridPhysicalItemArrangement.Init(new List<PhysicalItemGridPosNumData>(), this,physicalItemArrangeManager,instantiateManager);

        // アイテムデータをグリッドにセット(一つずつ高速移動を用いる)
        foreach (var itemData in itemDataList)
        {
            (int x, int y)? enptyZone = gridPhysicalItemArrangement.GetEmptyZone(itemData.SizeX, itemData.SizeY);
            if (enptyZone.HasValue)
            {
                //新しく生成
                PhysicalItemBase item = gridPhysicalItemArrangement.InstantiatePhysicalItem(
                    new PhysicalItemGridPosNumData(itemData, enptyZone.Value.x, enptyZone.Value.y, 1));

                //QuickMoveでセット
                physicalItemArrangeManager.QuickMove(item, null, this);
            }
        }
    }

    public void PullItem(int posX, int posY)
    {
        PhysicalItemBase b = null;

        if (isMoveAllItem)
        {
             b = gridPhysicalItemArrangement.RemoveItemFromList(posX, posY);
        }
        else
        {
            b = gridPhysicalItemArrangement.RemoveOneItemFromList(posX, posY);
        }
        //1スタックだけを取り出すようにする
        shopManager.ChangeDisplayMode(true);//アイテムを表示するモードに切り替える

        shopManager.OnBuied(b);//購入時メッセージ
    }

    public void PullItem(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(item);
        //全スタックを取り出すようにする
    }

    public SetItemResult PutItem(PhysicalItemBase item, IPhysicalItemZone itemZone)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item);

        return result;
    }

    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item, posX, posY);

        return result;
    }

    public void TestItemDump()
    {
        gridPhysicalItemArrangement.TestItemDump();
    }

    public (int x, int y)? GetItemPos(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.GetItemPos(item);
    }

    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        return gridPhysicalItemArrangement.GetEmptyZone(x, y);
    }

    public void SetData()
    {
        //記録しない
    }

    public bool TryBuyItem(PhysicalItemBase item,int num=1)
    {
        return shopManager.GetStashPanel().ReduceCoin(shopPriceRate.GetRate(item.tier.tier) * num);
    }

    public List<PhysicalItemBase> GetItems()
    {
        return gridPhysicalItemArrangement.GetItemInstances();
    }
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.SearchSameItems(item);
    }
}
