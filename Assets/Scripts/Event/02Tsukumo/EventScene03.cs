using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  イベントシーン03クラス
//
//--------------------------------------------------------------
public class EventScene03 : EventScene
{
    public override IEnumerator PlayEvent()
    {
        Debug.Log("***イベント03：ツクモ戦闘前を開始します***");


        //  BGMを止める
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);


        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_CONFUSE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_CONFUSE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  ボスを生成＆指定座標へ移動
        StartCoroutine(CreateBossAndMove(BossType.Tsukumo, new Vector2(-1, 5.5f)));


        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_CLOSEEYE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_ANGRY);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_SMILE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  ボス戦前イベントの終了処理を開始
        yield return StartCoroutine(EndBeforeBattle());


        Debug.Log("***イベント03：ツクモ戦闘前を完了しました***");
    }
}

