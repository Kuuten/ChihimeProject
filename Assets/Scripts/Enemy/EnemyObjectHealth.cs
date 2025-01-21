using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G���I�u�W�F�N�g�̗̑̓N���X
//
//--------------------------------------------------------------
public class EnemyObjectHealth : MonoBehaviour
{
    public float hp;
    private bool bSuperMode;

    //  ����G�t�F�N�g
    [SerializeField] private GameObject explosion;

    private float flashInterval;
    private int loopCount;
    private int step;

    //���C���J�����ɕt���Ă���^�O��
    private static readonly string MAIN_CAMERA_TAG_NAME = "MainCamera";

    //�J�����ɕ\������Ă��邩
    private bool isRendered = false;

    void Awake()
    {
        step = 0;
        hp = 0;
        bSuperMode = true;      //  �ŏ��͖��G���[�hON
        loopCount = 1;          //  ���[�v�J�E���g��ݒ�
        flashInterval = 0.2f;   //  �_�ł̊Ԋu��ݒ�
    }

    void Update()
    {
        if(hp <= 0)return;

        switch(step)
        {
            case 0: //  ������ʊO
                //  ��ʓ��ɂȂ�����
                if(isRendered)
                {
                    bSuperMode = false; //  ���G���[�h����
                    step++;
                }
                break;

            case 1: //  �ʏ�
                break;
        }
    }

    //-------------------------------------------------
    //  �����蔻��
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  ����ł��邩��ʊO�Ȃ珈�����Ȃ�
        if(hp <= 0 || step == 0)return;


        if (collision.CompareTag("DeadWall"))
        {
            if(bSuperMode)return;

            //Destroy(this.gameObject);
        }
        else if (collision.CompareTag("NormalBullet"))
        {
            //  �e�̏���
            Destroy(collision.gameObject);

            //  ���G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperMode)return;

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetNormalShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                Death();                       //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  �_���[�W����
           float d = collision.GetComponent<ConvertDoujiBullet>().GetInitialPower();
            Damage(d);

            //  ���U�������U��������
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  ���o�[�X�g�Q�[�W�𑝂₷
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  ���o�[�X�g�Q�[�W���������₷
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }
                    
            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                Death();                      //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                Death();                      //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("TsukumoConvert"))
        {
            //  �e������
            Destroy(collision.gameObject);

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  ���U�������U��������
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  ���o�[�X�g�Q�[�W�𑝂₷
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  ���o�[�X�g�Q�[�W���������₷
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }
                    
            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                Death();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("TsukumoKonburst"))
        {
            //  �e������
            Destroy(collision.gameObject);

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                Death();                      //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetBombPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                Death();                        //  ���ꉉ�o
            }
        }
    }

    //------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------

    //-------------------------------------------
    //  �_���[�W����
    //-------------------------------------------
    public void Damage(float value)
    {
        if(hp > 0.0f)
        {
            hp -= value;
        }
        else
        {
            hp = 0.0f;
        }
    }

    //-------------------------------------------
    //  �_���[�W���̓_�ŉ��o
    //-------------------------------------------
    public IEnumerator Blink(bool super, int loop_count, float flash_interval)
    {
        SpriteRenderer sp = this.GetComponent<SpriteRenderer>();

        //  ���G���[�hON
        if(super)bSuperMode = true;

        //�_�Ń��[�v�J�n
        for (int i = 0; i < loop_count; i++)
        {
            //flashInterval�҂��Ă���
            yield return new WaitForSeconds(flash_interval);

            //spriteRenderer���I�t
            if(sp)sp.enabled = false;

            //flashInterval�҂��Ă���
            yield return new WaitForSeconds(flash_interval);

            //spriteRenderer���I��
            if(sp)sp.enabled = true;
        }
    }

    //-------------------------------------------
    //  ���ꉉ�o(�ʏ�e�E�{��)
    //-------------------------------------------
    private void Death()
    {
        //  ����G�t�F�N�g
        Instantiate(explosion, transform.position, transform.rotation);

        // ����SE
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_ENEMY_DEATH);

        //  �I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }

    //------------------------------------------------------
    //  �_���[�WSE���Đ������㖳�G���[�h���I�t�ɂ���
    //------------------------------------------------------
    IEnumerator PlayDamageSFXandSuperModeOff()
    {
        float interval = 0.1f;  //  ���G�����������܂ł̎��ԁi�b�j

        //  �_���[�WSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_ENEMY_DAMAGE);

        //  ���b���҂�
        yield return new WaitForSeconds(interval);

        //  ���G���[�hOFF
        bSuperMode = false;
    }

    //------------------------------------------------------
    //  �J�����ɉf���Ă�ԂɌĂ΂��
    //------------------------------------------------------
    private void OnWillRenderObject()
    {
        //���C���J�����ɉf����������_isRendered��L����
        if(Camera.current.name != "SceneCamera"  && Camera.current.tag == MAIN_CAMERA_TAG_NAME){
            isRendered = true;
        }
    }
}
