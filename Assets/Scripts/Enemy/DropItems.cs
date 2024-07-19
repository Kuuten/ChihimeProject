using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

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
    //  �v���n�u�̃A�h���X
    private string[] adress =
    {
        "item_smallKon",    //  Items.smallKon
        "item_largeKon",    //  Items.largeKon
        "item_powerup",     //  Items.PowerUp
        "item_speedup"      //  Items.SpeedUp
    };
    
    //  �A�C�e���̃v���n�u�i�[�p
    List<GameObject> Prefabs = new List<GameObject>();

    private void Start()
    {
        Prefabs = EnemyManager.Instance.GetDropItems();
    }

    //------------------------------------------------------------
    //  �h���b�v�����̓G�����񂾎��Ƀ����_����
    //  �p���[�A�b�v�A�C�e�����h���b�v����
    //------------------------------------------------------------
    public void DropPowerupItem()
    {
        //  �h���b�v�Ȃ��Ȃ烊�^�[��
        if(EnemyManager.Instance.GetDropType() == EnemyManager.DROP_TYPE.None)return;

        //  �����_���ȃA�C�e���𐶐�����
        int rand = Random.Range(
            (int)Items.PowerUp,
            (int)Items.Max);

        //  �G�����ꂽ�ꏊ�ɐ�������
        Vector3 pos = this.transform.position;
        Instantiate(Prefabs[rand], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  num�������h���b�v����
    //------------------------------------------------------------
    public void DropKonPrefab(GameObject Prefab, int num)
    {
        //  �G�����ꂽ�ꏊ�ɐ�������
        Vector3 pos = this.transform.position;

        for(int i=0;i<num;i++)
        {
            GameObject obj = Instantiate(
                Prefab,
                pos,
                Quaternion.identity);

            //  ���ۂ̃V�[����ł̉������擾
            SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
            float spriteWidth = sprite.bounds.size.x;
            
            //  �����������炵�Ĕz�u����
            pos.y = this.transform.position.y + Random.Range(-2,2);
            pos.x = (transform.position.x - num * spriteWidth / 2) + i * spriteWidth;

            //  �Ĕz�u
            obj.transform.position = pos;
        }
    }

    //------------------------------------------------------------
    //  ���Ƃ����̋��z����v�Z���č��𐶐�����
    //------------------------------------------------------------
    public void DropKon(int money)
    {
        //  �印�P������̍��l���ʂ��擾
        int largeKon = MoneyManager.Instance.GetKonNumGainedFromLarge();

        if (money < largeKon)
        {
            //  smallKon�̐����������𐶐�
            int smallKon = money;
            Debug.Log("�����̐��ł΂��� :" + smallKon);
            DropKonPrefab(Prefabs[(int)Items.smallKon], smallKon);
        }
        else // money��largeKon�ȏ�̏ꍇ
        {
            //  �印����v�Z����
            int largeKonNum = money / largeKon;

            //  �]��ŏ������v�Z����
            int remainder = money % largeKon;

            Debug.Log("�印�̐� :" + largeKonNum);
            Debug.Log("�����̐� :" + remainder);

            //  largeKonNum�̐������印�𐶐�
            DropKonPrefab(Prefabs[(int)Items.largeKon], largeKonNum);

            //  remainder�̐����������𐶐�
            DropKonPrefab(Prefabs[(int)Items.smallKon], remainder);
        }
    }
}
