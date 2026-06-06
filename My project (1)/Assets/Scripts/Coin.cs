using UnityEngine;

public class Coin : MonoBehaviour
{
    public int scoreValue = 10; // 코인 하나당 금액

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 돈 추가 로직 호출
            CurrencyManager.Instance.AddGold(scoreValue);
            Destroy(gameObject); // 코인 제거
        }
    }
}
