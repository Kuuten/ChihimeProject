using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerShotManager;

//--------------------------------------------------------------
//
//  �v���C���[�̃c�N�����o�[�X�g�e�N���X
//
//--------------------------------------------------------------
public class KonburstTsukumoBullet : MonoBehaviour
{
    private int gamestatus;
    private bool fripY;
    private float priod = 5.0f;  //  �����i�b�j���R�b�ȏ�ɂ��邱�ƁI

    private bool isL;

    private Vector3 posL;
    private Vector3 posR;

    [SerializeField] private GameObject bulletPrefab;

    void Start()
    {
        //  ���������������
        Destroy(gameObject, priod);

        //  �����̂R�b�O�ɓ_�ŊJ�n
        StartCoroutine(Blink(15,0.2f)) ;
    }

    void Update()
    {
        //  �v���C���[�̍��W
        GameObject player = GameManager.Instance.GetPlayer();
        Vector3 playerPos = player.transform.position;

        //  �v���C���[�̍��E�x�N�g�������߂�
        Vector3 left = -player.transform.right;
        Vector3 right = player.transform.right;

        //  ���E�ɂP���ꂽ���W�����߂�
        float bias = 2.0f; 
        Vector3 posL = playerPos + left * bias;
        Vector3 posR = playerPos + right * bias;

        //  ���W���X�V
        if(isL)
        {
            transform.position = posL;
        }
        else
        {
            transform.position = posR;
        }

        //  �e�̍X�V
        UpdateBullet();
        
    }

    //-------------------------------------------
    //  �v���p�e�B
    //-------------------------------------------
    public void SetFripY(bool frip){ fripY = frip; }
    public bool GetFripY(){ return fripY; }
    public void SetPosL(Vector3 pos){ posL = pos; }
    public Vector3 GetPosL(){ return posL; }
    public void SetPosR(Vector3 pos){ posR = pos; }
    public Vector3 GetPosR(){ return posR; }
    public void SetIsL(bool b){ isL = b; }
    public bool GetIsL(){ return isL; }

    //-------------------------------------------
    //  �_�ŉ��o
    //-------------------------------------------
    private IEnumerator Blink(int loop_count, float flash_interval)
    {
        //  �R�b�O�ɂȂ�܂ő҂�
        yield return new WaitForSeconds(priod - 3.0f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        //�_�Ń��[�v�J�n
        for (int i = 0; i < loop_count; i++)
        {
             //  SE�Đ�
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM2,
                (int)SFXList.SFX_KONBURST_TSUKUMO_TIMELIMIT);

            //flashInterval�҂��Ă���
            yield return new WaitForSeconds(flash_interval);

            //spriteRenderer���I�t
            if(sr)sr.enabled = false;

            //flashInterval�҂��Ă���
            yield return new WaitForSeconds(flash_interval);

            //spriteRenderer���I��
            if(sr)sr.enabled = true;
        }
    }

    //-------------------------------------------
    //  �l�`����z�[�~���O�e���ˏo��������
    //-------------------------------------------
    private void HomingShot(bool flipY)
    {
        //  �ʏ�e����
        GameObject obj = Instantiate(
                            bulletPrefab,
                            transform.position,
                            Quaternion.identity);

        //  SpriteRenderer���擾
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        //  Y�𔽓]���邩�ǂ����ݒ肷��
        sr = obj.GetComponent<SpriteRenderer>();
        sr.flipY = flipY;
    }

    //-------------------------------------------
    //  �e���X�V
    //-------------------------------------------
    private void UpdateBullet()
    {
        //  GameManager�����Ԃ��擾
        gamestatus = GameManager.Instance.GetGameState();

        //  �e�̌���
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                HomingShot(true);
                break;
            case (int)eGameState.Boss:
                HomingShot(false);
                break;
            case (int)eGameState.Event:
                break;
        }
    }
}
