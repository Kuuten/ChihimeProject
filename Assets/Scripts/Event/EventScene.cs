using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using DG.Tweening;

//--------------------------------------------------------------
//
//  �C�x���g�V�[���̊�{�N���X
//
//--------------------------------------------------------------
public abstract class EventScene : MonoBehaviour
{
    //------------------------------------------------------------
    //  �C�x���g���J�n����
    //------------------------------------------------------------
    public abstract IEnumerator PlayEvent();

    //-------------------------------------------------------------
    //  �v���p�e�B
    //-------------------------------------------------------------


    //-------------------------------------------------------------
    //  ��UI�ƃe�L�X�g��ύX����
    //-------------------------------------------------------------
    protected IEnumerator ChangeFaceAndText(Frame frame,FaceType type)
    {
        //  �C�x���g�L�����o�X���L��������ĂȂ���ΗL��������
        if(!EventSceneManager.Instance.GetEventCanvasActive())
        {
            EventSceneManager.Instance.SetEventCanvasActive(true);
        }

        //  �w��t���[�����L��������ĂȂ���ΗL��������
        if(!EventSceneManager.Instance.GetFrameObjectActive(frame))
        {
            EventSceneManager.Instance.SetFrameObjectActive(frame, true);
        }

        //  ��UI���v���n�u���w�肵�ĕύX
        EventSceneManager.Instance.SetFaceToFrame(frame,type);

        //  �e�L�X�g�Ɉ������w��
        string gameText = EventSceneManager.Instance.GetGameText();

        EventSceneManager.Instance.SetTextToFrame(frame,gameText);

        yield return null;
    }

    //-------------------------------------------------------------
    //  �{�X�𐶐����Ďw��̍��W�Ɉړ�������
    //-------------------------------------------------------------
    protected IEnumerator CreateBossAndMove(BossType type,Vector2 target)
    {
        float duration = 1.0f;

        //  �{�X�𐶐�����
        Vector2 pos = new Vector2(-1,11);   //  �����ʒu
        EventSceneManager.Instance.InstantiateBossPrefab(type,pos);

        //  �{�X���Z�b�g
        EnemyManager.Instance.SetBoss(type, ePowerupItems.PowerUp);

        //  �{�X�̃^�C�v�ɂ���ăR���|�[�l���g�𖳌���
        GameObject bossObject = EventSceneManager.Instance.GetBossObject();
        if(type == BossType.Douji)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Tsukumo)
        {
            bossObject.GetComponent<BossTsukumo>().enabled = false;
        }
        else if(type == BossType.Kuchinawa)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Kurama)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Wadatsumi)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        else if(type == BossType.Hakumen)
        {
            bossObject.GetComponent<BossDouji>().enabled = false;
        }
        bossObject.GetComponent<BoxCollider2D>().enabled = false;

        //  �ڕW���W�Ɍ������Ĉړ��J�n
        bossObject.GetComponent<RectTransform>().DOAnchorPos(target,duration);

        //  �R�b�҂�
        yield return new WaitForSeconds(duration);
    }

    //-------------------------------------------------------------
    //  �{�X�̖��O���A���t�@�A�j��������
    //-------------------------------------------------------------
    protected IEnumerator AlphaAnimationBossName()
    {
        float duration = 5.0f;  //  �t�F�[�h�C���ɂ����鎞��
        float duration2 = 2.0f; //  �t�F�[�h�A�E�g�ɂ����鎞��

        //  �\��������e��\��
        string[] boss_name =
        {
            "�����V �h�E�W",
            "�����V �c�N��",
            "�֖��V �N�`�i��",
            "�����V �N���}",
            "�C���V ���_�c�~",
            "�����V �n�N����",
        };

        //******************************************************
        //  ���̃X�e�[�W�ɂ���ă{�X�̖��O��ݒ�
        //******************************************************

        //  �e�L�X�g��L����
        EventSceneManager.Instance.SetBossNameTextActive(true);

        //  �{�X�̖��O��ݒ�
        EventSceneManager.Instance.SetBossName(boss_name[(int)PlayerInfoManager.stageInfo]);

        //  �e�L�X�g�̃A���t�@��0�ɂ���
        EventSceneManager.Instance.GetBossNameTextObj()
            .GetComponent<TextMeshProUGUI>().DOFade(0f,0f);

        yield return null;

        //  ������Ԃ���������o������
         EventSceneManager.Instance.GetBossNameTextObj()
            .GetComponent<TextMeshProUGUI>().DOFade(1f,duration)
                .OnComplete
                (
                    ()=>EventSceneManager.Instance.GetBossNameTextObj()
                        .GetComponent<TextMeshProUGUI>().DOFade(0f, duration2)
                )
                .SetEase(Ease.Linear);
    }

    //-------------------------------------------------------------
    //  �{�X��O�C�x���g�̏I���������J�n
    //-------------------------------------------------------------
    protected IEnumerator EndBeforeBattle()
    {
        //  �L�����o�X��OFF
        EventSceneManager.Instance.SetEventCanvasActive(false);

        //  ���E�̏�C�I�u�W�F�N�g��L����
        EventSceneManager.Instance.SetFogObjectActiveL(true);
        EventSceneManager.Instance.SetFogObjectActiveR(true);

        //  ��p�̔w�i�I�u�W�F�N�g��L����
        EventSceneManager.Instance.SetBossBackGroundObj(true);

        //  �{�X�o��SE�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_BOSS_APPEAR);

        //  �{�X�̖��O���A���t�@�A�j��������
        StartCoroutine(AlphaAnimationBossName());

        //  �V�b�҂�
        yield return new WaitForSeconds(7);

        //  �{�X���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Boss);

        //  �{�X�R���|�[�l���g�𖳌���
        GameObject boss = EventSceneManager.Instance.GetBossObject();
        if(boss.GetComponent<BossDouji>())boss.GetComponent<BossDouji>().enabled = false;
        else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        //else if(boss.GetComponent<BossTsukumo>())boss.GetComponent<BossTsukumo>().enabled = false;
        
        Debug.Log("***�{�X�탂�[�h�ɂȂ�܂����B***");

        //  �{�X��J�n�t���OTRUE
        EventSceneManager.Instance.SetStartBoss(true);

        //  �C�x���g�V�[���}�l�[�W���[�𖳌���
        this.gameObject.SetActive(false);
    }

    //-------------------------------------------------------------
    //  �{�X���C�x���g�̏I���������J�n
    //-------------------------------------------------------------
    protected IEnumerator EndAfterBattle()
    {
        //  �t���[���I�u�W�F�N�g���\���ɂ���
        EventSceneManager.Instance.SetFrameObjectActive(Frame.TOP,false);
        EventSceneManager.Instance.SetFrameObjectActive(Frame.BOTTOM,false);

        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        Pauser.Pause();

        //  �v���C���[��Animator�𖳌���
        GameManager.Instance.GetPlayer().GetComponent<Animator>().enabled = false;

        //  ���ʕ\���J�n�t���OTRUE
        EventSceneManager.Instance.SetStartResult(true);

        yield return null;
    }
}

