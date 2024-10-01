using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

//  �Q�[�����̏��
public enum eGameState
{
    Zako,       //  �U�R�풆
    Boss,       //  �{�X�풆
    Event,      //  ��b�C�x���g��

    StateMax
}

//  ���ʕ\���p�I�u�W�F�N�g
public enum eResultObj
{
    RESULT_CANVAS,
    RESULT_IMAGE,
    SOUL_IMAGE,
    SOUL_TEXT,
    HONEG_IMAGE,
    HONEG_PANEL,
    HONEG_TEXT,
    BUTTON,
    KAMIFUBUKI,

    Max
}

//--------------------------------------------------------------
//
//  �Q�[���Ǘ��N���X
//
//--------------------------------------------------------------
public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject player;
    [SerializeField]private GameObject enemyGenerator;

    private int gameState;

    [SerializeField] private GameObject soundManager;
    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    //  �f�o�b�O�p
    private InputAction test;
    private bool testSwitch;


    //  �V���O���g���ȃC���X�^���X
    public static GameManager Instance
    {
        get; private set;
    }

    //  �����̉��l
    private const int smallKonValue = 50;
    public int GetSmallKonValue(){ return smallKonValue; }
    private const int bigKonValue = 100;
    public int GetBigKonValue(){ return bigKonValue; }

    //  �o���|�C���g
    private GameObject[] spawner;
    //  ����_
    private GameObject[] controlPoint;

    //  �X�e�[�W�N���A�t���O
    bool stageClearFlag;

    //  �V�[���؂�ւ��t���O
    bool sceneChangeFlag;

    //----------------------------------------------------
    //  ���U���g�\���p
    //----------------------------------------------------
    //  ���U���g�\���p�I�u�W�F�N�g
    [SerializeField] private GameObject[] resultObject;
    //  �C�x���g�L�����o�X�I�u�W�F�N�g
    [SerializeField] private GameObject eventCanvas;

    //------------------------------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------------------------------
    public int GetGameState()
    {
        return gameState;
    }
    public void SetGameState(int state)
    {
        gameState = state;
    }

    public GameObject GetPlayer(){ return player; }
    public bool GetStageClearFlag(){ return stageClearFlag; }
    public void SetStageClearFlag(bool b){ stageClearFlag = b; }
    public GameObject GetGameObject(int num){ return resultObject[num]; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        //  SoundManager���Ȃ���ΐ���
        if( !GameObject.Find("SoundManager") )
        {
            Instantiate(soundManager);
        }
#endif
    }

    //---------------------------------------------------------------------
    //  ������
    //---------------------------------------------------------------------
    void Start()
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton"];
        testSwitch = true;

        gameState = (int)eGameState.Zako;   //  �ŏ��̓U�R��
        stageClearFlag = false;
        sceneChangeFlag = false;

        //  �������J�n
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

        //  ���C�����[�v�J�n
        yield return StartCoroutine(GameLoop());
    }

    //---------------------------------------------------------------------
    //  �X�V
    //---------------------------------------------------------------------
    private IEnumerator GameLoop()
    {
        //yield return StartCoroutine(Tutorial());
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());
        yield return StartCoroutine(GameResult());

        yield return null;
    }

    void Update()
    {
        if (test.WasPressedThisFrame())
        {
            testSwitch = !testSwitch;

            if (testSwitch == false)
            {
                //  Pauser���t�����I�u�W�F�N�g���|�[�Y
                Pauser.Pause();

                //  �~�߂�
                Time.timeScale = 0;

                //  �|�[�Y��ɓ��͂������Ȃ��Ȃ�̂Ń��Z�b�g
                player.GetComponent<PlayerInput>().enabled = true;
            }
            else
            {
                //  �ĊJ����
                Time.timeScale = 1;

                //  Pauser���t�����I�u�W�F�N�g���|�[�Y
                Pauser.Resume();
            }
        }


    }

    //-----------------------------------------------------------------
    //  �Q�[���J�n�O
    //-----------------------------------------------------------------
    private IEnumerator GameStarting()
    {
        Debug.Log("GameStarting");

        //  ���̃X�e�[�W��ID���擾
        //int stageID = StageIdManager.Instance.GetStageID();

        //  �X�e�[�WBGM��ݒ�

        //  �L�����̔z�u�E�p�����[�^�����Z�b�g
        //ResetAllCharacters();

        //  �Q�[���J�n�O���b�Z�[�W�i�X�e�[�W�����A���t�@�̃A�j���[�V�����j
        //float msg_alpha_speed = 1.0f; //  �X�e�[�W���̃A���t�@���ő�ɂȂ�̂ɂ����鎞��
        //float msg_wait  = 3.0f;       //  ���b�Z�[�W�̕\������
        //yield return StartCoroutine(PreGameMessage
        //                            (
        //                                msg1speed, msg1wait
        //                            ));

        //  �G�����Ԋu�Ń|�b�v������
        //StartCoroutine(PopEnemyInIntervals(_EnemyPopInterval));

        
        //  �G�̏o���J�n
        StartCoroutine( EnemyManager.Instance.AppearEnemy() );
        

        yield return null;
    }

    //-----------------------------------------------------------------
    //  �Q�[����
    //-----------------------------------------------------------------
    private IEnumerator GamePlaying()
    {
        Debug.Log("GamePlaying");

        //  Inputsystem���v���C���[���[�h��
        //EnableCharacterControl();

        //  �X�e�[�W�N���A���ĂȂ��Ԃ͑҂�
        yield return new WaitUntil(()=> stageClearFlag == true);

        yield return null;
    }

    // �t�F�[�h�C���̊�����҂�
    public IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());
    }

    // �t�F�[�h�A�E�g�̊�����҂�
    public IEnumerator WaitingForFadeOut()
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

        yield return new WaitForSeconds(2.0f); //  2.0�b�҂�

        yield return StartCoroutine(DataCopyAndChangeScene());
    }

    //  �^�C�g����BGM���~�߂�
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
    }

    // ���ۑ����V�[���J��
    public IEnumerator DataCopyAndChangeScene()
    {
        //  �e������擾
        GameObject player = GameManager.Instance.GetPlayer();
        int maxHP = player.GetComponent<PlayerHealth>().GetCurrentMaxHealth();
        int hP = player.GetComponent<PlayerHealth>().GetCurrentHealth();
        int bombNum = player.GetComponent<PlayerBombManager>().GetBombNum();
        int kon = MoneyManager.Instance.GetKonNum();
        int shotLV = player.GetComponent<PlayerShotManager>().GetNormalShotLevel();;
        int speedLV = player.GetComponent<PlayerMovement>().GetSpeedLevel();

        //  ���ۑ�
        PlayerInfoManager.SetInfo(maxHP,hP,kon,bombNum,shotLV,speedLV);

        //  BGM���~�߂�
        StopBGM();

        //  ���݃X�e�[�W���X�V
        PlayerInfoManager.stageInfo = PlayerInfoManager.StageInfo.Stage02;

        //   �Q�[���N���A�V�[����
        LoadingScene.Instance.LoadNextScene("TrialEnding");

        ////   �V���b�v�V�[����
        //LoadingScene.Instance.LoadNextScene("Shop"); 

        ////  �̌��łł͓��ʃN���A��ʂɑJ��
        //if(Application.version == "0.5")
        //{
        //   //   �Q�[���N���A�V�[����
        //   LoadingScene.Instance.LoadNextScene("TrialEnding");
        //}
        ////  ���i�łł̓V���b�v�V�[���֑J��
        //else
        //{
   
        //}

        yield return null;
    }

    //-----------------------------------------------------------------
    //  �Q�[�����ʕ\��
    //-----------------------------------------------------------------
    private IEnumerator GameResult()
    {
        Debug.Log("***���ʕ\���҂��B***");

        //  ���ʕ\���J�n�t���O��TRUE�ɂ܂�܂ő҂�
        yield return new WaitUntil(()=> EventSceneManager.Instance.GetStartResult() == true);

        Debug.Log("***���ʕ\�����[�h�ɂȂ�܂����B***");

        //  ���ʕ\��Cnavas��\��
        resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(true);

        yield return null;
    }


    //-----------------------------------------------------------------
    //  ���ۑ����V���b�v�֑J��(�{�^���������ꂽ��Ă΂��)
    //-----------------------------------------------------------------
    public void AfterResult()
    {
        if(!sceneChangeFlag)
        {
            //  ���ʕ\���L�����o�X���\���ɂ���
            resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(false);

            //  ��������\���ɂ���
            resultObject[(int)eResultObj.KAMIFUBUKI].SetActive(false);

            //  ����SE��炷
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_SELECT);

            //  �C�x���g�L�����o�X���\���ɂ���
            eventCanvas.SetActive(false);

            sceneChangeFlag = true;
            StartCoroutine(WaitingForClosingScroll());
        }
    }
}
