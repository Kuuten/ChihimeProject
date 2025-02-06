using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.VisualScripting;
using UnityEngine.Timeline;
using UnityEngine.VFX;
using System.Linq;

//--------------------------------------------------------------
//
//  ボス・クチナワのクラス
//
//--------------------------------------------------------------
public class BossKuchinawa : BossBase
{
    //------------------------------------------------------------
    //  Phase2用
    //------------------------------------------------------------


    //------------------------------------------------------------
    //  Phase3用
    //------------------------------------------------------------

    //  オブジェクトリスト
    public enum eObjectList
    {
        Wave01,     //  波紋１
        Wave02,     //  波紋２
        Circle,     //  魔法陣
        Beam,       //  ビーム
        Signal,     //  シグナル

        Max
    }

    //  Wave01
    [SerializeField] GameObject wave01Prefab;
    private GameObject wave01Object;
    //  Wave02
    [SerializeField] GameObject wave02Prefab;
    private GameObject wave02Object;
    //  Circle
    [SerializeField] GameObject magicCirclePrefab;
    private GameObject magicCircleObject;
    //  Beam
    [SerializeField] GameObject beamPrefab;
    private GameObject beamObject;
    //  PlayableDirector
    private PlayableDirector director;
    private bool isTimelineFinished; // Director終了フラグ
    Coroutine coroutine;

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //  名前をここで設定しないとEnemyDataの読み込みに失敗する
        boss_id = "Kuchinawa";
    }

    protected override void Start()
    {
        base.Start();

        if (PlayableDirectorManager.Instance == null)
        {
            Debug.LogError("PlayableDirectorManager のインスタンスが null です！");
            return;
        }

        director = PlayableDirectorManager.Instance.GetDirector();

        if (director == null)
        {
            Debug.LogError("PlayableDirector が見つかりません！");
            return;
        }


        //  タイムライン終了時のイベントを登録
        director.stopped += OnTimelineStopped;

        //  行動開始
        StartCoroutine(StartAction());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        director.Stop();           //  Directorを停止

        director.stopped -= OnTimelineStopped;
    }

    //*********************************************************************************
    //
    //  更新
    //
    //*********************************************************************************
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
            //  Warning!(初回のみ)
            yield return StartCoroutine(Warning());
            //StartCoroutine(WildlyShot(7.0f));
            //yield return StartCoroutine(ShoujiKekkai());
            yield return StartCoroutine(LoopMove(1.0f, 1.0f));
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
            //StartCoroutine(BerserkFireworks());
            yield return StartCoroutine(GenerateBerserkBullet(2.5f));
            yield return StartCoroutine(LoopMoveBerserk(0.6f, 3.0f));
            StartCoroutine(GenerateBerserkBullet(2.5f));
            yield return StartCoroutine(WildlyShot(9.0f));
            yield return StartCoroutine(MoveToCenter());
        }
    }

    //------------------------------------------------------------------
    //  通常攻撃パターン(Phase1)
    //------------------------------------------------------------------
    protected override IEnumerator Shot()
    {
        yield return StartCoroutine(WildlyShot(7.0f));

        //yield return StartCoroutine(SnipeShot());

        //yield return StartCoroutine(OriginalShot());


    }

    //------------------------------------------------------------------
    //  バラマキ弾
    //------------------------------------------------------------------
    protected override IEnumerator WildlyShot(float speed)
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角
        int wayNum = 15;                //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum - 1);     //  弾一発毎にずらす角度
        float chainInterval = 0.03f;    //  連弾の間隔（秒）
        float AttackInterval = 0.5f;    //  弾幕毎の間隔（秒）
        Vector3[] vector = new Vector3[wayNum];

        //-----------------------------------------------
        //  右から左へバラマキ
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0, 0, 90) * -transform.up;

            vector[i] = Quaternion.Euler(0, 0, (i * -Degree)) * vector0;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_obj);

            EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);

            if (i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  攻撃間隔を開ける
        yield return new WaitForSeconds(AttackInterval);

        //-----------------------------------------------
        //  左から右へバラマキ
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0, 0, -90) * -transform.up;

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vector0;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_obj);

            EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);

            if (i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  攻撃間隔を開ける
        yield return new WaitForSeconds(AttackInterval);

        //-----------------------------------------------
        //  左右からバラマキ
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vectorR = Quaternion.Euler(0, 0, 90) * -transform.up;
            Vector3 vectorL = Quaternion.Euler(0, 0, -90) * -transform.up;

            //弾インスタンスを取得し、初速と発射角度を与える
            vector[i] = Quaternion.Euler(0, 0, (i * -Degree)) * vectorR;
            vector[i].z = 0f;

            GameObject Bullet_objR =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_objR);

            EnemyBullet enemyBulletR = Bullet_objR.GetComponent<EnemyBullet>();
            enemyBulletR.SetSpeed(speed);
            enemyBulletR.SetVelocity(vector[i]);
            enemyBulletR.SetPower(enemyData.Attack);

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vectorL;
            vector[i].z = 0f;

            GameObject Bullet_objL =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_objL);

            EnemyBullet enemyBulletL = Bullet_objL.GetComponent<EnemyBullet>();
            enemyBulletL.SetSpeed(speed);
            enemyBulletL.SetVelocity(vector[i]);
            enemyBulletL.SetPower(enemyData.Attack);

            if (i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  攻撃間隔を開ける
        yield return new WaitForSeconds(AttackInterval);
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

                vector[i] = Quaternion.Euler(0, 0, -Degree + (i * Degree)) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj =
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                //  リストに追加
                AddBulletFromList(Bullet_obj);

                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if (i == 0)
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
    //  オリジナル弾・ホーミングショット
    //------------------------------------------------------------------
    protected override IEnumerator OriginalShot()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        //  ランダムなコントロールポイントに向かって弾が飛んでいき、
        //  一定距離まで近づくとその周りを回転し始める。
        //  一定時間回転した弾はプレイヤーに向かって飛んでいく。

        int wayNum = 5;                 //  一度に撃つ弾数
        float speed = 7.0f;             //  弾速
        float chainInterval = 2f;       //  連弾の間隔（秒）
        float Interval = 3f;            //  次の行動までの間隔（秒）

        //  目標のコントロールポイント番号格納用
        List<int> targetNum = new List<int>();

        //  3～8までセット
        for (int i = 3; i < 3 + (wayNum + 1); i++)
        {
            targetNum.Add(i);
        }

        /* 弾生成する */
        for (int i = 0; i < wayNum; i++)
        {
            //  弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_obj);

            //  弾にデフォルトでEnemyBulletコンポーネントがあるのでそれを削除する
            Destroy(Bullet_obj.GetComponent<EnemyBullet>());

            //  代わりにTsukumoHomingBulletコンポーネントを追加する
            Bullet_obj.AddComponent<TsukumoHomingBullet>();

            //  3～8番までを重複なしでランダムに抽出
            if (targetNum.Count > 6/*目標座標の数*/- wayNum)
            {
                int index = Random.Range(0, targetNum.Count);

                int ransu = targetNum[index];

                //  弾に目標番号をセット
                Bullet_obj.GetComponent<TsukumoHomingBullet>().SetTargetNum(ransu);

                targetNum.RemoveAt(index);
            }

            //  必要な情報をセットする
            TsukumoHomingBullet enemyBullet = Bullet_obj.GetComponent<TsukumoHomingBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetPower(enemyData.Attack);

            if (i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
        }
        yield return new WaitForSeconds(chainInterval);

        yield return new WaitForSeconds(Interval);
    }

    //-------------------------------------------------------------------
    //  発狂弾生成処理
    //------------------------------------------------------------------
    protected override IEnumerator GenerateBerserkBullet(float duration)
    {
        //  ビーム発射！
        Play();

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  発狂移動
    //------------------------------------------------------------------
    protected override IEnumerator LoopMoveBerserk(float duration, float interval)
    {
        int currentlNum = (int)Control.Left;       //  現在位置
        List<int> targetList = new List<int>();    //  目標位置候補リスト
        int targetNum = (int)Control.Right;        //  目標位置

        Vector3 vec = Vector3.down;     //  弾のベクトル

        //  現在位置を求める（一番近い位置とする）
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1, this.transform.position);
        float d2 = Vector3.Distance(p2, this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);

        //  並び替え
        dList.Sort();

        if (dList[0] == d1) currentlNum = (int)Control.Left;
        if (dList[0] == d2) currentlNum = (int)Control.Right;

        //  リストをクリア
        targetList.Clear();

        //  目標の番号を設定
        if (currentlNum == (int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if (currentlNum == (int)Control.Right)
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

        //  現在の番号を更新
        currentlNum = targetNum;

        //  次の移動まで待つ
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  発狂ガトリングショット
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

                vector[i] = Quaternion.Euler(0, 0, -Degree + i * Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj =
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                //  リストに追加
                AddBulletFromList(Bullet_obj);

                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if (i == 0)
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

    /// <summary>
    ///     モデルを生成してアニメーションを再生
    /// </summary>
    /// <returns></returns>
	public IEnumerator PlayAnimation()
    {
        //  プレイ前にフラグをリセット
        isTimelineFinished = false;

        //  座標を設定しておく
        Vector3 centerPos = this.transform.position;
        Vector3 target1 = centerPos + new Vector3(0, -2, 0);

        //  波紋１を生成
        wave01Object = Instantiate(wave01Prefab);
        wave01Object.transform.position = target1;
        Animator animator01 = wave01Object.GetComponent<Animator>();

        //  波紋2を生成
        wave02Object = Instantiate(wave02Prefab);
        wave02Object.transform.position = target1;
        Animator animator02 = wave02Object.GetComponent<Animator>();

        //  魔法陣を生成
        magicCircleObject = Instantiate(magicCirclePrefab);
        magicCircleObject.transform.position = target1;
        Animator animator03 = magicCircleObject.GetComponent<Animator>();

        //  ビームを生成
        beamObject = Instantiate(beamPrefab);
        beamObject.transform.position = target1;
        Vector3 target2 = centerPos + new Vector3(0, -9, 0);
        beamObject.transform.position = target2;

        //  新しいゲームオブジェクトを生成して全ての親オブジェクトにする
        GameObject parent = new GameObject("冥土バスター");
        wave01Object.transform.SetParent(parent.transform);
        wave02Object.transform.SetParent(parent.transform);
        magicCircleObject.transform.SetParent(parent.transform);
        beamObject.transform.SetParent(parent.transform);

        //  親オブジェクトを弾リストに追加
        AddBulletFromList(parent);

        // // 取ってきたAnimatorをPlayableDirectorにバインド
        Bind(animator01, (int)eObjectList.Wave01);
        Bind(animator02, (int)eObjectList.Wave02);
        Bind(animator03, (int)eObjectList.Circle);
        Bind(null, (int)eObjectList.Beam);
        Bind(null, (int)eObjectList.Signal);

        // 再生して、再生終了後に生成したオブジェクトを消去する
        director.Play();

        //  Directorの再生終了待ち
        yield return new WaitUntil(() => isTimelineFinished == true);

        coroutine = null;

        Destroy(parent);
    }

    /// <summary>
    /// /   DirectorにAnimatorをバインド
    /// </summary>
    /// <param name="animator"></param>
	void Bind(Animator animator, int streamId)
    {
        // TimelineからCharacterAnimationTrackのトラックへの参照を取得して
        // 生成したAnimator情報を流し込む
        string[] streamName =
        {
            "AnimationTrackWave01",
            "AnimationTrackWave02",
            "AnimationTrackCircle",
            "Visual Effect Control Track",
            "Signal Track",
        };


        //  エラー処理
        if (streamName.Length != (int)eObjectList.Max)
        {
            Debug.LogError("streamNameとShaderEffectの要素数が一致しません！");
        }


        //  ストリーム名を検索して格納
        PlayableBinding binding = director.playableAsset.outputs.First(c => c.streamName == streamName[streamId]);


        //  バインド
        if (!binding.IsUnityNull())
        {
            if(streamId == (int)eObjectList.Signal)
            {
                director.SetGenericBinding(binding.sourceObject, this.GetComponent<SignalReceiver>());
            }
            else if(streamId == (int)eObjectList.Beam)
            {
                director.SetGenericBinding(binding.sourceObject, beamObject.GetComponent<VisualEffect>());
            }
            else 
            {
                director.SetGenericBinding(binding.sourceObject, animator);
            }
        }

    }

    /// <summary>
    /// 再生
    /// </summary>
	public void Play()
    {
        if (coroutine == null)
            coroutine = StartCoroutine(PlayAnimation());
    }

    /// <summary>
    ///     PlayableDirector の終了を検知
    /// </summary>
    void OnTimelineStopped(PlayableDirector pd)
    {
        if (pd == director)
        {
            isTimelineFinished = true; // `isTimelineFinished` を true にする
            Debug.Log("Timelineが停止しました");
        }
    }

    /// <summary>
    /// TimeLineから球のヒットボックス有効化を受け取った時の処理
    /// </summary>
    public void OnCircleColliderEnabled()
    {
        if (beamObject) beamObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    /// <summary>
    /// TimeLineからビーム当たり判定有効化を受け取った時の処理
    /// </summary>
    public void OnPolygonColliderEnabled()
    {
        if (beamObject) beamObject.GetComponent<PolygonCollider2D>().enabled = true;
    }

    /// <summary>
    /// 全当たり判定無効化を受け取った時の処理
    /// </summary>
    public void OnAllColliderDisabled()
    {
        if (beamObject)
        {
            beamObject.GetComponent<CircleCollider2D>().enabled = false;
            beamObject.GetComponent<PolygonCollider2D>().enabled = false;
        }
    }
}
