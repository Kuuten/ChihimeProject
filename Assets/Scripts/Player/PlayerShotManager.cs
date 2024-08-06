using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  プレイヤーのショット管理クラス
//
//--------------------------------------------------------------
//  ショットの種類
public enum SHOT_TYPE
{
    NORMAL,
    DOUJI,              // ドウジ
    DOUJI_BURST,        // ドウジ魂バースト
    TSUKUMO,            // ツクモ
    TSUKUMO_BURST,      // ツクモ魂バースト
    KUCHINAWA,          // クチナワ
    KUCHINAWA_BURST,    // クチナワ魂バースト
    KURAMA,             // クラマ
    KURAMA_BURST,       // クラマ魂バースト
    WADATSUMI,          // ワダツミ
    WADATSUMI_BURST,    // ワダツミ魂バースト
    HAKUMEN,            // ハクメン
    HAKUMEN_BURST,      // ハクメン魂バースト

    TYPE_MAX
}

//  ノーマル弾のレベルリスト
enum eNormalShotLevel
{
    Lv1 = 1,
    Lv2,
    Lv3,

    LvMax
}

public class PlayerShotManager : MonoBehaviour
{
    //  GameManager
    [SerializeField] private GameManager gameManager;

    //  弾の発射点
    [SerializeField]private Transform firePoint1;
    [SerializeField]private Transform firePoint2_L;
    [SerializeField]private Transform firePoint2_R;
    [SerializeField]private Transform firePoint3_L;
    [SerializeField]private Transform firePoint3_R;
    //  弾のプレハブ
    [SerializeField]private GameObject[] bulletPrefab;

    //  弾の移動ベクトル
    private Vector3 velocity;
    //  ノーマル弾のショット可能フラグ
    private bool canShot;
    //  ノーマル弾のショットCDのカウント用
    private float shotCount = 0;
    //  ノーマル弾の弾を何秒毎に撃てるか
    private float shotInterval = 0.05f;
    //  ノーマル弾のレベル
    private int normalShotLevel;
    //  ノーマル弾の移動量
    private const float normalSpeed = -20f; 
    //  ノーマル弾の攻撃力
    private float normalShotPower;

    //  テスト用
    public int gamestatus;
    InputAction test;
    bool b;

    //  入力
    InputAction shot;


    void Start()
    {
        // InputActionにMoveを設定
        PlayerInput playerInput = GetComponent<PlayerInput>();
        shot = playerInput.actions["Shot"];
        test = playerInput.actions["TestButton2"]; 

        normalShotPower = 1.0f;
        shotCount = 0;
        canShot = true;
        normalShotLevel = 1; // 最初はレベル１

        //  テスト用
        gamestatus = (int)eGameState.Zako;
        b = true;

        //  弾の向きはとりあえず通常弾に合わせる
        velocity = new Vector3(0,normalSpeed,0);   //  最初は下方向へ撃つ
    }

    void Update()
    {
        //  GameManagerから状態を取得
        //gamestatus = gameManager.GetGameState();

        //  Enterでザコボス切り替え
        if(test.WasPressedThisFrame())
        {
            b = !b;
            Debug.Log("切り替えフラグ:"+ b);

            if(b)gamestatus = (int)eGameState.Zako;
            else gamestatus = (int)eGameState.Boss;

            GameManager.Instance.SetGameState(gamestatus);
            
            Debug.Log("gamestatus:"+ gamestatus);
        }

        //  ゲーム段階別処理
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                NormalShot(true);                    //  通常弾
                break;
            case (int)eGameState.Boss:
                NormalShot(false);                   //  通常弾
                break;
            case (int)eGameState.Event:
                break;
        }

    }

    //---------------------------------------------------------
    //  プロパティ
    //---------------------------------------------------------
    public int GetNormalShotLevel(){ return normalShotLevel; }
    public void SetNormalShotLevel(int level)
    {
        Debug.Assert(normalShotLevel >= (int)eNormalShotLevel.Lv1 &&
            normalShotLevel <= (int)eNormalShotLevel.Lv3,
            "通常弾レベルの設定値が範囲外になっています！");
        if(normalShotLevel != level)normalShotLevel = level;
    }
    public float GetNormalShotPower(){ return normalShotPower; }
    public void SetNormalShotPower(float power) { normalShotPower = power; }

    //  弾の移動ベクトルを反転する
    public Vector3 GetReverseVelocity(int state)
    {
        //  ザコ戦中はデフォルト設定にする
        if( state == (int)eGameState.Zako )
        {
            //  移動ベクトル設定
            velocity.y = normalSpeed;
            return velocity;
        }
        else if( state == (int)eGameState.Boss ) // ボス戦中なら反転
        {
            //  移動ベクトル設定
            velocity.y = 20f;
            return velocity;
        }
        else if( state == (int)eGameState.Event ) // 会話イベント中なら撃てない
        {
            velocity.y = 0.0f;
            canShot = false;
            shotCount = 0;
        }

        return Vector3.zero;
    }

    //-------------------------------------------
    //  通常弾
    //-------------------------------------------
    private void NormalShot(bool flipY)
    {
        //  通常弾を撃つ
        if (!canShot)
        {
            if (shotCount >= shotInterval)
            {
                canShot = true;
                shotCount = 0;
            }
            else shotCount += Time.deltaTime;
        }
        else
        {
            //  弾発射
            if (shot.IsPressed())
            {
                //  フラグリセット
                canShot = false;

                //  オブジェクト一時格納用
                GameObject obj = null;

                //  通常弾の速度設定用
                NormalBullet n = null;;

                //  Velocity格納用
                Vector3 v = Vector3.zero;

                //  Y反転用のSpriteRenderer
                SpriteRenderer sr = null;

                //  Y反転時の発射口のY座標バイアス
                const float biasY = 0.44f;

                switch(normalShotLevel)
                {
                    case 1: //  レベル１
                        obj = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        Quaternion.identity);

                        //  Yを反転するかどうか設定する
                        sr = obj.GetComponent<SpriteRenderer>(); 
                        sr.flipY = flipY;

                        //  反転時に座標を調整
                        if(!sr.flipY)
                        {
                            obj.transform.position = 
                                new Vector3(firePoint1.position.x,
                                firePoint1.position.y + biasY,
                                firePoint1.position.z);
                        }

                        //  ボス戦かどうかでVelocityを取得して設定
                        v = GetReverseVelocity(gamestatus);
                        velocity = v;
                        n = obj.GetComponent<NormalBullet>();
                        n.SetVelocity(velocity);

                        break;
                    case 2: //  レベル２
                        const int lv2BulletNum = 3; //  一度に出る弾の数

                        //  弾の数分のリストを確保
                        List<Transform> firePointLv2= new List<Transform>(lv2BulletNum);
                        firePointLv2.Add(firePoint1.transform);
                        firePointLv2.Add(firePoint2_L.transform);
                        firePointLv2.Add(firePoint2_R.transform);

                        for(int i=0;i<firePointLv2.Count;i++)
                        {
                            obj = Instantiate(
                            bulletPrefab[(int)SHOT_TYPE.NORMAL],
                            firePointLv2[i].position,
                            Quaternion.identity);

                            //  Yを反転するかどうか設定する
                            sr = obj.GetComponent<SpriteRenderer>(); 
                            sr.flipY = flipY;

                            //  反転時に座標を調整
                            if(!sr.flipY)
                            {
                                obj.transform.position = 
                                    new Vector3(
                                        firePointLv2[i].position.x,
                                        firePointLv2[i].position.y + biasY,
                                        firePointLv2[i].position.z);
                            }

                            //  ボス戦かどうかでVelocityを取得して設定
                            v = GetReverseVelocity(gamestatus);
                            velocity = v;
                            n = obj.GetComponent<NormalBullet>();
                            n.SetVelocity(velocity);
                        }
                        break;
                    case 3: //  レベル３
                        const int lv3BulletNum = 5; //  一度に出る弾の数

                        //  弾の数分のリストを確保
                        List<Transform> firePointLv3 = new List<Transform>(lv3BulletNum);
                        firePointLv3.Add(firePoint1.transform);
                        firePointLv3.Add(firePoint2_L.transform);
                        firePointLv3.Add(firePoint2_R.transform);
                        firePointLv3.Add(firePoint3_L.transform);
                        firePointLv3.Add(firePoint3_R.transform);

                        for(int i=0;i<firePointLv3.Count;i++)
                        {
                            obj = Instantiate(
                            bulletPrefab[(int)SHOT_TYPE.NORMAL],
                            firePointLv3[i].position,
                            Quaternion.identity);

                            //  Yを反転するかどうか設定する
                            sr = obj.GetComponent<SpriteRenderer>(); 
                            sr.flipY = flipY;

                            //  反転時に座標を調整
                            if(!sr.flipY)
                            {
                                obj.transform.position = 
                                    new Vector3(firePointLv3[i].position.x,
                                    firePointLv3[i].position.y + biasY,
                                    firePointLv3[i].position.z);
                            }

                            //  ボス戦かどうかでVelocityを取得して設定
                            v = GetReverseVelocity(gamestatus);
                            velocity = v;
                            n = obj.GetComponent<NormalBullet>();
                            n.SetVelocity(velocity);
                        }
                        break;
                }

                
            }
        }

        //if(!canShot)
        //{
        //    if(shotCount >= shotInterval)
        //    {
        //        canShot = true;
        //        shotCount = 0;
        //    }
        //    else shotCount += Time.deltaTime;
        //}
        //else
        //{
        //    //  弾発射
        //    if( shot.WasPressedThisFrame() ) 
        //    {
        //        canShot = false;

        //        //  通常弾の連続で撃った数を0に
        //        bulletCount = 0;
        //    }
        //}

        ////  10フレームに1回撃つ
        //shotIntervalCount++;

        //if(shotIntervalCount % bulletInterval2 == 0)
        //{
        //    //  ５連ショット
        //    if(bulletCount < MaxContinuousShot)
        //    {
        //        Instantiate( bulletPrefab[(int)SHOT_TYPE.NORMAL], firePoint1_L.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);
        //        bulletCount++;
        //        shotIntervalCount = 0;
        //    }
        //}
    }

    //---------------------------------------------------
    //  通常弾のレベルアップ
    //---------------------------------------------------
    public void LevelupNormalShot()
    {
        if(normalShotLevel < (int)eNormalShotLevel.Lv3)normalShotLevel++;
    }


    //---------------------------------------------------
    //  通常弾のレベルダウン
    //---------------------------------------------------
    public void LeveldownNormalShot()
    {
        if(normalShotLevel > (int)eNormalShotLevel.Lv1)normalShotLevel--;
    }

}
