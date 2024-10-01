using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    //  ���o�[�g������
    [SerializeField] private GameObject doujiText;
    [SerializeField] private GameObject tsukumoText;
    [SerializeField] private GameObject kuchinawaText;
    [SerializeField] private GameObject kuramaText;
    [SerializeField] private GameObject wadatsumiText;
    [SerializeField] private GameObject hakumenText;

    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  ����
    [SerializeField] private GameObject soundManager;   //  SoundManager

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

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̊J�n�`�`�`�`�`�`�`�`�`�`�` */

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �������J���A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2�b�҂�

        /* �`�`�`�`�`�`�`�`�`�`�`���o�̏I���`�`�`�`�`�`�`�`�`�`�` */

        //  BGM�Đ�
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_GAMEOVER);

        //  �ŏ��̓h�E�W��I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(doujiButton.gameObject);
        preSelectedObject = doujiButton.gameObject;
    }

    void Update()
    {
        //  ����I������Ă���I�u�W�F�N�g���O��ƈ������SE�Đ�
        if(preSelectedObject != EventSystem.current.currentSelectedGameObject)
        {
            //  �Z���N�gSE�Đ�
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        //  �{�^���ɍ��킹�Đ�������\��
        if(EventSystem.current.currentSelectedGameObject == doujiButton.gameObject)
        {
            doujiText.SetActive(true);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == tsukumoButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(true);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == kuchinawaButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(true);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == kuramaButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(true);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == wadatsumiButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(true);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == hakumenButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(true);
        }

        //  ����I�����ꂽ�I�u�W�F�N�g��ۑ�
        preSelectedObject = EventSystem.current.currentSelectedGameObject;
    }

    //  �h�E�W�{�^��������������
    public void OnDoujiButtonDown()
    {
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
}
