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

    [Header("Optional Slider UI")]
    [Tooltip("Optional: simple slider controller to drive an integer slider (SetMaxHealth / SetHealth)")]
    public BossBarSliderController barController;

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

        // Prefer Animator on the same GameObject (boss components). If not found, fall back to child Animator.
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        col2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // set initial UI name if provided
        if (healthUI != null && !string.IsNullOrEmpty(bossName))
            healthUI.SetBossName(bossName);

        // Initialize UI visuals silently (do not show at start)
        if (healthUI != null)
            healthUI.SetHealthSilent(currentHealth / maxHealth);

        // keep UpdateHealthUI for immediate internal UI (healthUI) update,
        // slider-based controller is initialized in Start to ensure order
        //UpdateHealthUI(); // removed to prevent showing bar at startup
    }

    private void Start()
    {
        // If no barController assigned, try to find one in scene (convenience)
        if (barController == null)
            barController = Object.FindFirstObjectByType<BossBarSliderController>();

        if (barController != null)
        {
            // configure slider max and current values using integer interface
            barController.SetMaxHealth(Mathf.RoundToInt(maxHealth));
            barController.SetHealth(Mathf.RoundToInt(currentHealth));
        }
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
        // Do not trigger a 'Hit' animation — boss will flash instead
        if (hitVfxPrefab != null) Instantiate(hitVfxPrefab, hitPoint, Quaternion.identity);
        if (audioSource != null && hitSfx != null) audioSource.PlayOneShot(hitSfx);
        if (spriteToFlash != null) StartCoroutine(FlashCoroutine());

        // Apply knockback impulse
        if (rb != null)
        {
            Vector2 impulse = force * knockbackMultiplier;
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        // Show UI persistently first time the boss is hit
        if (healthUI != null && !uiShown)
        {
            healthUI.ShowPersistent();
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
        if (spriteToFlash == null) yield break;
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

        // Update UI to zero
        UpdateHealthUI();

        // Hide health bar immediately on death (and clear persistence)
        if (healthUI != null)
            healthUI.HideImmediate();

        yield return new WaitForSeconds(deathDelay);

        // Destroy or disable - destroy by default
        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        // existing normalized UI (BossHealthUI)
        if (healthUI != null)
            healthUI.SetHealth(currentHealth / maxHealth);

        // simple integer slider UI
        if (barController != null)
        {
            // ensure slider max is correct (in case maxHealth changed at runtime)
            barController.SetMaxHealth(Mathf.RoundToInt(maxHealth));
            barController.SetHealth(Mathf.RoundToInt(currentHealth));
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
#endif
}