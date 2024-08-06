using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[���Ǘ��N���X
//
//--------------------------------------------------------------
// ���ǂ̃V�[������ł��A�N�Z�X�ł���N���X��
public static class PlayerInfoManager
{
    // �ǂ̃V�[������ł��A�N�Z�X�ł���ϐ�
    public static int g_MAXHP = 2 * 3;      //  �K������
    public static int g_CURRENTHP = 2 * 3;
    public static int g_KONNUM = 0;
    public static int g_BOMBNUM = 0;
    //  ���ʏ�e���Z�b�g���Ȃ��悤�ɋC������
    public static SHOT_TYPE g_CONVERTSHOT = SHOT_TYPE.DOUJI;

    //  �v���C���[��������X�e�[�W���
    public enum StageInfo
    {
        Stage01,
        Stage02,
        Stage03,
        Stage04,
        Stage05,
        Stage06,

        Max
    }
    public static StageInfo stageInfo = StageInfo.Stage01;

    //  �����ꊇ�ŃZ�b�g
    public static void SetInfo(int maxHp,int currentHp,int konNum,int bombNum,SHOT_TYPE shotType)
    {
        g_MAXHP = maxHp;
        g_CURRENTHP = maxHp;
        g_KONNUM = maxHp;
        g_BOMBNUM = maxHp;
        g_CONVERTSHOT = shotType;
    }
}
