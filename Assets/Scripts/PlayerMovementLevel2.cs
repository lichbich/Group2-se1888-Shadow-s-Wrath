using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementLevel2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Animator animator;
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip jumpClip;


    private bool isDead = false;
    private bool canMove = false;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    [Header("Ground Check Settings")]
    public Vector2 boxSize = new Vector2(0.5f, 0.1f);
    public float castDistance = 0.2f;
    [SerializeField] private Transform groundcheck;
    public LayerMask groundLayer;
    private bool isground;

    [Header("Lose UI")]
    [SerializeField] private GameObject loseUI;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera camera;
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
        //  Nếu đã chết thì dừng toàn bộ xử lý input
        if (isDead) return;

        handleJump();

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "CameraTrigger")
        {
            camera.Target.TrackingTarget = transform;
            collision.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator StartRunningAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        canMove = true;
    }

    private void handleJump()
    {

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isground)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                FindFirstObjectByType<AudioLevel2>()?.playJumpSound();
            }
        }
        isground = Physics2D.OverlapCircle(groundcheck.position, 0.2f, groundLayer);
    }


    public void Die()
    {
        if (isDead) return;

        isDead = true;
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        animator.SetTrigger("isDead");
        FindFirstObjectByType<AudioLevel2>()?.playLoseSound();

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


