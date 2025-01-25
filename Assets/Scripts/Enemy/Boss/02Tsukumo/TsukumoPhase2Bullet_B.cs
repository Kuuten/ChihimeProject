using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Enemy;

//--------------------------------------------------------------
//
//  ツクモのPhase2の弾クラス
//
//--------------------------------------------------------------
public class TsukumoPhase2Bullet_B : MonoBehaviour
{
    private Vector3 velocity;
    private float speed;
    private int power;

    private float lifetime;   //  寿命（秒）

    private float hp;
    private bool bSuperMode;

    private int step;

    //  やられエフェクト
    [SerializeField] private GameObject explosion;

    private float flashInterval;
    private int loopCount;

    //メインカメラに付いているタグ名
    private static readonly string MAIN_CAMERA_TAG_NAME = "MainCamera";

    //カメラに表示されているか
    private bool isRendered = false;

    private float chaseTimer;
    private float degree;
    public Sprite alphaSprite;
    public Sprite endSprite;


    void Awake()
    {
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        hp = 10;
        bSuperMode = true;      //  最初は無敵モードON
        step = 0;
        loopCount = 1;          //  ループカウントを設定
        flashInterval = 0.2f;   //  点滅の間隔を設定
        chaseTimer = 3f;
        degree = 0;

        lifetime = 5f;    //  暫定
        Destroy(gameObject, lifetime);

        //  最初は薄い色のオブジェクト
        this.GetComponent<SpriteRenderer>().sprite = alphaSprite;
        //  現れるまでは当たり判定無効
        this.GetComponent<BoxCollider2D>().enabled = false;

        //  アルファアニメーション
        this.GetComponent<SpriteRenderer>().DOFade(0.5f,3f)
            .OnPlay(()=>
            {
                //  登場SE再生
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX_SYSTEM2,
                    (int)SFXList.SFX_TSUKUMO_SHOT2);
            })
            .OnComplete(()=>
            {
                bSuperMode = false;
                //  濃い色のオブジェクト
                this.GetComponent<SpriteRenderer>().sprite = endSprite;
                //  アルファを1.0に
                this.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
                //  当たり判定を有効化
                this.GetComponent<BoxCollider2D>().enabled = true;
                //  登場SE再生2
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX_SYSTEM2,
                    (int)SFXList.SFX_TSUKUMO_SHOT3);
            });
    }

    void Update()
    {
        switch(step)
        {
            case 0: //  プレイヤー追尾
                if(chaseTimer <= 0)
                {
                    step++;
                }
                else
                {
                    chaseTimer -= Time.deltaTime;

                    //  座標を更新
                    UpdatePosition();
                }
                break;
            case 1: //  何もしない
                break;
        
        }
    }

    //-------------------------------------------------
    //  当たり判定
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  死んでいるか画面外なら処理しない
        if(hp <= 0)return;


        if (collision.CompareTag("DeadWall"))
        {
            if(bSuperMode)return;

            //Destroy(this.gameObject);
        }
        //  人形にぶつかっっても壊れる
        else if (collision.CompareTag("Enemy"))
        {
            if(bSuperMode)return;

            Death();                       //  やられ演出
        }
        else if (collision.CompareTag("NormalBullet"))
        {
            //  無敵モードなら返す
            if(bSuperMode)return;

            //  弾の消去
            Destroy(collision.gameObject);

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetNormalShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                Death();                       //  やられ演出
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  ダメージ処理
           float d = collision.GetComponent<ConvertDoujiBullet>().GetInitialPower();
            Damage(d);

            //  中攻撃か強攻撃か判定
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  魂バーストゲージを増やす
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  魂バーストゲージを少し増やす
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }
                    
            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                Death();                      //  やられ演出
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                Death();                      //  やられ演出
            }
        }
        else if (collision.CompareTag("TsukumoConvert"))
        {
            //  弾を消す
            Destroy(collision.gameObject);

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  中攻撃か強攻撃か判定
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  魂バーストゲージを増やす
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  魂バーストゲージを少し増やす
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }
                    
            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                Death();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("TsukumoKonburst"))
        {
            //  弾を消す
            Destroy(collision.gameObject);

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                Death();                      //  やられ演出
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetBombPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                Death();                        //  やられ演出
            }
        }
    }

    //------------------------------------------------------
    //  プロパティ
    //------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public void SetSpeed(float s){ speed = s; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }
    public void SetDegree(float d){ degree = d; }
    public float GetHP(){ return hp; }

    //-------------------------------------------
    //  ダメージ処理
    //-------------------------------------------
    public void Damage(float value)
    {
        if(hp > 0.0f)
        {
            hp -= value;
        }
        else
        {
            hp = 0.0f;
        }
    }

    //-------------------------------------------
    //  ダメージ時の点滅演出
    //-------------------------------------------
    public IEnumerator Blink(bool super, int loop_count, float flash_interval)
    {
        SpriteRenderer sp = this.GetComponent<SpriteRenderer>();

        //  無敵モードON
        if(super)bSuperMode = true;

        //点滅ループ開始
        for (int i = 0; i < loop_count; i++)
        {
            //flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //spriteRendererをオフ
            if(sp)sp.enabled = false;

            //flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //spriteRendererをオン
            if(sp)sp.enabled = true;
        }
    }

    //-------------------------------------------
    //  やられ演出(通常弾・ボム)
    //-------------------------------------------
    private void Death()
    {
        //  やられエフェクト
        Instantiate(explosion, transform.position, transform.rotation);

        // やられSE
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_ENEMY_DEATH);

        //  オブジェクトを削除
        Destroy(this.gameObject);
    }

    //------------------------------------------------------
    //  ダメージSEを再生した後無敵モードをオフにする
    //------------------------------------------------------
    IEnumerator PlayDamageSFXandSuperModeOff()
    {
        float interval = 0.1f;  //  無敵が解除されるまでの時間（秒）

        //  ダメージSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_ENEMY_DAMAGE);

        //  何秒か待つ
        yield return new WaitForSeconds(interval);

        //  無敵モードOFF
        bSuperMode = false;
    }

    //------------------------------------------------------
    //  カメラに映ってる間に呼ばれる
    //------------------------------------------------------
    private void OnWillRenderObject()
    {
        //メインカメラに映った時だけ_isRenderedを有効に
        if(Camera.current.name != "SceneCamera"  && Camera.current.tag == MAIN_CAMERA_TAG_NAME){
            isRendered = true;
        }
    }

    //------------------------------------------------------
    //  障子の座標を更新
    //------------------------------------------------------
    private void UpdatePosition()
    {
        float totalDegree = 360;                 //  撃つ範囲の総角  
        int wayNum = 4;                          //  弾のway数
        float Degree = totalDegree / wayNum;     //  弾一発毎にずらす角度   
        Vector3 centerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 pos2 = centerPos + new Vector3(0,-1,0);
        Vector3 vector0 = (pos2 - centerPos).normalized;
        Vector3 vector = Vector3.zero;
        float distance = 3.0f;

        vector = Quaternion.Euler
            (0, 0, degree) * vector0;
            vector.z = 0f;

        //  配置する
        transform.position = centerPos + vector * distance;

    }
}
