using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ���@�~�N���X
//
//--------------------------------------------------------------
public class MagicCircle : MonoBehaviour
{
    [SerializeField, ShowAssetPreview]
    public Material mat;
    [Watch]public float radius;

    void Start()
    {
        mat = this.GetComponent<Renderer>().material;

        radius = 0f;

        // ���l�̕ύX
        DOTween.To(
            () => radius,                  // ����Ώۂɂ���̂�
            num => radius = num,           // �l�̍X�V
            1.0f,                          // �ŏI�I�Ȓl
            2.0f                           // �A�j���[�V��������
        ).SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        if (mat != null) {
            Destroy(mat);
        }
    }

    void Update()
    {
       mat.SetFloat("_Radius", radius ); 
    }
}
