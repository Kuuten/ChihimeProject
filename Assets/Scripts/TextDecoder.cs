using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class TextDecoder : MonoBehaviour
{
    int textNum = 0, count = 0;

    [SerializeField] TextMeshProUGUI Text;
    [SerializeField] TextAsset TextFile;

    List<string[]> TextData = new List<string[]>();

    void Start()
    {
        //-------------------------------------------------------
        //
        //  後でテキストファイルをロードするように変更する

        //-------------------------------------------------------

        StringReader reader = new StringReader(TextFile.text);

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            TextData.Add(line.Split(','));
        }

    }

    void Update()
    {
        string Times = TextData[textNum][count].ToString();

        if (Times != "ENDTEXT")
        {
            if (Times != "ENDLINE")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    count++;
                }

                Text.text = Times; //Textに入れます。
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    count = 0; //列（項目）をリセットする
                    textNum++; //行を下（次）にする
                }
            }
        }
    }
}
