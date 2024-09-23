using DG.Tweening;
using UnityEngine;


//--------------------------------------------------------------
//
//  オブジェクトを回転させるクラス
//
//--------------------------------------------------------------
public class RotateObject : MonoBehaviour
{
    //  時計回りか反時計回りかを設定する
    public enum eRotationDirection
    {
        Clockwise,          //  時計回り
        CounterClockwise,   //  反時計回り
    }
    public eRotationDirection rotationDirection = eRotationDirection.Clockwise;

    //  一周するのにかかる時間
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
