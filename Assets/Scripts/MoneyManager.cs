using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Live2D.Cubism.Framework.Json;

//--------------------------------------------------------------
//
//  �����Ǘ��N���X(�W�P�^�܂ŕ\��)
//
//--------------------------------------------------------------
public class MoneyManager : MonoBehaviour
{
    //  ���ɕ\�������e�L�X�g
    [SerializeField] private TextMeshProUGUI scoreText;
    private int money;
    private int targetMoney;
    private bool countFlag;

    //  �印�œ����鍰�̐�
    private const int konNumGainedFromLarge = 500;
    //  �����œ����鍰�̐�
    private const int konNumGainedFromSmall = 100;
    //  �ő勭�����ɋ����A�C�e���œ����鍰�̐�
    private const int konNumGainedFromPowerup = 300;

    //  �V���O���g���ȃC���X�^���X
    public static MoneyManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    void Start()
    {
        scoreText.text = "" + money;

        //  PlayerInfoManager��������Z�b�g
        money = PlayerInfoManager.g_KONNUM;
        targetMoney = money;
        countFlag = true;
    }

    void Update()
    {
        //  countFlag��true�Ȃ�A�j���[�V�����J�n
        if(countFlag)StartCoroutine(CountMoney());

        //  �����̃e�L�X�g���X�V
        scoreText.text = $"{money}";
    }

    //  ���i�����j�����Z
    public void AddMoney(int value )
    {
        //money += value;
        //scoreText.text = "" + money;

        //  �ڕW�l���X�V
        if(targetMoney >= 99999999)
        {
            targetMoney = 99999999;
        }
        else targetMoney += value;
    }

    //  ���i�����j�����Z
    public void SubMoney(int value )
    {
        //  �ڕW�l���X�V
        if(targetMoney <= 0)
        {
            targetMoney = 0;
        }
        else targetMoney -= value;
    }

    private IEnumerator CountMoney()
    {
        //  �t���O��false��
       countFlag = false;

       yield return StartCoroutine(CountAnimation());
    }

    private IEnumerator CountAnimation()
    {
        // ���l�̕ύX
        DOTween.To(
            () => money,          // ����Ώۂɂ���̂�
            num => money = num,   // �l�̍X�V
            targetMoney,          // �ŏI�I�Ȓl
            1.0f                  // �A�j���[�V��������
        )
            .OnComplete(() =>
            {
                //  �t���O�����Z�b�g
                countFlag = true;
            });

        yield return null;
    }

    //-------------------------------------------------------------------------------
    //  �v���p�e�B
    //-------------------------------------------------------------------------------
    public int GetKonNumGainedFromLarge(){ return konNumGainedFromLarge; }
    public int GetKonNumGainedFromSmall(){ return konNumGainedFromSmall; }
    public int GetKonNumGainedFromPowerup(){ return konNumGainedFromPowerup; }
    public int GetKonNum(){ return money; }
    public void SetKonNum(int num){ money = num; }

    //-------------------------------------------------------------------------------
    //  �c���ŃA�C�e�����w���\���ǂ����𔻒肷��(value:�A�C�e���̒l�i)
    //-------------------------------------------------------------------------------
    public bool CanBuyItem(int value)
    {
        int kon = targetMoney - value;

        //  ����������Ȃ�������
        if(kon < 0)
        {
            //  SE���Đ�
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_INCORRECT);

            return false;
        }
        //  �w�������I
        else
        {
            //  SE���Đ�
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_RESULT_CASH);

            //  ���������炷
            SubMoney(value);

            return true;
        }
    }
}
