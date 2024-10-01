using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  �p���[�A�b�v�A�C�e���N���X
//
//--------------------------------------------------------------
public class PowerupItem : MonoBehaviour
{
    //  �v���C���[�̃N���X���擾
    private GameObject player;

    void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    void Update()
    {
 
    }

    //-------------------------------------------------------
    //  �v���C���[�Ɠ���������p���[�A�b�v����
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  �^�O���v���C���[�Ȃ�p���[�A�b�v����
        if(collision.CompareTag("Player"))
        {
            PlayerShotManager ps = player.GetComponent<PlayerShotManager>();
            if(ps == null)return;

            //  �V���b�g���x�����ő傶��Ȃ���΃��x���A�b�v
            if(ps.GetNormalShotLevel() < (int)eNormalShotLevel.Lv3 )
            {
                //  �p���[�A�b�v
                ps.LevelupNormalShot();
                Debug.Log("�ʏ�e�̃p���[���x����" + ps.GetNormalShotLevel() + "�ɂȂ�܂����I");
            }
            else
            {
                //  �ő僌�x���̎����ƍ��l��
                int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
                MoneyManager.Instance.AddMoney( money );
                Debug.Log("�V���b�g���ő僌�x���Ȃ̂ō�" + money + "���l�����܂����I");
            }

            //  �A�C�e��������
            Destroy(this.gameObject);
        }
    }
}
