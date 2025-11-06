using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 200f;
    [Tooltip("Current health (readonly at runtime)")]
    public float currentHealth = 200f;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 0.2f;
    private bool isInvulnerable = false;

    [Header("Feedback")]
    public SpriteRenderer spriteToFlash;
    public GameObject hitVfxPrefab;
    public GameObject deathVfxPrefab;
    public AudioClip hitSfx;
    public AudioClip deathSfx;
    public AudioSource audioSource;

    [Header("Knockback")]
    [Tooltip("Multiplier applied to the incoming force before applying to Rigidbody2D")]
    public float knockbackMultiplier = 1f;

    [Header("Death")]
    public float deathDelay = 1f;

    [Header("UI")]
    public BossHealthUI healthUI;
    [Tooltip("Optional display name shown on the boss bar")]
    public string bossName;

    [Header("Events")]
    public UnityEvent OnHit;
    public UnityEvent OnDeath;

    private Animator animator;
    private Collider2D col2d;
    private Rigidbody2D rb;
    private bool isDead = false;

    // tracks whether we've shown the UI entrance already
    private bool uiShown = false;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        animator = GetComponentInChildren<Animator>();
        col2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // set initial UI name if provided
        if (healthUI != null && !string.IsNullOrEmpty(bossName))
            healthUI.SetBossName(bossName);

        UpdateHealthUI();
    }

    public void TakeDamage(float amount, Vector2 hitPoint, Vector2 force)
    {
        if (isDead) return;
        if (amount <= 0f) return;
        if (isInvulnerable) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Feedback
        OnHit?.Invoke();
        if (animator != null) animator.SetTrigger("Hit");
        if (hitVfxPrefab != null) Instantiate(hitVfxPrefab, hitPoint, Quaternion.identity);
        if (audioSource != null && hitSfx != null) audioSource.PlayOneShot(hitSfx);
        if (spriteToFlash != null) StartCoroutine(FlashCoroutine());

        // Apply knockback impulse
        if (rb != null)
        {
            Vector2 impulse = force * knockbackMultiplier;
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        // Show UI entrance first time the boss is hit
        if (healthUI != null && !uiShown)
        {
            healthUI.ShowEntrance();
            uiShown = true;
        }

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            StartCoroutine(DieCoroutine());
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private IEnumerator FlashCoroutine()
    {
        Color orig = spriteToFlash.color;
        spriteToFlash.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (spriteToFlash != null) spriteToFlash.color = orig;
    }

    private IEnumerator DieCoroutine()
    {
        if (isDead) yield break;
        isDead = true;

        // disable collider & other interactions
        if (col2d != null) col2d.enabled = false;

        OnDeath?.Invoke();
        if (animator != null) animator.SetTrigger("Death");
        if (deathVfxPrefab != null) Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
        if (audioSource != null && deathSfx != null) audioSource.PlayOneShot(deathSfx);

        // optionally freeze rigidbody
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;

        UpdateHealthUI();

        yield return new WaitForSeconds(deathDelay);

        // Destroy or disable - destroy by default
        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        if (healthUI != null)
            healthUI.SetHealth(currentHealth / maxHealth);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
#endif
}