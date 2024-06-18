using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーのショット管理クラス
//
//--------------------------------------------------------------
public class PlayerShotManager : MonoBehaviour
{
    //  弾の発射点
    [SerializeField]private Transform firePoint;
    //  弾のプレハブ
    [SerializeField]private GameObject[] bulletPrefab;

    //  通常ショット可能フラグ
    private bool canShot;
    //  ショット間隔のカウント用
    private float shotCount = 0;
    //  弾を何秒毎に撃てるか
    private float shotInterval = 1;
    //  弾の間隔(秒)
    private float bulletInterval = 0.1f;

    //  コルーチン
    private IEnumerator shotCoroutine;

    //  ショットの種類
    enum SHOT_TYPE
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

    void Start()
    {
        shotCount = 0;
        canShot = true;

        //　最初は通常弾   
        shotCoroutine = NormalShot();
    }

    void Update()
    {
        if(!canShot)
        {
            if(shotCount >= shotInterval)
            {
                canShot = true;
                shotCount = 0;
            }
            else shotCount += Time.deltaTime;
        }
        else
        {
            //  弾発射
            if( Input.GetKeyDown(KeyCode.Z)) 
            {
                canShot = false;

                shotCoroutine = NormalShot();

                StartCoroutine(shotCoroutine);
            }
        }
    }

    //---------------------------------------------------------
    //  プロパティ
    //---------------------------------------------------------
    private void SetShotInterval(float interval)
    {
        shotInterval = interval;
    }
    private void SetBulletInterva(float interval)
    {
        bulletInterval = interval;
    }
    private float GetBulletInterval()
    {
        return bulletInterval;
    }
    private void SetCanShot(bool shot)
    {
        canShot = shot;
    }
    private bool GetCanShot()
    {
        return canShot;
    }

    //-------------------------------------------
    //  通常弾コルーチン
    //-------------------------------------------
    private IEnumerator NormalShot()
    {
        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return new WaitForSeconds(bulletInterval);

        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return new WaitForSeconds(bulletInterval);

        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return new WaitForSeconds(bulletInterval);

        Instantiate( bulletPrefab[(int)SHOT_TYPE.HORMING], firePoint.position, bulletPrefab[(int)SHOT_TYPE.NORMAL].transform.rotation);

        yield return null;
    }

}
