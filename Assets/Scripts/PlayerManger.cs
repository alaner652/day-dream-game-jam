using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
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
    [Tooltip("死亡後重新開始延遲時間")]
    public float deathRestartDelay = 5f;

    [Header("音效設定")]
    [Tooltip("跳躍音效")]
    public AudioClip jumpSound;
    [Tooltip("死亡音效")]
    public AudioClip deathSound;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    private bool isGrounded;
    private float horizontalInput;

    private float coyoteTimeCounter;
    private float jumpCooldownTimer;


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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 設定音量
        audioSource.volume = 1.0f;

        rb.freezeRotation = true;

        // 確保初始狀態正確
        isDead = false;
        currentState = PlayerState.Idle;
        isGrounded = false;

        // 重置計時器
        coyoteTimeCounter = 0f;
        jumpCooldownTimer = 0f;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        HandleCoyoteTime();
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

        // 播放跳躍音效
        if (audioSource != null && jumpSound != null)
        {
            audioSource.volume = 1.0f;  // 跳躍音效正常音量
            audioSource.PlayOneShot(jumpSound);
        }

        UpdateAnimation();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag(deathTag))
        {
            Die();
        }
        else if (!string.IsNullOrEmpty(enemyTag) && collision.gameObject.CompareTag(enemyTag))
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
        if (other.CompareTag(deathTag))
        {
            Die();
        }
        else if (!string.IsNullOrEmpty(enemyTag) && other.gameObject.CompareTag(enemyTag))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return; // 避免重複死亡

        isDead = true;
        Debug.Log("玩家死亡！使用完全重置...");

        // 播放死亡音效
        float actualDelay = deathRestartDelay;
        if (audioSource != null && deathSound != null)
        {
            audioSource.Stop();
            audioSource.volume = 5f;  // 死亡音效特別大聲
            audioSource.PlayOneShot(deathSound, 5f);  // 使用 PlayOneShot 並指定音量

            // 確保延遲時間至少等於音效長度
            float soundLength = deathSound.length;
            actualDelay = Mathf.Max(deathRestartDelay, soundLength + 0.5f);
            Debug.Log($"死亡音效長度: {soundLength}秒, 延遲時間: {actualDelay}秒");
        }

        // 延遲後使用完全重置（像按 R 鍵一樣）
        Invoke(nameof(DeathFullReset), actualDelay);
    }

    void DeathFullReset()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("死亡觸發完全重置");
            GameManager.Instance.FullReset();
        }
        else
        {
            // 如果沒有 GameManager，直接重新開始
            ForceRestartGame();
        }
    }

    void ForceRestartGame()
    {
        Debug.Log("強制重新載入場景...");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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