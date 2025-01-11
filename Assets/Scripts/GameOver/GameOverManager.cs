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
    //  PressAnyButton�p
    [SerializeField] GameObject pressAnyButton;
    private bool bInputFlag = false;
    
    //  ���͗p
    PlayerInput input;
    InputAction anykey;

    //  ���̑�
    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  ����
    [SerializeField] private GameObject soundManager;   //  SoundManager

    void Start()
    {
        // InputAction��AnyButton��ݒ�
        input = GetComponent<PlayerInput>();
        anykey = input.actions["AnyButton"];

        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap title_ui = input.actions.FindActionMap("TITLE_UI");

        //  StageIdManager���Ȃ����͐�������
        if (!GameObject.Find("SoundManager"))
        {
            Debug.Log("SoundManager���Ȃ��̂Ő������܂�");
            Instantiate(soundManager);
        }

        //  �������J�n
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        /* �`�`�`�`�`�`�`�`�`�`�`���o�̊J�n�`�`�`�`�`�`�`�`�`�`�` */

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �������J���A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2�b�҂�

        //  �Q�[���I�[�o�[BGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_GAMEOVER);



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
                
                //  ���������A�j���[�V����
                StartCoroutine(WaitingForClosingScroll());
            }
        }
    }

    //  �Q�[���I�[�o�[��BGM���~�߂�
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
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

    //  �������J���A�j���[�V�����̊�����҂�
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
    }

    //  ���������A�j���[�V�����̊�����҂�
    IEnumerator WaitingForClosingScroll()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1�b�҂�

        //  BGM���~�߂ă^�C�g����
        StopBGM();
        LoadingScene.Instance.LoadNextScene("Title");
    }
}
