using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

//--------------------------------------------------------------
//
//  �^�C�g���Ǘ��N���X
//
//--------------------------------------------------------------
public class TitleManager : MonoBehaviour
{
    [SerializeField] private SoundManager Sound;
    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;
    [SerializeField] private LogoEasing logoEasing;
    [SerializeField] private LogoScaling logoScaling;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject ConfigPanel;
    [SerializeField] private GameObject[] DisableObject;

    private int Pos = 1;                    //  ��ԏ�
    private  int menuNum = 3;               //  ���j���[�̐�
    private float lineHeight = 111.7f;      //  1��ŏ㉺�ɓ�����

    PlayerInput _input;
    InputAction nevigate;
    float verticalInput;

    IEnumerator Start()
    {
        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �����A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �^�C�g�����S�C�[�Y�C��
        yield return StartCoroutine(WaitingForEasingTitlelogo());

       yield return new WaitForSeconds(1); //  1�b�҂�

        //  �^�C�g�����S�X�P�[�����OON
        logoScaling.enabled = true;

        //  �{�^����\��
        Buttons.SetActive(true);

        //  �^�C�g��BGM�Đ�
        Sound.Play((int)AudioChannel.MUSIC, (int)MusicList.BGM_TITLE);

        yield return null;
    }

    void Update()
    {
        UnityEngine.Vector2 inputNavigateAxis = nevigate.ReadValue<UnityEngine.Vector2>();
        verticalInput = inputNavigateAxis.y;

        //  ���͂��Ȃ��ꍇ�͒e��
        if(!nevigate.WasPressedThisFrame())return;
        else
        {
            //  �Z���N�gBGM�Đ�
            Sound.Play((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        if (verticalInput < 0 && nevigate.WasPressedThisFrame())
        {
            if (Pos != menuNum)
            {
                Cursor.GetComponent<RectTransform>().position += 
                new UnityEngine.Vector3(0,-lineHeight,0);
                Pos += 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().position += 
                new UnityEngine.Vector3(0,lineHeight*(menuNum-1),0);
                Pos = 1;
            }
            
        }
        else if (verticalInput > 0 && nevigate.WasPressedThisFrame())
        {
            if (Pos != 1)
            {
                Cursor.GetComponent<RectTransform>().position +=
                    new UnityEngine.Vector3(0, lineHeight, 0);
                Pos -= 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().position +=
                new UnityEngine.Vector3(0, -lineHeight * (menuNum - 1), 0);
                Pos = menuNum;
            }
        }

    }

    //----------------------------------------------------------------
    //  �A�N�V�����}�b�v�؂�ւ��p�i���ݐ؂�ւ���\��Ȃ��j
    //----------------------------------------------------------------
    private void OnEnable()
    {
        // InputAction��Move��ݒ�
        _input = GetComponent<PlayerInput>();
        nevigate = _input.actions["Navigate"];

        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap player = _input.actions.FindActionMap("Player");
        InputActionMap title_ui = _input.actions.FindActionMap("TITLE_UI");

        //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
        player["ModeChange"].started += ToTitleUIMode;
    }

    private void OnDisable()
    {
        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap player = _input.actions.FindActionMap("Player");
        InputActionMap title_ui = _input.actions.FindActionMap("TITLE_UI");

        //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
        player["ModeChange"].started -= ToTitleUIMode;
    }

    //  �A�N�V�����}�b�v��Title_UI�ɐ؂�ւ���
    private void ToTitleUIMode(InputAction.CallbackContext context)
    {
        _input.SwitchCurrentActionMap("Title_UI");
    }

    //  �A�N�V�����}�b�v��Player�ɐ؂�ւ���
    private void ToPlayerMode(InputAction.CallbackContext context)
    {
        _input.SwitchCurrentActionMap("Player");
    }

    //  �^�C�g����BGM���~�߂�
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

    //  �^�C�g�����S�̃C�[�Y�C����҂�
    IEnumerator WaitingForEasingTitlelogo()
    {
        yield return StartCoroutine(logoEasing.EasingTitlelogo());
    }

    //  �R���t�B�O�������ꂽ���̏���
    public void OnPressedConfig()
    {
        //  �w�i�ȊO���A�N�e�B�u�ɂ���
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(false);
        }

        //  �R���t�B�O�p�l�����A�N�e�B�u�ɂ���
        ConfigPanel.SetActive(true);
    }

     //  �R���t�B�O��ʂł��ǂ邪�����ꂽ���̏���
    public void OnPressedBack()
    {
        //  �w�i�ȊO���A�N�e�B�u�ɂ���
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  �R���t�B�O�p�l�����A�N�e�B�u�ɂ���
        ConfigPanel.SetActive(false);
    }
}
