using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 玩家輸入
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左右鍵
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S 或 上下鍵
        moveInput.Normalize(); // 防止斜角移動過快
    }

    void FixedUpdate()
    {
        // 移動
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

}
