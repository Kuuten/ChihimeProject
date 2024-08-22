using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

//--------------------------------------------------------------
//
//  ドロップアイテムをプレイヤーに全回収
//
//--------------------------------------------------------------
public class ItemMove : MonoBehaviour
{
    private float flowSpeed = 0.005f;

    private MoneyManager moneyManager = null;
    private GameObject player;
    private bool bField;
    private float speed;
    private float acceleration;

    void Start()
    {
        moneyManager = GameObject.Find("MoneyManager").GetComponent<MoneyManager>();
        Assert.IsTrue(moneyManager,"MoneyManagerオブジェクトがありません！");

        bField = false;
        speed = 1.0f;
        acceleration = 1.0f;
    }

    void Update()
    {


        //  吸魂フィールドに触れない限りは常に流れる
        if(bField)MoveToPlayer();
        else Flowing();
    }

    //  魂バート弾時にプレイヤーに加速しながら移動
    public void MoveToPlayer()
    {
        player = GameManager.Instance.GetPlayer();

        //  プレイヤーの体力がなければリターン
        if(player == null)return;

        //  アイテムからプレイヤーへのベクトル
        Vector3 vec = player.transform.position - this.transform.position;
        float distance = vec.magnitude;
        vec.Normalize();

        Vector3 pos = this.transform.position;
        const float d = 0.01f;   //  近づく距離の閾値

        if(distance > d )
        {
            speed += acceleration;
            pos += vec * speed * Time.deltaTime;
        }

        this.transform.position = pos;
    }

    //  常に結界の中心（ボス側）にアイテムが流れる
    private void Flowing()
    {
        this.transform.position += new Vector3(0, flowSpeed, 0);
    }

    //----------------------------------------------------------------
    //  プロパティ
    //----------------------------------------------------------------
    public void SetFieldFlag(bool flag){ bField = flag; }
    public bool GetFieldFlag(){ return bField; }

    //-------------------------------------------------------
    //  吸魂フィールドと当たったらプレイヤーに加速する
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("DeadWall")) //　消滅壁に当たったら
        {
            Debug.Log("消滅壁に当たったのでアイテムを消滅させます");

            //  ゲームオブジェクトを削除
            Destroy(this.gameObject);
        }

        //  タグが吸魂フィールドなら吸引ON
        if(collision.CompareTag("PlayerField"))
        {
            Debug.Log("回収フィールド吸引ON");

            //  回収フィールド吸引ON
            bField = true;
        }
    }
}
