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
    float horizontalInput;

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
    [SerializeField] private GameObject ConfigCanvas;

    //  �R���t�B�O�p�l��
    [SerializeField] private GameObject ConfigPanel;

    //  �R���t�B�O�p�l���̎q�I�u�W�F�N�g�ꗗ
    enum eConfigPanel
    {
        WaitingPanel,
        KeyConfigButton,
        SoundConfigButton,
        SaveButton,
        CancelButton,
    }

    //  �L�[�R���t�B�O�p�l��
    [SerializeField] private GameObject KeyConfigPanel;
    //  �T�E���h�R���t�B�O�p�l��
    [SerializeField] private GameObject SoundConfigPanel;

    //  �L�[�{�[�h�̏㉺���E�����\��
    [SerializeField] private GameObject keyboardNavigateTextObj;
    //  �Q�[���p�b�h�̃{�^���Z�b�g
    [SerializeField] private GameObject gamepadButtonSetObj;
    //  �L�[�{�[�h�̃{�^���Z�b�g
    [SerializeField] private GameObject keyboardButtonSetObj;


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
                MoveConfigMenuCursor();  //  �J�[�\���ړ�
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
        player["Pause"].started += ToTitleUIMode;
    }

    private void OnDisable()
    {
        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap player = _input.actions.FindActionMap("Player");
        InputActionMap title_ui = _input.actions.FindActionMap("TITLE_UI");

        //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
        player["Pause"].started -= ToTitleUIMode;
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

        //  �R���t�B�O�L�����o�X���A�N�e�B�u�ɂ���
        ConfigCanvas.SetActive(true);

        //  ���s���[�h���R���t�B�O���[�h�ɂ���
        titleMode = TitleMode.Config;

        //----------------------------------------------------------------
        //  �ڑ�����Ă���f�o�C�X�ɂ���ĕ\����ς���
        //----------------------------------------------------------------
        if(_input.currentControlScheme == "Gamepad")
        {
            keyboardNavigateTextObj.SetActive(false);
            gamepadButtonSetObj.SetActive(true);
            keyboardButtonSetObj.SetActive(false);

            //  �ŏ��̃{�^����I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(
                gamepadButtonSetObj.transform.GetChild(0).gameObject);
            
        }
        else if(_input.currentControlScheme == "Keyboard")
        {
            keyboardNavigateTextObj.SetActive(true);
            gamepadButtonSetObj.SetActive(false);
            keyboardButtonSetObj.SetActive(true);

            //  �ŏ��̃{�^����I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(
                keyboardButtonSetObj.transform.GetChild(0).gameObject);
        }
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
        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  �w�i�ȊO���A�N�e�B�u�ɂ���
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  �R���t�B�O�L�����o�X���A�N�e�B�u�ɂ���
        ConfigCanvas.SetActive(false);

        //  �R���t�B�O�{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(
            Buttons.transform.GetChild(1).gameObject);

        //  ���s���[�h��ʏ탂�[�h�ɂ���
        titleMode = TitleMode.Normal;
    }

     //  �R���t�B�O��ʂł��ǂ邪�����ꂽ���̏���
    public void OnPressedCancel()
    {
        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  �w�i�ȊO���A�N�e�B�u�ɂ���
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  �R���t�B�O�L�����o�X���A�N�e�B�u�ɂ���
        ConfigCanvas.SetActive(false);

        //  �R���t�B�O�{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(
            Buttons.transform.GetChild(1).gameObject);

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
        horizontalInput = inputNavigateAxis.x;

        //  ���͂��Ȃ��ꍇ�͒e��
        if(!nevigate.WasPressedThisFrame())return;
        else if(Mathf.Abs(verticalInput) > 0.0f)
        {
            //  �Z���N�gSE�Đ�
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        if (verticalInput < 0)
        {
            if (Pos != menuNum)
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition += 
                new UnityEngine.Vector2(0,-lineHeight);
                Pos += 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition += 
                new UnityEngine.Vector2(0,lineHeight*(menuNum-1));
                Pos = 1;
            }
            
        }
        else if (verticalInput > 0)
        {
            if (Pos != 1)
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition +=
                    new UnityEngine.Vector2(0, lineHeight);
                Pos -= 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition +=
                new UnityEngine.Vector2(0, -lineHeight * (menuNum - 1));
                Pos = menuNum;
            }
        } 
    }

    //---------------------------------------------------
    //  �R���t�B�O��ʂł̃J�[�\���ړ�
    //---------------------------------------------------
    private void MoveConfigMenuCursor()
    {
        UnityEngine.Vector2 inputNavigateAxis = nevigate.ReadValue<UnityEngine.Vector2>();
        verticalInput = inputNavigateAxis.y;
        horizontalInput = inputNavigateAxis.x;

        //  ���͂��Ȃ��ꍇ�͒e��
        if(!nevigate.WasPressedThisFrame())return;
        else
        {
            //  �Z���N�gSE�Đ�
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        } 
    }

    //---------------------------------------------------
    //  �L�[�R���t�B�O�{�^���������ꂽ���̏���
    //---------------------------------------------------
    public void OnPressedKeyConfig()
    {
        //  �L�[�R���t�B�O�{�^���𖳌��ɂ���
        ConfigPanel.transform.GetChild((int)eConfigPanel.KeyConfigButton)
            .GetComponent<Button>().interactable = false;

        //  �L�[�{�[�h�{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(
            KeyConfigPanel.transform.GetChild(0).gameObject);

    }

}
