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

    //  大魂で得られる魂の数
    private const int konNumGainedFromLarge = 500;
    //  小魂で得られる魂の数
    private const int konNumGainedFromSmall = 100;
    //  最大強化時に強化アイテムで得られる魂の数
    private const int konNumGainedFromPowerup = 300;

    //  シングルトンなインスタンス
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

    //  魂（お金）を加算
    public void AddMoney(int value )
    {
        money += value;
        scoreText.text = "" + money;
    }

    //------------------------------------------------
    //  プロパティ
    //------------------------------------------------
    public int GetKonNumGainedFromLarge(){ return konNumGainedFromLarge; }
    public int GetKonNumGainedFromSmall(){ return konNumGainedFromSmall; }
    public int GetKonNumGainedFromPowerup(){ return konNumGainedFromPowerup; }
}
