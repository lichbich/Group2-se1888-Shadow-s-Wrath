using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject slashPrefab;      // assign in Inspector
    public Transform attackSpawnPoint;  // empty child at front of player
    public float attackCooldown = 0.4f;

    [Header("Freeze")]
    [Tooltip("Duration to freeze player movement when attacking (seconds). 0 disables freeze).")]
    public float freezeDuration = 0.08f;

    // Optional: if PlayerMovement is not on the same GameObject you can assign it in Inspector
    public PlayerMovement playerMovement;

    private float nextAttackTime = 0f;
    private bool facingRight = true;    // kept for external callers if used
    private Rigidbody2D rb;
    private bool isFreezing = false;

    // Expose attacking state if other systems need to read it
    public bool IsAttacking { get; private set; }

    // state to restore after freeze
    private float savedGravityScale = 1f;
    private RigidbodyConstraints2D savedConstraints = RigidbodyConstraints2D.None;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            savedGravityScale = rb.gravityScale;
            savedConstraints = rb.constraints;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        if (slashPrefab == null || attackSpawnPoint == null) return;

        // Determine facing from player's localScale.x (works with CharacterController2D.Flip)
        bool isFacingRight = transform.localScale.x > 0f;
        facingRight = isFacingRight; // keep internal flag in sync

        // Spawn slash
        GameObject slash = Instantiate(slashPrefab, attackSpawnPoint.position, Quaternion.identity);

        // Ensure the slash X scale matches player's facing while preserving magnitude
        Vector3 s = slash.transform.localScale;
        s.x = Mathf.Abs(s.x) * (isFacingRight ? 1f : -1f);
        slash.transform.localScale = s;

        // Set movement direction in world space
        Slash slashScript = slash.GetComponent<Slash>();
        if (slashScript != null)
            slashScript.direction = isFacingRight ? Vector2.right : Vector2.left;

        // Start freeze if configured
        IsAttacking = true;
        if (freezeDuration > 0f && !isFreezing)
            StartCoroutine(FreezeCoroutine());
        else
            IsAttacking = false;
    }

    private IEnumerator FreezeCoroutine()
    {
        isFreezing = true;

        // Note: do NOT disable the PlayerMovement component here.
        // Keeping it enabled allows Update() to capture jump input during the short freeze,
        // so a player can still queue a double-jump while frozen for feedback.

        if (rb != null)
        {
            // save current physics state
            savedGravityScale = rb.gravityScale;
            savedConstraints = rb.constraints;

            // stop movement immediately
            rb.linearVelocity = Vector2.zero;

            // remove gravity so player stays in place, then freeze constraints to fully lock position/rotation
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        yield return new WaitForSeconds(freezeDuration);

        // Restore physics state
        if (rb != null)
        {
            rb.gravityScale = savedGravityScale;
            rb.constraints = savedConstraints;
        }

        // PlayerMovement remains enabled so it can process buffered jump input.
        isFreezing = false;
        IsAttacking = false;
    }

    // You can update facingRight based on your movement script if needed
    public void SetFacingDirection(bool isFacingRight)
    {
        facingRight = isFacingRight;
    }
}