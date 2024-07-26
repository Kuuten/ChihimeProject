using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//--------------------------------------------------------------
//
//  �����Ǘ��N���X
//
//--------------------------------------------------------------
public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private int money = 0;

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
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        scoreText.text = "" + money;
    }

    void Update()
    {
        
    }

    //  ���i�����j�����Z
    public void AddMoney(int value )
    {
        money += value;
        scoreText.text = "" + money;
    }

    //------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------
    public int GetKonNumGainedFromLarge(){ return konNumGainedFromLarge; }
    public int GetKonNumGainedFromSmall(){ return konNumGainedFromSmall; }
    public int GetKonNumGainedFromPowerup(){ return konNumGainedFromPowerup; }
}
