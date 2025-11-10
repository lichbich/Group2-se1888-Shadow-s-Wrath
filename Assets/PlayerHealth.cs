using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public static int healthCount;
    public TextMeshProUGUI healthText;

    [Tooltip("Optional: assign the hearts controller that will show/hide filled hearts")]
    public PlayerHeartsController heartsController;

    [Tooltip("Maximum player health (used for clamping and hearts).")]
    public int maxHealth = 3;

    [Header("Death Animation")]
    [Tooltip("Animator trigger used for the player's death animation.")]
    public string deathTrigger = "Death";

    [Header("Death Options")]
    [Tooltip("If > 0, destroys the player GameObject after this many seconds following death animation length.")]
    public float destroyDelay = 0.1f;

    private Animator animator;
    private bool isDead = false;
    private Rigidbody2D rb; // Cached rigidbody component

    // Public read-only accessor so other systems (like PlayerAttack) can check death state
    public bool IsDead => isDead;

    private void Start()
    {
        animator = GetComponent<Animator>();
        healthCount = Mathf.Clamp(maxHealth, 1, int.MaxValue);
        UpdateUI();
    }

    public static void AddHealth(int amount)
    {
        int max = GetMaxHealth();
        healthCount = Mathf.Clamp(healthCount + amount, 0, max);
        UpdateUIStatic();

        // If the player lost all health, trigger death animation then show lose UI
        if (healthCount <= 0)
        {
            var instance = FindObjectOfType<PlayerHealth>();
            if (instance != null)
            {
                instance.TriggerDeathAnimation();
            }

            GameUIManager.Instance?.ShowLose();
        }
    }

    public static void Resethealth()
    {
        healthCount = GetMaxHealth();
        UpdateUIStatic();
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = healthCount.ToString("00");

        if (heartsController != null)
            heartsController.SetHearts(healthCount);
    }

    private static void UpdateUIStatic()
    {
        var instance = FindObjectOfType<PlayerHealth>();
        if (instance != null)
        {
            if (instance.healthText != null)
                instance.healthText.text = healthCount.ToString("00");

            if (instance.heartsController != null)
                instance.heartsController.SetHearts(healthCount);
        }
    }

    // Helper that returns the configured maxHealth from the scene instance (fallback to 3)
    private static int GetMaxHealth()
    {
        var inst = FindObjectOfType<PlayerHealth>();
        if (inst != null)
            return Mathf.Max(1, inst.maxHealth);
        return 3;
    }

    // Instance API to trigger the death animation once
    public void TriggerDeathAnimation()
    {
        if (isDead) return;
        isDead = true;

        // First: stop known components (movement/attack/management) explicitly
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        var pmLevel2 = GetComponent<PlayerMovementLevel2>();
        if (pmLevel2 != null)
        {
            // PlayerMovementLevel2 has its own Die() method that properly sets its internal flags and triggers.
            pmLevel2.Die();
        }

        var pmLevel1 = GetComponent<PlayerManagementLevel1>();
        if (pmLevel1 != null)
            pmLevel1.enabled = false;

        var pa = GetComponent<PlayerAttack>();
        if (pa != null) pa.enabled = false;

        // Also disable generic controller/inputs on the same GameObject:
        var charCtrl = GetComponent<CharacterController2D>();
        if (charCtrl != null) (charCtrl as MonoBehaviour).enabled = false;

        // Disable all other MonoBehaviour scripts on this GameObject (except this script)
        // This ensures any other local input/update scripts stop running.
        var monos = GetComponents<MonoBehaviour>();
        foreach (var mb in monos)
        {
            if (mb == this) continue; // keep this script running so death handling continues
            // leave Animator (not a MonoBehaviour) alone; disabling MonoBehaviour scripts will stop input handlers
            try
            {
                mb.enabled = false;
            }
            catch
            {
                // ignore any that can't be disabled
            }
        }

        // freeze physics (optional)
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Play death on animator (animator is not a MonoBehaviour so it's unaffected by the loop above)
        if (animator != null)
        {
            // set the is-dead flag so any movement transitions are blocked
            animator.SetBool("IsDead", true);

            // ensure movement-related animator parameters are cleared/stopped so idle/run don't resume
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isRunning", false);

            // clear conflicting triggers and fire death trigger
            if (!string.IsNullOrEmpty(deathTrigger))
            {
                animator.ResetTrigger("Hit");
                animator.SetTrigger(deathTrigger);
            }
            else
            {
                // fallback: play the death state immediately
                animator.Play("Player_Death", 0, 0f);
            }

            // Let the animator play the death clip then lock animator and destroy the GameObject.
            StartCoroutine(DisableAnimatorAndDestroyAfterDeath());
        }
        else
        {
            // No animator: still destroy after short delay to allow other systems to react.
            StartCoroutine(DestroyAfterDelay(destroyDelay));
        }
    }

    // Waits for the death animation clip length (if found) then disables the Animator and destroys the player GameObject.
    private IEnumerator DisableAnimatorAndDestroyAfterDeath()
    {
        if (animator == null) yield break;

        // allow one frame for the death trigger to be processed
        yield return null;

        float deathClipLength = 0f;
        var rac = animator.runtimeAnimatorController;
        if (rac != null)
        {
            string searchKey = !string.IsNullOrEmpty(deathTrigger) ? deathTrigger.ToLower() : "death";
            foreach (var clip in rac.animationClips)
            {
                var clipName = clip.name.ToLower();
                if (clipName.Contains(searchKey) || clipName.Contains("death") || clipName.Contains("player_death"))
                {
                    deathClipLength = Mathf.Max(deathClipLength, clip.length);
                }
            }
        }

        if (deathClipLength <= 0f)
            deathClipLength = 1f; // fallback

        // Wait for the animation to finish (scaled time). If you show UITimeScale=0 you may want realtime.
        yield return new WaitForSeconds(deathClipLength + destroyDelay);

        // disable animator so it doesn't drive transitions back to entry/default
        if (animator != null)
            animator.enabled = false;

        // finally destroy the player GameObject
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
