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
    public Transform attackSpawnPoint;

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
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (aiPath == null) aiPath = GetComponent<AIPath>();
        if (attackSpawnPoint == null) attackSpawnPoint = transform;
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
        if (dist <= attackRange && !runningPattern)
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

            if (attackStep == 1)
            {
                TriggerAttack("Attack2");
                SpawnActiveFrame(attack1Prefab, attack1ActiveDuration);
                yield return new WaitForSeconds(waitAfterFirstAttack);
            }
            else if (attackStep == 2)
            {
                TriggerAttack("Attack2");
                SpawnActiveFrame(attack1Prefab, attack1ActiveDuration);
                yield return new WaitForSeconds(waitAfterSecondAttack);
            }
            else // 3
            {
                TriggerAttack("Attack1");
                SpawnActiveFrame(attack2Prefab, attack2ActiveDuration);
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

    private void SpawnActiveFrame(GameObject prefab, float activeDuration)
    {
        if (prefab == null) return;

        Vector3 spawnPos = attackSpawnPoint != null ? attackSpawnPoint.position : transform.position;
        if (attackSpawnPoint == null || attackSpawnPoint == transform)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
