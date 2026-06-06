using UnityEngine;
using TMPro; // TextMeshPro 사용 시 필수
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Dev Settings")]
    [SerializeField] private int totalGold = 0; // 인스펙터에서 실시간 확인 및 수정 가능
    private const string GoldKey = "SavedTotalGold";

    [Header("UI Reference")]
    public TextMeshProUGUI goldText; // 연결할 UI 텍스트



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGold();
        }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        UpdateUI();
    }

    // [개발용] 에디터에서 값을 수정하면 즉시 UI와 저장소에 반영
    private void OnValidate()
    {
        if (Application.isPlaying && Instance != null)
        {
            UpdateUI();
            SaveGold();
        }
    }

    public void AddGold(int amount)
    {
        totalGold += amount;
        SaveGold();
        UpdateUI();

      
    }

    private void SaveGold()
    {
        PlayerPrefs.SetInt(GoldKey, totalGold);
        PlayerPrefs.Save();
    }

    private void LoadGold()
    {
        totalGold = PlayerPrefs.GetInt(GoldKey, 0);
    }

    public void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = string.Format("{0:#,###}", totalGold); // 세 자리마다 콤마 표시
        }
    }

    public int GetTotGold() => totalGold;
    // 1. 현재 골드를 가져오는 함수 추가
    public int GetTotalGold()
    {
        return totalGold;
    }

    // 2. 골드 차감이 가능한지 확인하고 차감하는 함수 (권장)
    public bool TrySpendGold(int amount)
    {
        if (totalGold >= amount)
        {
            totalGold -= amount;
            SaveGold();
            UpdateUI();
            return true;
        }
        return false; // 골드 부족
    }
}