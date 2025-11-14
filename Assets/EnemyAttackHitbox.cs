using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Tooltip("Positive number of HP this hitbox removes from the player.")]
    public int damage = 1;

    [Tooltip("If true the hitbox GameObject will be destroyed immediately after hitting the player.")]
    public bool destroyOnHit = false;

    private Collider2D _collider;
    private bool _hitRegistered = false;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
            Debug.LogWarning($"{nameof(EnemyAttackHitbox)} on '{gameObject.name}' has no Collider2D.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitRegistered) return;

        // Look for PlayerHealth on the collided object or its parents
        var pv = other.GetComponentInParent<PlayerHealth>();
        if (pv != null)
        {
            _hitRegistered = true;

            // Use the existing method name in PlayerHealth (Addhealth)
            PlayerHealth.AddHealth(-damage);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            else if (_collider != null)
            {
                // disable to prevent multiple hits from same active frame
                _collider.enabled = false;
            }
        }
    }
}