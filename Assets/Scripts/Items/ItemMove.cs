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
    [SerializeField] private Items itemType;
    private float flowSpeed = 0.005f;

    private SHOT_TYPE shotType;
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
        //  アイテムからプレイヤーへのベクトル
        player = GameManager.Instance.GetPlayer();
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

    //-------------------------------------------------------
    //  吸魂フィールドと当たったらプレイヤーに加速する
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  タグが吸魂フィールド以外ならreturn
        if(!collision.CompareTag("PlayerField"))return;

        //  回収フィールド吸引ON
        bField = true;
    }
}
