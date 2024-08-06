using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  �Q�[���I�[�o�[�V�[���̊Ǘ��N���X
//
//--------------------------------------------------------------
public class GameOverManager : MonoBehaviour
{
    //  �摜�v���n�u�z��
    [SerializeField] GameObject[] gameOverImage;

    //  PressAnyButton�p
    [SerializeField] GameObject pressAnyButton;
    private bool bInputFlag = false;
    
    //  ���͗p
    PlayerInput input;
    InputAction anykey;

    //  ���̑�
    [SerializeField] private SoundManager Sound;        //  SoundManager
    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  ����

    IEnumerator Start()
    {
        //  �z��`�F�b�N
        if(gameOverImage.Length > (int)PlayerInfoManager.StageInfo.Max)
            Debug.LogError("�摜�v���n�u�̔z��̐����I�[�o�[���Ă��܂��I");

        //  ���݂̃X�e�[�W�ԍ��ɂ������摜�𐶐�
        int num = (int)PlayerInfoManager.stageInfo;
        Instantiate(gameOverImage[num]);

        // InputAction��AnyButton��ݒ�
        input = GetComponent<PlayerInput>();
        anykey = input.actions["AnyButton"];

        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap title_ui = input.actions.FindActionMap("TITLE_UI");

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̊J�n�`�`�`�`�`�`�`�`�`�`�` */

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �����A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        //  �Q�[���I�[�o�[BGM�Đ�
        if(Sound == null)
        {
            Sound = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        }
        Sound.Play((int)AudioChannel.MUSIC, (int)MusicList.BGM_GAMEOVER);

        yield return new WaitForSeconds(3); //  3�b�҂�

        //  ���͉\�ɂ���
        bInputFlag = true;

        //  PressAnyButton��\��
        pressAnyButton.SetActive(true);

    }

    void Update()
    {
        if(bInputFlag)
        {
            //  �ǂꂩ�{�^������������
            if(anykey.triggered)
            {
                bInputFlag = false;
                //  BGM���~�߂ă^�C�g����
                StopBGM();
                LoadingScene.Instance.LoadNextScene("Title");
            }
        }
    }

    //  �Q�[���I�[�o�[��BGM���~�߂�
    public void StopBGM()
    {
        Sound.Stop((int)AudioChannel.MUSIC);
    }

    // �t�F�[�h�C���̊�����҂�
    IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());
    }

    // �t�F�[�h�A�E�g�̊�����҂�
    IEnumerator WaitingForFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut());
    }

    //  �����A�j���[�V�����̊�����҂�
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
    }
}
