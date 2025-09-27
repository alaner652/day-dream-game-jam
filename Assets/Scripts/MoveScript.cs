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
        // ���a��J
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D �� ���k��
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S �� �W�U��
        moveInput.Normalize(); // ����ר����ʹL��
    }

    void FixedUpdate()
    {
        // ����
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

}
