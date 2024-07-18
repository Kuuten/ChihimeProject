using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//--------------------------------------------------------------
//
//  お金管理クラス
//
//--------------------------------------------------------------
public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private int money = 0;

    //  シングルトンなインスタンス
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

    //  魂（お金）を加算
    public void AddMoney(int value )
    {
        money += value;
        scoreText.text = "" + money;
    }
}
