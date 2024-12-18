using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �印���v���C���[�Ɏ擾������N���X
//
//--------------------------------------------------------------
public class LargeKon : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  �v���C���[�Ɠ��������炨���𑝂₷
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  �v���C���[�Ȃ獰���������Ă����𑝂₷
        if(collision.CompareTag("Player"))
        {
            // �A�C�e���l��SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_KON);

            //  �����𑝂₷
            int money = MoneyManager.Instance.GetKonNumGainedFromLarge();

            MoneyManager.Instance.AddMoney(money);

            //  �I�u�W�F�N�g������
            Destroy(this. gameObject);
        }
    }
}
