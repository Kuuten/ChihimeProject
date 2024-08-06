using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �{���A�C�e���N���X
//
//--------------------------------------------------------------
public class BombItem : MonoBehaviour
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
    //  �v���C���[�Ɠ���������{�������PUP����
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //---------------------------
        //  �{��1���񕜂���
        //---------------------------

        //  �^�O���v���C���[�ȊO�Ȃ�return
        if(!collision.CompareTag("Player"))return;

        PlayerBombManager pb = player.GetComponent<PlayerBombManager>();
        if (pb == null) return;

        //  �{�����ő傶��Ȃ���Α���
        if (pb.GetBombNum() < pb.GetBombMaxNum())
        {
            //  �{���𑝉�
            pb.AddBomb();

            Debug.Log($"�v���C���[�̃{�������P�񕜂���\n" +
                $"{pb.GetBombNum()}�ɂȂ�܂����I");
        }
        else
        {
            //  �ő�̗͂̎����ƍ��l��
            int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
            MoneyManager.Instance.AddMoney(money);
            Debug.Log($"�{�����ő�Ȃ̂ō�{money}���l�����܂����I");
        }

        //  �A�C�e��������
        Destroy(this.gameObject);
    }
}
