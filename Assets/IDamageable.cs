using UnityEngine;

public interface IDamageable
{
    // Apply damage with hit point and impulse force (knockback).
    void TakeDamage(float amount, Vector2 hitPoint, Vector2 force);
}