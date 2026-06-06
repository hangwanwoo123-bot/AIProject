using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider hpSlider;

    [Header("UI Settings")]
    public GameObject gameOverUI;

    [Header("Invincible Settings")]
    public float invincibilityDuration = 1.5f; // 피격 후 무적 시간 (초)
    private bool isInvincible = false;         // 피격 무적 상태
    private bool isRollingInvincible = false;  // 구르기 무적 상태

    [Header("Damage Settings")]
    public int contactDamage = 10;

    private SpriteRenderer sr;
    private CharacterAnimation _animation;
    private Coroutine _hitEffectCoroutine;
    private Color originalColor = Color.white;

    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public float knockbackForce = 7f;      // 밀려나는 힘의 세기
    public float knockbackDuration = 0.2f; // 넉백 동안 조작 불가능한 시간

    [Header("Low HP Warning Settings")]
    public Image bloodOverlay;          // UI Image 연결
    public float warningThreshold = 0.3f; // 체력 30% 이하일 때 발동
    public float pulseSpeed = 2f;       // 깜빡임 속도
    private bool isWarning = false;
    public bool isKnockback = false; // [추가] 넉백 상태 확인용 변수

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UsePotion();
        }
        CheckLowHealth();
    }
    void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        _propBlock = new MaterialPropertyBlock();

        if (sr != null) originalColor = Color.white; // 기본 흰색 저장

        _animation = GetComponent<CharacterAnimation>();

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        // 무적 상태거나 이미 사망한 경우 무시
        if (isInvincible || isRollingInvincible || currentHealth <= 0) return;

        // 1. 체력 감소 처리
        currentHealth -= damage;

        // [추가] 상세 데미지 로그 출력
        // <color> 태그를 사용하면 유니티 콘솔에서 훨씬 눈에 잘 띕니다.
        Debug.Log($"<color=red>[피격]</color> 데미지: <b>{damage}</b> 수신 | <color=green>남은 체력: {currentHealth}</color>");

        // 2. UI 업데이트
        if (hpSlider != null) hpSlider.value = currentHealth;

        // 3. 넉백 실행
        StartCoroutine(KnockbackRoutine(attackerPosition));
        if (_animation != null)
        {
            _animation.Hit(); // Any State에서 Hit으로 전이되어 Jump를 덮어씁니다. 
        }
        // 4. 사망 체크 및 후속 처리
        if (currentHealth <= 0)
        {
            PlayerDie();
        }
        else
        {
            // 넉백 방향 계산 (공격자 반대 방향)
            Vector2 knockbackDir = (transform.position - attackerPosition).normalized;

            // 대각선 위로 살짝 뜨게 설정 (선택 사항)
            knockbackDir += Vector2.up * 0.5f;
            knockbackDir = knockbackDir.normalized;

            StartCoroutine(KnockbackRoutine(knockbackDir));
            StartCoroutine(InvincibilityRoutine());

            if (_hitEffectCoroutine != null) StopCoroutine(_hitEffectCoroutine);
            _hitEffectCoroutine = StartCoroutine(HitColorEffect());
        }
    }

    // 피격 후 무적 처리 및 깜빡임 효과 코루틴
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // 1. 피격 순간 빨간색으로 변경
        ApplyColor(Color.red);
        yield return new WaitForSeconds(0.1f);

        // 2. 무적 시간 동안 깜빡거리는 효과
        float timer = 0;
        while (timer < invincibilityDuration)
        {
            // 투명하게 (알파값 0.5)
            ApplyColor(new Color(1, 1, 1, 0.4f));
            yield return new WaitForSeconds(0.1f);

            // 다시 원래대로 (불투명)
            ApplyColor(originalColor);
            yield return new WaitForSeconds(0.1f);

            timer += 0.2f;
        }

        // 3. 무적 종료 및 색상 복구
        ApplyColor(originalColor);
        isInvincible = false;
        _hitEffectCoroutine = null;
    }

    private void ApplyColor(Color targetColor)
    {
        if (sr == null) return;
        sr.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorId, targetColor);
        sr.SetPropertyBlock(_propBlock);
    }

    public void SetRollingInvincible(bool status)
    {
        isRollingInvincible = status;
        if (sr == null) return;

        if (isRollingInvincible)
        {
            // 구르기 시작 시 피격 코루틴 멈춤
            if (_hitEffectCoroutine != null) StopCoroutine(_hitEffectCoroutine);
            ApplyColor(Color.cyan); // 구르기는 하늘색
        }
        else
        {
            ApplyColor(originalColor);
        }
    }



    private void ApplyKnockback(Vector3 enemyPosition)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log("넉백 로직 실행됨!"); // 1. 이 로그가 콘솔에 찍히는지 확인
            Vector2 knockbackDir = (transform.position - enemyPosition).normalized;
            rb.linearVelocity = Vector2.zero;

            // 힘을 아주 크게(예: 20) 줘서 반응이 오는지 확인해 보세요.
            rb.AddForce(knockbackDir * 20f, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("Rigidbody2D를 찾을 수 없습니다!"); // 2. 이게 찍히면 참조 오류
        }
    }

    private void PlayerDie()
    {
        if (this.enabled == false) return; // 중복 실행 방지

        Debug.Log("<color=black><b>[사망]</b></color> 플레이어가 쓰러졌습니다.");

        // 1. 조작 및 물리 기능 차단
        this.enabled = false;
        CharacterController2D controller = GetComponent<CharacterController2D>();
        if (controller != null) controller.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero; // 미끄러짐 방지

        // 2. 사망 애니메이션 실행
        if (_animation != null) _animation.Die();

        // 3. 애니메이션이 재생될 시간을 기다린 후 UI를 띄우는 코루틴 시작
        StartCoroutine(GameOverSequence());
    }
    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockback = true;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // [추가] 넉백 시작 시 애니메이터의 모든 이동/점프 상태를 강제로 초기화합니다.
        if (_animation != null)
        {
            // SetState를 직접 호출하기 어렵다면 Animator를 통해 강제로 끕니다.
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetBool("Jump", false);
                anim.SetBool("Fall", false);
                anim.SetBool("Run", false);
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", false);
                anim.SetTrigger("Hit"); // 피격 모션 실행
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = direction * knockbackForce;
        }

        // 넉백 지속 시간을 0.4f 정도로 약간 늘리는 것을 권장합니다.
        yield return new WaitForSeconds(knockbackDuration);

        isKnockback = false;
    }

    // 3. OnCollisionEnter2D 수정 (기존 호출 방식 유지용)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.layer == 8)
        {
            if (!isInvincible && !isRollingInvincible)
            {
                TakeDamage(contactDamage, collision.transform.position); // 위치 전달
            }
        }
    }
    private void CheckLowHealth()
    {
        if (bloodOverlay == null || currentHealth <= 0) return;

        float healthPercent = (float)currentHealth / maxHealth;

        // 체력이 임계값 이하인 경우
        if (healthPercent <= warningThreshold)
        {
            isWarning = true;
            // 시간에 따라 알파값을 0 ~ 0.4 사이로 왕복시킵니다.
            float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f * 0.4f;
            ApplyOverlayAlpha(alpha);
        }
        else
        {
            // 체력이 회복되면 효과 끔
            if (isWarning)
            {
                isWarning = false;
                ApplyOverlayAlpha(0f);
            }
        }
    }
    private void ApplyOverlayAlpha(float alpha)
    {
        Color color = bloodOverlay.color;
        color.a = alpha;
        bloodOverlay.color = color;
    }
    private IEnumerator GameOverSequence()
    {
        // 애니메이션 길이에 맞춰 대기 (보통 1초~1.5초 정도가 적당합니다)
        // 이 시간 동안은 Time.timeScale이 1이므로 애니메이션이 정상 재생됩니다.
        yield return new WaitForSeconds(1.2f);

        // 4. 게임 오버 UI 활성화
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 5. 최종적으로 시간 정지
        Time.timeScale = 0f;
        if (PotionInventory.Instance != null)
        {
            PotionInventory.Instance.ResetInventory();
        }

        // GameManager를 통한 게임 종료 처리
        GameManager.instance.EndGame();
        yield return null;
    }
    public void UsePotion()
    {
        // 1. 인벤토리에 물약이 있는지 확인하고 차감
        if (PotionInventory.Instance != null && PotionInventory.Instance.TryUsePotion())
        {
            // 2. 체력 회복 (최대 체력을 넘지 않도록 설정)
            int restoreAmount = 30;
            currentHealth += restoreAmount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            // 3. UI 업데이트
            if (hpSlider != null)
                hpSlider.value = currentHealth;

            Debug.Log("물약 사용! 체력 30 회복. 현재 체력: " + currentHealth);

            // (선택 사항) 회복 시 시각 효과: 잠시 초록색으로 반짝이게 하고 싶다면?
            StartCoroutine(HealColorEffect());
        }
        else
        {
            Debug.Log("물약이 부족합니다!");
        }
    }
    private IEnumerator HealColorEffect()
    {
        if (sr != null)
        {
            // 1. 초록색 적용 (MaterialPropertyBlock 사용)
            _propBlock.SetColor(ColorId, Color.green);
            sr.SetPropertyBlock(_propBlock);

            // 2. 잠시 대기 (0.2초)
            yield return new WaitForSeconds(0.2f);

            // 3. 원래 색상(originalColor)으로 복구
            _propBlock.SetColor(ColorId, originalColor);
            sr.SetPropertyBlock(_propBlock);
        }
    }
    private IEnumerator HitColorEffect()
    {
        if (sr != null)
        {
            // 빨간색 적용
            _propBlock.SetColor(ColorId, Color.red);
            sr.SetPropertyBlock(_propBlock);

            yield return new WaitForSeconds(0.1f);

            // 원래 색상으로 복구
            _propBlock.SetColor(ColorId, originalColor);
            sr.SetPropertyBlock(_propBlock);
        }
    }
}