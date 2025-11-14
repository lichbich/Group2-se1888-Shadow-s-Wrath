using UnityEngine;
using Pathfinding;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Tooltip("If true, update the animator 'IsJumping' bool parameter when jumping/landing.")]
    public bool updateAnimatorJump = true;
    [Tooltip("Animator bool parameter name used to indicate jumping.")]
    public string animatorJumpParam = "IsJumping";

    [Tooltip("If true, update the animator 'IsFalling' bool parameter when falling.")]
    public bool updateAnimatorFall = true;
    [Tooltip("Animator bool parameter name used to indicate falling.")]
    public string animatorFallParam = "IsFalling";

    [Tooltip("Optional: animator float parameter name to pass vertical velocity (leave empty to ignore).")]
    public string animatorVerticalVelocityParam = "VerticalVelocity";
    [Tooltip("Velocity threshold above which the character is considered \"rising\".")]
    public float riseVelocityThreshold = 0.1f;
    [Tooltip("Velocity threshold below which the character is considered \"falling\".")]
    public float fallVelocityThreshold = -0.1f;

    [Header("Jumping")]
    [Tooltip("Enable automatic jumping to player or to path waypoint on higher platform")]
    public bool enableJumpToPlayerPlatform = true;
    [Tooltip("Assign the player's transform (if null, found by tag 'Player' at Awake)")]
    public Transform player;
    [Tooltip("Minimum vertical difference (target.y - boss.y) to consider jumping")]
    public float minJumpHeight = 0.5f;
    [Tooltip("Maximum vertical difference to attempt to jump")]
    public float maxJumpHeight = 4f;
    [Tooltip("Maximum horizontal distance allowed between boss and target to attempt a jump")]
    public float jumpHorizontalRange = 6f;
    [Tooltip("Multiplier applied to computed vertical velocity for safety/arc tuning")]
    public float jumpVelocityBoost = 1.05f;
    [Tooltip("Seconds between allowed jumps")]
    public float jumpCooldown = 1.0f;
    [Tooltip("Layer mask used for ground detection")]
    public LayerMask groundLayer = ~0;
    [Tooltip("Ground check ray distance (from pivot down)")]
    public float groundCheckDistance = 0.12f;
    [Tooltip("Ground check circle radius (more robust than a single ray)")]
    public float groundCheckRadius = 0.08f;

    // New option: prefer jumping to AI steering target (next waypoint) when available
    [Header("Jumping: Path-aware options")]
    [Tooltip("If true, prefer ai.steeringTarget as jump target when available (otherwise fall back to player).")]
    public bool preferSteeringTargetForJump = true;

    [Header("Daze (replacement for knockback)")]
    [Tooltip("Default number of FixedUpdate frames the boss will be dazed when ApplyDaze(int frames) is called.")]
    public int dazeFrames = 10;
    [Tooltip("If checked, draw parameter gizmos in the scene view.")]
    public bool debugGizmos = true;

    [Header("Runtime debug")]
    [Tooltip("Enable logging for jump decision/debugging (disable in production).")]
    public bool debugJumpLogging = false;

    // runtime fields
    private float vxSmoothRef = 0f;
    private float lastRepathTime = -10f;
    private float lastY;
    private float lastJumpTime = -10f;

    // whether the boss has already jumped and must touch ground before jumping again
    private bool hasJumped = false;

    // DAZE state
    private int dazeFramesRemaining = 0;
    private bool prevAiCanMove = true;

    // Ground state for landing detection
    private bool prevGrounded = true;

    // animator parameter existence flags
    private bool hasAnimatorSpeedParam;
    private bool hasAnimatorJumpParam;
    private bool hasAnimatorFallParam;
    private bool hasAnimatorVerticalVelocityParam;

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

        // Cache which animator parameters actually exist to avoid runtime errors
        hasAnimatorSpeedParam = AnimatorHasParameter(animatorSpeedParam);
        hasAnimatorJumpParam = AnimatorHasParameter(animatorJumpParam);
        hasAnimatorFallParam = AnimatorHasParameter(animatorFallParam);
        hasAnimatorVerticalVelocityParam = AnimatorHasParameter(animatorVerticalVelocityParam);

        // Warn if user enabled animator updates but parameter is missing
        if (animator != null)
        {
            if (updateAnimatorSpeed && !hasAnimatorSpeedParam)
                Debug.LogWarning($"{name} animator missing float parameter '{animatorSpeedParam}'. Disable updateAnimatorSpeed or add the parameter.");
            if (updateAnimatorJump && !hasAnimatorJumpParam)
                Debug.LogWarning($"{name} animator missing bool parameter '{animatorJumpParam}'. Disable updateAnimatorJump or add the parameter.");
            if (updateAnimatorFall && !hasAnimatorFallParam)
                Debug.LogWarning($"{name} animator missing bool parameter '{animatorFallParam}'. Disable updateAnimatorFall or add the parameter.");
            if (!string.IsNullOrEmpty(animatorVerticalVelocityParam) && !hasAnimatorVerticalVelocityParam)
                Debug.LogWarning($"{name} animator missing float parameter '{animatorVerticalVelocityParam}'. Leave empty or add the parameter.");
        }

        // Try to find player if not assigned
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
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

        // initialize prevGrounded to current grounded state to avoid an immediate "land" event on start
        prevGrounded = IsGrounded();
        // ensure animator flags match initial state
        if (animator != null)
        {
            if (updateAnimatorJump && hasAnimatorJumpParam) animator.SetBool(animatorJumpParam, false);
            if (updateAnimatorFall && hasAnimatorFallParam) animator.SetBool(animatorFallParam, false);
            if (hasAnimatorVerticalVelocityParam) animator.SetFloat(animatorVerticalVelocityParam, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (ai == null || rb == null) return;

        // Respect AI movement enabled state. If AI was paused (ai.canMove == false) we should not apply AI velocities.
        bool aiCanMove = ai.canMove;

        // If dazed, force stand-still and skip normal movement/jump logic.
        if (dazeFramesRemaining > 0)
        {
            dazeFramesRemaining--;
            // prevent AI from trying to move while dazed
            if (ai != null) ai.canMove = false;
            // zero horizontal movement (preserve vertical)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            // ensure animations reflect being idle
            if (updateAnimatorSpeed && hasAnimatorSpeedParam)
                animator.SetFloat(animatorSpeedParam, 0f);
            if (updateAnimatorJump && hasAnimatorJumpParam)
                animator.SetBool(animatorJumpParam, false);
            if (updateAnimatorFall && hasAnimatorFallParam)
                animator.SetBool(animatorFallParam, false);
            if (hasAnimatorVerticalVelocityParam)
                animator.SetFloat(animatorVerticalVelocityParam, rb.linearVelocity.y);

            // update lastY to avoid spurious repath triggers while dazed
            lastY = transform.position.y;

            // If daze ended this frame, restore AI canMove
            if (dazeFramesRemaining == 0)
            {
                if (ai != null) ai.canMove = prevAiCanMove;
            }

            // Update prevGrounded to current grounded state so landing detection doesn't fire immediately after daze
            prevGrounded = IsGrounded();

            return;
        }

        // When AI movement is disabled externally, force zero horizontal velocity and keep animator consistent.
        if (!aiCanMove)
        {
            // prevent unintended horizontal motion
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (updateAnimatorSpeed && hasAnimatorSpeedParam)
                animator.SetFloat(animatorSpeedParam, 0f);

            // still update vertical param if present
            if (hasAnimatorVerticalVelocityParam)
                animator.SetFloat(animatorVerticalVelocityParam, rb.linearVelocity.y);

            // Update grounding & lastY but skip movement/jump logic
            lastY = transform.position.y;
            prevGrounded = IsGrounded();

            return;
        }

        // Reset jump availability when grounded and detect landing to update animator
        bool grounded = IsGrounded();
        if (grounded)
        {
            if (!prevGrounded)
            {
                // just landed
                if (updateAnimatorJump && hasAnimatorJumpParam)
                    animator.SetBool(animatorJumpParam, false);
                if (updateAnimatorFall && hasAnimatorFallParam)
                    animator.SetBool(animatorFallParam, false);
            }

            hasJumped = false;
        }

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

        // Update vertical velocity parameter (if used) and set jump/fall states based on vertical velocity
        float vy = rb.linearVelocity.y;
        if (hasAnimatorVerticalVelocityParam && animator != null)
        {
            animator.SetFloat(animatorVerticalVelocityParam, vy);
        }

        // Determine rising/falling animation states when airborne
        if (!grounded)
        {
            if (vy >= riseVelocityThreshold)
            {
                if (updateAnimatorJump && hasAnimatorJumpParam) animator.SetBool(animatorJumpParam, true);
                if (updateAnimatorFall && hasAnimatorFallParam) animator.SetBool(animatorFallParam, false);
            }
            else if (vy <= fallVelocityThreshold)
            {
                if (updateAnimatorFall && hasAnimatorFallParam) animator.SetBool(animatorFallParam, true);
                if (updateAnimatorJump && hasAnimatorJumpParam) animator.SetBool(animatorJumpParam, false);
            }
            // if vy in small dead zone, keep previous flags (no change)
        }

        // Try jumping to the player's platform or to the next path waypoint
        if (enableJumpToPlayerPlatform)
        {
            TryJumpToPlayerPlatform();
        }

        // Update animator 'Speed' parameter (use absolute horizontal velocity)
        if (updateAnimatorSpeed && hasAnimatorSpeedParam && animator != null)
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

        // update lastY and prevGrounded for next check
        lastY = transform.position.y;
        prevGrounded = grounded;
    }

    // Sync the AI's internal simulated position to the Rigidbody-driven transform after physics runs.
    // Keeps seeker gizmos and steeringTarget aligned with the visible boss.
    private void LateUpdate()
    {
        if (ai != null)
            ai.Teleport(transform.position, false);
    }

    // Improved grounded check: use OverlapCircle at a point slightly below the pivot.
    // This is more robust across different pivot/scale setups than a CircleCast starting at the pivot.
    private bool IsGrounded()
    {
        Vector2 checkCenter = (Vector2)transform.position + Vector2.down * groundCheckDistance;
        Collider2D[] cols = Physics2D.OverlapCircleAll(checkCenter, groundCheckRadius, groundLayer);
        foreach (var col in cols)
        {
            if (col == null) continue;
            if (col.isTrigger) continue;
            if (col.attachedRigidbody == rb) continue;
            return true;
        }
        return false;
    }

    private void TryJumpToPlayerPlatform()
    {
        if (debugJumpLogging) Debug.Log($"{name} TryJumpToPlayerPlatform: timeSinceLastJump={(Time.time - lastJumpTime):F2}, hasJumped={hasJumped}, grounded={IsGrounded()}");

        if (Time.time - lastJumpTime < jumpCooldown) return;

        // Prevent multi-jumping until boss touches ground again
        if (hasJumped) return;

        if (!IsGrounded()) return;

        Vector3 targetPos = Vector3.zero;
        bool haveTarget = false;

        // Prefer steering target from AI path only if that steering target is meaningfully higher
        // and within horizontal range. This avoids using low or same-height steering targets that
        // make the boss jitter vertically instead of performing a single jump.
        if (preferSteeringTargetForJump && ai != null && ai.hasPath)
        {
            var st = ai.steeringTarget;
            float stDeltaY = st.y - transform.position.y;
            float stDx = Mathf.Abs(st.x - transform.position.x);

            if (debugJumpLogging) Debug.Log($"{name} steeringTarget: {st} deltaY={stDeltaY:F2} dx={stDx:F2}");

            // Only accept steering target if it would actually require a jump (higher by at least minJumpHeight)
            // and is within horizontal range and within allowed max jump height.
            if (stDeltaY >= minJumpHeight && stDeltaY <= maxJumpHeight && stDx <= jumpHorizontalRange)
            {
                targetPos = st;
                haveTarget = true;
            }
        }

        // Fallback to player if no suitable steering target found
        if (!haveTarget && player != null)
        {
            targetPos = player.position;
            haveTarget = true;
        }

        if (!haveTarget) return;

        float deltaY = targetPos.y - transform.position.y;
        // Only consider jumping if target is higher within bounds
        if (deltaY < minJumpHeight || deltaY > maxJumpHeight)
        {
            if (debugJumpLogging) Debug.Log($"{name} Jump aborted: deltaY {deltaY:F2} out of [{minJumpHeight},{maxJumpHeight}]");
            return;
        }

        float dx = Mathf.Abs(targetPos.x - transform.position.x);
        if (dx > jumpHorizontalRange)
        {
            if (debugJumpLogging) Debug.Log($"{name} Jump aborted: dx {dx:F2} > jumpHorizontalRange {jumpHorizontalRange:F2}");
            return;
        }

        // compute effective gravity magnitude (positive). Use fallbacks so we don't end up with zero.
        float g = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
        // If effective g is nearly zero (project gravity or gravityScale disabled), fall back to a sane default.
        if (g < 0.0001f)
        {
            float projG = Mathf.Abs(Physics2D.gravity.y);
            float fallbackScale = Mathf.Max(0.25f, rb.gravityScale);
            g = (projG > 0.0001f ? projG : 9.81f) * fallbackScale;
        }

        // minimal initial vertical velocity to reach deltaY: v = sqrt(2 * g * deltaY)
        float requiredVy = Mathf.Sqrt(2f * g * deltaY) * jumpVelocityBoost;

        if (debugJumpLogging) Debug.Log($"{name} computed requiredVy={requiredVy:F2} for deltaY={deltaY:F2} g={g:F2}");

        // Only apply jump if a meaningful vertical velocity is computed.
        if (requiredVy > 0.01f)
        {
            // Set vertical velocity (preserve current horizontal)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, requiredVy);

            // Set animator jumping flag
            if (updateAnimatorJump && hasAnimatorJumpParam)
                animator.SetBool(animatorJumpParam, true);

            // ensure falling flag cleared on jump
            if (updateAnimatorFall && hasAnimatorFallParam)
                animator.SetBool(animatorFallParam, false);

            lastJumpTime = Time.time;
            hasJumped = true;

            if (debugJumpLogging) Debug.Log($"{name} Jump applied: vy={requiredVy:F2}");
        }
    }

    /// <summary>
    /// Apply a daze to the boss. While dazed the boss stands still (horizontal velocity zero) and AI movement is suspended.
    /// Parameter is number of FixedUpdate frames to remain dazed.
    /// </summary>
    public void ApplyDaze(int frames)
    {
        if (frames <= 0) return;

        // store previous ai.canMove only when entering daze
        if (dazeFramesRemaining <= 0 && ai != null)
            prevAiCanMove = ai.canMove;

        dazeFramesRemaining = Mathf.Max(dazeFramesRemaining, frames);

        if (ai != null) ai.canMove = false;

        // prevent immediate re-jump while dazed
        hasJumped = true;

        // clear jumping/falling flag while dazed (boss stands still)
        if (updateAnimatorJump && hasAnimatorJumpParam)
            animator.SetBool(animatorJumpParam, false);
        if (updateAnimatorFall && hasAnimatorFallParam)
            animator.SetBool(animatorFallParam, false);
        if (hasAnimatorVerticalVelocityParam)
            animator.SetFloat(animatorVerticalVelocityParam, 0f);
    }

    // Draw parameter gizmos and helpful visuals in scene view
    private void OnDrawGizmos()
    {
        if (!debugGizmos) return;

        // basic visuals (works in runtime & editor)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // draw repath vertical threshold as a vertical band
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Vector3 a = transform.position + Vector3.up * verticalRepathThreshold;
        Vector3 b = transform.position + Vector3.down * verticalRepathThreshold;
        Gizmos.DrawLine(a + Vector3.left * 0.2f, a + Vector3.right * 0.2f);
        Gizmos.DrawLine(b + Vector3.left * 0.2f, b + Vector3.right * 0.2f);

        // draw player and steering target if available
        if (player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
            Gizmos.DrawWireSphere(player.position, 0.12f);
        }

        if (ai != null && ai.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ai.steeringTarget, 0.12f);
            Gizmos.DrawLine(transform.position, ai.steeringTarget);
        }

    }

    // Helper: checks whether the configured animator contains a parameter with the given name.
    private bool AnimatorHasParameter(string paramName)
    {
        if (animator == null) return false;
        if (string.IsNullOrEmpty(paramName)) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }
}