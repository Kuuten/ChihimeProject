using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  イベントシーン04クラス
//
//--------------------------------------------------------------
public class EventScene04 : EventScene
{
    public override IEnumerator PlayEvent()
    {
        Debug.Log("***イベント04：ツクモ戦闘後を開始します***");


        //  BGMを止める
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);


        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_SMILE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_CLOSEEYE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.HONEG_CONFUSE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.TSUKUMO_SMILE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  ボス戦後イベントの終了処理を開始
        yield return StartCoroutine(EndAfterBattle());


        Debug.Log("***イベント05：ツクモ戦闘後を完了しました***");
    }
}

