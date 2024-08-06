using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーのボム管理クラス
//
//--------------------------------------------------------------
public class PlayerBombManager : MonoBehaviour
{
    //  BOMBに表示されるテキスト
    [SerializeField] private TextMeshProUGUI bombText;
    private int bombNum;
    private const int bombMaxNum = 9;

    void Start()
    {
        bombNum = 3;
    }

    void Update()
    {
        //  ボムのテキストを更新
        bombText.text = $"{bombNum}";
    }

    //  ボムを加算
    public void AddBomb()
    {
        if(bombNum < bombMaxNum)
        {
            bombNum++;
        }
        else bombNum = bombMaxNum;
    }

    //  ボムを減算
    public void SubBomb()
    {
        if(bombNum > 0)
        {
            bombNum--;
        }
        else bombNum = 0;
    }

    //------------------------------------------------
    //  プロパティ
    //------------------------------------------------
    public int GetBombNum(){ return bombNum; }
    public int GetBombMaxNum(){ return bombMaxNum; }
}
