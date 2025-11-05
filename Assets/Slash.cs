using UnityEngine;

public class Slash : MonoBehaviour
{
    public float lifetime = 0.12f;   // how long it stays visible
    public float damage = 10f;      // damage to apply to enemies
    public float speed = 0f;        // optional: move slash forward
    public Vector2 direction = Vector2.right;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (speed != 0)
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // apply damage if you have enemy health system
            //other.GetComponent<EnemyHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
