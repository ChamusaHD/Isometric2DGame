using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [SerializeField] private float damage = 1;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask layer;
    private Collider2D[] hitColliders;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
    }

    public void MeleeAttack()
    {
        boxCollider.enabled = true;
        hitColliders = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0f, layer);
        print(hitColliders.Length);

        if (hitColliders != null)
        {
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag == "Enemy")
                {
                    print(" dmg enemy");
                    collider.gameObject.GetComponent<Enemy>().TakeDamage(damage);
                }
                else if (collider.gameObject.tag == "Player")
                {
                    print(" dmg Player");
                    collider.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
                }
            }
        }
    }

}
