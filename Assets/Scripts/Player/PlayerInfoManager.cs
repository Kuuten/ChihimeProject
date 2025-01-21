using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤー情報管理クラス
//
//--------------------------------------------------------------
// ★どのシーンからでもアクセスできるクラス★
public static class PlayerInfoManager
{
    //  デフォルト値定数
    private static readonly int MAXHP = 2 * 3;      //  必ず偶数
    private static readonly int KONNUM = 0;
    private static readonly int BOMBNUM = 2;
    private static readonly int SHOT_LV = 1;        // 最初はレベル１
    private static readonly int SPEED_LV = 0;       // 0～2
    private static readonly bool IS_SHIELD = false; // 最初はシールドなし

    // どのシーンからでもアクセスできる変数
    public static int g_MAXHP = MAXHP;      //  必ず偶数
    public static int g_CURRENTHP = MAXHP;
    public static int g_KONNUM = KONNUM;
    public static int g_BOMBNUM = BOMBNUM;
    public static int g_SHOT_LV = SHOT_LV;
    public static int g_SPEED_LV = SPEED_LV;
    public static bool g_IS_SHIELD = IS_SHIELD;

    //  ※通常弾をセットしないように気をつける
    public static SHOT_TYPE g_CONVERTSHOT = SHOT_TYPE.DOUJI;

    //  プレイヤーが今いるステージ情報
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

    //  情報を一括でセット
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

    //  情報を一括でリセット
    public static void ResetInfo()
    {
        //  初期値でリセット
        SetInfo(MAXHP, MAXHP, KONNUM, BOMBNUM, SHOT_LV, SPEED_LV, IS_SHIELD);

        //  魂バートも一応リセット
        g_CONVERTSHOT = SHOT_TYPE.DOUJI;

        //  ステージ情報もリセット
        stageInfo = StageInfo.Stage01;
    }

    //  情報を一括でリセット(ステージNo以外)
    public static void ResetInfoStage()
    {
        //  初期値でリセット
        SetInfo(MAXHP, MAXHP, KONNUM, BOMBNUM, SHOT_LV, SPEED_LV, IS_SHIELD);
    }
}
