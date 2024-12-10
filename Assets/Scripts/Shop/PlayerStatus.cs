using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ショップシーンのプレイヤーステータスクラス
//
//--------------------------------------------------------------
//  魂以外の体力・ショット威力・スピード・ボムの数を表示する
//  体力のみ更新する

public class PlayerStatus : MonoBehaviour
{
    //  ショット威力アイコンの生成位置
    [SerializeField] private GameObject power_root;
    //  ショット威力アイコン
    [SerializeField] private GameObject power_icon;

    //  スピードアイコンの生成位置
    [SerializeField] private GameObject speed_root;
    //   スピードアイコン
    [SerializeField] private GameObject speed_icon;

    //  ボムアイコンの生成位置
    [SerializeField] private GameObject bomb_root;
    //   ボムアイコン
    [SerializeField] private GameObject bomb_icon;

    //  ハートアイコンの生成位置
    [SerializeField] private GameObject heart_root;
    //   ハートアイコン
    [SerializeField] private GameObject heart_icon;
    //  ハートフレームオブジェクトのリスト
    private List<GameObject> heartList = new List<GameObject>();
    //  ハートのタイプ
    enum HeartType
    {
        Half,   //  半分
        Full,   //  ハート１個分

        Max
    }

    void Start()
    {
        //  ショット威力
        for(int i=0; i<PlayerInfoManager.g_SHOT_LV;i++)
        {
            GameObject power = Instantiate(power_icon);
            power.transform.parent = power_root.transform;
        }

        //  スピード
        for(int i=0; i<PlayerInfoManager.g_SPEED_LV;i++)
        {
            GameObject speed = Instantiate(speed_icon);
            speed.transform.parent = speed_root.transform;
        }

        //  ボム
        for(int i=0; i<PlayerInfoManager.g_BOMBNUM;i++)
        {
            GameObject bomb = Instantiate(bomb_icon);
            bomb.transform.parent = bomb_root.transform;
        }

        //  ハート
        for(int i=0; i<PlayerInfoManager.g_CURRENTHP/2;i++)
        {
            GameObject heart = Instantiate(heart_icon);
            heart.transform.parent = heart_root.transform;

            heart.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            heart.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);


            heartList.Add( heart );   //  リストに追加
        }

    }

    void Update()
    {
        //  ハート画像を更新
        CalculateHealthUI(PlayerInfoManager.g_CURRENTHP);
    }

    //---------------------------------------------------
    //  現在体力を受け取って体力UIを計算する
    //---------------------------------------------------
    private void CalculateHealthUI(int health)
    {
        if(health < 0)
        {
            health = 0;
        }

            //  体力0ならハートを全部非表示にする
            if (health == 0)
            {
                for(int i=0;i<heartList.Count;i++)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }
            }
            else if(health == 1)
            {
                for(int i=0;i<heartList.Count;i++)
                {
                    if(i==0)
                    {
                        heartList[i].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                    }

                    //  残りを非表示にする
                    for(int j=1;j<heartList.Count;j++)
                    {
                        heartList[j].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                    }

                } 
            }
            else // 体力が２以上の時
            {
                //  一旦現在体力のとこまで全部フルで埋める
                int fullNum = health / 2;
                for(int i=0;i<fullNum;i++)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(true);
                }

                //  奇数だった場合は最後の番号だけハーフにする
                int taegetNum = health - fullNum;
                if(health % 2 != 0)
                {
                    heartList[taegetNum-1].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[taegetNum-1].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }

                //  残りを非表示にする
                for(int i=taegetNum;i<heartList.Count;i++)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }
            }
    }
}
