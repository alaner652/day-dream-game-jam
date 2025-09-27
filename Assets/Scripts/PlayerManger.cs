using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerManger : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("跳躍輔助設定")]
    [Tooltip("土狼時間 - 離開平台後仍可跳躍的時間")]
    public float coyoteTime = 0.2f;
    [Tooltip("跳躍緩衝時間 - 提前按跳躍鍵的有效時間")]
    public float jumpBufferTime = 0.2f;
    [Tooltip("跳躍冷卻時間")]
    public float jumpCooldown = 1f;
    [Tooltip("地面檢測距離")]
    public float groundCheckDistance = 0.1f;
    [Tooltip("地面檢測寬度（用於邊緣容錯）")]
    public float groundCheckWidth = 0.8f;

    [Header("牆跳設定")]
    [Tooltip("牆跳最大次數")]
    public int maxWallJumps = 3;
    [Tooltip("牆跳力度")]
    public float wallJumpForce = 4f;

    [Header("地面檢測")]
    public string groundTag = "Ground";

    [Header("死亡設定")]
    [Tooltip("死亡區域標籤")]
    public string deathTag = "DeathZone";
    [Tooltip("敵人標籤")]
    public string enemyTag = "Enemy";

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool wasGrounded;
    private float horizontalInput;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCooldownTimer;

    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;
    private int wallJumpCount;
    private bool canWallJump;

    private bool isDead = false;

    public enum PlayerState
    {
        Idle,
        Walk,
        Jump,
        Dead
    }

    private PlayerState currentState = PlayerState.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;

        // 確保初始狀態正確
        isDead = false;
        currentState = PlayerState.Idle;
        isGrounded = false;

        // 重置計時器
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpCooldownTimer = 0f;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleJumpCooldown();
        HandleInput();
        HandleMovement();
        UpdateState();
    }

    void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void HandleJumpBuffer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void HandleJumpCooldown()
    {
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }
    }

    void HandleInput()
    {
        // 死亡時無法控制
        if (isDead)
        {
            return;
        }

        // 只有在遊戲進行中才能控制
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || coyoteTimeCounter > 0f) && jumpCooldownTimer <= 0f)
        {
            Jump();
        }
    }

    void HandleMovement()
    {
        // 死亡時無法移動
        if (isDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 只有在遊戲進行中才能移動
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
        {
            return;
        }

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

        // 死亡狀態優先
        if (isDead)
        {
            if (currentState != PlayerState.Dead)
            {
                currentState = PlayerState.Dead;
            }
        }
        // 首先檢查跳躍狀態
        else if (!isGrounded)
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
        if (animator == null) return;

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
            case PlayerState.Dead:
                animator.SetTrigger("isDead");
                break;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        currentState = PlayerState.Jump;
        coyoteTimeCounter = 0f;
        jumpCooldownTimer = jumpCooldown; // 啟動冷卻時間

        UpdateAnimation();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag(deathTag) || collision.gameObject.CompareTag(enemyTag))
        {
            Die();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(deathTag) || other.CompareTag(enemyTag))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return; // 避免重複死亡

        isDead = true;
        Debug.Log("玩家死亡！");

        // 通知 GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void Respawn()
    {
        isDead = false;
        currentState = PlayerState.Idle;
        Debug.Log("玩家重生！");
    }

}