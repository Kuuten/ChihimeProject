using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;


//--------------------------------------------------------------
//
//  �X�e�[�W�N���A�\���N���X
//
//--------------------------------------------------------------
public class ResultAnimation : MonoBehaviour
{
    //  �u������𐬌��I�v�̉摜�I�u�W�F�N�g
    private GameObject resultImage;
    //  �l���\�E���̉摜�I�u�W�F�N�g
    private GameObject soulImage;
    //  �l���\�E���̃e�L�X�g�I�u�W�F�N�g
    private GameObject soulText;
    //  ��G�̉摜�I�u�W�F�N�g
    private GameObject honeGImage;
    //  ��G�̃e�L�X�g�p�l���I�u�W�F�N�g
    private GameObject honeGTextPanel;
    //  ��G�̃e�L�X�g�I�u�W�F�N�g
    private GameObject honeGText;
    //  �{�^���I�u�W�F�N�g
    private GameObject nextButton;
    //  ������I�u�W�F�N�g
    private GameObject kamiFubuki;

    //  �J�E���g�p�\�E���̐�
    int soulNum;

    void Start()
    {
        //  �\�E���̐�������
        soulNum = 0;

        // GameManager����I�u�W�F�N�g�����蓖�Ă�
        resultImage = GameManager.Instance.GetGameObject((int)eResultObj.RESULT_IMAGE);
        soulImage = GameManager.Instance.GetGameObject((int)eResultObj.SOUL_IMAGE);
        soulText = GameManager.Instance.GetGameObject((int)eResultObj.SOUL_TEXT);
        honeGImage = GameManager.Instance.GetGameObject((int)eResultObj.HONEG_IMAGE);
        honeGTextPanel = GameManager.Instance.GetGameObject((int)eResultObj.HONEG_PANEL);
        honeGText = GameManager.Instance.GetGameObject((int)eResultObj.HONEG_TEXT);
        nextButton = GameManager.Instance.GetGameObject((int)eResultObj.BUTTON);
        kamiFubuki = GameManager.Instance.GetGameObject((int)eResultObj.KAMIFUBUKI);

        //  ���ʕ\���J�n
        StartCoroutine( ResultStart() );
    }

    void Update()
    {
        // �e�L�X�g���X�V
        soulText.GetComponent<TextMeshProUGUI>().text = $"{soulNum}";
    }

    //-----------------------------------------------------------------
    //  �Q�[�����ʕ\���A�j���[�V����
    //-----------------------------------------------------------------
    private IEnumerator ResultStart()
    {
        Debug.Log("���ʕ\�����J�n���܂�");

        //  BGM���Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_RESULT);

        //  �L�����o�X��L����
        this.gameObject.SetActive(true);

        //  �u������𐬌��I��L����
        resultImage.SetActive(true);

        //  �u������𐬌��I�̃A�j���[�V�����J�n
        resultImage.transform.localScale = new Vector3(1,0,1);
        resultImage.GetComponent<RectTransform>().DOScaleY(1.0f,0.5f);

        //  SE��炷
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_NORMAL_SHOT);
        
        //  �A�j���[�V������҂�
        yield return new WaitForSeconds(0.5f);

        //  �l���\�E����L����
        soulImage.SetActive(true);
        soulImage.transform.localScale = new Vector3(1,0,1);
        soulImage.GetComponent<RectTransform>().DOScaleY(1.0f,0.5f);

        //  SE��炷
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_NORMAL_SHOT);

        //  �A�j���[�V������҂�
        yield return new WaitForSeconds(0.5f);

        //  �l���\�E���e�L�X�g�L����
        soulText.SetActive(true);

        //  �҂�
        yield return new WaitForSeconds(0.5f);

        //  �l���\�E���̐������J�E���g�A�j���[�V�����J�n
        int targetNum = MoneyManager.Instance.GetKonNum();
        int count = 0;

        // ���l�̕ύX
        DOTween.To(
            () => soulNum,                  // ����Ώۂɂ���̂�
            num => soulNum = num,           // �l�̍X�V
            targetNum,                      // �ŏI�I�Ȓl
            2.0f                            // �A�j���[�V��������
        )
        .OnUpdate( () => 
        { 
            count++;
            if(count%5 != 0)return;
            //  �J�E���gSE��炷
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM,
                (int)SFXList.SFX_RESULT_COUNT);
        })
        .OnComplete( () =>
        {
            //  ����SE��炷
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM,
                (int)SFXList.SFX_RESULT_CASH);
        });

        //  �A�j���[�V������҂�
        yield return new WaitForSeconds(3);

        //  �\�E���e�L�X�g�̃X�P�[�����O�J�n
        soulText.GetComponent<RepeatScaling>().enabled = true;

        //  ������𕑂��U�点��
        kamiFubuki.SetActive(true);

        //  ��G�摜��L����
        honeGImage.SetActive(true);

        yield return null;

        //  ��G�A�j���[�V�����J�n
        honeGImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-487,660);

        

        honeGImage.GetComponent<RectTransform>().DOAnchorPosY(37,2.0f)
            .SetEase(Ease.OutBounce);

        //  �A�j���[�V������҂�
        yield return new WaitForSeconds(0.45f);

        //  SE��炷���v�^�C�~���O����
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_RESULT_BOUND);

        //  �A�j���[�V������҂�
        yield return new WaitForSeconds(0.5f);

        //  SE��炷���v�^�C�~���O����
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_RESULT_BOUND);

        //  �p�l����\��
        honeGTextPanel.SetActive(true);
        
        //  �e�L�X�g��\��
        honeGText.SetActive(true);

        //  ��G�ɂ̃��b�Z�[�W��ݒ�
        string[] message =
        {
            "��������l�I\n��l�̗E�p�ɖ�͊����ł����`�I�I",
            "��������l�I\n��l�̗E�p�ɖ�͊����ł����`�I�I",
            "��������l�I\n��l�̗E�p�ɖ�͊����ł����`�I�I",
            "��������l�I\n��l�̗E�p�ɖ�͊����ł����`�I�I",
            "��������l�I\n��l�̗E�p�ɖ�͊����ł����`�I�I",
            "��������l�I\n��l�̗E�p�ɖ�͊����ł����`�I�I",
        };
        //  �X�e�[�W�ԍ����擾
        int stageNum = (int)PlayerInfoManager.stageInfo;
        //  1����������ɂ����鎞��
        float duration = 0.1f;

        //  �e�L�X�g�A�j���[�V�����J�n
        honeGText.GetComponent<TextMeshProUGUI>().text = "";

        //�ω��O�̃e�L�X�g
        var beforeText = honeGText.GetComponent<TextMeshProUGUI>().text;

        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(honeGText.GetComponent<TextMeshProUGUI>());
        honeGText.GetComponent<TextMeshProUGUI>().DOText(message[stageNum],duration*message[stageNum].Length)
        .OnUpdate( () => 
        {
              var currentText = honeGText.GetComponent<TextMeshProUGUI>().text;
              if (beforeText == currentText)
                return;

            //  SE��炷
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM,
                (int)SFXList.SFX_RESULT_TEXTNEXT);

            //���̃`�F�b�N�p�Ƀe�L�X�g�X�V
            beforeText = currentText;
 
        });

        //  �A�j���[�V������҂�
        yield return new WaitForSeconds(duration*message[stageNum].Length);

        //  �I�������{�^����\��
        nextButton.SetActive(true);

        //  PlayerInput�̃}�b�v��UI���[�h�ɕύX
        PlayerInput playerInput = GameManager.Instance.GetPlayer()
            .GetComponent<PlayerInput>();
        playerInput.enabled = true;
        playerInput.SwitchCurrentActionMap("Title_UI");

        //  �{�^����I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(nextButton.gameObject);



        yield return null;
    }
}
