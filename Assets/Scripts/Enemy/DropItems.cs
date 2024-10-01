using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using static EnemyManager;

//--------------------------------------------------------------
//
//  �G�ɃA�C�e�����h���b�v������N���X
//
//--------------------------------------------------------------

// ���A�C�e���̎�� 
public enum KonItems
{
    smallKon,
    largeKon,
    
    Max
}

// �p���[�A�b�v�A�C�e���̎�� 
public enum ePowerupItems
{
    None = -2,
    Random,      // �����_��
    PowerUp,     //  �e�̃��x���A�b�v
    SpeedUp,     //  ���@�̃X�s�[�h�A�b�v
    Heal,        //  �n�[�g����
    Bomb,        //  �{�����PUP
    Ossan,       //  �������Ⴂ��������
    
    Max
}

public class DropItems : MonoBehaviour
{
    //  �v���n�u�̃A�h���X
    private string[] adress =
    {
        "item_smallKon",    //  KonItems.smallKon
        "item_largeKon",    //  KonItems.largeKon
        "item_powerup",     //  ePowerupItems.PowerUp
        "item_speedup",     //  ePowerupItems.SpeedUp
        "item_heart",       //  ePowerupItems.Heal
        "item_bomb",        //  ePowerupItems.Bomb
    };
    
    //  �A�C�e���̃v���n�u�i�[�p
    List<GameObject> konPrefabs = new List<GameObject>();
    List<GameObject> powerupPrefabs = new List<GameObject>();

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    [SerializeField] EnemyManager.DROP_TYPE dropType;

    void Start()
    {
        konPrefabs = EnemyManager.Instance.GetKonItems();
        powerupPrefabs = EnemyManager.Instance.GetPowerupItems();
    }

    //------------------------------------------------------------
    //  �h���b�v�����̓G�����񂾎��Ƀ����_����
    //  �p���[�A�b�v�A�C�e�����h���b�v����
    //------------------------------------------------------------
    public void DropRandomPowerupItem()
    {
        //  �h���b�v�Ȃ��Ȃ烊�^�[��
        if(dropType == EnemyManager.DROP_TYPE.None)return;

        //  �����_���ȃA�C�e���𐶐�����
        int rand = Random.Range(0,100);

        Vector3 pos = this.transform.position;

        if(rand == 0)    //  �m���P���ł������Ⴂ�������񂪏o��
        {
            EnemyManager.Instance.SetEnemy(
                EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.EX),
                pos
            );
        }
        else // ���̑��͊m���Ȃ��̃����_��
        {
            int num = Random.Range(
                (int)ePowerupItems.PowerUp,
                (int)ePowerupItems.Ossan
            );

            //  �G�����ꂽ�ꏊ�ɐ�������
            Instantiate(powerupPrefabs[num], pos, Quaternion.identity);
        }
    }

    //------------------------------------------------------------
    //  �h���b�v�����̓G�����񂾎��Ɏw�肳�ꂽs
    //  �p���[�A�b�v�A�C�e�����h���b�v����
    //------------------------------------------------------------
    public void DropPowerupItem(ePowerupItems item)
    {
        //  �h���b�v�Ȃ��Ȃ烊�^�[��
        if(dropType == EnemyManager.DROP_TYPE.None)return;

        if(item == ePowerupItems.None || item == ePowerupItems.Random)
            return;

        //  �������Ⴂ��������̃Z�b�g
        if(item == ePowerupItems.Ossan)
        {            
            //  �G�����ꂽ�ꏊ�ɐ�������
            Vector3 pos = this.transform.position;
            EnemyManager.Instance.SetEnemy(
                EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.EX),
                pos
            );
        }
        else // ���̑��͒ʏ�ʂ�
        {
            //  �G�����ꂽ�ꏊ�ɐ�������
            Vector3 pos = this.transform.position;
            Instantiate(powerupPrefabs[(int)item], pos, Quaternion.identity);
        }
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
        int largeKon = MoneyManager.Instance.GetKonNumGainedFromLarge(); // 500 
        int smallKon = MoneyManager.Instance.GetKonNumGainedFromSmall(); // 100

        if (money < largeKon)
        {
            //  smallKon�̐����������𐶐�
            int small = money / smallKon;
            Debug.Log("�����̐� :" + small);
            DropKonPrefab(konPrefabs[(int)KonItems.smallKon], small);
        }
        else // money��largeKon�ȏ�̏ꍇ
        {
            //  �印����v�Z����
            int largeKonNum = money / largeKon;

            //  �]��ŏ������v�Z����
            int remainder = money % largeKon;
            int small = remainder / smallKon;

            Debug.Log("�印�̐� :" + largeKonNum);
            Debug.Log("�����̐� :" + small);

            //  largeKonNum�̐������印�𐶐�
            DropKonPrefab(konPrefabs[(int)KonItems.largeKon], largeKonNum);

            //  remainder�̐����������𐶐�
            DropKonPrefab(konPrefabs[(int)KonItems.smallKon], small);
        }
    }
}
