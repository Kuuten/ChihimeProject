using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �C�x���g�V�[��12�N���X
//
//--------------------------------------------------------------
public class EventScene12 : EventScene
{
    public override IEnumerator PlayEvent()
    {
        Debug.Log("***�C�x���g12�F�n�N�����퓬����J�n���܂�***");


        //  BGM���~�߂�
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


        //  �{�X���C�x���g�̏I���������J�n
        yield return StartCoroutine(EndAfterBattle());


        Debug.Log("***�C�x���g12�F�n�N�����퓬����������܂���***");
    }
}

