using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  ���ŋ��V�[���Ǘ��N���X
//
//--------------------------------------------------------------
public class IntroManager : MonoBehaviour
{
    [SerializeField] private GameObject UICanvas;       //  UI�L�����o�X(Fade/����)
    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  ����

    bool canContorol;   //  ����\�t���O

    [SerializeField] private TextMeshProUGUI sceneText; //  Canvas�̃e�L�X�g�I�u�W�F�N�g
    [SerializeField] private Image sceneImage;          //  Canvas�̉摜�I�u�W�F�N�g
    [SerializeField] private GameObject skipPanel;      //  SkipPanel�I�u�W�F�N�g
    [SerializeField] private GameObject noButton;       //  �u�������v�{�^���I�u�W�F�N�g

    //  ���ŋ���ID
    private enum SceneType
    {
        Scene01,
        Scene02,
        Scene03,    //  ����image�̓_�~�[�ŕ\������Ȃ�
        Scene04,
        Scene05,

        Max
    };

    //  �C���X�g
    [SerializeField] private Sprite[] image;

    //  �e�L�X�g
    private string[] text =
    {
        "���͐̂̕���B\r\n��̓I�ɂ�20�N�قǑO�B\r\n�l�Ɨd���̊Ԃɑ傫�Ȑ킪�����������ȥ������B",
        "�d����������d���̌R���ɗ������������̂�\r\n\r\n��������l�̛ޏ�������\r\n\r\n�n�͗􂯎R�͊���\r\n\r\n",
        "�邱�Ƃ��Ȃ�\r\n\r\n�ނ�͂Ђ�����Ɛ����������",
        "����Ȑl�d�̑��i�����������j��\r\n\r\n�ޏ��Ɨd���̉������΂�邱�Ƃ�\r\n\r\n�������Ȃ��������낵��",
        "�����Ď��͗���\r\n\r\n�ޏ��Ɨd�����̊ԂɎY�܂ꂽ���l���d�̎q��\r\n\r\n�P�S�΂ɂȂ���������̕���ł���"
    };

    //  �t�F�[�h����
    private static readonly float fade_duraiton = 3f;

    //  ��ʂ̍X�V�Ԋu
    private static readonly float update_interval = 7f;

    //  ���͊֌W
    PlayerInput _input;
    InputAction pause;

    void Start()
    {
        StartCoroutine(StartInit());
    }

    // �t�F�[�h�C���̊�����҂�
    IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());

        yield return new WaitForSeconds(1f); //  1�b�҂�
    }

    //  ���t�F�[�h�C����
    IEnumerator WaitingForWhiteFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn(new Color(1f,1f,1f)));

        yield return new WaitForSeconds(1f); //  1�b�҂�
    }

    // �t�F�[�h�A�E�g�̊�����҂�
    IEnumerator WaitingForFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut());

        yield return new WaitForSeconds(1f); //  1�b�҂�
    }

    //  ���t�F�[�h�A�E�g��
    IEnumerator WaitingForWhiteFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut(new Color(1f,1f,1f)));
    }

    //  �������J���A�j���[�V�����̊�����҂�
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
    }

    //  �^�C�g����BGM���~�߂�
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
    }

    //  ���������A�j���[�V�����̊�����҂�
    IEnumerator WaitingForClosingScroll()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1�b�҂�
    }

    //--------------------------------------------------------------
    //  �������R���[�`��
    //--------------------------------------------------------------
    IEnumerator StartInit()
    {
        // InputAction��Navigate��ݒ�
        _input = GetComponent<PlayerInput>();
        pause = _input.actions["Pause"];

        //  �ŏ��̃y�[�W�ɂ���
        sceneImage.sprite = image[(int)SceneType.Scene01];
        sceneText.text = text[(int)SceneType.Scene01];

        //  �X�L�b�v�p�l�����\���ɂ���
        skipPanel.SetActive(false);

        //  �K���\���ɂ��Ă���
        UICanvas.SetActive(true);

        //  �摜�̃A���t�@��0�ɂ���
        sceneImage.DOFade(0f, 0f);
        
        //  �e�L�X�g�̃A���t�@��0�ɂ���
        sceneText.DOFade(0f, 0f);

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        //  �����A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2f); //  2�b�҂�

        //  IntroBGM�Đ�
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_SHOP);

        //  ����\�ɂ���
        canContorol = true;

        /**************************�������I��****************************/

        //-----------------------------
        //  ���ŋ��J�n
        //-----------------------------
        StartCoroutine(StartScene());
    }

    void Update()
    {
        //  ����s�\�Ȃ烊�^�[��
        if(!canContorol)return;

        //  ����{�^���Ńp�l����\��
        if(pause.WasPressedThisFrame())
        {
            //  �X�L�b�v�p�l����\���ɂ���
            skipPanel.SetActive(true);

            //  �u�������v�{�^����I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(noButton.gameObject);

            //  ��~
            Time.timeScale = 0f;
        }
    }

    //--------------------------------------------------------------
    //  �V�[���̍Đ�
    //--------------------------------------------------------------
    IEnumerator StartScene()
    {
        StartCoroutine(StartFadeImage(SceneType.Scene01 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene01 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        StartCoroutine(StartFadeImage(SceneType.Scene02 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene02 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        //  �e�L�X�g�����ύX
        StartCoroutine(StartFadeText(SceneType.Scene03 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        StartCoroutine(StartFadeImage(SceneType.Scene03 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene04 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        StartCoroutine(StartFadeImage(SceneType.Scene04 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene05 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        //-------------------------�Đ��I��-------------------------

        //  �V�[���I������
        yield return StartCoroutine(EndIntroScene());
    }

    //--------------------------------------------------------------
    //  �w��V�[���̊G���t�F�[�h�C��������
    //--------------------------------------------------------------
    IEnumerator StartFadeImage(SceneType sceneType, float duration)
    {
        //  �܂��w��V�[���̉摜�ɍ����ւ���
        sceneImage.sprite = image[(int)sceneType];

        //  �摜�̃A���t�@��0�ɂ���
        sceneImage.DOFade(0f, 0f);

        yield return null;

        //  �摜���t�F�[�h�C��������
        sceneImage.DOFade(1f,duration);

        yield return new WaitForSeconds(duration);
    }

    //--------------------------------------------------------------
    //  �w��V�[���̃e�L�X�g���t�F�[�h�C��������
    //--------------------------------------------------------------
    IEnumerator StartFadeText(SceneType sceneType, float duration)
    {
        //  �܂��w��V�[���̃e�L�X�g�ɍ����ւ���
        sceneText.text = text[(int)sceneType];

        //  �e�L�X�g�̃A���t�@��0�ɂ���
        sceneText.DOFade(0f, 0f);

        yield return null;

        //  �摜���t�F�[�h�C��������
        sceneText.DOFade(1f,duration);

        yield return new WaitForSeconds(duration);
    }

    //--------------------------------------------------------------
    //  �V�[���̏I������
    //--------------------------------------------------------------
    IEnumerator EndIntroScene()
    {
        //  �����A�j���[�V����
        yield return StartCoroutine(WaitingForClosingScroll());

        //  �t�F�[�h�A�E�g
        //yield return StartCoroutine(WaitingForFadeOut());

        //  BGM���~�߂ă��C����
        StopBGM();
        LoadingScene.Instance.LoadNextScene("Stage01");
    }

    //--------------------------------------------------------------
    //  �X�L�b�v���j���[�Łu�͂��v�����������̏���
    //--------------------------------------------------------------
    public void OnSkipButtonDown()
    {
        //  ���ɖ߂�
        Time.timeScale = 1f;

        //  �V�[���I������
        StartCoroutine(EndIntroScene());
    }

    //--------------------------------------------------------------
    //  �X�L�b�v���j���[�Łu�������v�����������̏���
    //--------------------------------------------------------------
    public void OnContinueButtonDown()
    {
        //  ���ɖ߂�
        Time.timeScale = 1f;

        //  �X�L�b�v�p�l�����\���ɂ���
        skipPanel.SetActive(false);
    }
}
