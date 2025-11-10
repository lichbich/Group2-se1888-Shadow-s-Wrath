using UnityEngine;

[AddComponentMenu("Boss/Animation Spawn Property")]
public class AnimationSpawnProperty : MonoBehaviour
{
    [Tooltip("Reference to the BossCombat on the same GameObject (auto-find if empty).")]
    public BossCombat bossCombat;

    [Header("Spawn flags (animate these)")]
    [Tooltip("Set to true for one or more frames in the animation to spawn Attack1's active frame.")]
    public bool spawnAttack1 = false;
    [Tooltip("Optional delay (seconds) to pass into the float overload. If <= 0, immediate spawn is used.")]
    public float spawnAttack1Delay = 0f;

    [Tooltip("Set to true for one or more frames in the animation to spawn Attack2's active frame.")]
    public bool spawnAttack2 = false;
    [Tooltip("Optional delay (seconds) to pass into the float overload. If <= 0, immediate spawn is used.")]
    public float spawnAttack2Delay = 0f;

    // previous-frame storage to detect rising edge
    private bool prevSpawn1 = false;
    private bool prevSpawn2 = false;

    private void Reset()
    {
        if (bossCombat == null)
            bossCombat = GetComponent<BossCombat>();
    }

    private void Awake()
    {
        if (bossCombat == null)
            bossCombat = GetComponent<BossCombat>();
    }

    private void Update()
    {
        if (bossCombat == null) return;

        // Attack1 rising edge
        if (spawnAttack1 && !prevSpawn1)
        {
            if (spawnAttack1Delay > 0f)
                bossCombat.Animation_SpawnAttack1(spawnAttack1Delay);
            else
                bossCombat.Animation_SpawnAttack1();
        }

        // Attack2 rising edge
        if (spawnAttack2 && !prevSpawn2)
        {
            if (spawnAttack2Delay > 0f)
                bossCombat.Animation_SpawnAttack2(spawnAttack2Delay);
            else
                bossCombat.Animation_SpawnAttack2();
        }

        prevSpawn1 = spawnAttack1;
        prevSpawn2 = spawnAttack2;
    }
}