using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// バトル中の敵データの生成、管理
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyCardZone[] enemyCardZones;//敵配置場所の配列

    [SerializeField] private GameObject enemyPrefab;//敵のプレハブ

    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private CardEffectExecute cardEffectExecute;
    [SerializeField] private HandManager handManager;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private EnemySummonManager enemySummonManager;

    [SerializeField] private EnemyActionToActionIcon enemyActionToActionIcon;
    [SerializeField] private EnemyAnimation enemyAnimation;

    [SerializeField] private DataPerBattle dataPerBattle;

    //フィールド上に存在する敵
    //enemies[0]が先頭
    //必ずENEMYMAX個存在し、敵がいない場所にはnullが入っている
    private Enemy[] enemies;

    static int ENEMYMAX = 3;//敵の数は常にこの数(Null含む)

    //ForTest
    [SerializeField] private EnemyDataSO[] testEnemyData;//テスト用敵データ配列

    /// <summary>
    /// 初期化処理。敵データの生成
    /// </summary>
    /// <param name="encountType">どのような種類のバトルか</param>
    public void Init(EncountTypeDefine.EnecountType encountType)
    {
        //EnemyCardZoneをnullで初期化//すべてを初期化するため
        foreach (var zone in enemyCardZones) 
        { 
            zone.Init(fieldManager,null,cardEffectExecute,handManager);
        }

        enemies = new Enemy[ENEMYMAX];//enemies初期化

        //敵の生成。DungeonManagerからそれぞれのレアリティの敵の出現確率を取得して使用
        CreateEnemies(encountType);

        enemyAnimation.EnemyAppearAnimation(enemies);//enemyの出現演出を行う
    }

    #region 敵の生成、削除
    /// <summary>
    /// 敵の生成処理全体。
    /// 3体生成する
    /// </summary>
    /// <param name="encountType">どのような種類のバトルか</param>
    private void CreateEnemies(EncountTypeDefine.EnecountType encountType)
    {
        //ForTest
        //テスト用データがあるなら
        if (testEnemyData.Length > 0)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                //テストデータをインスタンス化
                enemies[i] = InstantiateEnemy(testEnemyData[i], enemyCardZones[i]);
            }
        }
        else
        {
            switch (encountType)
            {
                case EncountTypeDefine.EnecountType.Normal:
                    //ランダムスポーンの戦闘
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        //敵のTierは弱い
                        TierDefine.Tier enemyTier= DecideEnemyTier(false);

                        //レア度、ダンジョンに対応した敵の生成
                        EnemyDataSO createdEnemy = EnemyData.GetEnemyDataRandomOne(dungeonManager.GetDungeonType(), enemyTier);

                        //インスタンス化
                        enemies[i] = InstantiateEnemy(createdEnemy, enemyCardZones[i]);
                    }
                    break;
                case EncountTypeDefine.EnecountType.Treasure:
                    //ランダムスポーンの戦闘
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        //敵のTierは強い
                        TierDefine.Tier enemyTier = DecideEnemyTier(true);

                        //レア度、ダンジョンに対応した敵の生成
                        EnemyDataSO createdEnemy = EnemyData.GetEnemyDataRandomOne(dungeonManager.GetDungeonType(), enemyTier);

                        //インスタンス化
                        enemies[i] = InstantiateEnemy(createdEnemy, enemyCardZones[i]);
                    }
                    break;
                case EncountTypeDefine.EnecountType.Traveler:
                    EnemyData.TravelerType travelerType = EnemyData.GetRandomTravelerType();
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        //固定の敵との戦闘
                        //対応した敵の生成
                        EnemyDataSO createdEnemy = EnemyData.GetTravelerData(travelerType, i);

                        //インスタンス化
                        enemies[i] = InstantiateEnemy(createdEnemy, enemyCardZones[i]);
                    }
                    break;
                case EncountTypeDefine.EnecountType.Boss:
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        //固定の敵との戦闘
                        //対応した敵の生成
                        EnemyDataSO createdEnemy = EnemyData.GetBossData(dungeonManager.GetDungeonType(), dungeonManager.GetFloor(), i);

                        //インスタンス化
                        enemies[i] = InstantiateEnemy(createdEnemy, enemyCardZones[i]);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 敵の生成時に使用。
    /// 生成する敵のレア度を、
    /// DungeonManagerからそれぞれのレアリティの敵の出現確率を取得して使用
    /// </summary>
    /// <param name="isTreasure">宝箱のマスか</param>
    /// <returns>生成する敵のレア度</returns>
    private TierDefine.Tier DecideEnemyTier(bool isTreasure)
    {
        //スポーンレートの値の合計。
        //それぞれのレア度の値を合計で割った値が割合。
        int spawnRateSum = 0;
        //それぞれの位置にレアリティに対応したスポーンレート
        int spawnRateCommon = dungeonManager.GetSpawnRate(TierDefine.Tier.Common, isTreasure);
        spawnRateSum += spawnRateCommon;
        int spawnRateRare = dungeonManager.GetSpawnRate(TierDefine.Tier.Rare, isTreasure); ;
        spawnRateSum += spawnRateRare;
        int spawnRateMeta = dungeonManager.GetSpawnRate(TierDefine.Tier.Meta, isTreasure); ;
        spawnRateSum += spawnRateMeta;

        //0からspawnRateSumまでの範囲をspawnRate
        //で区切っていき、生成された乱数がその区切りのどこにあるかで
        //生成するタイルを判定する
        int ran = UnityEngine.Random.Range(0, spawnRateSum);

        ran -= spawnRateCommon;
        if (ran < 0)
        {
            return TierDefine.Tier.Common;
        }

        ran -= spawnRateRare;
        if (ran < 0)
        {
            return TierDefine.Tier.Rare;
        }
        return TierDefine.Tier.Meta;
    }

    /// <summary>
    /// バトル終了ウィンドウを閉じるときに、バトル中に生成したものを削除する
    /// enemyの削除
    /// </summary>
    public void DeleteEnemies()
    {
        //enemyをすべて削除
        for (int i = enemies.Length - 1; i >= 0; i--)
        {
            if(enemies[i] != null)
            {
                GameObject.Destroy(enemies[i].gameObject);
            }
        }

        enemies = new Enemy[ENEMYMAX];//enemies初期化
    }

    /// <summary>
    /// 敵のインスタンス化
    /// enemyDataがnullならnullを返す
    /// </summary>
    /// <param name="enemyData">インスタンス化する敵に紐づく敵データ</param>
    /// <param name="enemyCardZone">配置されるカードゾーン</param>
    /// <returns>インスタンス化したEnemy</returns>
    private Enemy InstantiateEnemy(EnemyDataSO enemyData, EnemyCardZone enemyCardZone)
    {
        if (enemyData is null) return null;

        //インスタンス化
        //親はEnemyCardZone
        GameObject enemyObj =Instantiate(enemyPrefab, enemyCardZone.gameObject.transform.position,Quaternion.identity, enemyCardZone.gameObject.transform);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemy.Init(enemyData, this, enemyActionToActionIcon, enemyCardZone);//初期化

        enemyCardZone.Init(fieldManager,enemy,cardEffectExecute,handManager);
        return enemy;
    }

    /// <summary>
    /// バトル中に敵が増える召喚
    /// 演出も行う
    /// </summary>
    /// <param name="sequence">召喚演出が乗るSequence</param>
    /// <param name="type">召喚される敵</param>
    /// <param name="policy">召喚方針</param>
    /// <returns>召喚した敵</returns>
    public Enemy SummonEnemy(Sequence sequence, EnemySummonManager.SummonedEnemyType type, EnemySummonManager.SummonPolicy policy)
    {
        bool flag = false;
        //敵は前に詰められている前提
        //nullがなければ終了
        foreach (Enemy e in enemies)
        {
            if (e is null)
            {
                flag = true;
                break;
            }
        }
        if (!flag) return null;

        if(policy == EnemySummonManager.SummonPolicy.Back)
        {//最後尾に召喚する
            int i = 0;
            for(i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] is null) break;
            }
            //iが召喚位置

            EnemyDataSO enemyData = enemySummonManager.GetSummonedEnemyData(type);

            //敵のインスタンス化
            Enemy newEnemy = InstantiateEnemy(enemyData, enemyCardZones[i]);

            enemies[i] = newEnemy;

            Enemy[] newEnemies = new Enemy[]
            {
                newEnemy
            };

            //初期行動を実行
            newEnemy.InitialEffectExecute();

            return newEnemy;
        }
        else if(policy == EnemySummonManager.SummonPolicy.Front)
        {//最前列に召喚する(pos:0)
            //前列のEnemyをそれぞれ動かす(pos:1とpos:0)
            for (int i = ENEMYMAX - 2; i >=0 ; i--)
            {
                if (enemies[i] != null)
                {//その場所Enemyがいるなら
                    enemyCardZones[i + 1].SetEnemy(enemies[i]);//後列のCardZoneにEnemyを設定
                    enemyCardZones[i].SetEnemy(null);//前の場所からEnemyを移動
                    enemies[i+1] = enemies[i];//enemies内のデータも移動
                    enemies[i] = null;
                }
            }

            EnemyDataSO enemyData = enemySummonManager.GetSummonedEnemyData(type);

            //敵のインスタンス化
            Enemy newEnemy = InstantiateEnemy(enemyData, enemyCardZones[0]);

            enemies[0] = newEnemy;

            Enemy[] newEnemies = new Enemy[]
            {
                newEnemy
            };

            //初期行動を実行
            newEnemy.InitialEffectExecute();

            return newEnemy;
        }
        else
        {//未定義
            return null;
        }
    }

    /// <summary>
    /// enemiesのposの位置にある敵を削除し、後方の敵は前に詰める
    /// </summary>
    /// <param name="pos">敵の位置</param>
    public void DestroyEnemy(int pos)
    {
        if (pos < 0 || pos >= ENEMYMAX||enemies[pos]==null)
        {//入力値チェック
            Debug.Log("pos error at DestroyEnemy");
            return;
        }

        //ドロップアイテム取得
        //posの位置にある敵が従属かをチェック
        bool isDependent = false;
        enemies[pos].SearchAndUseState<StateDependent>(a => isDependent = true);
        if (!isDependent)
        {//従属でない敵なら
            //敵を倒したことを記録
            dataPerBattle.SetDefeatedEnemy(enemies[pos].baseEnemyData);
        }
        

        //従属の敵があるかをチェックし、残りが従属のみならそれらを削除
        if (CheckEnemyDependent(pos))
        {//従属の敵のみなのですべて削除
            for(int i=enemies.Length-1; i>=0; i--)
            {
                if (enemies[i] == null) continue;
                //EnemyCardZoneからenemyを削除した後、Enemyそのものも消す
                enemyCardZones[i].SetEnemy(null);
                enemies[i].Destroy();

                enemies[i] = null;//配列から削除
            }
        }
        else
        {//普通の場合。全ての敵が従属ではない。
            //EnemyCardZoneからenemyを削除した後、Enemyそのものも消す
            enemyCardZones[pos].SetEnemy(null);
            enemies[pos].Destroy();

            enemies[pos] = null;//配列から削除

            //後列のEnemyをそれぞれ動かす
            for (int i = pos; i < ENEMYMAX - 1; i++)
            {
                if (enemies[i + 1] != null)
                {//次のEnemyがいるなら
                    enemyCardZones[i + 1].SetEnemy(null);//後列のCardZoneからEnemyを外す
                    enemyCardZones[i].SetEnemy(enemies[i + 1]);//Enemyを前列に移動
                    enemies[i] = enemies[i + 1];//enemies内のデータも移動
                    enemies[i + 1] = null;
                }
            }
        }

        //リストにEnemyが残っているなら終了
        for (int i = 0; i < ENEMYMAX - 1; i++)
        {
            if (enemies[i] != null)
            {
                return;
            }
        }
        //リストにEnemyがもう残っていないなら
        //敵を倒した時の処理に入る。
        fieldManager.WinToEnemy();
    }
    #endregion

    #region 敵全体に対して発生するエフェクト
    /// <summary>
    /// 敵全体にエフェクト効果を発生させる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void EnemyAllEffect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        //敵のそれぞれに同じエフェクトを発生させる
        for (int i = 0; i < ENEMYMAX; i++)
        {
            if (enemies[i] != null)
            {//Enemyがいるなら
                enemies[i].Effect(actionType, enemyAction, value);
            }
        }
    }

    /// <summary>
    /// 敵全体にエフェクト効果を発生させる
    /// posの位置は発生させない
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    /// <param name="pos">発生させない位置</param>
    public void EnemyAllEffectExceptThis(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value,int pos)
    {
        //敵のそれぞれに同じエフェクトを発生させる
        for (int i = 0; i < ENEMYMAX; i++)
        {
            if (enemies[i] == null || i == pos) continue;
            enemies[i].Effect(actionType, enemyAction, value);
        }
    }

    /// <summary>
    /// Enemy全てに発生しているエフェクト全てに削除演出を行う
    /// </summary>
    public void DestroyEffects()
    {
        for (int i = 0; i < ENEMYMAX; i++)
        {
            if (enemies[i] != null)
            {//Enemyがいるなら
                enemies[i].DestroyEffects();
            }
        }
    }
    #endregion

    #region ターン開始終了時処理

    /// <summary>
    /// 敵全体の次に行う行動を更新する
    /// </summary>
    /// <param name="isFirstTurn">最初のターンであるか</param>
    public void UpdateEnemiesNextAction(bool isFirstTurn)
    {
        foreach(Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            //行動を更新
            enemy.UpdateNextAction(isFirstTurn);
        }
    }

    /// <summary>
    /// 敵全体の行動をそれぞれ実行する
    /// 敵全体の行動が終了すると
    /// TurnEndAfterEnemiesActionがコールバックで呼ばれる
    /// </summary>
    public void ExecuteEnemiesAction(Sequence sequence)
    {
        enemyAnimation.EnemyActionAnimation(enemies,sequence);
    }

    /// <summary>
    /// 敵が行動した後の終了時処理
    /// </summary>
    public void TurnEndAfterEnemiesAction()
    {
        fieldManager.TurnEndAfterEnemiesAction();
    }

    /// <summary>
    /// 敵全体の盾を初期化する
    /// </summary>
    public void ResetEnemiesShield()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            //行動を更新
            enemy.ResetShield();
        }
    }

    /// <summary>
    /// ターン終了時に発生するStateの減少を発生させる。
    /// 実際にはEnemyが実行
    /// </summary>
    public void DecreaseState()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.DecreaseState();
        }
    }

    /// <summary>
    /// 敵全てに対して、バトル開始時に発生する効果を全て発生させる
    /// </summary>
    public void InitialEffectExecute()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.InitialEffectExecute();
        }
    }
    #endregion


    #region ダメージなどを与える処理

    /// <summary>
    /// プレイヤー側にダメージを与える
    /// </summary>
    /// <param name="value">ダメージの量</param>
    public void DamageToPlayer(int value)
    {
        fieldManager.DamageToPlayer(value);
    }

    public void StateToPlayer(IState state)
    {
        fieldManager.StateToPlayer(state);
    }

    /// <summary>
    /// 敵全体に状態異常を付与する
    /// </summary>
    /// <param name="type">状態異常のType</param>
    /// <param name="value">状態異常の値</param>
    public void StateToEnemyAll(Type type,int value)
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (!type.GetInterfaces().Contains(typeof(IState))){
                return;
            }

            var args = new object[] {}; //コンストラクタ引数に渡す値
            var iState = (IState)Activator.CreateInstance(type, args);
            iState.value = value;
            //行動を更新
            enemy.GetState(iState); ;
        }
    }

    /// <summary>
    /// 敵全体に状態異常を付与する
    /// posの位置には付与しない
    /// </summary>
    /// <param name="type">状態異常のType</param>
    /// <param name="value">状態異常の値</param>
    /// <param name="pos">付与しない位置</param>
    public void StateToEnemyAllExceptThis(Type type, int value,int pos)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null || i == pos) continue;
            var args = new object[] {}; //コンストラクタ引数に渡す値
            var iState = (IState)Activator.CreateInstance(type, args);
            iState.value = value;//値は後から設定
            //状態異常を設定
            enemies[i].GetState(iState); ;
        }
    }


    /// <summary>
    /// あるEnemyの場所を除いたすべての敵に対してActionを実行する
    /// </summary>
    /// <param name="action">実行する動作</param>
    /// <param name="pos">除外する場所</param>
    public void ActionToEnemyAllExceptThis(Action<Enemy> action,int pos)
    {
        for(int i=enemies.Length-1;i>=0;i--)
        {
            if (enemies[i] == null || i == pos) continue;

            action(enemies[i]);
        }
    }

    /// <summary>
    /// ターンの開始時に、ターンごとにカウントのある状態異常の値をリセットする
    /// </summary>
    public void ResetIStateCountPerTurn()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.ResetIStateCountPerTurn();
        }
    }

    #endregion

    #region ボスなどの特殊処理

    /// <summary>
    /// 大大剣のスケルトンロードの剣先が死亡した場合。
    /// 剣中の画像を変更し、スケルトンロードの行動パターンを変化
    /// </summary>
    public void BBSwordFrontDead()
    {
        int sLoadPos = GetUniqueTagPos(EnemyUniqueDefine.UniqueTag.BBSwordSkeletonLoad);
        int sMiddlePos = GetUniqueTagPos(EnemyUniqueDefine.UniqueTag.BBSwordMiddle);
        //スケルトンロードの行動パターン変化
        if (sLoadPos != -1)
        {
            enemies[sLoadPos].SetActionPattern(2);
            enemies[sLoadPos].NameChange("中大剣のスケルトンロード");
        }
        //剣中の画像変更
        if (sMiddlePos != -1)
        {
            enemies[sMiddlePos].PicChange(1);
            enemies[sMiddlePos].NameChange("大大剣(剣先)");
        }
    }

    /// <summary>
    /// 大大剣のスケルトンロードの剣中が死亡した場合。
    /// 剣先を破壊し、スケルトンロードの行動パターンを変化,画像変化
    /// </summary>
    public void BBSwordMiddleDead()
    {
        int sLoadPos = GetUniqueTagPos(EnemyUniqueDefine.UniqueTag.BBSwordSkeletonLoad);
        int sFrontPos = GetUniqueTagPos(EnemyUniqueDefine.UniqueTag.BBSwordFront);
        //スケルトンロードの行動パターン変化
        if (sLoadPos != -1)
        {
            enemies[sLoadPos].SetActionPattern(3);
            enemies[sLoadPos].PicChange(1);
            enemies[sLoadPos].NameChange("小剣のスケルトンロード");
        }
        //剣先を破壊
        if (sFrontPos != -1)
        {
            DestroyEnemy(sFrontPos);
        }
    }

    #endregion

    /// <summary>
    /// pos以外の全ての敵がDependentのStateをもっているならtrue
    /// </summary>
    /// <param name="pos">Dependentを調べない敵(この敵が死亡したときに発生)</param>
    /// <returns>pos以外の全ての敵がDependentのStateをもっているならtrue</returns>
    private bool CheckEnemyDependent(int pos)
    {
        for(int i= 0;i< enemies.Length; i++)
        {
            if(enemies[i] == null||i==pos) continue;

            //if(enemies[i].GetEnemyStateValue(StateDefine.StateType.Dependent)==-1) return false;
            //EnemyがStateDependentを持っているなら,一時的なboolをfalseにする。
            //bool bがfalseになっているならそのチェックはパス。
            bool b = true;
            enemies[i].SearchAndUseState<StateDependent>(a => b = false);
            if (b) return false;
        }

        return true;
    }

    /// <summary>
    /// 敵の中のユニークなタグを持つ手期の場所を返す。
    /// 存在しないなら-1
    /// </summary>
    /// <param name="uniqueTag">場所を検索するEnemyUniqueDefine.UniqueTag</param>
    /// <returns>タグを持つ敵の場所。いないなら-1</returns>
    private int GetUniqueTagPos(EnemyUniqueDefine.UniqueTag uniqueTag)
    {
        for(int i=0;i< enemies.Length; i++)
        {
            if (enemies[i] == null) continue;
            if (enemies[i].GetUniqueTag() == uniqueTag)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// プレイヤーの状態異常を検索し、実行する
    /// </summary>
    /// <typeparam name="T">検索する状態異常のタイプ</typeparam>
    /// <param name="useAction">a => a.UseA()のように、Tのメソッドを使用する指示</param>
    public void SearchAndUsePlayerState<T>(Action<T> useAction) where T : class, IState
    {
        fieldManager.SearchAndUsePlayerState<T>(useAction);
    }

    /// <summary>
    /// Playerのインスタンスを取得
    /// </summary>
    /// <returns>現在のPlayer</returns>
    public Player GetPlayer()
    {
        return fieldManager.GetPlayer();
    }

    public Enemy GetRandomEnemy()
    {
        //敵の中からランダムに一体を返す
        List<Enemy> enemyList = enemies.Where(e => e != null).ToList();
        if (enemyList.Count == 0) return null;//敵がいないならnullを返す
        int ran = UnityEngine.Random.Range(0, enemyList.Count);
        return enemyList[ran];
    }

    public Enemy[] GetEnemies()
    {
        return enemies;
    }

    public CardEffectExecute GetCardEffectExecute()
    {
        return cardEffectExecute;
    }

    public void TestDestroyAllEnemy()
    {
        for (int i = 0; i < ENEMYMAX; i++)
        {
            //敵は倒すたびに前に詰めるので、最初の敵だけを倒す
            if (enemies[0] != null)
            {
                DestroyEnemy(0);
            }
        }
    }
}
