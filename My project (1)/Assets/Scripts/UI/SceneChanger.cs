using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수

public class SceneChanger : MonoBehaviour
{
    // 버튼 클릭 시 호출될 함수
    public void GoToTown()
    {
        // "Town"은 업로드하신 Town.unity 씬 이름과 일치해야 합니다.
        SceneManager.LoadScene("Town");
    }
}
