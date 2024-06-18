using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HormingBullet : MonoBehaviour
{
    //  �e�̃X�s�[�h
    private Vector3 velocity;
    private float lifeTime = 0;
    private const float lifeMax = 60*3;
    [SerializeField] GameObject target;
    [SerializeField] float period;   //  �ڕW���B�ɂ����鎞�ԁi�b�j
    
    void Update()
    {
        //  ���Ԍo�߂Ŏ��S������Ƃ�
        if(lifeTime >= lifeMax)
        {
            lifeTime = 0;
            Destroy(this.gameObject);
        }
        else lifeTime += Time.deltaTime;

        Vector3 acceleration = Vector3.zero;
        target = GameObject.Find("Target");

        //  �����̌���
        Vector3 diff = target.transform.position - transform.position;
        acceleration += (diff - velocity*period) * 2f / (period*period);

        period -= Time.deltaTime;
        if(period < 0f)return;

        velocity += acceleration * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    //  �����蔻��
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //  �ǂɏՓ�
        if(collision.collider.CompareTag("WALL") ||
           collision.collider.CompareTag("ENEMY")
          )
        {
            Destroy(this.gameObject);
        }
    }
}
