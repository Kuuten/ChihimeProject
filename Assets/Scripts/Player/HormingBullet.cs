using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HormingBullet : MonoBehaviour
{
    //  �e�̃X�s�[�h
    private Vector3 velocity;
    [SerializeField] GameObject target;
    [SerializeField] float period;   //  �ڕW���B�ɂ����鎞�ԁi�b�j
    
    void Update()
    {
        Vector3 acceleration = Vector3.zero;
        target = GameObject.Find("Target");

        //  �����̌���
        Vector3 diff = target.transform.position - transform.position;
        acceleration += (diff - velocity*period) * 2f / (period*period);

        period -= Time.deltaTime;
        if(period < 0f)Destroy(this.gameObject);

        velocity += 3 * acceleration * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    ////  �����蔻��
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    //  �ǂɏՓ�
    //    if(collision.CompareTag("ENEMY"))
    //    {
    //        Destroy(this.gameObject);
    //    }
    //}
}
