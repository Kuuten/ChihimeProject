using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  イベントシーン01クラス
//
//--------------------------------------------------------------
public class EventScene01 : EventScene
{
    public override IEnumerator PlayEvent()
    {
        Debug.Log("***イベント01：ドウジ戦闘前を開始します***");


        //  BGMを止める
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);


        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  ボスを生成＆指定座標へ移動
        StartCoroutine(CreateBossAndMove(BossType.Douji, new Vector2(-1, 5.5f)));


        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.DOUJI_SURPRISED);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.DOUJI_ANGRY);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DODOME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_EXCITE);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());

        yield return ChangeFaceAndText(Frame.TOP, FaceType.DOUJI_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  ボス戦前イベントの終了処理を開始
        yield return StartCoroutine(EndBeforeBattle());


        Debug.Log("***イベント01：ドウジ戦闘前を完了しました***");
    }
}

