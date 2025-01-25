using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �C�x���g�V�[��01�N���X
//
//--------------------------------------------------------------
public class EventScene01 : EventScene
{
    public override IEnumerator PlayEvent()
    {
        Debug.Log("***�C�x���g01�F�h�E�W�퓬�O���J�n���܂�***");


        //  BGM���~�߂�
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);


        yield return ChangeFaceAndText(Frame.BOTTOM, FaceType.CHIHIME_NORMAL);
        yield return new WaitUntil(() => EventSceneManager.Instance.GetTextNextInput());


        //  �{�X�𐶐����w����W�ֈړ�
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


        //  �{�X��O�C�x���g�̏I���������J�n
        yield return StartCoroutine(EndBeforeBattle());


        Debug.Log("***�C�x���g01�F�h�E�W�퓬�O���������܂���***");
    }
}

