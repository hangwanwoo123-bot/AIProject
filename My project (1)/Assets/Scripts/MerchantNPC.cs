using Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts;
using TMPro;
using UnityEngine;

public class MerchantNPC : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject shopUI; // 상점 창 (버튼이 포함된 패널)
    public TextMeshProUGUI dialogueText;

    [Header("Economy")]
    public int potionPrice = 50;

    [Header("Interaction")]
    public float detectRange = 2.5f;
    private Transform player;
    private bool isShopOpen = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (shopUI != null) shopUI.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Z키 입력 시 상점 열기/닫기
        if (dist <= detectRange && Input.GetKeyDown(KeyCode.Z))
        {
            ToggleShop();
        }

        // 멀어지면 상점 자동 닫기
        if (dist > detectRange && isShopOpen)
        {
            CloseShop();
        }
    }

    void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        shopUI.SetActive(isShopOpen);
        dialogueText.text = isShopOpen ? "내가 만든 물약이야!" : "";

        // [추가] 상점 상태에 따라 플레이어 조작 제어
        SetPlayerMovement(!isShopOpen);
    }

    public void CloseShop()
    {
        isShopOpen = false;
        if (shopUI != null) shopUI.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";

        // [추가] 상점을 닫을 때 플레이어 조작 복구
        SetPlayerMovement(true);
    }

    // 구매 버튼에 연결할 함수
    public void BuyPotion()
    {
        if (CurrencyManager.Instance == null) return;

        // TrySpendGold가 성공(true)하면 포션을 추가합니다.
        if (CurrencyManager.Instance.TrySpendGold(potionPrice))
        {
            if (PotionInventory.Instance != null)
            {
                PotionInventory.Instance.AddPotion(1);
                dialogueText.text = "탁월한 선택일세! (구매 완료)";
            }
        }
        else
        {
            // 골드가 부족할 경우
            dialogueText.text = "골드가 부족하구만... (필요: " + potionPrice + ")";
        }
    }
    private void SetPlayerMovement(bool enable)
    {
        if (player != null)
        {
            var controller = player.GetComponent<CharacterController2D>();
            var animator = player.GetComponent<Animator>();
            var animScript = player.GetComponent<Assets.PixelFantasy.PixelHeroes.Common.Scripts.ExampleScripts.CharacterAnimation>();

            if (controller != null)
            {
                controller.canMove = enable;
                // 입력값 초기화 (물리 이동 멈춤)
                controller.moveInput = Vector2.zero;
            }

            if (!enable) // 상점을 열 때 (조작 금지 상태)
            {
                if (animator != null)
                {
                    // [수정] 컨트롤러에 실제 존재하는 Bool 파라미터들을 제어합니다.
                    // 모든 이동 관련 상태를 false로 만들고 Idle만 true로 설정합니다.
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    animator.SetBool("Jump", false);
                    animator.SetBool("Fall", false);
                    animator.SetBool("Idle", true); // 대기 상태 강제 활성화
                }

                if (animScript != null)
                {
                    animScript.Idle();
                }
            }
        }
    }
}
