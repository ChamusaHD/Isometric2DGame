using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 5.0f;
    [SerializeField] float arrowDamage = 1f;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Rigidbody2D rb;

    public float timer = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        shootPoint = GameObject.Find("ShootPoint").transform;
        //shootPoint = GetComponentInParent<Transform>();
    }
    void Update()// オブジェクトを目標点まで移動させる
    {
        rb.velocity = shootPoint.right * speed;
       // transform.position = Vector3.MoveTowards(transform.position, shootPoint.right * 3, speed * Time.deltaTime);
        Destroy(gameObject, timer);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(arrowDamage);
        }

       //Destroy(gameObject);
    }

}
