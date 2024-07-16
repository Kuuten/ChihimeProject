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
    public static GameManager _Instance
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
                //  �|�[�Y��ɓ��͂������Ȃ��Ȃ�̂Ń��Z�b�g
                player.GetComponent<PlayerInput>().enabled = true;
            }
        }

        if (test2.WasPressedThisFrame())
        {
            testSwitch = true;

            if(testSwitch == true)
            {
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
        float msg_alpha_speed = 1.0f; //  �X�e�[�W���̃A���t�@���ő�ɂȂ�̂ɂ����鎞��
        float msg_wait  = 3.0f;       //  ���b�Z�[�W�̕\������
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

        //  �v���C���[��HP��0�ɂȂ�܂�

        //while (!OneBaseLeft())
        //{
        //    //  ����ł���L�����̓��X�g����폜
        //    DeleteDeadObjectFromList(_Players);
        //    DeleteDeadObjectFromList(_Enemies)
        //    yield return null;
        //}

        //************�ȍ~�̓v���C���[���S************

        //  �v���C���[�̂��ꉉ�o�����s���I����҂�
        //  ������Q�l��

        //loseSide.DeadStaging();    //  ���S���o�Đ�
        ////  ���o�I���܂ő҂�
        //while (!loseSide.GetIsDeadStagingEnd())
        //{
        //    yield return null;
        //}

        //  �R���e�B�j���[��UI��L��������
        //_InGameUI.SetActive(false);

        //  �|�[�Y����

        //  Inputsystem��UI���[�h��
        //DisableCharacterControl();

        //  BGM�X�g�b�v
        //_AudioSource[(int)AudioType.BGM].Stop();

        //  �u�Â���v�������ꂽ�ꍇ
        //  �|�[�Y����
        //  BGM���ĊJ
        //  �v���C���[��������Ԃփ��Z�b�g�i�̗́E�{�����񕜁j

        //  �u������߂�v�������ꂽ�ꍇ
        //  �Q�[���I�[�o�[�V�[����

        yield return null;
    }
}
