using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching

	// NEW: configure how many jumps the player gets (2 = double jump)
	[SerializeField] private int m_MaxJumps = 2;
	private int m_JumpsRemaining;

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();

		m_JumpsRemaining = m_MaxJumps;
	}

	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider2D col = colliders[i];
			if (col == null)
				continue;

			// Ignore trigger colliders (not real ground)
			if (col.isTrigger)
				continue;

			// Ignore this GameObject's own colliders and any child colliders
			if (col.gameObject == gameObject || col.transform.IsChildOf(transform))
				continue;

			m_Grounded = true;
			if (!wasGrounded)
				OnLandEvent.Invoke();
			// We found valid ground, no need to check further
			break;
		}

		// Reset available jumps when grounded
		if (m_Grounded)
		{
			m_JumpsRemaining = m_MaxJumps;
		}
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// Move the character by finding the target velocity
			// Use Rigidbody2D.velocity (not a non-existent linearVelocity)
			Vector3 currentVel = new Vector3(m_Rigidbody2D.linearVelocity.x, m_Rigidbody2D.linearVelocity.y, 0f);
			Vector3 targetVelocity = new Vector3(move * 10f, m_Rigidbody2D.linearVelocity.y, 0f);
			Vector3 newVel = Vector3.SmoothDamp(currentVel, targetVelocity, ref m_Velocity, m_MovementSmoothing);
			m_Rigidbody2D.linearVelocity = new Vector2(newVel.x, newVel.y);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}

		// Jump handling: allow jump when request present and there are jumps remaining.
		if (jump && m_JumpsRemaining > 0)
		{
			// Optional: zero vertical velocity so jump height is consistent whether rising/falling
			m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0f);

			// Add a vertical force to the player.
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
			m_JumpsRemaining--;

			// Mark as not grounded (prevents immediate re-trigger)
			m_Grounded = false;
		}
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
