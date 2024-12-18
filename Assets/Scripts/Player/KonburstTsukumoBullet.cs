using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerShotManager;

//--------------------------------------------------------------
//
//  プレイヤーのツクモ魂バースト弾クラス
//
//--------------------------------------------------------------
public class KonburstTsukumoBullet : MonoBehaviour
{
    private int gamestatus;
    private bool fripY;
    private float priod = 5.0f;  //  寿命（秒）※３秒以上にすること！

    private bool isL;

    private Vector3 posL;
    private Vector3 posR;

    [SerializeField] private GameObject bulletPrefab;

    void Start()
    {
        //  寿命が来たら消す
        Destroy(gameObject, priod);

        //  寿命の３秒前に点滅開始
        StartCoroutine(Blink(15,0.2f)) ;
    }

    void Update()
    {
        //  プレイヤーの座標
        GameObject player = GameManager.Instance.GetPlayer();
        Vector3 playerPos = player.transform.position;

        //  プレイヤーの左右ベクトルを求める
        Vector3 left = -player.transform.right;
        Vector3 right = player.transform.right;

        //  左右に１離れた座標を求める
        float bias = 2.0f; 
        Vector3 posL = playerPos + left * bias;
        Vector3 posR = playerPos + right * bias;

        //  座標を更新
        if(isL)
        {
            transform.position = posL;
        }
        else
        {
            transform.position = posR;
        }

        //  弾の更新
        UpdateBullet();
        
    }

    //-------------------------------------------
    //  プロパティ
    //-------------------------------------------
    public void SetFripY(bool frip){ fripY = frip; }
    public bool GetFripY(){ return fripY; }
    public void SetPosL(Vector3 pos){ posL = pos; }
    public Vector3 GetPosL(){ return posL; }
    public void SetPosR(Vector3 pos){ posR = pos; }
    public Vector3 GetPosR(){ return posR; }
    public void SetIsL(bool b){ isL = b; }
    public bool GetIsL(){ return isL; }

    //-------------------------------------------
    //  点滅演出
    //-------------------------------------------
    private IEnumerator Blink(int loop_count, float flash_interval)
    {
        //  ３秒前になるまで待つ
        yield return new WaitForSeconds(priod - 3.0f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        //点滅ループ開始
        for (int i = 0; i < loop_count; i++)
        {
             //  SE再生
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM2,
                (int)SFXList.SFX_KONBURST_TSUKUMO_TIMELIMIT);

            //flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //spriteRendererをオフ
            if(sr)sr.enabled = false;

            //flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //spriteRendererをオン
            if(sr)sr.enabled = true;
        }
    }

    //-------------------------------------------
    //  人形からホーミング弾を射出し続ける
    //-------------------------------------------
    private void HomingShot(bool flipY)
    {
        //  通常弾生成
        GameObject obj = Instantiate(
                            bulletPrefab,
                            transform.position,
                            Quaternion.identity);

        //  SpriteRendererを取得
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        //  Yを反転するかどうか設定する
        sr = obj.GetComponent<SpriteRenderer>();
        sr.flipY = flipY;
    }

    //-------------------------------------------
    //  弾を更新
    //-------------------------------------------
    private void UpdateBullet()
    {
        //  GameManagerから状態を取得
        gamestatus = GameManager.Instance.GetGameState();

        //  弾の向き
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                HomingShot(true);
                break;
            case (int)eGameState.Boss:
                HomingShot(false);
                break;
            case (int)eGameState.Event:
                break;
        }
    }
}
