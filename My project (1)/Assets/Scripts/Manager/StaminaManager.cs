using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float regenRate = 15f;      // 초당 회복량
    public float rollCost = 30f;       // 구르기 1회당 소모량

    [Header("UI Reference")]
    public Slider staminaSlider;       // 연결할 UI 슬라이더

    void Awake()
    {
        if (Instance == null) Instance = this;
        currentStamina = maxStamina;
    }

    void Start()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    void Update()
    {
        // 실시간 스테미나 회복
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            UpdateUI();
        }
    }

    // 구르기 가능 여부 확인 및 소모
    public bool CanRoll()
    {
        return currentStamina >= rollCost;
    }

    public void UseStaminaForRoll()
    {
        currentStamina -= rollCost;
        if (currentStamina < 0) currentStamina = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }
}