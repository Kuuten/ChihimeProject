using System.Collections;
using System.Collections.Generic;
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
    POWER,         // ドウジ
    WIDE,          // クラマ
    PENETRATION,   // クチナワ
    HORMING,       // ツクモ
    SHIELD,        // ワダツミ
    ALMIGHT,       // ハクメン

    TYPE_MAX
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

    //  弾の方向
    private Quaternion shotRotation;
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

    public int gamestatus = 0;


    //  入力
    InputAction shot;

    void Start()
    {
        // InputActionにMoveを設定
        PlayerInput playerInput = GetComponent<PlayerInput>();
        shot = playerInput.actions["Shot"];

        shotCount = 0;
        canShot = true;
        normalShotLevel = 1; // 最初はレベル１
        //  弾の向きはとりあえず通常弾に合わせる
        shotRotation = bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation;
        velocity = new Vector3(0,normalSpeed,0);   //  最初は下方向へ撃つ
    }

    void Update()
    {
        //  GameManagerから状態を取得
        int gamestatus = gameManager.GetGameState();

        //  通常弾
        NormalShot(gamestatus);
        
    }

    //---------------------------------------------------------
    //  プロパティ
    //---------------------------------------------------------

    //  弾の画像の向きと移動ベクトルを反転する
    public void Reverse(int state)
    {
        //  ザコ戦中はデフォルト設定にする
        if( state == (int)eGameState.Zako )
        {
            shotRotation = Quaternion.Euler(0,0,180);

            //  移動ベクトル設定
            velocity = new Vector3(0, normalSpeed, 0);
        }
        else // ボス戦か会話イベント中なら反転
        {
            //  弾の移動ベクトルが下向きなら
            if(velocity.y == normalSpeed)
            {
                //  画像の向き反転
                shotRotation = Quaternion.Euler(0,0,0);

                //  移動ベクトル設定
                velocity.y = -normalSpeed;
            }
        }
    }

    //-------------------------------------------
    //  通常弾
    //-------------------------------------------
    private void NormalShot(int state)
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

                //  弾の画像の向き反転
                Reverse(state);

                switch(normalShotLevel)
                {
                    case 1: //  レベル１
                        NormalBullet n = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n.SetVelocity(velocity);
                        n.SetRotation(shotRotation);
                        
                        break;
                    case 2: //  レベル２
                        NormalBullet n2 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n2.SetVelocity(velocity);
                        n2.SetRotation(shotRotation);

                        NormalBullet n3 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_L.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n3.SetVelocity(velocity);
                        n3.SetRotation(shotRotation);

                        NormalBullet n4 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_R.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n4.SetVelocity(velocity);
                        n4.SetRotation(shotRotation);
                        break;
                    case 3: //  レベル３
                        NormalBullet n5 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint1.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n5.SetVelocity(velocity);
                        n5.SetRotation(shotRotation);

                        NormalBullet n6 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_L.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n6.SetVelocity(velocity);
                        n6.SetRotation(shotRotation);

                        NormalBullet n7 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint2_R.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n7.SetVelocity(velocity);
                        n7.SetRotation(shotRotation);

                        NormalBullet n8 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint3_L.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n8.SetVelocity(velocity);
                        n8.SetRotation(shotRotation);

                        NormalBullet n9 = Instantiate(
                        bulletPrefab[(int)SHOT_TYPE.NORMAL],
                        firePoint3_R.position,
                        shotRotation).GetComponent<NormalBullet>();
                        n9.SetVelocity(velocity);
                        n9.SetRotation(shotRotation);
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

}
