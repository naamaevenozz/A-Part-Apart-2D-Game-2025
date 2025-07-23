using System;
using System.Collections;
using APA.Core;
using UnityEngine;
namespace _APA.Scripts
{
    public class APAPlayerMovement : APAMonoBehaviour
    {
        [Header("Key Bindings")]
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode jumpKey = KeyCode.W;

        [Header("Movement Settings")]
        public float acceleration = 50f;
        public float maxSpeed = 7f;
        public float jumpForce = 12f;

        [Header("Ground Check Settings")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.1f;
        public LayerMask groundLayer;

        protected Rigidbody2D rb;
        private Animator anim;

        private bool isTryJump;
        private float horizontalInput;
        private bool isGrounded;
        private bool wasInAir = false;
        [SerializeField] private string playerId;

        protected Coroutine fixedUpdateCoroutine;
        protected Coroutine updateCoroutine;
        private bool isFacingRight = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
        }
        private void Start()
        {
            fixedUpdateCoroutine = StartCoroutine(FixedUpdateCoroutine());
            updateCoroutine = StartCoroutine(UpdateCoroutine());

            AddListener(APAEventName.OnPlayerCanMove, OnPlayerCanMove);
        }
        private IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                if (Input.GetKey(moveRightKey))
                    horizontalInput = 1f;
                else if (Input.GetKey(moveLeftKey))
                    horizontalInput = -1f;
                else
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    horizontalInput = 0f;
                }

                isTryJump = Input.GetKey(jumpKey);
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
                UpdateAnimations();
                FlipSprite();

                yield return null;
            }
        }
        private void OnDestroy()
        {
            RemoveListener(APAEventName.OnPlayerCanMove, OnPlayerCanMove);
        }

        private void OnPlayerCanMove(object obj)
        {            
            APADebug.Log("Player can move from PlayerMovement");

            if (obj is bool canPlayerMove)
            {
                if (canPlayerMove)
                {
                    if (fixedUpdateCoroutine != null)
                    {
                        StopCoroutine(fixedUpdateCoroutine);
                    }
                    fixedUpdateCoroutine = StartCoroutine(FixedUpdateCoroutine());

                    if (updateCoroutine != null)
                    {
                        StopCoroutine(updateCoroutine);
                    }
                    updateCoroutine = StartCoroutine(UpdateCoroutine());
                }
                else
                {
                    if (fixedUpdateCoroutine != null)
                    {
                        StopCoroutine(fixedUpdateCoroutine);
                    }
                    anim.SetFloat("Speed", 0);
                    if (updateCoroutine != null)
                    {
                        StopCoroutine(updateCoroutine);
                    }
                }
            }
            APADebug.Log("Change player location from PlayerMovement");
            if(playerId == "Light"){transform.position = new Vector3(90f, 20f, 0f);}
            else
            {
                transform.position = new Vector3(90f, 5f, 0f);
            }
            
        }

        protected IEnumerator FixedUpdateCoroutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                if (horizontalInput != 0)
                {
                    rb.AddForce(new Vector2(horizontalInput * acceleration, 0f));
                }

                float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
                rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);

                if (isGrounded && isTryJump)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    anim.SetTrigger("JumpTrigger");
                    wasInAir = true;
                }

                yield return waitForFixedUpdate;
            }
        }
        private void UpdateAnimations()
        {
            float speedX = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Speed", speedX);
            if (isGrounded && wasInAir)
            {
                wasInAir = false;
            }
        }
        private void FlipSprite()
        {
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}