using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    }

    void Start()
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton"];
        test2 = playerInput.actions["TestButton2"];

        gameState = (int)eGameState.Zako;   //  �ŏ��̓U�R��

        //  ���C�����[�v�J�n
        StartCoroutine(GameLoop());
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

        //************�ȍ~�̓v���C���[���S************

        //  �v���C���[�̂��ꉉ�o�����s���I����҂�
        //  ������Q�l��

        //loseSide.DeadStaging();    //  ���S���o�Đ�
        ////  ���o�I���܂ő҂�
        //while (!loseSide.GetIsDeadStagingEnd())
        //{
        //    yield return null;
        //}

        //  �Q�[���I�[�o�[�V�[����

        yield return null;
    }
}
