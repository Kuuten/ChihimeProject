using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[�̃{���Ǘ��N���X
//
//--------------------------------------------------------------
public class PlayerBombManager : MonoBehaviour
{
    //  BOMB�ɕ\�������e�L�X�g
    [SerializeField] private TextMeshProUGUI bombText;
    private int bombNum;
    private const int bombMaxNum = 9;

    void Start()
    {
        bombNum = 3;
    }

    void Update()
    {
        //  �{���̃e�L�X�g���X�V
        bombText.text = $"{bombNum}";
    }

    //  �{�������Z
    public void AddBomb()
    {
        if(bombNum < bombMaxNum)
        {
            bombNum++;
        }
        else bombNum = bombMaxNum;
    }

    //  �{�������Z
    public void SubBomb()
    {
        if(bombNum > 0)
        {
            bombNum--;
        }
        else bombNum = 0;
    }

    //------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------
    public int GetBombNum(){ return bombNum; }
    public int GetBombMaxNum(){ return bombMaxNum; }
}
