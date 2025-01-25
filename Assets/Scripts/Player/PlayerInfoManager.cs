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
    //  �f�t�H���g�l�萔
    private static readonly int MAXHP = 2 * 3;      //  �K������
    private static readonly int KONNUM = 0;
    private static readonly int BOMBNUM = 2;
    private static readonly int SHOT_LV = 1;        // �ŏ��̓��x���P
    private static readonly int SPEED_LV = 0;       // 0�`2
    private static readonly bool IS_SHIELD = false; // �ŏ��̓V�[���h�Ȃ�

    // �ǂ̃V�[������ł��A�N�Z�X�ł���ϐ�
    public static int g_MAXHP = MAXHP;      //  �K������
    public static int g_CURRENTHP = MAXHP;
    public static int g_KONNUM = KONNUM;
    public static int g_BOMBNUM = BOMBNUM;
    public static int g_SHOT_LV = SHOT_LV;
    public static int g_SPEED_LV = SPEED_LV;
    public static bool g_IS_SHIELD = IS_SHIELD;

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
    public static StageInfo stageInfo = StageInfo.Stage02;

    //  �����ꊇ�ŃZ�b�g
    public static void SetInfo(int maxHp,int currentHp,int konNum,int bombNum, int shotLv, int speedLv, bool isShield)
    {
        g_MAXHP = maxHp;
        g_CURRENTHP = maxHp;
        g_KONNUM = konNum;
        g_BOMBNUM = bombNum;
        g_SHOT_LV = shotLv;
        g_SPEED_LV = speedLv;
        g_IS_SHIELD = isShield;
    }

    //  �����ꊇ�Ń��Z�b�g
    public static void ResetInfo()
    {
        //  �����l�Ń��Z�b�g
        SetInfo(MAXHP, MAXHP, KONNUM, BOMBNUM, SHOT_LV, SPEED_LV, IS_SHIELD);

        //  ���o�[�g���ꉞ���Z�b�g
        g_CONVERTSHOT = SHOT_TYPE.DOUJI;

        //  �X�e�[�W�������Z�b�g
        stageInfo = StageInfo.Stage01;
    }

    //  �����ꊇ�Ń��Z�b�g(�X�e�[�WNo�ȊO)
    public static void ResetInfoStage()
    {
        //  �����l�Ń��Z�b�g
        SetInfo(MAXHP, MAXHP, KONNUM, BOMBNUM, SHOT_LV, SPEED_LV, IS_SHIELD);
    }
}
