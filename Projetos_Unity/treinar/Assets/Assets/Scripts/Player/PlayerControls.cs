using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private Controls controls;
    private float valueX;
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float jumpForce;
    private Animator anim;
    private bool grounded;
    private bool attackActive = false;
    [SerializeField] private PolygonCollider2D polyColl;
    [SerializeField] private GameObject attackSfx; 
    [Header("GroundCheck")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayLength;
    [SerializeField] private LayerMask groundLayer;
 
    public void EnableControls()
    {
        controls.Enable();
    }
    public void DisableControls()
    {
        controls.Disable();
        valueX = 0;
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        controls = new Controls();
        controls.Player.Move.performed += Movement;
        controls.Player.Move.canceled += StopMovement;
        controls.Player.Jump.performed += Jumping;
        controls.Player.Attack.performed += Attacking;
        controls.Enable();
    }

    private void Movement(InputAction.CallbackContext ctx)
    {
        valueX = ctx.ReadValue<float>();
    }
    private void StopMovement(InputAction.CallbackContext ctx)
    {
        valueX = 0;
    }
    private void Jumping(InputAction.CallbackContext ctx)
    {
        if (grounded)
        {
            rb.AddForce(Vector2.up*jumpForce, ForceMode2D.Impulse);
        }
    }
    private void Attacking(InputAction.CallbackContext ctx)
    {
        if (grounded && attackActive == false)
        {
            anim.SetTrigger("Attack");
            attackActive = true;
            attackSfx.SetActive(true);
            
        }
            
    }
 
    public void ActivateAttackCollider()
    {
        polyColl.enabled = true;
    }
    public void AttackReset()
    {
        attackActive = false;
        attackSfx.SetActive(false);
        polyColl.enabled = false;
    }
    private void FixedUpdate()
    {
        grounded = Physics2D.Raycast(rayOrigin.position, Vector2.down, rayLength, groundLayer);
        rb.linearVelocity = new Vector2(speed * valueX, rb.linearVelocity.y);
    }
    void Update()
    {
        if (valueX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (valueX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        UpdateAnimationValues();
    }

    private void UpdateAnimationValues()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("Grounded", grounded);
        anim.SetFloat("vSpeed", rb.linearVelocity.y);
    }
    private void OnDisable()
    {
        controls.Player.Move.performed -= Movement;
        controls.Player.Move.canceled -= StopMovement;
        controls.Player.Jump.performed -= Jumping;
        controls.Player.Attack.performed -= Attacking;
        controls.Disable();
    }

}
