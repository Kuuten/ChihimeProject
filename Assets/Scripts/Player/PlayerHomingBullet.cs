using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------
//
//  プレイヤーのホーミング弾
//
//---------------------------------------------------
public class PlayerHomingBullet : MonoBehaviour
{
    //  一番近い敵オブジェクト格納用
    GameObject target;
    //  ゲームの状態
    private int gamestatus;
    //
    private float speed = 5.0f;
    //  弾の寿命
    //[SerializeField] private float lifetime = 3;

    void Start()
    {
        target = null;

        //  ホーミング弾SE再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_NORMAL_SHOT,
            (int)SFXList.SFX_NORMAL_SHOT);
    }

    void Update()
    {
        //  GameManagerから状態を取得
        gamestatus = GameManager.Instance.GetGameState();

        //  必中ホーミング
        Homing();
    }

    //-----------------------------------------------------------
    //  必中ホーミング(当たるまで追いかけ続ける)
    //-----------------------------------------------------------
    private void Homing()
    {
        //  一番近い敵オブジェクトを取得する
        if(gamestatus == (int)eGameState.Zako)
        {
            target = EnemyManager.Instance.GetNearestEnemyFromPlayer();
        }
        else if(gamestatus == (int)eGameState.Boss)
        {
            target = EventSceneManager.Instance.GetBossObject();
        }else if(gamestatus == (int)eGameState.Event)
        {
            Destroy(this.gameObject);
            return;
        }

        //  敵がいなければ弾を消去して戻る
        if(target == null)
        {
            //Debug.Log("敵がいない！");
            Destroy(this.gameObject);
            return;
        }

        //  弾から敵へのベクトルを求める
        Vector3 vec = target.transform.position - this.transform.position;

        //  ベクトルのオイラー角を求める
        Quaternion q = Quaternion.Euler(vec);
        float degree = q.eulerAngles.z;

        //  弾の向きを敵に向ける
        this.transform.Rotate(0,0,degree);

        //  一番近い敵へ突撃
        this.transform.position += vec * speed * Time.deltaTime;
    }
}
