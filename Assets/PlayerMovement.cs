using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;

    public float runSpeed = 40f;

    float horizontalMove = 0f;

    bool jump = false;
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.GetAxisRaw("Horizontal"));
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate
    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        // Consume the jump request so holding the button won't auto-jump on landing
        jump = false;
    }
}
