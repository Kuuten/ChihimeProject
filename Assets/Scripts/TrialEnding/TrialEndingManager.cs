using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  �̌��ŏI����ʊǗ��N���X
//
//--------------------------------------------------------------
public class TrialEndingManager : MonoBehaviour
{
    PlayerInput _input;
    private InputAction submit;

    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    bool canCommand;  //  ����\�t���O

    void Start()
    {
        _input = GetComponent<PlayerInput>();
        _input.SwitchCurrentActionMap("Title_UI");

        submit = _input.actions["Submit"];

        canCommand = false;

        //  ���o�J�n
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �����A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2�b�҂�

        //  ����\�I
        canCommand = true;
    }

    void Update()
    {
        //  ����{�^������������
        if (submit.WasPressedThisFrame() && canCommand)
        {
            //  ����s�\�Ƀ��Z�b�g
            canCommand = false;

            //  ���݃X�e�[�W�����Z�b�g
            PlayerInfoManager.stageInfo = PlayerInfoManager.StageInfo.Stage01;

           //   �^�C�g���V�[����
           StartCoroutine(WaitingForClosingScroll());
        }
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

        //   �^�C�g���V�[����
        LoadingScene.Instance.LoadNextScene("Title"); 
    }
}
