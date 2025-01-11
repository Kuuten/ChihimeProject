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
    private GameObject redheartButton,doubleupHeartButton,goldheartButton,bombButton,
        PowerupButton,SpeedupButton,ShieldButton;

    //  �ē��׃{�^���I�u�W�F�N�g
    [SerializeField] private GameObject regenerateButton;

    //  �������I�u�W�F�N�g�̎q�I�u�G�N�gID
    private static readonly int soldout_id = 4;

    //  �A�C�e���̒l�i���{�^������擾����p
    private string RedHeartValueText;
    private string DoubleupHeartValueText;
    private string GoldHeartValueText;
    private string BombValueText;
    private string PowerupValueText;
    private string SpeedupValueText;
    private string ShieldValueText;

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
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }
    }
    //--------------------------------------------------------------
    //  �V���b�g�����{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnPowerupButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(PowerupValueText));

        Debug.Log($"�V���b�g�����̒l�i��{value}�ł�");

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  �ʏ�V���b�g�̃��x���A�b�v
            PlayerShotManager.Instance.LevelupNormalShot();
            PlayerShotManager.Instance.UpdateShotPowerIcon();

            //  Button��Interctive�𖳌���
            PowerupButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            PowerupButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }

    }
    //--------------------------------------------------------------
    //  �X�s�[�h�����{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnSpeedupButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(SpeedupValueText));

        Debug.Log($"�X�s�[�h�����̒l�i��{value}�ł�");

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  �X�s�[�h�̃��x���A�b�v
            PlayerMovement.Instance.LevelupMoveSpeed();
            PlayerMovement.Instance.UpdateSpeedLevelIcon();

            //  Button��Interctive�𖳌���
            SpeedupButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            SpeedupButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));
        }
        else
        {
            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(failedText));
        }

    }
    //--------------------------------------------------------------
    //  �V�[���h�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnShieldButtonDown()
    {
        //  �{�^���̃e�L�X�g���璼�ڑ�����擾
        int value = int.Parse(RemoveKonText(ShieldValueText));

        Debug.Log($"�V�[���h�̒l�i��{value}�ł�");

        //  �w���\�Ȃ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        {
            // �v���C���[�ɃV�[���h��ǉ����� 
            int hp =  PlayerHealth.Instance.GetCurrentHealth();
            PlayerHealth.Instance.SetIsShielded(true);
            PlayerHealth.Instance.CalculateHealthUI(hp);

            //  Button��Interctive�𖳌���
            ShieldButton.GetComponent<Button>().interactable = false;

            //  ��������\��(�{�^���I�u�W�F�N�g����ID�S�̎q�I�u�W�F�N�g)
            ShieldButton.transform.GetChild(soldout_id).gameObject.SetActive(true);

            //  �ē��׃{�^���I�u�W�F�N�g��I����Ԃɂ���
            EventSystem.current.SetSelectedGameObject(regenerateButton);

            //  ���b�Z�[�W�\��
            StartCoroutine(DisplayMessage(successText));

            //  �V�[���h���t�^����Ă��邩�ǂ������擾
            //bool canShield = PlayerHealth.Instance.GetBombNum();
            Debug.Log($"�V�[���h���t�^���ꂽ�I");
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
    public void SetPowerupButton(GameObject obj){ PowerupButton = obj; }
    public void SetSpeedupButton(GameObject obj){ SpeedupButton = obj; }
    public void SetShieldButton(GameObject obj){ ShieldButton = obj; }

    public void SetRedHeartValueText(string s){ RedHeartValueText = s; }
    public void SetDoubleupValueText(string s){ DoubleupHeartValueText = s; }
    public void SetGoldHeartValueText(string s){ GoldHeartValueText = s; }
    public void SetBombValueText(string s){ BombValueText = s; }
    public void SetPowerupValueText(string s){ PowerupValueText = s; }
    public void SetSpeedupValueText(string s){ SpeedupValueText = s; }
    public void SetShieldValueText(string s){ ShieldValueText = s; }
}
