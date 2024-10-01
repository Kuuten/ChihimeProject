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

//--------------------------------------------------------------
//
//  �^�C�g���Ǘ��N���X
//
//--------------------------------------------------------------
public class TitleManager : MonoBehaviour
{
    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    [SerializeField] private LogoEasing logoEasing;
    [SerializeField] private LogoScaling logoScaling;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject[] DisableObject;

    private int Pos = 1;                    //  ��ԏ�
    private  int menuNum = 3;               //  ���j���[�̐�
    private float lineHeight = 111.7f;      //  1��ŏ㉺�ɓ�����

    PlayerInput _input;
    InputAction nevigate;
    float verticalInput;

    bool canContorol;   //  ����\�t���O

    enum TitleMode
    {
        Normal,
        Config,

        Max
    }
    TitleMode titleMode;

    //[SerializeField] private EventSystem eventSystem;

    //--------------------------------------------------------------
    //  Config
    //--------------------------------------------------------------
    [SerializeField] private GameObject ConfigPanel;
    [SerializeField] private GameObject KeyConfigButton;

    void Start()
    {
        //  �ŏ��͒ʏ탂�[�h
        titleMode = TitleMode.Normal;

        canContorol = false;

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

        //  �^�C�g�����S�C�[�Y�C��
        yield return StartCoroutine(WaitingForEasingTitlelogo());

       yield return new WaitForSeconds(1); //  1�b�҂�

        //  �^�C�g�����S�X�P�[�����OON
        logoScaling.enabled = true;

        //  �{�^����\��
        Buttons.SetActive(true);

        //  ����\�ɂ���
        canContorol = true;

        //  �^�C�g��BGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TITLE);

        yield return null;
    }

    void Update()
    {
        //  ����s�\�Ȃ烊�^�[��
        if(!canContorol)return;

        switch(titleMode)
        {
            case TitleMode.Normal:  //  �ʏ탂�[�h
                MoveTitleMenuCursor();  //  �J�[�\���ړ�
                break;

            case TitleMode.Config:  //  �R���t�B�O���[�h
                break;
        }
        

    }

    //----------------------------------------------------------------
    //  �A�N�V�����}�b�v�؂�ւ��p�i���ݐ؂�ւ���\��Ȃ��j
    //----------------------------------------------------------------
    private void OnEnable()
    {
        // InputAction��Navigate��ݒ�
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

        //  BGM���~�߂ă��C����
        StopBGM();
        LoadingScene.Instance.LoadNextScene("SelectConvert");
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

        //  �L�[�R���t�B�O�{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(KeyConfigButton.gameObject);


        //  ���s���[�h���R���t�B�O���[�h�ɂ���
        titleMode = TitleMode.Config;
    }

     //  �Q�[���J�n�������ꂽ���̏���
    public void OnPressedStart()
    {
        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        StartCoroutine( WaitingForClosingScroll() );
    }

     //  �R���t�B�O��ʂŃZ�[�u���Ė߂邪�����ꂽ���̏���
    public void OnPressedSave()
    {
        //  �w�i�ȊO���A�N�e�B�u�ɂ���
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  �R���t�B�O�p�l�����A�N�e�B�u�ɂ���
        ConfigPanel.SetActive(false);

        //  �X�^�[�g�{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(0).gameObject);

        //  ���s���[�h��ʏ탂�[�h�ɂ���
        titleMode = TitleMode.Normal;
    }

     //  �R���t�B�O��ʂł��ǂ邪�����ꂽ���̏���
    public void OnPressedCancel()
    {
        //  �w�i�ȊO���A�N�e�B�u�ɂ���
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  �R���t�B�O�p�l�����A�N�e�B�u�ɂ���
        ConfigPanel.SetActive(false);

        //  �X�^�[�g�{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(0).gameObject);

        //  ���s���[�h��ʏ탂�[�h�ɂ���
        titleMode = TitleMode.Normal;
    }

    //  �I���������ꂽ���̏���
    public void OnPressedExit()
    {
#if UNITY_EDITOR
        //�Q�[���v���C�I��
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //�Q�[���v���C�I��
        Application.Quit();
#endif
    }

    //---------------------------------------------------
    //  �^�C�g����ʂł̃J�[�\���ړ�
    //---------------------------------------------------
    private void MoveTitleMenuCursor()
    {
        UnityEngine.Vector2 inputNavigateAxis = nevigate.ReadValue<UnityEngine.Vector2>();
        verticalInput = inputNavigateAxis.y;

        //  ���͂��Ȃ��ꍇ�͒e��
        if(!nevigate.WasPressedThisFrame())return;
        else
        {
            //  �Z���N�gSE�Đ�
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
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

    //  �L�[�R���t�B�O�{�^���������ꂽ��
    public void OnPressedKeyConfig()
    {
    
    }

    //  �T�E���h�R���t�B�O�{�^���������ꂽ��
    public void OnPressedSoundConfig()
    {
    
    }

}
