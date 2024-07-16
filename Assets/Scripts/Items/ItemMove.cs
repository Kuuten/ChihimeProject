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
    [SerializeField] private GameObject player;

    void Start()
    {
        moneyManager = GameObject.Find("MoneyManager").GetComponent<MoneyManager>();
        Assert.IsTrue(moneyManager,"MoneyManagerオブジェクトがありません！");
    }

    void Update()
    {
        //  吸魂フィールドに触れない限りは常に流れる
        Flowing();
    }

    //  魂バート弾時にプレイヤーに加速しながら移動
    public void MoveToPlayer()
    {
        //  アイテムからプレイヤーへのベクトル
        Vector3 vec = player.transform.position - this.transform.position;
        vec.Normalize();

        Vector3 pos = this.transform.position;
        float speed = 1.0f;
        pos += vec * speed * Time.deltaTime;

        this.transform.position = pos;
    }

    //  常に結界の中心（ボス側）にアイテムが流れる
    private void Flowing()
    {
        this.transform.position += new Vector3(0, flowSpeed, 0);
    }
}
