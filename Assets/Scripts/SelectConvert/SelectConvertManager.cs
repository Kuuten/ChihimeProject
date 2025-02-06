using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

//--------------------------------------------------------------
//
//  ���o�[�g�e�I���V�[���N���X
//
//--------------------------------------------------------------
public class SelectConvertManager : MonoBehaviour
{
    //  ���o�[�g�e�{�^��
    [SerializeField,EnumIndex(typeof(BossType))]
    private Button[] BossButton;

    //  ���j���[�{�^���̃C���f�b�N�X
    private int menuIndex;

    //  ��������̔w�i�I�u�W�F�N�g
    [SerializeField] private GameObject explanationMovieBack;

    //  �����e�L�X�g
    [SerializeField,EnumIndex(typeof(BossType))]
    private GameObject[] explanationTextObj;

    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  ����
    [SerializeField] private GameObject soundManager;   //  SoundManager

    //  ���o�[�g�p�̓���N���b�v
    [SerializeField,EnumIndex(typeof(BossType))]

    private VideoClip[] videoClip;
    //  ���o�[�g�p�̓���v���C���[
    [SerializeField] private VideoPlayer videoPlayer;

    bool bCanDecision;  //  �{�^���̌���\�t���O

    PlayerInput _input;
    InputAction navigate;
    float verticalInput;

    //  �O��I������Ă����I�u�W�F�N�g
    private GameObject preSelectedObject;

    IEnumerator Start()
    {
        //  StageIdManager���Ȃ����͐�������
        if (!GameObject.Find("SoundManager"))
        {
            Debug.Log("SoundManager���Ȃ��̂Ő������܂�");
            Instantiate(soundManager);
        }

        //  �{�^���̐��`�F�b�N
        if(BossButton.Length > (int)BossType.Max)
        {
            Debug.LogError("BossButton�̐���BossType.Max�������Ă��܂�");
        }

        _input = GetComponent<PlayerInput>();
        navigate =  _input.actions["Navigate"];
        verticalInput = default;
        menuIndex = (int)BossType.Douji;

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̊J�n�`�`�`�`�`�`�`�`�`�`�` */

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �������J���A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2�b�҂�

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̏I���`�`�`�`�`�`�`�`�t���O�`�`�` */

        //  BGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_SELECTCONVERT);

        //  �ŏ��̓h�E�W��I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(BossButton[(int)BossType.Douji].gameObject);
        preSelectedObject = BossButton[(int)BossType.Douji].gameObject;
        UpdateButtonScale(BossButton[(int)BossType.Douji].gameObject);

        //  ����Ɠ���̘g��\������
        explanationMovieBack.SetActive(true);

        //  ����\�t���OON
        bCanDecision = true;

    }

    void Update()
    {
        if(!bCanDecision)return;

        //  3D���j���[����]������
        RotateMenu();

        //  �I���{�^���ɂ���Đ���������Đ�����
        PlayExplanationMovie();

        //  ����I������Ă���I�u�W�F�N�g���O��ƈ������SE�Đ�
        if(preSelectedObject != EventSystem.current.currentSelectedGameObject)
        {
            //  �Z���N�gSE�Đ�
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        //  ����I�����ꂽ�I�u�W�F�N�g��ۑ�
        preSelectedObject = EventSystem.current.currentSelectedGameObject;
    }

    //  ���݂̃X�e�[�WNo.�ɂ���Ď��̑J�ڐ�����߂�
    private void LoadNextScene()
    {
        //  BGM���~�߂�
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);


        if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage01)
        {
            //  Intro�V�[����
            LoadingScene.Instance.LoadNextScene("Intro");
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage02)
        {
            //  �X�e�[�W�Q��
            LoadingScene.Instance.LoadNextScene("Stage02");
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage03)
        {
            //  �X�e�[�W�R��
            LoadingScene.Instance.LoadNextScene("Stage03");
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage04)
        {
            //  �X�e�[�W�S��
            LoadingScene.Instance.LoadNextScene("Stage04");
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage05)
        {
            //  �X�e�[�W�T��
            LoadingScene.Instance.LoadNextScene("Stage05");
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage06)
        {
            //  �X�e�[�W�U��
            LoadingScene.Instance.LoadNextScene("Stage06");
        }
    }

    //  �h�E�W�{�^��������������
    public void OnDoujiButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        for(int i=0;i<(int)BossType.Max;i++)
        {
            BossButton[i].GetComponent<Button>().enabled = false;
        }

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.DOUJI;

        //  ���̃V�[�������[�h
        LoadNextScene();
    }

    //  �c�N���{�^��������������
    public void OnTsukumoButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        for(int i=0;i<(int)BossType.Max;i++)
        {
            BossButton[i].GetComponent<Button>().enabled = false;
        }

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.TSUKUMO;

        //  ���̃V�[�������[�h
        LoadNextScene();
    }


    //  �N�`�i���{�^��������������
    public void OnKuchinawaButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        for(int i=0;i<(int)BossType.Max;i++)
        {
            BossButton[i].GetComponent<Button>().enabled = false;
        }

        ////  ���艹�Đ�
        //SoundManager.Instance.PlaySFX(
        //    (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        ////  ���������A�j���[�V�����̊�����҂�
        //StartCoroutine(WaitingForClosingScroll());
    
        ////  ���o�[�g�e���Z�b�g
        //PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.KUCHINAWA;


        //  ����s��SE��炷
        OnNotImplementedButtonDown();
    }


    //  �N���}�{�^��������������
    public void OnKuramaButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        for(int i=0;i<(int)BossType.Max;i++)
        {
            BossButton[i].GetComponent<Button>().enabled = false;
        }

        ////  ���艹�Đ�
        //SoundManager.Instance.PlaySFX(
        //    (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        ////  ���������A�j���[�V�����̊�����҂�
        //StartCoroutine(WaitingForClosingScroll());
    
        ////  ���o�[�g�e���Z�b�g
        //PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.KURAMA;


        //  ����s��SE��炷
        OnNotImplementedButtonDown();
    }


    //  ���_�c�~�{�^��������������
    public void OnWadatsumiButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        for(int i=0;i<(int)BossType.Max;i++)
        {
            BossButton[i].GetComponent<Button>().enabled = false;
        }

        ////  ���艹�Đ�
        //SoundManager.Instance.PlaySFX(
        //    (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        ////  ���������A�j���[�V�����̊�����҂�
        //StartCoroutine(WaitingForClosingScroll());
    
        ////  ���o�[�g�e���Z�b�g
        //PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.WADATSUMI;

        //  ����s��SE��炷
        OnNotImplementedButtonDown();
    }

    //  �n�N�����{�^��������������
    public void OnHakumenButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        for(int i=0;i<(int)BossType.Max;i++)
        {
            BossButton[i].GetComponent<Button>().enabled = false;
        }

        ////  ���艹�Đ�
        //SoundManager.Instance.PlaySFX(
        //    (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        ////  ���������A�j���[�V�����̊�����҂�
        //StartCoroutine(WaitingForClosingScroll());
    
        ////  ���o�[�g�e���Z�b�g
        //PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.WADATSUMI;

        //  ����s��SE��炷
        OnNotImplementedButtonDown();
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
    }

    //----------------------------------------------------
    //  3D���j���[����]������
    //----------------------------------------------------
    private void RotateMenu()
    {
        //  �㉺�̓��͂����o����
        Vector2 inputNavigateAxis = navigate.ReadValue<Vector2>();
        verticalInput = inputNavigateAxis.y;

        //  3D���j���[����]������
        if (verticalInput > 0 && Rotate3DMenu.Instance.GetComplete())
        {
            Debug.Log("����]");
            Rotate3DMenu.Instance.TurnMenu(true);

            bCanDecision = false;

            //  �C���f�b�N�X�v���X
            if(menuIndex >= (int)BossType.Hakumen)
            {
                menuIndex = (int)BossType.Douji;
            }
            else menuIndex++;

            //  �{�^���̑I����Ԃ��X�V
            SelectButtonByMenuIndex();

            //  �{�^���̃X�P�[�����X�V
            for(int i=0;i<(int)BossType.Max;i++)
            {
                UpdateButtonScale(BossButton[i].gameObject);
            }
        }
        else if (verticalInput < 0 && Rotate3DMenu.Instance.GetComplete())
        {
            Debug.Log("���]");
            Rotate3DMenu.Instance.TurnMenu(false);

            bCanDecision = false;

            //  �C���f�b�N�X�}�C�i�X
            if(menuIndex <= (int)BossType.Douji)
            {
                menuIndex = (int)BossType.Hakumen;
            }
            else menuIndex--;

            //  �{�^���̑I����Ԃ��X�V
            SelectButtonByMenuIndex();

            //  �{�^���̃X�P�[�����X�V
            for(int i=0;i<(int)BossType.Max;i++)
            {
                UpdateButtonScale(BossButton[i].gameObject);
            }
        }
    }

    //----------------------------------------------------
    //  �I�����ꂽ�{�^����傫������
    //----------------------------------------------------
    private void UpdateButtonScale(GameObject obj)
    {
        float duration = 0.7f;

        if(obj == EventSystem.current.currentSelectedGameObject)
        {
            obj.transform.DOScale(1.2f,duration)
                .SetEase(Ease.InOutElastic)
                .OnComplete(()=>{ bCanDecision = true; });
        }
        else
        {
            obj.transform.DOScale(1.0f,duration)
                .SetEase(Ease.InOutElastic);
        }
    }

    //----------------------------------------------------
    //  �������{�^���������ꂽ���̏���
    //----------------------------------------------------
    private void OnNotImplementedButtonDown()
    {
        //  �s����SE���Đ����ă��^�[��
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_INCORRECT);

        return;
    }

    //----------------------------------------------------
    //  ���o�[�g�e�̐���������Đ�����
    //----------------------------------------------------
    private void PlayExplanationMovie()
    {
        if(!Rotate3DMenu.Instance.GetComplete() || !bCanDecision)return;

        //  �I���{�^���ɂ���Đ���������Đ�����
        for(int i=0;i<(int)BossType.Max;i++)
        {
            if(BossButton[i].gameObject == EventSystem.current.currentSelectedGameObject)
            {
                videoPlayer.clip = videoClip[i];
                videoPlayer.Play();
            } 
        }
    }

    //----------------------------------------------------
    //  �C���f�b�N�X�Ń{�^����I������
    //----------------------------------------------------
    private void SelectButtonByMenuIndex()
    {
        //  �C���f�b�N�X�Ń{�^����I����Ԃɂ���
        for(int index=0;index<(int)BossType.Max;index++)
        {
            if(index == menuIndex)
            {
                EventSystem.current.SetSelectedGameObject(BossButton[menuIndex].gameObject);
                
                explanationTextObj[menuIndex].SetActive(true);
            }
        }
    }
}
