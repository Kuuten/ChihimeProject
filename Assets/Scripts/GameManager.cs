using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//  �Q�[�����̏��
public enum eGameState
{
    Zako,       //  �U�R�풆
    Boss,       //  �{�X�풆
    Event,      //  ��b�C�x���g��

    StateMax
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

    //  �f�o�b�O�p
    private InputAction test;
    private InputAction test2;
    private bool testSwitch = true;
    IEnumerator shotCorourine;


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

    void Start()
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton"];
        test2 = playerInput.actions["TestButton2"];

        gameState = (int)eGameState.Zako;   //  �ŏ��̓U�R��

        //  �������J�n
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        //  ���C�����[�v�J�n
        yield return StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        //yield return StartCoroutine(Tutorial());
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());

        yield return null;
    }

    void Update()
    {
        if (test.WasPressedThisFrame())
        {
            testSwitch = false;

            if(testSwitch == false)
            {
                //  Pauser���t�����I�u�W�F�N�g���|�[�Y
                Pauser.Pause();

                //  �~�߂�
                Time.timeScale = 0;

                //  �|�[�Y��ɓ��͂������Ȃ��Ȃ�̂Ń��Z�b�g
                player.GetComponent<PlayerInput>().enabled = true;
            }
        }

        if (test2.WasPressedThisFrame())
        {
            testSwitch = true;

            if(testSwitch == true)
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

        //  �X�e�[�W�N���A�t���O������������ۑ����ăV���b�v�֑J��
        bool clearFlag = false; //  ������

        GameObject player = GameManager.Instance.GetPlayer();
        int maxHP = player.GetComponent<PlayerHealth>().GetCurrentMaxHealth();
        int hP = player.GetComponent<PlayerHealth>().GetCurrentHealth();
        int bombNum = player.GetComponent<PlayerBombManager>().GetBombNum();
        int kon = MoneyManager.Instance.GetKonNum();
        int shotLV = player.GetComponent<PlayerShotManager>().GetNormalShotLevel();;
        int speedLV = player.GetComponent<PlayerMovement>().GetSpeedLevel();

        if(clearFlag)
        {
            //  ���ۑ�
            PlayerInfoManager.SetInfo(maxHP,hP,kon,bombNum,shotLV,speedLV);

            // �����A�j���[�V����

            //  �t�F�[�h�A�E�g��҂�
            yield return StartCoroutine(WaitingForFadeOut());

            //  �V���b�v�V�[���֑J��
        }

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
}
