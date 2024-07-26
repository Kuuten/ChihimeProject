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
    private int currentMaxHealth;
    private int currentHealth;
    private const int limitHealth = 12;
    private bool bSuperMode;
    private bool bDeath;

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


    void Start()
    {
        //  最初はハート３個分
        currentMaxHealth = 6;
        currentHealth = 6;

        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();

        //  ループカウントを設定
        loopCount = 30;

        //  点滅の間隔を設定
        flashInterval = 0.02f;

        //  死亡フラグOFF
        bDeath = false;

        //  最初は無敵モードOFF
        bSuperMode = false;
    }

    void Update()
    {      


        //if (test.WasPressedThisFrame())
        //{
        //    testSwitch = !testSwitch;

        //    //  敵のジェネレーターを無効化
        //    SetGeneratorActive(testSwitch);

        //    if(testSwitch == false)
        //    {
        //        //  Pauserが付いたオブジェクトをポーズ
        //        Pauser.Pause();
        //        //  ポーズ後に入力がきかなくなるのでリセット
        //        this.GetComponent<PlayerInput>().enabled = true;
        //    }
        //    else
        //    {
        //        //  Pauserが付いたオブジェクトをポーズ
        //        Pauser.Resume(); 
        //    }
        //}
    }

    //----------------------------------------------------------------
    //  プレイヤーの当たり判定
    //----------------------------------------------------------------
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //  無敵か死亡しているなら飛ばす
        if(bSuperMode || bDeath)return;

        if(collision.CompareTag("Enemy") ||         //  敵自体
            collision.CompareTag("EnemyBullet"))    //  敵弾
        {
            //  プレイヤーのダメージ処理
            EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();
            Damage( ed.Attack );

            //  死亡フラグON
            if(currentHealth <= 0)
            {
                bDeath = true;
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

            //  点滅演出
            var task = Blink();
            await task;

        }
    }

    //-------------------------------------------
    //  ダメージ時の点滅演出
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
    //  ダメージ処理
    //-------------------------------------------
    public void Damage(int value)
    {
        if(currentHealth > 0)
        {
            currentHealth -= value;
        }
        else
        {
            currentHealth = 0;
        }
        
    }

    //-------------------------------------------
    //  回復処理
    //-------------------------------------------
    public void Heal(int value)
    {
        int target = currentHealth + value;

        // 数値の変更
        DOTween.To(
            () => currentHealth,          // 何を対象にするのか
            num => currentHealth = num,   // 値の更新
            target,                       // 最終的な値
            value/2                       // アニメーション時間
        );
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

    //-------------------------------------------
    //  やられ演出
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  ライトOFF
        directionalLight.gameObject.SetActive(false);

        //  子オブジェクトの吸魂フィールドを非アクティブに
        this.gameObject.transform.Find("Field").gameObject.SetActive(false);

        //  プレイヤーを止める
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
        this.GetComponent<PlayerMovement>().enabled = false;
        this.GetComponent<PlayerShotManager>().enabled = false;

        //  プレイヤーのアニメ終了を待つ(最悪秒数で待つawaitとか)

        //  プレイヤーのやられエフェクト
        GameObject obj = Instantiate(
            playerDeathEffect, 
            this.transform.position,
            Quaternion.identity);

        //  Pauserが付いたオブジェクトをポーズ
        Pauser.Pause();


        yield return null;
    }


    private IEnumerator Death2()
    {
        //-------------------------------------------
        //  上へ飛び上がった後、下へ落ちていく
        //-------------------------------------------
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        bool complete = false;

        //  指定位置へ移動
        sequence.Append
            (
                    transform.DOLocalMoveY(2,0.5f)
                    .SetEase(Ease.OutCubic)
                    .SetRelative(true)  //  相対値移動
                    .OnStart(() => {
                        //Sound.Play( (int)AudioChannel.SFX, (int)SFXList.SFX_DASH);
                    })
                    .OnComplete(() =>{

                    })

            )
            .Append
            (
                transform.DOMoveY(-13,1.5f)
                .SetEase(Ease.InSine)  
                .OnStart(() => {
                    
                })
                .OnComplete(() =>{

                })
            );

        //  完了まで待つ
        yield return new WaitUntil(() => complete == true);
    }
}
