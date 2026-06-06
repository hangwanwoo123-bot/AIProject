using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
using System.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 450f;
    public float rollSpeed = 12f;

    [Header("Detection")]
    public LayerMask groundLayer;
    public float rayLength = 0.55f;
    public bool isGrounded;

    private Rigidbody2D _rigidbody;
    private CharacterAnimation _animation;
    private PlayerHealth _playerHealth;

    public Vector2 moveInput;
    private bool _jump;
    private bool _isRolling = false;
    private bool _wasGrounded; // 이전 프레임 지면 상태
    public bool canMove = true;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animation = GetComponent<CharacterAnimation>();
        _playerHealth = GetComponent<PlayerHealth>();
        _wasGrounded = true;
    }

    void Update()
    {
        // 1. 지면 체크 업데이트
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

        // 2. 구르기 입력 (N키) - 최우선 순위
        if (canMove && UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) && !_isRolling)
        {
            if (StaminaManager.Instance != null && StaminaManager.Instance.CanRoll())
            {
                StartCoroutine(RollRoutine());
                return; // 구르기 시작 시 이번 프레임의 다른 로직 스킵
            }
        }

        // 3. 이동 입력 처리
        if (!canMove || _isRolling || (_playerHealth != null && _playerHealth.isKnockback))
        {
            moveInput = Vector2.zero;
        }
        else
        {
            float moveX = 0;
            if (Input.GetKey(KeyCode.D)) moveX += 1f;
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;

            // 수직 이동(moveInput.y)은 0으로 고정됩니다.
            moveInput = new Vector2(moveX, 0f);
        }
         
        // 4. 애니메이션 제어 (구르기 중이 아닐 때만)
        if (_animation != null && !_isRolling)
        {
            HandleAnimations();
        }

        // 5. 점프 입력
        if (canMove && !_isRolling && UnityEngine.Input.GetButtonDown("Jump") && isGrounded)
        {
            _jump = true;
        }

        _wasGrounded = isGrounded;
    }

    private void HandleAnimations()
    {
        // 1. 넉백 중에는 어떤 애니메이션 명령도 내리지 않음
        if (_playerHealth != null && _playerHealth.isKnockback) return;

        // 2. [추가] 넉백은 아니지만 아직 공중에 떠 있고, 수직 속도가 하강 중이라면 
        // 점프 대신 낙하(Fall)나 대기 상태를 유지하도록 유도합니다.
        if (!isGrounded && _rigidbody.linearVelocity.y < 0)
        {
            if (_animation.GetState() != CharacterState.Fall)
                _animation.Fall();
            return;
        }
        // 착지 감지 (공중 -> 땅)
        if (isGrounded && !_wasGrounded)
        {
            // PixelHeroes의 Land는 내부적으로 Idle/Walk로 전이되므로 Idle 호출로 충분합니다.
            _animation.Idle();
        }

        if (isGrounded)
        {
            // 지면에 있을 때
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                // 이미 달리는 중이면 다시 호출하지 않음 (애니메이션 반복 방지)
                if (_animation.GetState() != CharacterState.Run)
                    _animation.Run();
            }
            else
            {
                if (_animation.GetState() != CharacterState.Idle)
                    _animation.Idle();
            }
        }
        else
        {// [중요] 넉백 중이 아닐 때만 점프/하강 애니메이션을 처리합니다.
         // 이미 위에서 return을 해줬지만, 안전을 위해 조건을 한 번 더 확인합니다.
            if (_playerHealth != null && !_playerHealth.isKnockback)
            {
                if (_rigidbody.linearVelocity.y > 0.1f)
                {
                    if (_animation.GetState() != CharacterState.Jump)
                        _animation.Jump();
                }
                else if (_rigidbody.linearVelocity.y < -0.1f)
                {
                    if (_animation.GetState() != CharacterState.Fall)
                        _animation.Fall();
                }
            }
        }
    }

    void FixedUpdate()
    {
        // [중요] 넉백 중일 때는 컨트롤러가 속도를 0으로 덮어쓰지 못하게 리턴
        if (_playerHealth != null && _playerHealth.isKnockback)
        {
            return;
        }
        // 구르기 중이거나 조작 불가 시
        if (_isRolling || !canMove)
        {
            // 구르기 중에는 RollRoutine에서 속도를 제어하므로 건드리지 않음
            if (!canMove && !_isRolling)
                _rigidbody.linearVelocity = new Vector2(0, _rigidbody.linearVelocity.y);
            return;
        }

        // 일반 수평 이동
        float targetVelocityX = moveInput.x * moveSpeed;
        _rigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody.linearVelocity.y);

        // 방향 전환
        if (moveInput.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);

        // 점프 물리
        if (_jump)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, 0);
            _rigidbody.AddForce(Vector2.up * jumpForce);
            _jump = false;
        }
    }

    private IEnumerator RollRoutine()
    {
        _isRolling = true;

        // 자원 소모 및 무적
        if (StaminaManager.Instance != null) StaminaManager.Instance.UseStaminaForRoll();
        if (_playerHealth != null) _playerHealth.SetRollingInvincible(true);

        // 구르기 애니메이션 실행
        if (_animation != null) _animation.Roll();

        // 방향 고정 (구르기 시작 시점의 방향)
        float rollDir = transform.localScale.x;
        float rollDuration = 0.45f;
        float timer = 0f;

        while (timer < rollDuration)
        {
            // 구르기 물리 이동 (Unity 6 linearVelocity)
            _rigidbody.linearVelocity = new Vector2(rollDir * rollSpeed, _rigidbody.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        // 상태 복구
        if (_playerHealth != null) _playerHealth.SetRollingInvincible(false);
        _isRolling = false;

        // 구르기 종료 후 즉시 애니메이션 갱신
        if (_animation != null) _animation.Idle();
    }
}