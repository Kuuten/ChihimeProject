using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  ボス・ドウジのクラス
//
//--------------------------------------------------------------
public class BossDouji : BossBase
{
    //  ギミック弾の使用済み番号格納用
    private int[] kooniNum = new int[(int)DoujiPhase2Bullet.KooniDirection.MAX];

    //------------------------------------------------------------
    //  Phase2用
    //------------------------------------------------------------
    //  WARNING時の予測ライン
    private GameObject[] dangerLineObject;

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //  名前をここで設定しないとEnemyDataの読み込みに失敗する
        boss_id = "Douji";
    }

    protected override void Start()
    {
        base.Start();

        //  WARNING時の予測ラインオブジェクトを取得
        dangerLineObject = new GameObject[(int)DoujiPhase2Bullet.KooniDirection.MAX];
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.TOP] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Top);
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.BOTTOM] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Bottom);
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.LEFT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Left);
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.RIGHT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Right);

        //  ギミック弾の使用済み番号を初期化
        for(int i=0;i<(int)DoujiPhase2Bullet.KooniDirection.MAX;i++)
        {
            kooniNum[i] = -1;
        }

        //  行動開始
        StartCoroutine(StartAction());
    }

    /// <summary>
    /// 更新
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }

    //******************************************************************
    //
    //  移動パターン
    //
    //******************************************************************

    /// <summary>
    ///  Phase1
    /// </summary>
    protected override IEnumerator Phase1()
    {
        Debug.Log("フェーズ１開始");

        //  フェーズ１
        while (!bStopPhase1)
        {
            yield return StartCoroutine(LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());


            //yield return StartCoroutine(Douji_LoopMove(1.0f, 1.0f));
            //yield return StartCoroutine(Warning());
            //StartCoroutine(KooniParty());
            //StartCoroutine(KooniParty());
            //StartCoroutine(KooniParty());
            //yield return StartCoroutine(KooniParty());


            //yield return StartCoroutine(Douji_BerserkBarrage());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    /// <summary>
    ///  Phase2
    /// </summary>
    protected override IEnumerator Phase2()
    {
        Debug.Log("フェーズ２へ移行");

        //  フェーズ２
        while (!bStopPhase2)
        {
            StartCoroutine(WildlyShotSmall());

            //  Warning!(初回のみ)
            yield return StartCoroutine(Warning());

            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());

            yield return StartCoroutine(KooniParty());

            yield return StartCoroutine(LoopMove(1.0f,1.0f));
        }
    }

    /// <summary>
    ///  Phase3
    /// </summary>
    protected override IEnumerator Phase3()
    {
        Debug.Log("フェーズ３へ移行");

        //  フェーズ３
        while (true)
        {
            yield return StartCoroutine(LoopMoveBerserk(0.6f, 1.0f));

            yield return StartCoroutine(BerserkBarrage());
            yield return StartCoroutine(BerserkGatling());

            yield return StartCoroutine(LoopMoveBerserk(0.6f, 1.0f));

            yield return StartCoroutine(BerserkBarrage());
            yield return StartCoroutine(BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  通常攻撃パターン(Phase1)
    //------------------------------------------------------------------
     protected override IEnumerator Shot()
    {
        int rand = Random.Range(0,100);

        //  発生確率の閾値で呼び分ける
        if (rand <= 49f)
        {
            yield return StartCoroutine(WildlyShot(7.0f));

            yield return StartCoroutine(SnipeShot());
        }
        else if (rand <= 99f)
        {
            yield return StartCoroutine(OriginalShot());

            yield return StartCoroutine(StraightShot());

            yield return StartCoroutine(StraightShot());
        }
    }

    //------------------------------------------------------------------
    //  バラマキ弾(小)
    //------------------------------------------------------------------
    protected override IEnumerator WildlyShotSmall()
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 5;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 3.0f;             //  弾速
        int chain = 5;                  //  連弾数
        float chainInterval = 0.8f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-10,11)) * -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  バラマキ弾
    //------------------------------------------------------------------
    protected override IEnumerator WildlyShot(float speed)
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 360;        //  撃つ範囲の総角  
        int wayNum = 9;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        int chain = 10;                 //  連弾数
        float chainInterval = 0.4f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                /*　開始軸をランダムにずらす　*/
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-90,91)) * -transform.up;
                //Vector3 vector0 = -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  自機狙い弾
    //------------------------------------------------------------------
    protected override IEnumerator SnipeShot()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        float Degree = 15;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 9.0f;            //  弾速
        int chain = 3;                  //  連弾数
        float chainInterval = 1f;       //  連弾の間隔（秒）



        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  敵からプレイヤーへのベクトルを取得
                Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
                Vector3 vector0 = (playerPos - transform.position).normalized;
                Vector3[] vector = new Vector3[wayNum];

                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  オリジナル弾・ガトリングショット
    //------------------------------------------------------------------
    protected override IEnumerator OriginalShot()
    {
        //  弾のプレハブを取得
        GameObject bulletL = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        GameObject bulletR = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 1;                 //  弾のway数(必ず3way以上の奇数にすること)
        int chain = 5;                  //  連弾数
        float Degree = totalDegree/chain;     //  弾一発毎にずらす角度         
        float speed = 7.0f;             //  弾速
        float chainInterval = 0.05f;    //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * -Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        //  １秒間隔を空ける
        yield return new WaitForSeconds(1.0f);

        //  敵の前方ベクトルを取得
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        //  敵の前方ベクトルを取得
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed*2);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);

            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * -Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed*2);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  撃ち下ろしショット
    //------------------------------------------------------------------
    private IEnumerator StraightShot()
    {
        int currentlNum = (int)Control.Left;       //  現在位置
        List<int> targetList = new List<int>();    //  目標位置候補リスト
        int targetNum = (int)Control.Right;        //  目標位置

        Vector3 vec = Vector3.down;     //  弾のベクトル
        float duration = 2.0f;
        int bulletNum = 3;
        float interval = 2.0f;

        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        //  現在位置を求める（一番近い位置とする）
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        
        //  並び替え
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Right;

        //  リストをクリア
        targetList.Clear();

        //  目標の番号を設定
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
        }

        //  目標番号を設定
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  目標座標を取得
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  横移動開始
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  弾を生成
        GameObject bullet1 = Instantiate(bullet,transform.position,Quaternion.identity);
        bullet1.transform.DOMoveY(-15,duration)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .OnComplete(()=>Destroy(bullet1));

        yield return new WaitForSeconds(duration/bulletNum);

        GameObject bullet2 = Instantiate(bullet,transform.position,Quaternion.identity);
        bullet2.transform.DOMoveY(-15,duration)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .OnComplete(()=>Destroy(bullet2));

        yield return new WaitForSeconds(duration/bulletNum);

        GameObject bullet3 = Instantiate(bullet,transform.position,Quaternion.identity);
        bullet3.transform.DOMoveY(-15,duration)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .OnComplete(()=>Destroy(bullet3));

        yield return new WaitForSeconds(duration/bulletNum);

        //  現在の番号を更新
        currentlNum = targetNum;

        //  次の移動まで待つ
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  Phase2:子鬼の群れの進路を表示する
    //------------------------------------------------------------------
    private IEnumerator DisplayDirection(DoujiPhase2Bullet.KooniDirection direction, Vector2 pos)
    {
        GameObject line = null;

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_DOUJI_WARNING);

        //  予測進路のスライダーを生成
        GameObject canvas = EnemyManager.Instance.GetDangerLineCanvas();
        line = Instantiate(dangerLineObject[(int)direction]);
        line.transform.SetParent(canvas.transform);
        line.GetComponent<RectTransform>().anchoredPosition = pos;

        yield return new WaitForSeconds(1);

        if(line.gameObject)Destroy(line.gameObject);
    }

    //------------------------------------------------------------------
    //  Phase2:ランダムなスポナーから子鬼が突撃してくる
    //------------------------------------------------------------------
    private IEnumerator KooniParty()
    {
        int fourDirection = -1;

        while(true)
        {
            //  まずは４方向で抽選
            fourDirection  = Random.Range(0,4);

            //  番号が使用済みではなかったら
            if(kooniNum[fourDirection] == -1)
            {
                //  今回の番号を記録
                kooniNum[fourDirection] = fourDirection;
                break;
            }
        }

        //  発生確率の閾値で呼び分ける
        if (fourDirection == 0)         //  上方向とする
        {
            //  ギミック弾のプレハブを取得
            /*プレハブの種類が増える予定*/
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Top);

            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(0);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(-375,300)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(1);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(-60,300)
                        ));
            }
            else if(rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(2);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(250,300)
                        ));
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.TOP);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 1)    //  下方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Bottom);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(8);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(-375,-300)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(7);


                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(-60,-300)
                        ));
            }
            else if(rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(6);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(250,-300)
                        ));
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.BOTTOM);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 2)    //  左方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Left);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(11);

                //  座標をセット
                //pos2 = new Vector2(-560,190);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,180)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(10);

                //  座標をセット
                //pos2 = new Vector2(-560,-18);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,-15)
                        ));
            }
            else if(rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(9);

                //  座標をセット
                //pos2 = new Vector2(-560,-215);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,-220)
                        ));
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.LEFT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 3)    //  右方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Right);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(3);

                //  座標をセット
                //pos2 = new Vector2(440,190);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,185)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(4);

                //  座標をセット
                //pos2 = new Vector2(440,-18);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,-15)
                        ));
            }
            else if(rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(5);

                //  座標をセット
                //pos2 = new Vector2(440,-215);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,-215)
                        ));
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.RIGHT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }

        yield return null;
    }

    //-------------------------------------------------------------------
    //  発狂弾生成処理
    //------------------------------------------------------------------
    protected override IEnumerator GenerateBerserkBullet(float duration)
    {
        //  発狂弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Douji_Berserk_Bullet);

        //  弾を生成
        GameObject obj = Instantiate(bullet,transform.position,Quaternion.identity);
        DoujiPhase3Bullet enemyBullet = obj.GetComponent<DoujiPhase3Bullet>();

        enemyBullet.SetPower(enemyData.Attack);

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_DOUJI_SHOT1);

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  発狂弾
    //------------------------------------------------------------------
    protected override IEnumerator LoopMoveBerserk(float duration,float interval)
    {
        int currentlNum = (int)Control.Left;       //  現在位置
        List<int> targetList = new List<int>();    //  目標位置候補リスト
        int targetNum = (int)Control.Right;        //  目標位置
        int bulletNum = 3;

        Vector3 vec = Vector3.down;     //  弾のベクトル
        
        //  現在位置を求める（一番近い位置とする）
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        
        //  並び替え
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Right;

        //  リストをクリア
        targetList.Clear();

        //  目標の番号を設定
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
        }

        //  目標番号を設定
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  目標座標を取得
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  横移動開始
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  発狂弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Douji_Berserk_Bullet);

        //  弾を生成
        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        //  現在の番号を更新
        currentlNum = targetNum;

        //  次の移動まで待つ
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  発狂弾幕
    //------------------------------------------------------------------
    private IEnumerator BerserkBarrage()
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 9;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 8.0f;             //  弾速
        int chain = 5;                  //  連弾数
        float chainInterval = 0.3f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-10,11)) * -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;

        yield return null;
    }

    //------------------------------------------------------------------
    //  自機狙い発狂ガトリングショット
    //------------------------------------------------------------------
    protected override IEnumerator BerserkGatling()
    {
        //  弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        int wayNum = 3;                 //  弾のway数
        float Degree = 20;              //  ずらす角度
        int chain = 2;                  //  連弾数         
        float speed = 8.0f;             //  弾速
        float chainInterval = 0.5f;     //  連弾の間隔（秒）

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  敵からプレイヤーへのベクトルを取得
                Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
                Vector3 vector0 = (playerPos - transform.position).normalized;
                Vector3[] vector = new Vector3[wayNum];

                vector[i] = Quaternion.Euler( 0, 0, -Degree + i * Degree ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        //  3秒待つ
        yield return new WaitForSeconds(1.0f);
    }
}
