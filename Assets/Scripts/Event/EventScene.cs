using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using DG.Tweening;

//--------------------------------------------------------------
//
//  イベントシーンの基本クラス
//
//--------------------------------------------------------------
public abstract class EventScene : MonoBehaviour
{
    //------------------------------------------------------------
    //  イベントを開始する
    //------------------------------------------------------------
    public abstract IEnumerator PlayEvent();

    //-------------------------------------------------------------
    //  プロパティ
    //-------------------------------------------------------------


    //-------------------------------------------------------------
    //  顔UIとテキストを変更する
    //-------------------------------------------------------------
    protected IEnumerator ChangeFaceAndText(Frame frame,FaceType type)
    {
        //  イベントキャンバスが有効化されてなければ有効化する
        if(!EventSceneManager.Instance.GetEventCanvasActive())
        {
            EventSceneManager.Instance.SetEventCanvasActive(true);
        }

        //  指定フレームが有効化されてなければ有効化する
        if(!EventSceneManager.Instance.GetFrameObjectActive(frame))
        {
            EventSceneManager.Instance.SetFrameObjectActive(frame, true);
        }

        //  顔UIをプレハブを指定して変更
        EventSceneManager.Instance.SetFaceToFrame(frame,type);

        //  テキストに引数を指定
        string gameText = EventSceneManager.Instance.GetGameText();

        EventSceneManager.Instance.SetTextToFrame(frame,gameText);

        yield return null;
    }

    //-------------------------------------------------------------
    //  ボスを生成して指定の座標に移動させる
    //-------------------------------------------------------------
    protected IEnumerator CreateBossAndMove(BossType type,Vector2 target)
    {
        float duration = 1.0f;

        //  ボスを生成する
        Vector2 pos = new Vector2(-1,11);   //  初期位置
        EventSceneManager.Instance.InstantiateBossPrefab(type,pos);

        //  ボス情報セット
        EnemyManager.Instance.SetBoss(type, ePowerupItems.PowerUp);

        //  ボスのタイプによってコンポーネントを無効化
        GameObject bossObject = EventSceneManager.Instance.GetBossObject();
        if(type == BossType.Douji)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Tsukumo)
        {
            bossObject.GetComponent<BossTsukumo>().enabled = false;
        }
        else if(type == BossType.Kuchinawa)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Kurama)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Wadatsumi)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Hakumen)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        bossObject.GetComponent<BoxCollider2D>().enabled = false;

        //  目標座標に向かって移動開始
        bossObject.GetComponent<RectTransform>().DOAnchorPos(target,duration);

        //  ３秒待つ
        yield return new WaitForSeconds(duration);
    }

    //-------------------------------------------------------------
    //  ボスの名前をアルファアニメさせる
    //-------------------------------------------------------------
    protected IEnumerator AlphaAnimationBossName()
    {
        float duration = 5.0f;  //  フェードインにかかる時間
        float duration2 = 2.0f; //  フェードアウトにかかる時間

        //  表示する内容を表示
        string[] boss_name =
        {
            "剛魔天 ドウジ",
            "操魔天 ツクモ",
            "輪魔天 クチナワ",
            "楓魔天 クラマ",
            "海魔天 ワダツミ",
            "焔魔天 ハクメン",
        };

        //******************************************************
        //  今のステージによってボスの名前を設定
        //******************************************************

        //  テキストを有効化
        EventSceneManager.Instance.SetBossNameTextActive(true);

        //  ボスの名前を設定
        EventSceneManager.Instance.SetBossName(boss_name[(int)PlayerInfoManager.stageInfo]);

        //  テキストのアルファを0にする
        EventSceneManager.Instance.GetBossNameTextObj()
            .GetComponent<TextMeshProUGUI>().DOFade(0f,0f);

        yield return null;

        //  透明状態からゆっくり出現する
         EventSceneManager.Instance.GetBossNameTextObj()
            .GetComponent<TextMeshProUGUI>().DOFade(1f,duration)
                .OnComplete
                (
                    ()=>EventSceneManager.Instance.GetBossNameTextObj()
                        .GetComponent<TextMeshProUGUI>().DOFade(0f, duration2)
                )
                .SetEase(Ease.Linear);
    }

    //-------------------------------------------------------------
    //  ボス戦前イベントの終了処理を開始
    //-------------------------------------------------------------
    protected IEnumerator EndBeforeBattle()
    {
        //  キャンバスをOFF
        EventSceneManager.Instance.SetEventCanvasActive(false);

        //  左右の障気オブジェクトを有効化
        EventSceneManager.Instance.SetFogObjectActiveL(true);
        EventSceneManager.Instance.SetFogObjectActiveR(true);

        //  夜用の背景オブジェクトを有効化
        EventSceneManager.Instance.SetBossBackGroundObj(true);

        //  ボス登場SE再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_BOSS_APPEAR);

        //  ボスの名前をアルファアニメさせる
        StartCoroutine(AlphaAnimationBossName());

        //  ７秒待つ
        yield return new WaitForSeconds(7);

        //  ボスモードへ移行
        GameManager.Instance.SetGameState((int)eGameState.Boss);

        //  ボスコンポーネントを無効化
        GameObject boss = EventSceneManager.Instance.GetBossObject();
        if(boss.GetComponent<BossDouji>())boss.GetComponent<BossDouji>().enabled = false;
        else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        
        Debug.Log("***ボス戦モードになりました。***");

        //  ボス戦開始フラグTRUE
        EventSceneManager.Instance.SetStartBoss(true);

        //  イベントシーンマネージャーを無効化
        this.gameObject.SetActive(false);
    }

    //-------------------------------------------------------------
    //  ボス戦後イベントの終了処理を開始
    //-------------------------------------------------------------
    protected IEnumerator EndAfterBattle()
    {
        //  フレームオブジェクトを非表示にする
        EventSceneManager.Instance.SetFrameObjectActive(Frame.TOP,false);
        EventSceneManager.Instance.SetFrameObjectActive(Frame.BOTTOM,false);

        //  Pauserが付いたオブジェクトをポーズ
        Pauser.Pause();

        //  プレイヤーのAnimatorを無効化
        GameManager.Instance.GetPlayer().GetComponent<Animator>().enabled = false;

        //  結果表示開始フラグTRUE
        EventSceneManager.Instance.SetStartResult(true);

        yield return null;
    }
}

