using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // [중요] 어디서든 GameManager.instance로 접근할 수 있게 해주는 변수
    public static GameManager instance;

    public GameObject gameOverUI; // 유니티 에디터에서 GameOverPanel을 연결하세요.

    void Awake()
    {
        // 싱글톤 초기화
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EndGame()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 애니메이션이 이미 다 끝난 후이므로 여기서 시간을 멈춰도 안전합니다
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        // 1. 멈췄던 시간을 다시 흐르게 합니다.
        Time.timeScale = 1f;

        // 2. 패널을 즉시 비활성화합니다. (씬 로드 전 시각적 피드백)
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // 3. 현재 씬을 다시 로드합니다.
        // 씬이 새로 로드되면 모든 UI 상태가 초기화되지만, 
        // 간혹 싱글톤이나 DontDestroyOnLoad 오브젝트 때문에 남아있을 수 있어 명시적으로 끄는 것이 좋습니다.
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    public void RestartGame()
    {
        Time.timeScale = 1f; // 반드시 시간을 1로 돌려놓아야 합니다!
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}