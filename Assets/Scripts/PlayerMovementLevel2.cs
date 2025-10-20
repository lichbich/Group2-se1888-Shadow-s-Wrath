using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementLevel2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Animator animator;
    [SerializeField] private float startDelay = 3f;

    private bool isDead = false;
    private bool canMove = false;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    [Header("Ground Check Settings")]
    public Vector2 boxSize = new Vector2(0.5f, 0.1f);
    public float castDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Lose UI")]
    [SerializeField] private GameObject loseUI;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        animator.SetBool("isRunning", false);
        StartCoroutine(StartRunningAfterDelay());
    }

    private void Update()
    {
        // 🔒 Nếu đã chết thì dừng toàn bộ xử lý input
        if (isDead) return;

        // Nhảy khi nhấn W
        if (Keyboard.current.wKey.wasPressedThisFrame && CheckGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Tự chạy sau delay
        if (canMove)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            animator.SetBool("isRunning", true);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
        }
    }

    private System.Collections.IEnumerator StartRunningAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        canMove = true;
    }

    public bool CheckGrounded()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0f, -transform.up, castDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        animator.SetTrigger("isDead");

        // Gọi UI sau một chút để animation kịp hiển thị
        Invoke(nameof(ShowLoseUI), 1f);
    }

    private void ShowLoseUI()
    {
        if (loseUI != null)
        {
            loseUI.SetActive(true);
            Time.timeScale = 0f; // Dừng game
        }
        else
        {
            Debug.LogWarning("LoseUI chưa được gán trong Inspector!");
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Trap"))
        {
            Die();
        }
    }
}
