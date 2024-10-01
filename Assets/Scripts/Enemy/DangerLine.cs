using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �h�E�W��Phase2�̒e�̗\���i�H��\������N���X
//
//--------------------------------------------------------------
public class DangerLine : MonoBehaviour
{
    // �ǂ����̕����̃��C����
    public enum LINE_TYPE {
        Horizontal,    //  �c
       Vertical        //  ��
    }
    public LINE_TYPE lineType = LINE_TYPE.Horizontal;

    void Start()
    {
        float duration = 0.5f; //  �����鎞��
        int loopNum = 2;       //  ���[�v��

        if(lineType == LINE_TYPE.Horizontal)
        {
            //  ��U���������Ƃ�
            this.GetComponent<RectTransform>().localScale = new Vector3(0,1,1);
            //  ���A�j���[�V����
            this.GetComponent<RectTransform>().DOScaleX(1.0f,duration)
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
            //  �Q�[�W�̃A�j���[�V����
            Slider slider = this.GetComponent<Slider>();
            slider.value = 0f;
            DOTween.To(
                () => slider.value,             // ����Ώۂɂ���̂�
                num => slider.value = num,      // �l�̍X�V
                1.0f,                           // �ŏI�I�Ȓl
                duration                        // �A�j���[�V��������
                )
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
        }
        else
        {
            //  ��U���������Ƃ�
            this.GetComponent<RectTransform>().localScale = new Vector3(0,1,1);
            this.GetComponent<RectTransform>().DOScaleX(1.0f,duration)
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
            //  �Q�[�W�̃A�j���[�V����
            Slider slider = this.GetComponent<Slider>();
            slider.value = 0f;
            DOTween.To(
                () => slider.value,             // ����Ώۂɂ���̂�
                num => slider.value = num,      // �l�̍X�V
                1.0f,                           // �ŏI�I�Ȓl
                duration                        // �A�j���[�V��������
                )
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
        }
    }

    void Update()
    {
        
    }
}
