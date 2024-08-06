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
    // どのシーンからでもアクセスできる変数
    public static int g_MAXHP = 2 * 3;      //  必ず偶数
    public static int g_CURRENTHP = 2 * 3;
    public static int g_KONNUM = 0;
    public static int g_BOMBNUM = 0;
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
    public static StageInfo stageInfo = StageInfo.Stage01;

    //  情報を一括でセット
    public static void SetInfo(int maxHp,int currentHp,int konNum,int bombNum,SHOT_TYPE shotType)
    {
        g_MAXHP = maxHp;
        g_CURRENTHP = maxHp;
        g_KONNUM = maxHp;
        g_BOMBNUM = maxHp;
        g_CONVERTSHOT = shotType;
    }
}
