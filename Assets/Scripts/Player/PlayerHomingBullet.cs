using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------
//
//  �v���C���[�̃z�[�~���O�e
//
//---------------------------------------------------
public class PlayerHomingBullet : MonoBehaviour
{
    //  ��ԋ߂��G�I�u�W�F�N�g�i�[�p
    GameObject target;
    //  �Q�[���̏��
    private int gamestatus;
    //
    private float speed = 5.0f;
    //  �e�̎���
    //[SerializeField] private float lifetime = 3;

    void Start()
    {
        target = null;

        //  �z�[�~���O�eSE�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_NORMAL_SHOT,
            (int)SFXList.SFX_NORMAL_SHOT);
    }

    void Update()
    {
        //  GameManager�����Ԃ��擾
        gamestatus = GameManager.Instance.GetGameState();

        //  �K���z�[�~���O
        Homing();
    }

    //-----------------------------------------------------------
    //  �K���z�[�~���O(������܂Œǂ�����������)
    //-----------------------------------------------------------
    private void Homing()
    {
        //  ��ԋ߂��G�I�u�W�F�N�g���擾����
        if(gamestatus == (int)eGameState.Zako)
        {
            target = EnemyManager.Instance.GetNearestEnemyFromPlayer();
        }
        else if(gamestatus == (int)eGameState.Boss)
        {
            target = EventSceneManager.Instance.GetBossObject();
        }else if(gamestatus == (int)eGameState.Event)
        {
            Destroy(this.gameObject);
            return;
        }

        //  �G�����Ȃ���Βe���������Ė߂�
        if(target == null)
        {
            //Debug.Log("�G�����Ȃ��I");
            Destroy(this.gameObject);
            return;
        }

        //  �e����G�ւ̃x�N�g�������߂�
        Vector3 vec = target.transform.position - this.transform.position;

        //  �x�N�g���̃I�C���[�p�����߂�
        Quaternion q = Quaternion.Euler(vec);
        float degree = q.eulerAngles.z;

        //  �e�̌�����G�Ɍ�����
        this.transform.Rotate(0,0,degree);

        //  ��ԋ߂��G�֓ˌ�
        this.transform.position += vec * speed * Time.deltaTime;
    }
}
