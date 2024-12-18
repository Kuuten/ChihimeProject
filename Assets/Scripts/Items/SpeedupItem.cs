using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  �X�s�[�h�A�b�v�A�C�e���N���X
//
//--------------------------------------------------------------
public class SpeedupItem : MonoBehaviour
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
        //  �^�O���v���C���[�ȊO�Ȃ�return
        if(!collision.CompareTag("Player"))return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if(pm == null)return;

        //  �X�s�[�h���x�����ő傶��Ȃ���΃��x���A�b�v
        if(pm.GetSpeedLevel() < (int)eSpeedLevel.Lv3 )
        {
            // �A�C�e���l��SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_POWERUP);

            //  �X�s�[�h�A�b�v
            pm.LevelupMoveSpeed();
            Debug.Log("���@���X�s�[�h���x��" + pm.GetSpeedLevel() + "�ɂȂ�܂����I");
        }
        else
        {
            // �A�C�e���l��SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_KON);

            //  �ő僌�x���̎����ƍ��l��
            int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
            MoneyManager.Instance.AddMoney( money );
            Debug.Log("���@�X�s�[�h���ő僌�x���Ȃ̂ō�" + money + "���l�����܂����I");
        }
        
        //  �A�C�e��������
        Destroy(this.gameObject);
    }
}
