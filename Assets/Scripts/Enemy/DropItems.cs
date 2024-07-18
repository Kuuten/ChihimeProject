using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//--------------------------------------------------------------
//
//  �G�ɃA�C�e�����h���b�v������N���X
//
//--------------------------------------------------------------

// �h���b�v�A�C�e���̎�� 
public enum Items
{
    smallKon,
    largeKon,
    PowerUp,     //  �e�̃��x���A�b�v
    SpeedUp,     //  ���@�̃X�s�[�h�A�b�v
    
    Max,
}

public class DropItems : MonoBehaviour
{
    //  �h���b�v����A�C�e���̃v���n�u�B
    [SerializeField] private GameObject[] dropItems;

    void Start()
    {
        Assert.IsFalse((int)Items.Max != dropItems.GetLength(0),
            "�z��̗v�f���Ɨ񋓂̐��������܂���I");
    }

    //------------------------------------------------------------
    //  �h���b�v�����̓G�����񂾎��Ƀ����_����
    //  �p���[�A�b�v�A�C�e�����h���b�v����
    //------------------------------------------------------------
    public void DropPowerupItem()
    {
        //  �����_���ȃA�C�e���𐶐�����
        int rand = Random.Range(
            (int)Items.PowerUp,
            (int)Items.Max);

        //  �G�����ꂽ�ꏊ�ɐ�������
        Vector3 pos = this.transform.position;
        Instantiate(dropItems[rand], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  �������h���b�v����
    //------------------------------------------------------------
    public void DropSmallKon()
    {
        //  �G�����ꂽ�ꏊ�ɐ�������
        float bias = 5.0f;
        Vector3 pos = this.transform.position;
        pos.y = this.transform.position.y + 1.0f;
        pos.x = Random.Range( transform.position.x-bias, transform.position.x + bias );

        Instantiate(dropItems[(int)Items.smallKon], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  �印���h���b�v����
    //------------------------------------------------------------
    public void DropLargeKon()
    {
        //  �G�����ꂽ�ꏊ�ɐ�������
        float bias = 5.0f;
        Vector3 pos = this.transform.position;
        pos.y = this.transform.position.y + 1.0f;
        pos.x = Random.Range( transform.position.x-bias, transform.position.x + bias );

        Instantiate(dropItems[(int)Items.largeKon], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  ���Ƃ����̋��z����v�Z���č��𐶐�����
    //------------------------------------------------------------
    public void DropKon(int money)
    {
        
    }
}
