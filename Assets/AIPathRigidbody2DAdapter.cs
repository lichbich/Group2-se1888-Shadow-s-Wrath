using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Rigidbody2D))]
public class AIPathRigidbody2DAdapter : MonoBehaviour
{
    private AIPath ai;
    private Rigidbody2D rb;

    [Header("Tuning")]
    [Tooltip("Multiplier applied to AIPath.desiredVelocity.x")]
    public float horizontalSpeedMultiplier = 1f;

    [Tooltip("Smooth time used by SmoothDamp for horizontal velocity (seconds).")]
    public float velocitySmoothTime = 0.08f;

    [Tooltip("If true, flip localScale.x to face movement direction.")]
    public bool autoFlipSprite = true;

    [Tooltip("Deadzone for desired velocity. Velocities smaller than this will be treated as 0.")]
    public float desiredVelocityDeadzone = 0.05f;

    [Tooltip("Clamp resulting horizontal velocity to zero if below this absolute value.")]
    public float minSpeedThreshold = 0.06f;

    [Tooltip("If remaining distance is below this, stop horizontal movement.")]
    public float stopDistance = 0.15f;

    [Header("Repath on vertical change")]
    [Tooltip("If transform.y changes more than this (world units), request a new path immediately.")]
    public float verticalRepathThreshold = 0.25f;
    [Tooltip("If Rigidbody2D.velocity.y is below this (falling), request a new path immediately.")]
    public float fallRepathVelocity = -1.0f;
    [Tooltip("Minimum seconds between forced repath requests.")]
    public float repathCooldown = 0.12f;

    [Header("Animation")]
    [Tooltip("Optional: Animator to update movement parameters (e.g. 'Speed'). If null, will try to find one on this GameObject or its children.")]
    public Animator animator;
    [Tooltip("If true, update the animator 'Speed' parameter each FixedUpdate using horizontal velocity.")]
    public bool updateAnimatorSpeed = true;
    [Tooltip("Animator float parameter name used for horizontal speed.")]
    public string animatorSpeedParam = "Speed";

    private float vxSmoothRef = 0f;
    private float lastRepathTime = -10f;
    private float lastY;

    private void Awake()
    {
        ai = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();
        if (ai == null) Debug.LogError("AIPath component required on " + name);
        if (rb == null) Debug.LogError("Rigidbody2D component required on " + name);

        // If animator not assigned in inspector, prefer Animator on the same GameObject, then children
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        // Let AI compute paths and desiredVelocity but do not let it overwrite transform/rotation.
        if (ai != null)
        {
            ai.canMove = true;           // AI still computes desired velocity
            ai.updatePosition = false;   // we move via Rigidbody2D
            ai.updateRotation = false;

            // IMPORTANT: disable AI-internal gravity so the AI's simulatedPosition doesn't "fall"
            ai.gravity = Vector3.zero;

            // Sync AI's internal simulated position to the Rigidbody/transform at start
            // so gizmos/steeringTarget match the visible object
            ai.Teleport(transform.position, false);
        }

        // Prefer physics interpolation to reduce visible jitter
        if (rb != null)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        lastY = transform.position.y;
    }

    private void FixedUpdate()
    {
        if (ai == null || rb == null) return;

        // Get desired velocity from the AI (respects maxSpeed, local avoidance etc.)
        Vector2 desiredVel = ai.desiredVelocity * horizontalSpeedMultiplier;
        float targetVx = desiredVel.x;

        // Deadzone: avoid tiny micro-adjustments
        if (Mathf.Abs(targetVx) < desiredVelocityDeadzone) targetVx = 0f;

        // If close to destination, stop horizontal movement to avoid oscillation
        bool reached = false;
        // AIPath exposes reachedDestination / remainingDistance; use whichever is available
        try
        {
            reached = ai.reachedDestination;
        }
        catch
        {
            // fallback if property not present
            reached = ai.remainingDistance <= stopDistance;
        }

        if (reached || ai.remainingDistance <= stopDistance)
        {
            targetVx = 0f;
        }

        // Smoothly ramp the horizontal velocity to the target
        float newVx = Mathf.SmoothDamp(rb.linearVelocity.x, targetVx, ref vxSmoothRef, Mathf.Max(0.0001f, velocitySmoothTime), Mathf.Infinity, Time.fixedDeltaTime);

        // Clamp very small speeds to zero to eliminate micro-motion
        if (Mathf.Abs(newVx) < minSpeedThreshold) newVx = 0f;

        // Apply horizontal velocity while preserving vertical velocity (gravity)
        rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);

        // Update animator 'Speed' parameter (use absolute horizontal velocity)
        if (updateAnimatorSpeed && animator != null)
        {
            // Use the smoothed horizontal velocity so animation matches visible movement
            animator.SetFloat(animatorSpeedParam, Mathf.Abs(newVx));
        }

        // Optional: flip sprite to face movement direction
        if (autoFlipSprite && Mathf.Abs(newVx) > 0.05f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(newVx) * Mathf.Abs(s.x);
            transform.localScale = s;
        }

        // If the boss has moved vertically a lot (dropped) or is falling, force a path recalculation.
        // Rate-limited to avoid spamming pathfinder.
        if (Time.time - lastRepathTime >= repathCooldown)
        {
            float dy = Mathf.Abs(transform.position.y - lastY);
            bool largeVerticalMove = dy >= verticalRepathThreshold;
            bool falling = rb.linearVelocity.y <= fallRepathVelocity;

            if ((largeVerticalMove || falling) && !ai.pathPending)
            {
                ai.SearchPath();
                lastRepathTime = Time.time;
            }
        }

        // update lastY for next check
        lastY = transform.position.y;
    }

    // Sync the AI's internal simulated position to the Rigidbody-driven transform after physics runs.
    // Keeps seeker gizmos and steeringTarget aligned with the visible boss.
    private void LateUpdate()
    {
        if (ai != null)
            ai.Teleport(transform.position, false);
    }
}