using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public PlayerAttack playerAttack;    // drag PlayerAttack component (or leave null and it will be ignored)
    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;

    bool jump = false;

    private void Awake()
    {
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        // If currently attacking and you want to freeze horizontal control, zero horizontalMove.
        // Still allow jump input so double-jump works while attacking.
        if (playerAttack != null && playerAttack.IsAttacking)
        {
            horizontalMove = 0f;
        }
        else
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        }

        // update animator speed (show zero when attacking for crisp feedback)
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        // Optional: tell attack about facing direction (useful if you rely on this)
        if (playerAttack != null)
        {
            if (horizontalMove > 0f) playerAttack.SetFacingDirection(true);
            else if (horizontalMove < 0f) playerAttack.SetFacingDirection(false);
        }
    }

    public void OnLanding() 
    {
        if (animator != null);
    }
        
    // FixedUpdate is called at a fixed interval and is independent of frame rate
    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        // Consume the jump request so holding the button won't auto-jump on landing
        jump = false;
    }
}