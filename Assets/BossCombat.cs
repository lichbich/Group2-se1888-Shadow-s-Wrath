using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Animator))]
public class BossCombat : MonoBehaviour
{
    [Header("References")]
    public Animator animator;                       // Animator with triggers "Attack1" and "Attack2"
    public Transform player;                        // player transform (can be assigned in inspector)
    public AIPath aiPath;                           // AIPath to pause movement during attacks

    [Header("Attack Prefabs (active frames)")]
    public GameObject attack1Prefab;
    public GameObject attack2Prefab;

    [Header("Spawn Points")]
    [Tooltip("Default spawn point used when no specific spawn point is set.")]
    public Transform attackSpawnPoint;
    [Tooltip("Optional: spawn point specifically for Attack1 (overrides default when assigned).")]
    public Transform attack1SpawnPoint;
    [Tooltip("Optional: spawn point specifically for Attack2 (overrides default when assigned).")]
    public Transform attack2SpawnPoint;

    [Header("Attack Pattern Timing (seconds)")]
    public float waitAfterFirstAttack = 1f;         // A
    public float waitAfterSecondAttack = 1f;        // B
    public float waitAfterThirdAttack = 2f;         // C

    [Header("Active frame durations")]
    public float attack1ActiveDuration = 0.25f;     // used to pause AI while the hit is active
    public float attack2ActiveDuration = 0.4f;

    [Header("Behavior")]
    public float attackRange = 2.5f;
    public bool loopPatternWhileInRange = true;
    public bool pauseAiDuringActiveFrame = true;    // pause AI for the active-frame duration

    // new: how long the boss should remain completely still AFTER the active frame (in seconds)
    // set to 0 to keep original behaviour
    [Tooltip("Duration (s) the boss stays still AFTER the active frame (added to active-frame).")]
    public float postAttackFreezeDuration = 0.15f;

    // new: extra pause to apply specifically to AI pathing (use this to lengthen AI pause without changing animation timings)
    [Tooltip("Extra seconds added to AI pause after an attack. Use this to extend AI pathing pause without altering animation timings.")]
    public float aiPauseExtraDuration = 0f;

    // new: whether to freeze facing (prevent flipping X scale) while paused
    public bool freezeFacingDuringAttack = true;

    [Header("Grounding (attack allowed only when grounded)")]
    [Tooltip("Layer mask used for ground detection")]
    public LayerMask groundLayer = ~0;
    [Tooltip("Ground check circle radius (more robust than a single ray)")]
    public float groundCheckRadius = 0.08f;
    [Tooltip("Ground check ray distance (from pivot down)")]
    public float groundCheckDistance = 0.12f;

    // runtime
    private bool runningPattern = false;
    private int attackStep = 0; // 1..3
    private Coroutine aiResumeCoroutine;

    // facing freeze (prevents frantic flipping during attack + post-attack freeze)
    private bool freezeFacing = false;
    private Coroutine freezeFacingCoroutine;
    private float frozenFacingSign = 1f;

    // cache Rigidbody2D for zeroing velocity when needed
    private Rigidbody2D rb;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        if (attackSpawnPoint == null) attackSpawnPoint = transform;
        if (attack1SpawnPoint == null) attack1SpawnPoint = attackSpawnPoint;
        if (attack2SpawnPoint == null) attack2SpawnPoint = attackSpawnPoint;
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (aiPath == null) aiPath = GetComponent<AIPath>();
        if (attackSpawnPoint == null) attackSpawnPoint = transform;
        // preserve explicit per-attack points if already set, otherwise fallback to default
        if (attack1SpawnPoint == null) attack1SpawnPoint = attackSpawnPoint;
        if (attack2SpawnPoint == null) attack2SpawnPoint = attackSpawnPoint;

        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist <= attackRange && !runningPattern && IsGrounded())
        {
            StartCoroutine(AttackPatternRoutine());
        }
    }

    // enforce frozen facing in LateUpdate so other scripts' flips are overridden while frozen
    private void LateUpdate()
    {
        if (freezeFacing)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * frozenFacingSign;
            transform.localScale = s;
        }
    }

    private IEnumerator AttackPatternRoutine()
    {
        runningPattern = true;

        // store AI movement state so we can restore it when done
        bool prevCanMove = true;
        if (aiPath != null) prevCanMove = aiPath.canMove;

        do
        {
            attackStep = (attackStep % 3) + 1;

            // ensure boss is grounded before performing each attack
            // if boss becomes airborne, wait until grounded or until player leaves range
            while (!IsGrounded())
            {
                if (player == null || Vector2.Distance(player.position, transform.position) > attackRange)
                    break;
                yield return null;
            }

            // Trigger animation only. Animator / Animation Events must spawn active frames now.
            if (attackStep == 1)
            {
                TriggerAttack("Attack2");
                yield return new WaitForSeconds(waitAfterFirstAttack);
            }
            else if (attackStep == 2)
            {
                TriggerAttack("Attack2");
                yield return new WaitForSeconds(waitAfterSecondAttack);
            }
            else // 3
            {
                TriggerAttack("Attack1");
                yield return new WaitForSeconds(waitAfterThirdAttack);
            }

            if (!loopPatternWhileInRange) break;

        } while (player != null && Vector2.Distance(player.position, transform.position) <= attackRange);

        if (aiPath != null) aiPath.canMove = prevCanMove;
        runningPattern = false;
    }

    private void TriggerAttack(string triggerName)
    {
        if (animator == null) return;

        // clear the other trigger to avoid accidental cross-triggers
        if (triggerName == "Attack1") animator.ResetTrigger("Attack2");
        else if (triggerName == "Attack2") animator.ResetTrigger("Attack1");

        // trigger animation
        animator.SetTrigger(triggerName);

        // Pause AI and optionally freeze facing for the configured durations.  
        // We include postAttackFreezeDuration and aiPauseExtraDuration so the boss stands still for a few frames after the active frame
        // and so the AI pathing can be extended independently in the inspector.
        float activeDuration = triggerName == "Attack1" ? attack1ActiveDuration : attack2ActiveDuration;
        float totalPause = activeDuration + postAttackFreezeDuration + aiPauseExtraDuration;

        if (pauseAiDuringActiveFrame && aiPath != null)
        {
            PauseAiTemporarily(totalPause);
        }

        if (freezeFacingDuringAttack)
        {
            StartFreezeFacingFor(activeDuration + postAttackFreezeDuration);
        }
    }

    private void StartFreezeFacingFor(float seconds)
    {
        // capture current facing (sign of X scale)
        frozenFacingSign = Mathf.Sign(transform.localScale.x);
        freezeFacing = true;

        if (freezeFacingCoroutine != null) StopCoroutine(freezeFacingCoroutine);
        freezeFacingCoroutine = StartCoroutine(UnfreezeFacingAfter(seconds));
    }

    private IEnumerator UnfreezeFacingAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        freezeFacing = false;
        freezeFacingCoroutine = null;
    }

    /// <summary>
    /// Public API: temporarily stop AI movement and optionally freeze facing for a duration.
    /// Call this from damage/knockback code to keep the boss from flipping/moving while hit.
    /// </summary>
    public void TemporarilyStop(float seconds, bool freezeFacingAlso = true, bool zeroVelocity = true)
    {
        if (aiPath != null)
        {
            // stop any pending resume to avoid overlapping resumes
            if (aiResumeCoroutine != null) StopCoroutine(aiResumeCoroutine);
            aiPath.canMove = false;
            aiResumeCoroutine = StartCoroutine(ResumeAIAfter(seconds));
        }

        if (freezeFacingAlso)
        {
            frozenFacingSign = Mathf.Sign(transform.localScale.x);
            freezeFacing = true;
            if (freezeFacingCoroutine != null) StopCoroutine(freezeFacingCoroutine);
            freezeFacingCoroutine = StartCoroutine(UnfreezeFacingAfter(seconds));
        }

        if (zeroVelocity && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void PauseAiTemporarily(float seconds)
    {
        if (aiPath == null) return;
        // stop any pending resume to avoid multiple overlapping resumes
        if (aiResumeCoroutine != null) StopCoroutine(aiResumeCoroutine);
        aiPath.canMove = false;
        aiResumeCoroutine = StartCoroutine(ResumeAIAfter(seconds));
    }

    private IEnumerator ResumeAIAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (aiPath != null) aiPath.canMove = true;
        aiResumeCoroutine = null;
    }

    /// <summary>
    /// Spawn helpers callable from Animation Events.
    /// - Add an Animation Event that calls one of these on the boss GameObject.
    /// - The float overload can be used to delay spawn by that many seconds.
    /// Examples:
    ///   Animation Event -> Animation_SpawnAttack1
    ///   Animation Event (with float = 0.05) -> Animation_SpawnAttack2(0.05)
    /// </summary>
    public void Animation_SpawnAttack1()
    {
        if (!IsGrounded()) return;
        SpawnActiveFrame(attack1Prefab, attack1ActiveDuration);
    }

    public void Animation_SpawnAttack2()
    {
        if (!IsGrounded()) return;
        SpawnActiveFrame(attack2Prefab, attack2ActiveDuration);
    }

    // Animation Events can pass a single float parameter. Use this to delay spawn from the event.
    public void Animation_SpawnAttack1(float delay)
    {
        StartCoroutine(SpawnActiveFrameDelayed(attack1Prefab, attack1ActiveDuration, delay, ""));
    }

    public void Animation_SpawnAttack2(float delay)
    {
        StartCoroutine(SpawnActiveFrameDelayed(attack2Prefab, attack2ActiveDuration, delay, ""));
    }

    /// <summary>
    /// Wait the configured delay, then spawn the active frame. If requiredStateName is provided (not used by animation event overloads above)
    /// you could extend this to validate the animator state before spawning.
    /// </summary>
    private IEnumerator SpawnActiveFrameDelayed(GameObject prefab, float activeDuration, float delay, string requiredStateName)
    {
        if (prefab == null) yield break;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // (optional) check animator state if requiredStateName is supplied:
        if (animator != null && !string.IsNullOrEmpty(requiredStateName))
        {
            var current = animator.GetCurrentAnimatorStateInfo(0);
            bool inCurrent = current.IsName(requiredStateName);
            bool inNext = false;
            if (animator.IsInTransition(0))
            {
                var next = animator.GetNextAnimatorStateInfo(0);
                inNext = next.IsName(requiredStateName);
            }

            if (!(inCurrent || inNext)) yield break;
        }

        // respect grounding for delayed spawns as well
        if (!IsGrounded()) yield break;

        SpawnActiveFrame(prefab, activeDuration);
    }

    private void SpawnActiveFrame(GameObject prefab, float activeDuration)
    {
        if (prefab == null) return;

        // choose spawn point:
        Transform chosen = null;
        if (prefab == attack1Prefab && attack1SpawnPoint != null) chosen = attack1SpawnPoint;
        else if (prefab == attack2Prefab && attack2SpawnPoint != null) chosen = attack2SpawnPoint;
        else if (attackSpawnPoint != null) chosen = attackSpawnPoint;
        else chosen = transform;

        Vector3 spawnPos = chosen.position;

        // If no explicit spawn transform was provided (we're using the boss transform),
        // keep the previous behaviour of offsetting slightly in front of the boss.
        bool usedDefaultTransform = (chosen == transform);
        if (usedDefaultTransform)
        {
            float facing = Mathf.Sign(transform.localScale.x);
            spawnPos += Vector3.right * 0.6f * facing;
        }

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        Vector3 s = go.transform.localScale;
        s.x = Mathf.Abs(s.x) * Mathf.Sign(transform.localScale.x);
        go.transform.localScale = s;

        if (activeDuration > 0f) Destroy(go, activeDuration);
    }

    private bool IsGrounded()
    {
        Vector2 origin = transform.position;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, groundCheckRadius, Vector2.down, groundCheckDistance, groundLayer);
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.isTrigger) continue;
            if (rb != null && hit.collider.attachedRigidbody == rb) continue;
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // draw grounding check for convenience
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
