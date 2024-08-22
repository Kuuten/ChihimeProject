using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System;
using DG.Tweening;
using Unity.VisualScripting;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using static Unity.Collections.AllocatorManager;
using System.Threading;
using UnityEditor.Animations;
using DG.Tweening.Core.Easing;

//--------------------------------------------------------------
//
//  プレイヤーの体力管理クラス
//
//--------------------------------------------------------------

//  基本体力はハート３個分（半ハート６個分）
//  最大体力はハート６個分（半ハート１２個分）
//  最小単位の半ハート換算でプログラムする
public class PlayerHealth : MonoBehaviour
{
    //  ハートのタイプ
    enum HeartType
    {
        Half,   //  半分
        Full,   //  ハート１個分

        Max
    }

    [SerializeField] private int currentMaxHealth;  //  必ず偶数
    [SerializeField] private int currentHealth;
    private const int limitHealth = 12;
    private bool bSuperMode;
    private bool bDeath;
    private bool bDamage;

    //  ダメージ演出用のAnimator
    [SerializeField] private AnimatorController animPlayerFront;
    [SerializeField] private AnimatorController animPlayerFrontDamage;
    [SerializeField] private AnimatorController animPlayerBack;
    [SerializeField] private AnimatorController animPlayerBackDamage;

    //  死亡演出用のAnimator
    [SerializeField] private AnimatorController animPlayerDeath1;
    [SerializeField] private AnimatorController animPlayerDeath2;

    //  点滅させるためのSpriteRenderer
    SpriteRenderer sp;

    //  点滅の間隔
    private float flashInterval;

    //  点滅させるときのループカウント
    private int loopCount;

    //  プレイヤーの死亡エフェクト
    [SerializeField] private GameObject playerDeathEffect;
    //  ディレクショナルライト
    [SerializeField] private GameObject directionalLight;
    //  ハート画像の親オブジェクトの位置取得用
    [SerializeField] private GameObject heartRootObj;
    //  ハートフレームのプレハブ
    [SerializeField] private GameObject heartFrameObj;
    //  ハートフレームオブジェクトのリスト
    private List<GameObject> heartList = new List<GameObject>();

    void Awake()
    {
 
    }

    void Start()
    {
        //  PlayerInfoManagerから初期化
        currentMaxHealth = PlayerInfoManager.g_MAXHP;
        if(currentMaxHealth > limitHealth)
            Debug.LogError("最大体力値が制限を超過しています！");
        if(currentMaxHealth % 2 !=0)
            Debug.LogError("currentMaxHealthは必ず偶数でなければいけません！");
        currentHealth = PlayerInfoManager.g_CURRENTHP;
        if(currentHealth > currentMaxHealth)
            Debug.LogError("現在体力値が最大体力値を超過しています！");

        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();

        //  ループカウントを設定
        loopCount = 30;

        //  点滅の間隔を設定
        flashInterval = 0.02f;

        //  ダメージフラグOFF
        bDamage = false;

        //  死亡フラグOFF
        bDeath = false;

        //  最初は無敵モードOFF
        bSuperMode = false;

        //  親オブジェクトの子オブジェクトとしてハートフレームを生成
        for( int i=0; i<currentMaxHealth/2;i++ )
        {
            GameObject obj = Instantiate(heartFrameObj);
            obj.transform.SetParent( heartRootObj.transform );
            obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);
            obj.transform.localScale = Vector3.one;
            obj.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;


            heartList.Add( obj );   //  リストに追加
        }
    }

    void Update()
    {
        //  プレイヤーが死んでいたら処理しない
        if(bDeath)return;

        //  ゲーム段階別でAnimatorの切り替え
       int gamestatus = GameManager.Instance.GetGameState();
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                ChangePlayerSpriteToFront(true);     //  手前向き
                break;
            case (int)eGameState.Boss:
                ChangePlayerSpriteToFront(false);    //  奥向き
                break;
            case (int)eGameState.Event:
                break;
        }

        //  ハート画像を更新
        CalculateHealthUI(currentHealth);
    }

    //----------------------------------------------------------------
    //  プレイヤーの当たり判定
    //----------------------------------------------------------------
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //  無敵か死亡しているなら飛ばす
        if(bSuperMode || bDeath)return;

        if(collision.CompareTag("Enemy") )    //  敵本体にHIT！
        {
            //  プレイヤーのダメージ処理
            EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();
            Damage( ed.Attack );

            //  死亡フラグON
            if(currentHealth <= 0)
            {
                bDamage = false;
                bDeath = true;
                //  HitCircleを非表示にする
                this.transform.GetChild(6).gameObject.SetActive(false);
                StartCoroutine(Death());       //  やられ演出
                return;
            }

            //  全強化１段階ダウン
            PlayerShotManager ps = this.GetComponent<PlayerShotManager>();
            ps.LeveldownNormalShot();
            PlayerMovement pm = this.GetComponent<PlayerMovement>();
            pm.LeveldownMoveSpeed();

            //  デバッグ表示
            Debug.Log("全強化１段階ダウン！");
            Debug.Log("ショット強化 :" + ps.GetNormalShotLevel() 
                +"" + "スピード強化" + pm.GetSpeedLevel());
            Debug.Log("Playerの体力 :" + currentHealth);

            //  ダメージ時の赤くなる点滅演出開始
            var task1 = DamageAnimation();
            await task1;

            //  無敵演出開始
            var task2 = Blink();
            await task2;

        }
        else if(collision.CompareTag("EnemyBullet"))    //  敵弾にHIT！
        {
            //  プレイヤーのダメージ処理

            /* ここで弾にEnemyが付いてないのでエラーが出るハズ */
            /* 敵の弾用のクラスを作成して外部で弾生成時に威力を設定して */
            /* それをゲッターでここで取得できるようにしたい↓　*/
            //EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();
            //Damage( ed.Attack );
            Damage( 2 );    //  仮ダメージ処理


            //  死亡フラグON
            if(currentHealth <= 0)
            {
                bDamage = false;
                bDeath = true;
                //  HitCircleを非表示にする
                this.transform.GetChild(6).gameObject.SetActive(false);
                StartCoroutine(Death());       //  やられ演出
                return;
            }

            //  全強化１段階ダウン
            PlayerShotManager ps = this.GetComponent<PlayerShotManager>();
            ps.LeveldownNormalShot();
            PlayerMovement pm = this.GetComponent<PlayerMovement>();
            pm.LeveldownMoveSpeed();

            //  デバッグ表示
            Debug.Log("全強化１段階ダウン！");
            Debug.Log("ショット強化 :" + ps.GetNormalShotLevel() 
                +"" + "スピード強化" + pm.GetSpeedLevel());
            Debug.Log("Playerの体力 :" + currentHealth);

            //  ダメージ時の赤くなる点滅演出開始
            var task1 = DamageAnimation();
            await task1;

            //  無敵演出開始
            var task2 = Blink();
            await task2;
        }
    }

    //-------------------------------------------
    //  ダメージ時の赤くなる点滅演出
    //-------------------------------------------
    private async UniTask DamageAnimation()
    {
        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();

        //  一瞬色が変わる
        await UniTask.Delay (TimeSpan.FromSeconds(0.3f))
        .AttachExternalCancellation(token);

        //  ダメージフラグOFF
        bDamage = false;

    }

    //-------------------------------------------
    //  ダメージ時の無敵点滅演出
    //-------------------------------------------
    private async UniTask Blink()
    {
        //  無敵モードON
        bSuperMode = true;

        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();

        //点滅ループ開始
        for (int i = 0; i < loopCount; i++)
        {
            //flashInterval待ってから
            await UniTask.Delay (TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRendererをオフ
            sp.enabled = false;
            
            //flashInterval待ってから
            await UniTask.Delay (TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRendererをオン
            sp.enabled = true;
        }

        //  無敵モードOFF
        bSuperMode = false;
    }

    //-------------------------------------------
    //  プレイヤーのスプライトを差し替える
    //-------------------------------------------
   private void ChangePlayerSpriteToFront(bool front)
    {
        if(front)   //  ザコ戦中
        {
            if(bDamage) //  ダメージ中！
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerFrontDamage;
            }
            else // 通常
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerFront;
            }

        }
        else        //  ボス戦中
        {
            if(bDamage) //  ダメージ中！
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerBackDamage;
            }
            else // 通常
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerBack;
            }
        }
    }

    //-------------------------------------------
    //  ダメージ処理
    //-------------------------------------------
    public void Damage(int value)
    {
        //  ダメージ！
        bDamage = true;

        int target = currentHealth - value;
        //  最低体力で止める
        if (target <= 0)
        {
            currentHealth = 0;
        }

        currentHealth = target;

        //// 数値の変更
        //DOTween.To(
        //    () => currentHealth,          // 何を対象にするのか
        //    num => currentHealth = num,   // 値の更新
        //    target,                       // 最終的な値
        //    value/2                       // アニメーション時間
        //);

        //  デバッグ表示
        Debug.Log($"プレイヤーの体力が{value}減少して\n" +
            $"{currentHealth}になりました！");
        
    }

    //-------------------------------------------
    //  回復処理
    //-------------------------------------------
    public void Heal(int value)
    {
        int target = currentHealth + value;
        //  最大体力で止める
        if (target >= currentMaxHealth)
        {
            target = currentMaxHealth;
        }

        currentHealth = target;

        //// 数値の変更
        //DOTween.To(
        //    () => currentHealth,          // 何を対象にするのか
        //    num => currentHealth = num,   // 値の更新
        //    target,                       // 最終的な値
        //    value/2                       // アニメーション時間
        //);

        //  デバッグ表示
        Debug.Log($"プレイヤーの体力が{value}回復して\n" +
            $"{currentHealth}になりました！");
    }

    //-------------------------------------------
    //  最大体力増加処理
    //-------------------------------------------
    public void IncreaseHP(int value)
    {
        int target = currentMaxHealth + value;
        //  最大体力で止める
        if (target >= limitHealth)
        {
            target = limitHealth;
        }

        currentMaxHealth = target;

        //// 数値の変更
        //DOTween.To(
        //    () => currentMaxHealth,         // 何を対象にするのか
        //    num => currentMaxHealth = num,  // 値の更新
        //    target,                         // 最終的な値
        //    value                           // アニメーション時間
        //);

        //  デバッグ表示
        Debug.Log($"プレイヤーの最大体力が{value}増加して\n" +
            $"{currentMaxHealth}になりました！");
    }

    //-------------------------------------------
    //  プロパティ
    //-------------------------------------------
    public void SetCurrentHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > currentMaxHealth),
            "体力に範囲外の数が設定されています！");

        if(currentMaxHealth != value)currentMaxHealth = value;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentMaxHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > limitHealth),
            "最大体力に範囲外の数が設定されています！");

        if(currentMaxHealth != value)currentMaxHealth = value;
    }

    public int GetCurrentMaxHealth()
    {
        return currentMaxHealth;
    }

    public void SetDamageFlag(bool flag)
    {
        bDamage = flag;
    }

    public bool GetDamageFlag()
    {
        return  bDamage;
    }

    public void SetSuperMode( bool flag ){ bSuperMode = flag; }

    public bool GetSuperMode(){ return bSuperMode; }

    //-------------------------------------------
    //  やられ演出
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  デバッグ表示
        Debug.Log("プレイヤーが死亡しました！");

        //  ライトOFF
        directionalLight.gameObject.SetActive(false);

        //  子オブジェクトの吸魂フィールドを非アクティブに
        this.gameObject.transform.Find("Field").gameObject.SetActive(false);

        //  プレイヤーを止める
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<PlayerMovement>().enabled = false;
        this.GetComponent<PlayerShotManager>().enabled = false;

        //  Animatorを「death1」に差し替え
        this.GetComponent<Animator>().runtimeAnimatorController =
            animPlayerDeath1;

        //  0.8秒待つ
        yield return new WaitForSeconds(0.8f);

        //  Animatorを「death2」に差し替え
        this.GetComponent<Animator>().runtimeAnimatorController =
            animPlayerDeath2;

        //  1秒待つ
        yield return new WaitForSeconds(1.0f);

        //  プレイヤーを非表示にする
        this.GetComponent<SpriteRenderer>().enabled = false;

        //  プレイヤーのやられエフェクト
        GameObject obj = Instantiate(
            playerDeathEffect, 
            this.transform.position,
            Quaternion.identity);

        //  やられエフェクトの終了を待つ
        yield return new WaitForSeconds(0.583f);

        //  GameOver表示を待つ

        //  GameOverへシーン遷移
        LoadingScene.Instance.LoadNextScene("GameOver");

        //  Pauserが付いたオブジェクトをポーズ
        //Pauser.Pause();

        yield return null;
    }

    //---------------------------------------------------
    //  現在体力を受け取って体力UIを計算する
    //---------------------------------------------------
    private void CalculateHealthUI(int health)
    {
        if(health < 0)Debug.LogError("healthにマイナスの値が入っています！");

        //  体力0ならハートを全部非表示にする
        if(health == 0)
        {
            for(int i=0;i<heartList.Count;i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
            }
        }
        else if(health == 1)
        {
            for(int i=0;i<heartList.Count;i++)
            {
                if(i==0)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }

                //  残りを非表示にする
                for(int j=1;j<heartList.Count;j++)
                {
                    heartList[j].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[j].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }

            } 
        }
        else // 体力が２以上の時
        {
            //  一旦現在体力のとこまで全部フルで埋める
            int fullNum = health / 2;
            for(int i=0;i<fullNum;i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(true);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(true);
            }

            //  奇数だった場合は最後の番号だけハーフにする
            int taegetNum = health - fullNum;
            if(health % 2 != 0)
            {
                heartList[taegetNum-1].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(true);
                heartList[taegetNum-1].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
            }

            //  残りを非表示にする
            for(int i=taegetNum;i<heartList.Count;i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
            }
        }
    }
}
