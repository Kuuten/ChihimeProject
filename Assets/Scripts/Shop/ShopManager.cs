using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �V���b�v�V�[���Ǘ��N���X(����������)
//
//--------------------------------------------------------------
public class ShopManager : MonoBehaviour
{
    enum eMenuList
    {
        RedHeart,           //  �Ԃ��n�[�g
        DoubleupHeart,      //  �_�u���A�b�v�n�[�g
        GoldHeart,          //  ���F�̃n�[�g
        Bomb,               //  ��G�{��

    }

    //  �q�G�����L�[�̃e�L�X�g�N���X
    [SerializeField] private TextMeshProUGUI shopText;

    //  �ʏ�e�L�X�g
    private readonly string normalText = 
        "�܂��ǁI�_������Ł`�B\n����������Ȃ��Ă邩�甃���Ă��Ă�`�I";
    //  ���������s���e�L�X�g
    private readonly string failedText = "��������ւ�Ł`�I";
    //  �������������e�L�X�g
    private readonly string successText = "�������ɁI";

    bool canContorol;   //  ����\�t���O

    //  ���������{�^���̃I�u�W�F�N�g
    private GameObject redheartButton,doubleupHeartButton,goldheartButton,bombButton;

    //  �ē��׃{�^���I�u�W�F�N�g
    [SerializeField] private GameObject regenerateButton;

    //  �������I�u�W�F�N�g�̎q�I�u�G�N�gID
    private static readonly int soldout_id = 4;

    //  �A�C�e���̒l�i���{�^������擾����p
    private string RedHeartValueText;
    private string DoubleupHeartValueText;
    private string GoldHeartValueText;
    private string BombValueText;

    void Start()
    {
        StartCoroutine(StartInit());
    }

    //--------------------------------------------------------------
    //  �������R���[�`��
    //--------------------------------------------------------------
    IEnumerator StartInit()
    {
        //  �V���b�vBGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_SHOP);

        //  ����\�ɂ���
        canContorol = true;

        yield return null;
    }

    void Update()
    {
        //  ����s�\�Ȃ烊�^�[��
        if(!canContorol)return;
    }

    //--------------------------------------------------------------
    //  ���b�Z�[�W�E�B���h�E���o������
    //--------------------------------------------------------------
    IEnumerator DisplayMessage(string msg)
    {
        //  ���b�Z�[�W�I�u�W�F�N�g��\��
        shopText.text = msg;

        //  �P�b�҂�
        yield return new WaitForSeconds(2);

        //  �ʏ�e�L�X�g�ɖ߂�
        shopText.text = normalText;
    }
    
    //--------------------------------------------------------------
    //  ���s���b�Z�[�W�E�B���h�E���o������
    //--------------------------------------------------------------
    public IEnumerator DisplayFailedMessage()
    {
        //  ���b�Z�[�W�I�u�W�F�N�g��\��
        shopText.text = failedText;

        //  �P�b�҂�
        yield return new WaitForSeconds(2);

        //  �ʏ�e�L�X�g�ɖ߂�
        shopText.text = normalText;
    }

    //--------------------------------------------------------------
    //  �Ԃ��n�[�g�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnRedHeartButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(RedHeartValueText));

        Debug.Log($"�Ԃ��n�[�g�̒l�i��{value}�ł�");

        //  �񕜗�
        int heal_value = 2;

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  �̗͂��n�[�g�P�����₷
            PlayerHealth ph = GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>();
            ph.Heal(heal_value);

            //  Button��Interctive�𖳌���
            redheartButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            redheartButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));

            //  ���ݑ̗͂��擾
            int health = ph.GetCurrentHealth();
            Debug.Log($"�̗͂�{heal_value}�񕜂���{health}�ɂȂ����I");
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }

        
    }

    //--------------------------------------------------------------
    //  �_�u���A�b�v�n�[�g�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnDoubleupHeartButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(DoubleupHeartValueText));

        Debug.Log($"�_�u���A�b�v�n�[�g�̒l�i��{value}�ł�");

        //  �񕜗�
        int heal_value = 0;

        //  50%�̊m���Œǉ���
        int rand = Random.Range(1,101);

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            if(rand % 2 == 0)   //  �ǉ���
            {
                heal_value = 4;
            }
            else // �P������
            {
                heal_value = 2;
            }

            //  �̗͂𑝂₷
            PlayerHealth ph = GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>();
            ph.Heal(heal_value);

            //  Button��Interctive�𖳌���
            doubleupHeartButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            doubleupHeartButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));

            //  ���ݑ̗͂��擾
            int health = ph.GetCurrentHealth();
            Debug.Log($"�̗͂�{heal_value}�񕜂���{health}�ɂȂ����I");
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }
    }

    //--------------------------------------------------------------
    //  ���F�̃n�[�g�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnGoldHeartButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(GoldHeartValueText));;

        Debug.Log($"���F�̃n�[�g�̒l�i��{value}�ł�");

        //  �n�[�g�P���̗̑�
        int one_heart = int.Parse(RemoveKonText(GoldHeartValueText));

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  �ő�̗͂��n�[�g�P�����₷
            GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>()
                .IncreaseHP(one_heart);

            //  �ő�̗͂��擾
            PlayerHealth ph = GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>();
            int limit_health = ph.GetCurrentMaxHealth();
            
            //  �̗͂�S��
            GameManager.Instance.GetPlayer().GetComponent<PlayerHealth>()
                .Heal(limit_health);

            //  Button��Interctive�𖳌���
            goldheartButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            goldheartButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));

            //  ���ݑ̗͂��擾
            int health = ph.GetCurrentHealth();
            Debug.Log($"�ő�̗͂�2�����đS�񕜂����I���ݑ̗�:{health}");
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }
    }

    //--------------------------------------------------------------
    //  ��G�̃{���{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnHoneGBombButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(BombValueText));

        Debug.Log($"�{���̒l�i��{value}�ł�");

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            PlayerBombManager.Instance.AddBomb();

            //  Button��Interctive�𖳌���
            bombButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            bombButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));

            //  ���݂̃{�������擾
            int bombNum = PlayerBombManager.Instance.GetBombNum();
            Debug.Log($"�{�����P�R������{bombNum}�R�ɂȂ����I");
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }

    }

    //--------------------------------------------------------------
    //  �����񂩂獰����菜��
    //--------------------------------------------------------------
    private string RemoveKonText(string s)
    {
        return s.Replace("��","");
    }

    //--------------------------------------------------------------
    //  �v���p�e�B
    //--------------------------------------------------------------
    public void SetRedHeartButton(GameObject obj){ redheartButton = obj; }
    public void SetDoubleupHeartButton(GameObject obj){ doubleupHeartButton = obj; }
    public void SetGoldHeartButton(GameObject obj){ goldheartButton = obj; }
    public void SetBombButton(GameObject obj){ bombButton = obj; }
    public void SetRedHeartValueText(string s){ RedHeartValueText = s; }
    public void SetDoubleupValueText(string s){ DoubleupHeartValueText = s; }
    public void SetGoldHeartValueText(string s){ GoldHeartValueText = s; }
    public void SetBombValueText(string s){ BombValueText = s; }
}
