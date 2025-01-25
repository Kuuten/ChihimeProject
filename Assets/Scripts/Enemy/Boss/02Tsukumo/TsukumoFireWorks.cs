using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̒e�N���X
//
//--------------------------------------------------------------
public class TsukumoFireworks : MonoBehaviour
{
    //  �O������e�̃x�N�g���ƈړ��X�s�[�h���擾���A�ړ����邾��

    private Vector3 velocity;
    private float speed;
    private float accel;

    private int power;
    
    void Awake()
    {
        speed = 0f;
        power = 0;
        accel = 0.1f;
        velocity = Vector3.zero;
    }

    private void Start()
    {
        Debug.Log($"�x�N�g���̑傫��:{velocity.magnitude}");

        StartCoroutine(Disapeear(5f, 1f));
    }

    void Update()
    {
        transform.position += speed * velocity * Time.deltaTime;
        speed -= accel * Time.deltaTime;
        accel += 0.01f * Time.deltaTime;
    }

    //-------------------------------------------------
    //  �����蔻��
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadWall") ||
            collision.CompareTag("DeadWallBottom"))
        {
            Destroy(this.gameObject);
        }
    }


    //------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public void SetSpeed(float s){ speed = s; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }

    //------------------------------------------------------
    //  �����蔻��𖳌������ăA���t�@�A�j���ŏ���
    //------------------------------------------------------
    private IEnumerator Disapeear(float duration,float fade_time)
    {
        Debug.Log("Ready");

        yield return new WaitForSeconds(duration);

        Debug.Log("Go");

        //  �����蔻�������
        this.GetComponent<CircleCollider2D>().enabled = false;

        //  �A���t�@�A�j���[�V����
        this.GetComponent<SpriteRenderer>().DOFade(0f,fade_time)
            .OnComplete(()=>{ Destroy(this.gameObject);});
    }

}
