using UnityEngine;
using TMPro;

public class PotionInventory : MonoBehaviour
{
    public static PotionInventory Instance;

    public int potionCount = 0;
    public TextMeshProUGUI potionText; // UI 텍스트 연결

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddPotion(int amount)
    {
        potionCount += amount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (potionText != null)
            potionText.text = "x " + potionCount;
    }

    // 플레이어 사망 시 호출 (GameManager나 PlayerHealth에서 호출)
    public void ResetInventory()
    {
        potionCount = 0;
        UpdateUI();
    }
    public bool TryUsePotion()
    {
        if (potionCount > 0)
        {
            potionCount--;
            UpdateUI();
            return true;
        }
        return false;
    }
}