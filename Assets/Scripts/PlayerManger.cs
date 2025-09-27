using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerManger : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float horizontalInput;

    public enum PlayerState
    {
        Idle,
        Walk,
        Jump
    }

    private PlayerState currentState = PlayerState.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        HandleInput();
        HandleMovement();
        UpdateState();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && currentState != PlayerState.Jump)
        {
            Jump();
        }
    }

    void HandleMovement()
    {
        float moveX = horizontalInput * moveSpeed;
        rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);

        // 角色翻轉
        if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void UpdateState()
    {
        PlayerState previousState = currentState;

        // 首先檢查跳躍狀態
        if (!isGrounded)
        {
            if (currentState != PlayerState.Jump)
            {
                currentState = PlayerState.Jump;
            }
        }
        // 在地面時的狀態邏輯
        else
        {
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                if (currentState != PlayerState.Walk)
                {
                    currentState = PlayerState.Walk;
                }
            }
            else
            {
                if (currentState != PlayerState.Idle)
                {
                    currentState = PlayerState.Idle;
                }
            }
        }

        // 只有狀態真正改變時才觸發動畫
        if (previousState != currentState)
        {
            UpdateAnimation();
        }
    }

    void UpdateAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        // 使用 Trigger 觸發狀態轉換
        switch (currentState)
        {
            case PlayerState.Idle:
                animator.SetTrigger("isIdle");
                break;
            case PlayerState.Walk:
                animator.SetTrigger("isWalk");
                break;
            case PlayerState.Jump:
                animator.SetTrigger("isJump");
                break;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        currentState = PlayerState.Jump;

        // 立即更新動畫
        UpdateAnimation();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}