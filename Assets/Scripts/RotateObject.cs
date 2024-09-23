using DG.Tweening;
using UnityEngine;


//--------------------------------------------------------------
//
//  �I�u�W�F�N�g����]������N���X
//
//--------------------------------------------------------------
public class RotateObject : MonoBehaviour
{
    //  ���v��肩�����v��肩��ݒ肷��
    public enum eRotationDirection
    {
        Clockwise,          //  ���v���
        CounterClockwise,   //  �����v���
    }
    public eRotationDirection rotationDirection = eRotationDirection.Clockwise;

    //  �������̂ɂ����鎞��
    [SerializeField] private float rotationTime = 5.0f;

    void Start()
    {
        if(rotationDirection == eRotationDirection.Clockwise)
        {
           transform.DOLocalRotate(new Vector3(0, 0, -360f), rotationTime, RotateMode.FastBeyond360)  
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Restart); 
        }
        else
        {
           transform.DOLocalRotate(new Vector3(0, 0, 360f), rotationTime, RotateMode.FastBeyond360)  
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Restart);  
        }

    }

    void Update()
    {
        
    }
}
