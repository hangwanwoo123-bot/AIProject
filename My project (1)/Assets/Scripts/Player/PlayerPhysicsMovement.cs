using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPhysicsMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("바닥 체크 설정")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 boxSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            float left = Keyboard.current.aKey.isPressed ? -1f : 0f;
            float right = Keyboard.current.dKey.isPressed ? 1f : 0f;
            horizontalInput = left + right;

            isGrounded = Physics2D.OverlapBox(groundCheck.position, boxSize, 0f, groundLayer);

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        UpdateAnimations();
        FlipSprite();
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f;

        // 업로드하신 Controller.controller의 파라미터 이름에 정확히 맞췄습니다.
        anim.SetBool("Walk", isMoving);
        anim.SetBool("Idle", !isMoving && isGrounded);
        anim.SetBool("Jump", !isGrounded && rb.linearVelocity.y > 0.1f);
        anim.SetBool("Fall", !isGrounded && rb.linearVelocity.y < -0.1f);

        // 컨트롤러의 전제 조건인 Action 파라미터를 강제로 false로 유지 (다른 액션 방해 방지)
        anim.SetBool("Action", false);
    }

    void FlipSprite()
    {
        if (horizontalInput > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
        // Unity 6 권장 방식
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}

