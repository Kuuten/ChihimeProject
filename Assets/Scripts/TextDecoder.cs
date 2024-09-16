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
        //  ��Ńe�L�X�g�t�@�C�������[�h����悤�ɕύX����

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

                Text.text = Times; //Text�ɓ���܂��B
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    count = 0; //��i���ځj�����Z�b�g����
                    textNum++; //�s�����i���j�ɂ���
                }
            }
        }
    }
}
