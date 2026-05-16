using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装備場所に物理アイテムを配置するクラス
/// </summary>
public class EquipPhysicalItemArrangement : MonoBehaviour
{
    //アイテムサイズ取得のためのプレハブ
    [SerializeField] private GameObject _physicalItem1_1Prefab;

    //アイテムを並べるためのマネージャー
    [SerializeField] private PhysicalItemArrangeManager _physicalItemArrangeManager;

    //プレハブ生成のためのマネージャー
    [SerializeField] private PhysicalItemInstantiateManager _physicalItemInstantiateManager;

    //装備アイテムはそれぞれの場所に対応したCardZoneの上に配置する
    //[4]の配列に記録する。0:{Weapon1,Weapon2,Weapon3,Weapon4}
    //1:{Gear1,Consumables1,Consumables2,Consumables3}
    [SerializeField] private EquipCardZone[] _equipCardZones0;
    [SerializeField] private EquipCardZone[] _equipCardZones1;

    private IEquipPhysicalItemHolder _equipPhysicalItemHolder;

    //生成し、確保されているアイテムのリスト.nullなどは格納しない。
    private List<PhysicalItemGridPosNum> _items;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="weapons">装備している武器</param>
    /// <param name="gear">装備ギア</param>
    /// <param name="consumables">消耗品</param>
    /// <param name="equipPhysicalItemHolder">Holder</param>
    public void Init(List<PhysicalItemDataSO> weapons, PhysicalItemDataSO gear, List<PhysicalItemDataSO> consumables,
        IEquipPhysicalItemHolder equipPhysicalItemHolder)
    {
        _equipPhysicalItemHolder = equipPhysicalItemHolder;

        if (_items != null && _items.Count > 0)
        {//既にアイテムが配置されているなら削除
            foreach (var item in _items)
            {
                //アイテムを削除
                Destroy(item.Item.gameObject);
            }
            _items.Clear();
        }

        for (int i = 0; i < _equipCardZones0.Length; i++)
        {
            _equipCardZones0[i].Init(equipPhysicalItemHolder, i, 0, PhysicalItemTypeDefine.PhysicalItemType.Weapon);
        }
        for (int i = 0; i < _equipCardZones1.Length; i++)
        {
            if(i == 0)
                _equipCardZones1[i].Init(equipPhysicalItemHolder, i, 1, PhysicalItemTypeDefine.PhysicalItemType.Gear);
            else
                _equipCardZones1[i].Init(equipPhysicalItemHolder, i, 1, PhysicalItemTypeDefine.PhysicalItemType.Consumables);  
        }

        //アイテムデータ取得
        _items = new List<PhysicalItemGridPosNum>();

        //_itemsのリストにアイテムを格納する。
        //アイテム情報と、座標を同時に確保する。
        //個数が少なくても、nullなどは格納しない。
        int count = 0;//配置位置を決めるためのカウンター
        for(int i = 0; i < 4; i++)
        {
            if (weapons.Count<=i||weapons[i] == null)
            {
                count++;
                continue;
            }
            
            var item = InstantiatePhysicalItem(weapons[i], count,0);
            _items.Add(new PhysicalItemGridPosNum(item, count, 0));
            count++;
        }
        if (gear != null)
        {
            var item = InstantiatePhysicalItem(gear, 0, 1);
            _items.Add(new PhysicalItemGridPosNum(item, 0, 1));
        }
        for (int i=0;i<3;i++)
        {
            if (consumables.Count <= i || consumables[i] == null)
            {
                count++;
                continue;
            }

            if (consumables[i] == null) continue;

            var item = InstantiatePhysicalItem(consumables[i], i+1, 1);
            _items.Add(new PhysicalItemGridPosNum(item, i+1, 1 ));
        }
    }

    /// <summary>
    /// 指定されたデータを元に、物理的アイテムをインスタンス化する。
    /// </summary>
    /// <param name="itemData">アイテムデータ</param>
    /// <param name="x">アイテム配置場所X</param>
    /// <param name="y">アイテム配置場所Y</param>
    /// <returns>生成したアイテム</returns>
    private PhysicalItemBase InstantiatePhysicalItem(PhysicalItemDataSO itemData,int x,int y)
    {
        EquipCardZone targetCardZone = null;
        if (y == 0)
        {
            targetCardZone = _equipCardZones0[x];
        }
        else if (y == 1)
        {
            targetCardZone = _equipCardZones1[x];
        }
        else
        {
            Debug.Log("Error: Invalid y value for EquipPhysicalItemArrangement: " + y);
            return null;
        }

        Transform transform = targetCardZone.GetTransform();

        //生成
        PhysicalItemBase item = _physicalItemInstantiateManager.InstantiatePhysicalItem(
            itemData, transform.position, transform
            );

        //初期化
        //装備のスタックは必ず1なので、1を指定する
        item.Init(itemData, targetCardZone, _physicalItemArrangeManager, 1);

        //サイズ変更指示
        item.PutToHolder(_equipPhysicalItemHolder);

        return item;
    }

    /// <summary>
    /// 与えられたCardZoneにアイテムを配置する。
    /// </summary>
    /// <param name="item">配置したアイテム</param>
    /// <param name="itemZone">配置するCardZone</param>
    /// <returns>配置結果</returns>
    public SetItemResult AddAndSetItem(PhysicalItemBase item, EquipCardZone itemZone)
    {
        return AddAndSetItem(item, itemZone.GetPos().x, itemZone.GetPos().y);
    }

    /// <summary>
    /// 装備上の指定した位置にアイテムを配置する。
    /// アイテムの交換が発生したなら、その交換結果を返す。
    /// 交換しない場合も成否がクラスに入る。
    /// </summary>
    /// <param name="item">配置するアイテム</param>
    /// <param name="posX">配置する場所X</param>
    /// <param name="posY">配置する場所Y</param>
    /// <returns>アイテム配置結果のクラス</returns>
    public SetItemResult AddAndSetItem(PhysicalItemBase item, int posX, int posY)
    {
        EquipCardZone targetCardZone = null;

        if(posY<= -1 || posY >= 2||posX<=-1||posX>=4)
        {
            //領域外にセットしようとしているので無効
            return new SetItemResult(false);
        }

        if (posY == 0)
        {
            targetCardZone = _equipCardZones0[posX];
        }
        else if (posY == 1)
        {
            targetCardZone = _equipCardZones1[posX];
        }

        //場所に配置できるアイテムかを確認
        if (targetCardZone.GetItemType() != item.ItemType)
        {//配置できない種類のアイテム
            //アイテムのセットに失敗した
            return new SetItemResult(false);
        }

        //既にアイテムがあるかを確認
        PhysicalItemGridPosNum alternativeItem = null;
        foreach (var it in _items)
        {
            if (it.X == posX && it.Y == posY)
            {//既にアイテムがある
                //アイテムを交換する
                alternativeItem = it;
                break;
            }
        }

        if (item.Stack >= 2)
        {//2個以上アイテムがスタックしているなら、1つのみを配置する
            if (alternativeItem is not null)
            {
                //さらに、元からアイテムがあるなら、配置失敗
                return new SetItemResult(false);
            }

            //アイテムのスタック数を1引いて手元に持つ
            item.SetStack(item.Stack-1);
            alternativeItem = new PhysicalItemGridPosNum(item, posX, posY);

            //新しくアイテムを生成して、配置
            item = InstantiatePhysicalItem(item.BaseItemData, posX, posY);
        }

        //データセット
        //交換したアイテムをリストから取り除く
        _items.Remove(alternativeItem);

        //配置可能なので、アイテムを配置する
        _items.Add(new PhysicalItemGridPosNum(item, posX, posY));

        //アイテムの位置を設定
        item.gameObject.transform.position = targetCardZone.GetPosition();

        //アイテムのカードゾーンを設定
        item.SetCardZone(targetCardZone);

        //配置処理が終わり、交換したアイテムを返す
        return new SetItemResult(true, alternativeItem);
    }

    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧から
    /// 位置に該当するものを検索して削除
    /// </summary>
    /// <param name="posX">検索するX位置</param>
    /// <param name="posY">検索するY位置</param>
    public void RemoveItemFromList(int posX, int posY)
    {
        //アイテムの位置を検索
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].X == posX && _items[i].Y == posY)
            {
                //見つかったので削除
                _items.RemoveAt(i);
                return;
            }
        }
        //見つからなかったので何もしない
    }

    /// <summary>
    /// GridPhysicalItemArrangementが持っているアイテムの一覧から
    /// 与えられたitemに該当するものを検索して削除
    /// なければスキップ
    /// </summary>
    /// <param name="item">検索して削除するitem</param>
    public void RemoveItemFromList(PhysicalItemBase item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (ReferenceEquals(item, _items[i].Item))
            {
                _items.RemoveAt(i);
                return;
            }
        }
    }

    public (List<PhysicalItemDataSO> weapons,PhysicalItemDataSO gear,List<PhysicalItemDataSO> consumables) GetEquips()
    {
        List<PhysicalItemDataSO> weapons = new List<PhysicalItemDataSO>() { 
        null, null, null, null
        };
        PhysicalItemDataSO gear = null;
        List<PhysicalItemDataSO> consumables = new List<PhysicalItemDataSO>() {
        null,null,null
        };
        //装備アイテムを取得
        foreach (var item in _items)
        {
            if(item.Y == 0)
            {//武器
                weapons[item.X] = item.Item.GetItemData();
            }else if(item.Y == 1)
            {//装備か消耗品
                if (item.X == 0)
                {//装備
                    gear = item.Item.GetItemData();
                }
                else
                {//消耗品
                    consumables[item.X - 1] = item.Item.GetItemData();
                }
            }
        }
        return (weapons, gear, consumables);
    }
}
