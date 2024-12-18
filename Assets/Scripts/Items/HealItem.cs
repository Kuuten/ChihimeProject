using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �񕜃A�C�e���N���X
//
//--------------------------------------------------------------
public class HealItem : MonoBehaviour
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
    //  �v���C���[�Ɠ���������񕜂���
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //---------------------------
        //  �n�[�g1���񕜂���
        //---------------------------

        //  �^�O���v���C���[�ȊO�Ȃ�return
        if(!collision.CompareTag("Player"))return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if(ph == null)return;

        //  �̗͂��ő傶��Ȃ���Ή�
        if(ph.GetCurrentHealth() < ph.GetCurrentMaxHealth() )
        {
            // �A�C�e���l��SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_HEART);

            //  ��
            int heal_health = 2;
            ph.Heal(heal_health);
            int health = ph.GetCurrentHealth();
        }
        else
        {
            // �A�C�e���l��SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_KON);

            //  �ő�̗͂̎����ƍ��l��
            int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
            MoneyManager.Instance.AddMoney( money );
            Debug.Log($"�̗͂��ő�Ȃ̂ō�{money}���l�����܂����I");
        }

        //  �A�C�e��������
        Destroy(this.gameObject);
    }
}
