using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

//--------------------------------------------------------------
//
//  敵の管理クラス
//
//--------------------------------------------------------------

// 敵の出現セット 
public enum EnemyPattern
{
    //  １面
    E01_3x1,    //  縦３ｘ横１
    E01_3x1B,   //  縦３ｘ横１B

    Max
}

//  敵の種類
public enum EnemyType
{
    Kooni,          //  子鬼
    Onibi,          //  鬼火
    Ibarakidouji,   //  茨城童子
    Douji,          //  ボス・ドウジ
    
    Max
}

public class EnemyManager : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static EnemyManager Instance
    {
        get; private set;
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -5.5f;
    EnemySetting enemySetting;

    private int appearStep = 0;
    private float timer = 0;
    private int waveNum = 0;    //  １ステージあたりの敵ウェーブ数


    void Start()
    {
        appearStep = 0;
        timer = 0;

    }

    private void OnDestroy()
    {
        // 解放
        Addressables.Release(enemySetting);
    }

    void Update()
    {
        AppearEnemy_Stage01();
        
    }

    //------------------------------------------------
    //  ランダムなX座標を返す
    //------------------------------------------------
    private float GetRandomX()
    {
        return Random.Range(-8.5f,-0.5f);
    }

    //------------------------------------------------
    //  指定の座標に指定の敵セットを生成する
    //------------------------------------------------
    private void SetEnemy(GameObject prefab,Vector3 pos)
    {
        Instantiate(prefab,pos,transform.rotation);
    }

    //------------------------------------------------
    //  タイマー
    //------------------------------------------------
    public bool Timer(int time)
    {
        timer += Time.deltaTime;
        if(timer >= time)
        {
            timer = 0;
            return true;
        }

        return false;
    }

    //------------------------------------------------
    //  ステージ１のザコ敵の出現パターン
    //------------------------------------------------
    public void AppearEnemy_Stage01()
    {
        //  アイテムドロップは１ステージ５回程度！

        switch(appearStep)
        {
            case 0:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3x1B],
                        new Vector3(GetRandomX(),appearY,0));
                appearStep++;
                break;
            case 1:
                if(Timer(3))appearStep++;
                break;
            //case 2:
            //    SetEnemy(
            //        enemyPrefab[(int)EnemyPattern.E01_3x1],
            //        new Vector3(GetRandomX(),appearY,0));
            //    appearStep++;
            //    break;
            //case 3:
            //    if(Timer(3))appearStep++;
            //    break;
            //case 4:
            //    SetEnemy(
            //            enemyPrefab[(int)EnemyPattern.E01_3x1],
            //            new Vector3(GetRandomX(),appearY,0));
            //    break;
            //case 5:
            //    if(Timer(3))appearStep++;
            //    break;
            //case 6:
            //    SetEnemy(
            //            enemyPrefab[(int)EnemyPattern.E01_3x1B],
            //            new Vector3(GetRandomX(),appearY,0));
            //    break;
            //case 7:
            //    if(Timer(3))appearStep++;
            //    break;
            //case 8:
            //    SetEnemy(
            //            enemyPrefab[(int)EnemyPattern.E01_3x1],
            //            new Vector3(GetRandomX(),appearY,0));
            //    break;
            //case 9:
            //    if(Timer(3))appearStep++;
            //    break;
            //case 10:
            //    SetEnemy(
            //            enemyPrefab[(int)EnemyPattern.E01_3x1],
            //            new Vector3(GetRandomX(),appearY,0));
            //    break;
            //case 11:
            //    if(Timer(3))appearStep++;
            //    break;
        }
    }
}
