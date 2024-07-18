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
}
