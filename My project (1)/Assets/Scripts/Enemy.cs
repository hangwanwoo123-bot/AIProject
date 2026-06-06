using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    public bool usePatrol = true;
    public float patrolDistance = 3f;
    public float patrolWaitTime = 2f;
    private Vector2 patrolStartPos;
    private Vector2 targetPatrolPos;
    private bool isWaiting = false;
    private int directionFactor = 1;

    [Header("Health Settings")]
    public int maxHealth = 30;
    private int currentHealth;
    private bool isDead = false;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.3f;

    [Header("Combat Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;

    private bool isKnockback = false;
    private Transform target;
    private Rigidbody2D rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    [Header("Attack Range Settings")]
    public Transform attackPoint;
    public float attackRange = 1.0f;
    public LayerMask playerLayer;

    [Header("Drop Settings")]
    public GameObject coinPrefab;

    private float _currentMoveDirX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // 플레이어 태그로 타겟 자동 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;

        patrolStartPos = transform.position;
        targetPatrolPos = patrolStartPos + new Vector2(patrolDistance, 0);
    }

    void Update()
    {
        if (isDead || isKnockback || target == null) return;

        float distToPlayer = Vector2.Distance(transform.position, target.position);

        if (distToPlayer <= detectionRange)
        {
            // 플레이어 감지 시 정찰 중지 및 대기 상태 해제
            isWaiting = false;
            FollowPlayer();
        }
        else if (usePatrol && !isWaiting)
        {
            Patrol();
        }
        else if (usePatrol && isWaiting)
        {
            // 대기 중일 때는 속도 0 및 애니메이션 정지
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (_animator != null) _animator.SetBool("IsMoving", false);
        }
    }

    void Patrol()
    {
        // 방향 계산 및 이동
        float dirX = targetPatrolPos.x - transform.position.x;
        float moveDir = dirX > 0 ? 1 : -1;

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
        Flip(moveDir);

        if (_animator != null) _animator.SetBool("IsMoving", true);

        // [핵심 수정] 도달 판정을 Vector2.Distance < 0.2f 정도로 여유 있게 설정
        if (Mathf.Abs(transform.position.x - targetPatrolPos.x) < 0.2f)
        {
            StartCoroutine(PatrolRoutine());
        }
    }

    IEnumerator PatrolRoutine()
    {
        isWaiting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (_animator != null) _animator.SetBool("IsMoving", false);

        yield return new WaitForSeconds(patrolWaitTime);

        // 다음 목표 지점 반전
        directionFactor *= -1;
        targetPatrolPos = patrolStartPos + new Vector2(directionFactor * patrolDistance, 0);

        isWaiting = false;
    }

    void FollowPlayer()
    {
        float dist = Vector2.Distance(transform.position, target.position);
        float dirX = target.position.x - transform.position.x;
        float moveDir = dirX > 0 ? 1 : -1;

        if (dist > stopDistance)
        {
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
            Flip(moveDir);
            if (_animator != null) _animator.SetBool("IsMoving", true);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (_animator != null) _animator.SetBool("IsMoving", false);

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void Flip(float dirX)
    {
        if (Mathf.Abs(dirX) > 0.01f)
        {
            transform.localScale = new Vector3(dirX > 0 ? 1 : -1, 1, 1);
        }
    }

    void Attack()
    {
        if (_animator != null) _animator.SetTrigger("Attack");

        // 1. 공격 범위 내의 플레이어 레이어 확인
        Collider2D playerCollider = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

        if (playerCollider != null)
        {
            // 2. 플레이어로부터 PlayerHealth 스크립트 가져오기
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 3. 넉백 방향 계산 (몬스터 -> 플레이어 방향)
                Vector2 knockbackDir = (playerCollider.transform.position - transform.position).normalized;

                // 4. 플레이어에게 데미지와 넉백 전달
                playerHealth.TakeDamage(attackDamage, knockbackDir);

                Debug.Log("플레이어에게 " + attackDamage + "의 데미지를 입혔습니다!");
            }
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        StopCoroutine("KnockbackRoutine");
        StartCoroutine(KnockbackRoutine(knockbackDirection));
        StartCoroutine(HitColorEffect());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator KnockbackRoutine(Vector2 dir)
    {
        isKnockback = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * 8f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        isKnockback = false;
    }

    private IEnumerator HitColorEffect()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = Color.white;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (_animator != null) _animator.SetTrigger("Die");
        if (coinPrefab != null) Instantiate(coinPrefab, transform.position, Quaternion.identity);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}