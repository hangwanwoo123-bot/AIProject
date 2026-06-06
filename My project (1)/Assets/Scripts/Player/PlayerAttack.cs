using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;    // 공격 중심점 (빈 오브젝트 생성 후 연결)
    public float attackRange = 0.5f; // 공격 사거리
    public LayerMask enemyLayers;    // Enemy 레이어 선택 필수
    public int attackDamage = 10;
    private CharacterAnimation _charAnim;
    private CharacterController2D _controller;
    private bool _isAttacking = false; // 중복 공격 방지용
    void Start()
    {
        // 캐릭터 애니메이션 컴포넌트 참조 [cite: 41]
        _charAnim = GetComponent<CharacterAnimation>();
        _controller = GetComponent<CharacterController2D>();
    }
  

    void Update()
    {
        if (_controller != null && !_controller.canMove) return;

        if (Input.GetKeyDown(KeyCode.Mouse0) && !_isAttacking)
        {
            Attack();
        }
    }

    void Attack()
    {
        _isAttacking = true;
        if (_charAnim != null) _charAnim.Slash();

        StartCoroutine(AttackRoutine());
        CharacterAnimation anim = GetComponent<CharacterAnimation>();
        if (anim != null) anim.Slash();
        // 1. 공격 범위 내의 모든 콜라이더를 가져옵니다.
        // [오류 해결] hitObject 대신 'hitEnemies'라는 배열을 사용합니다.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // 2. 찾아낸 모든 적에게 데미지와 넉백을 줍니다.
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 넉백 방향 계산: (적 위치 - 내 위치)
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;

                // [오류 해결] 수정된 Enemy.TakeDamage(데미지, 방향) 호출
                enemy.TakeDamage(attackDamage, knockbackDir);

                Debug.Log(enemyCollider.name + "에게 넉백 공격!");
            }
        }
        if (_charAnim != null)
        {
            _charAnim.Slash();
            // 공격 중 잠시 이동을 멈추고 싶다면 아래 코루틴 활용 가능
            StartCoroutine(StopMovementRoutine());
        }
    }

    // [오류 해결] 이 함수는 다른 함수(Attack 등) "밖에" 따로 선언해야 합니다.
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // 씬 뷰에서 공격 사거리를 빨간 원으로 보여줍니다.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    IEnumerator StopMovementRoutine()
    {
        var controller = GetComponent<CharacterController2D>();
        if (controller != null)
        {
            controller.canMove = false; // 속도를 0으로 바꾸는 대신 플래그 사용
            yield return new WaitForSeconds(0.3f);
            controller.canMove = true;
        }
    }
    IEnumerator AttackRoutine()
    {
        if (_controller != null)
        {
            _controller.canMove = false;

            // [추가] 공격 애니메이션이 시작될 때만 속도를 한 번 0으로 멈춰줍니다.
            // 이후에는 리턴되므로 외부 힘(넉백 등)에 반응할 수 있습니다.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(0.3f);

        if (_controller != null) _controller.canMove = true;
        _isAttacking = false;
    }
}