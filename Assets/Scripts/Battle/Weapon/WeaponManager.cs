using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponCardZone[] weaponCardZones;
    [SerializeField] private GameObject weaponPrefab;//Weaponのプレハブ
    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private CardEffectExecute cardEffectExecute;
    [SerializeField] private HandManager handManager;

    //プレイヤーが持っている武器
    //InitでweaponNum個入れられる
    private Weapon[] weapons;
    //プレイヤーが持っている武器の数
    private int weaponNum;

    /// <summary>
    /// 初期化処理。武器データの生成
    /// </summary>
    public void Init()
    {
        weaponNum = InventoryData.GetWeaponEquipMax();//武器の数を設定
        weapons = new Weapon[weaponNum];

        //物理アイテムとしての武器を取得し、その内部のWeaponDataSOを利用して武器を生成する
        List<PhysicalItemDataSO> physicalWeaponsList = InventoryData.GetEquippingPhysicalWeapons();

        //武器の生成
        for (int i = 0; i < weaponNum; i++)
        {
            weapons[i] = InstantiateWeapon(physicalWeaponsList[i], weaponCardZones[i],i);
        }
    }

    /// <summary>
    /// 武器のインスタンス化
    /// </summary>
    /// <param name="weaponData">インスタンス化する武器に紐づく武器データ</param>
    /// <param name="weaponCardZone">配置されるカードゾーン</param>
    /// <param name="pos">配置されるカードゾーンの場所</param>
    /// <returns>インスタンス化したWeapon</returns>
    private Weapon InstantiateWeapon(PhysicalItemDataSO weaponData,WeaponCardZone weaponCardZone,int pos)
    {
        //インスタンス化
        //親はWeaponCardZone
        GameObject weaponObj= Instantiate(weaponPrefab, weaponCardZone.gameObject.transform.position,Quaternion.identity, weaponCardZone.gameObject.transform);
        Weapon weapon=weaponObj.GetComponent<Weapon>();
        weapon.Init(weaponData, weaponCardZone, fieldManager,pos);

        weaponCardZone.Init(fieldManager,weapon,cardEffectExecute,handManager);

        return weapon;
    }

    /// <summary>
    /// 全ての武器オブジェクトを削除
    /// </summary>
    public void DeleteWeapons()
    {
        for (int i= weapons.Length - 1; i >= 0; i --)
        {
            GameObject.Destroy(weapons[i].gameObject);
        }
        weapons = new Weapon[weaponNum];
    }

    /// <summary>
    /// 武器全ての表示を更新する
    /// </summary>
    public void RefreshWeapon()
    {
        foreach (Weapon weapon in weapons)
        {
            weapon.RefreshWeapon();
        }
    }
}
