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
    private const int konNumGainedFromLarge = 10;
    //  �����œ����鍰�̐�
    private const int konNumGainedFromSmall = 1;

    //  �V���O���g���ȃC���X�^���X
    public static MoneyManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
}
