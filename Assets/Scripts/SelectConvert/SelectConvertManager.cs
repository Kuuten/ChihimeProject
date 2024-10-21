using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Button doujiButton;
    [SerializeField] private Button tsukumoButton;
    [SerializeField] private Button kuchinawaButton;
    [SerializeField] private Button kuramaButton;
    [SerializeField] private Button wadatsumiButton;
    [SerializeField] private Button hakumenButton;

    //  ���j���[�{�^���̃C���f�b�N�X
    private int menuIndex;

    //  ��������̔w�i�I�u�W�F�N�g
    [SerializeField] private GameObject explanationMovieBack;

    //  �����e�L�X�g
    [SerializeField] private GameObject[] explanationTextObj;

    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  ����
    [SerializeField] private GameObject soundManager;   //  SoundManager

    //  ���o�[�g�p�̓���N���b�v
    [SerializeField] private VideoClip[] videoClip;
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

        _input = GetComponent<PlayerInput>();
        navigate =  _input.actions["Navigate"];
        verticalInput = default;
        menuIndex = (int)BossType.Douji;

        bCanDecision = true;

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̊J�n�`�`�`�`�`�`�`�`�`�`�` */

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �������J���A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2�b�҂�

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̏I���`�`�`�`�`�`�`�`�t���O�`�`�` */

        //  BGM�Đ�
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_GAMEOVER);

        //  �ŏ��̓h�E�W��I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(doujiButton.gameObject);
        preSelectedObject = doujiButton.gameObject;

        //  ����Ɠ���̘g��\������
        explanationMovieBack.SetActive(true);

    }

    void Update()
    {
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

    //  �h�E�W�{�^��������������
    public void OnDoujiButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        doujiButton.GetComponent<Button>().enabled = false;
        tsukumoButton.GetComponent<Button>().enabled = false;
        kuchinawaButton.GetComponent<Button>().enabled = false;
        kuramaButton.GetComponent<Button>().enabled = false;
        wadatsumiButton.GetComponent<Button>().enabled = false;
        hakumenButton.GetComponent<Button>().enabled = false;

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        //StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.DOUJI;

        //  BGM���~�߂ă^�C�g����
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
        LoadingScene.Instance.LoadNextScene("Main");
    }

    //  �c�N���{�^��������������
    public void OnTsukumoButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        //  �{�^���𖳌���
        doujiButton.GetComponent<Button>().enabled = false;
        tsukumoButton.GetComponent<Button>().enabled = false;
        kuchinawaButton.GetComponent<Button>().enabled = false;
        kuramaButton.GetComponent<Button>().enabled = false;
        wadatsumiButton.GetComponent<Button>().enabled = false;
        hakumenButton.GetComponent<Button>().enabled = false;

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.TSUKUMO;
    }


    //  �N�`�i���{�^��������������
    public void OnKuchinawaButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        ////  �{�^���𖳌���
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.KUCHINAWA;
    }


    //  �N���}�{�^��������������
    public void OnKuramaButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        ////  �{�^���𖳌���
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.KURAMA;
    }


    //  ���_�c�~�{�^��������������
    public void OnWadatsumiButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        ////  �{�^���𖳌���
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.WADATSUMI;
    }

    //  �n�N�����{�^��������������
    public void OnHakumenButtonDown()
    {
        //  ����\�t���O��false�Ȃ烊�^�[��
        if(!bCanDecision)return;

        ////  �{�^���𖳌���
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

        //  ���艹�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  ���������A�j���[�V�����̊�����҂�
        StartCoroutine(WaitingForClosingScroll());
    
        //  ���o�[�g�e���Z�b�g
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.WADATSUMI;
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
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
        LoadingScene.Instance.LoadNextScene("Main");
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
            UpdateButtonScale(doujiButton.gameObject);
            UpdateButtonScale(tsukumoButton.gameObject);
            UpdateButtonScale(kuchinawaButton.gameObject);
            UpdateButtonScale(kuramaButton.gameObject);
            UpdateButtonScale(wadatsumiButton.gameObject);
            UpdateButtonScale(hakumenButton.gameObject);
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
            UpdateButtonScale(doujiButton.gameObject);
            UpdateButtonScale(tsukumoButton.gameObject);
            UpdateButtonScale(kuchinawaButton.gameObject);
            UpdateButtonScale(kuramaButton.gameObject);
            UpdateButtonScale(wadatsumiButton.gameObject);
            UpdateButtonScale(hakumenButton.gameObject);
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
        if(doujiButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Douji];
            videoPlayer.Play();
        }
        else if(tsukumoButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Tsukumo];
            videoPlayer.Play();
        }
        else if(kuchinawaButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Kuchinawa];
            videoPlayer.Play();
        }
        else if(kuramaButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Kurama];
            videoPlayer.Play();
        }
        else if(wadatsumiButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Wadatsumi];
            videoPlayer.Play();
        }
        else if(hakumenButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Hakumen];
            videoPlayer.Play();
        }
    }

    //----------------------------------------------------
    //  �C���f�b�N�X�Ń{�^����I������
    //----------------------------------------------------
    private void SelectButtonByMenuIndex()
    {
        //  �C���f�b�N�X�Ń{�^����I����Ԃɂ���
        if(menuIndex == (int)BossType.Douji)
        {
            EventSystem.current.SetSelectedGameObject(doujiButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(true);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Tsukumo)
        {
            EventSystem.current.SetSelectedGameObject(tsukumoButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(true);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Kuchinawa)
        {
            EventSystem.current.SetSelectedGameObject(kuchinawaButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(true);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Kurama)
        {
            EventSystem.current.SetSelectedGameObject(kuramaButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(true);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Wadatsumi)
        {
            EventSystem.current.SetSelectedGameObject(wadatsumiButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(true);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Hakumen)
        {
            EventSystem.current.SetSelectedGameObject(hakumenButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(true);
        }
    }
}
