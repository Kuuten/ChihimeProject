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
    //  �A�C�e���̃v���n�u�i�[�p
    GameObject SmallKonPrefab;
    GameObject LargeKonPrefab;
    List<GameObject> powerupPrefabs;
    List<GameObject> speedupPrefabs;
    List<GameObject> bombPrefabs;
    List<GameObject> healPrefabs;

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    [SerializeField] EnemyManager.DROP_TYPE dropType;

    void Start()
    {
        //SmallKonPrefab = new GameObject();
        //LargeKonPrefab = new GameObject();
        powerupPrefabs = new List<GameObject>();
        speedupPrefabs = new List<GameObject>();
        bombPrefabs = new List<GameObject>();
        healPrefabs = new List<GameObject>();


        SmallKonPrefab = EnemyManager.Instance.GetSmallKon();
        LargeKonPrefab = EnemyManager.Instance.GetLargeKon();
        powerupPrefabs = EnemyManager.Instance.GetPowerupItems();
        speedupPrefabs = EnemyManager.Instance.GetSpeedupItems();
        bombPrefabs = EnemyManager.Instance.GetBombItems();
        healPrefabs = EnemyManager.Instance.GetHealItems();
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

            if(num == (int)ePowerupItems.PowerUp)
            {
                //  �G�����ꂽ�ꏊ�ɐ�������
                Instantiate(powerupPrefabs[0], pos, Quaternion.identity);
            }
            else if(num == (int)ePowerupItems.SpeedUp)
            {
                //  �G�����ꂽ�ꏊ�ɐ�������
                Instantiate(speedupPrefabs[0], pos, Quaternion.identity);
            }
            else if(num == (int)ePowerupItems.Bomb)
            {
                //  �G�����ꂽ�ꏊ�ɐ�������
                Instantiate(bombPrefabs[0], pos, Quaternion.identity);
            }
            else if(num == (int)ePowerupItems.Heal)
            {
                //  �G�����ꂽ�ꏊ�ɐ�������
                Instantiate(healPrefabs[0], pos, Quaternion.identity);
            }

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

        //  �G�����ꂽ�ꏊ�ɐ�������
        Vector3 pos = this.transform.position;

        //  �������Ⴂ��������̃Z�b�g
        if(item == ePowerupItems.Ossan)
        {            
            EnemyManager.Instance.SetEnemy(
                EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.EX),
                pos
            );
        }
        else if(item == ePowerupItems.PowerUp)
        {
            Instantiate(powerupPrefabs[0], pos, Quaternion.identity);
        }
        else if(item == ePowerupItems.SpeedUp)
        {
            Instantiate(speedupPrefabs[0], pos, Quaternion.identity);
        }
        else if(item == ePowerupItems.Bomb)
        {
            Instantiate(bombPrefabs[0], pos, Quaternion.identity);
        }
        else if(item == ePowerupItems.Heal)
        {
            Instantiate(healPrefabs[0], pos, Quaternion.identity);
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
            DropKonPrefab(SmallKonPrefab, small);
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
            DropKonPrefab(LargeKonPrefab, largeKonNum);

            //  remainder�̐����������𐶐�
            DropKonPrefab(SmallKonPrefab, small);
        }
    }
}
