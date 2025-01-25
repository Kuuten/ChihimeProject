using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  イベントシーン01クラス
//
//--------------------------------------------------------------
public class EventScene02 : EventScene
{
    public override IEnumerator PlayEvent()
    {
        Debug.Log("***イベント02：ドウジ戦闘後を開始します***");


        //  BGMを止める
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);


        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_ANGRY);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_ANGRY);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  ボス戦後イベントの終了処理を開始
        yield return StartCoroutine(EndAfterBattle());


        Debug.Log("***イベント02：ドウジ戦闘後を完了しました***");
    }
}

