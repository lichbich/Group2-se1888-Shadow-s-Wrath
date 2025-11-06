using UnityEngine;

public class Slash : MonoBehaviour
{
    public float lifetime = 0.12f;   // how long it stays visible
    public float damage = 10f;      // damage to apply to enemies
    public float speed = 0f;        // optional: move slash forward
    public Vector2 direction = Vector2.right;

    // Knockback impulse magnitude applied to victim (passed as an impulse vector)
    public float knockback = 5f;

    // If true the slash GameObject will be destroyed immediately on hit.
    // If false (default) the collider will be disabled and the visual will play to completion.
    public bool destroyOnHit = false;

    private Collider2D _collider;
    private bool _hitRegistered = false;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        // Ensure the slash is removed after its visual lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (speed != 0)
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitRegistered) return;

        // Prefer interface-based damage application so any enemy/boss can receive the hit.
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            _hitRegistered = true;

            // apply damage and pass knockback impulse
            Vector2 hitPoint = transform.position;
            Vector2 impulse = direction.normalized * knockback;
            damageable.TakeDamage(damage, hitPoint, impulse);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            else
            {
                // disable the collider so we don't keep hitting the same enemy
                if (_collider != null)
                    _collider.enabled = false;

                // optionally play hit VFX / sound here without destroying visual prefab
            }
        }
    }
}
