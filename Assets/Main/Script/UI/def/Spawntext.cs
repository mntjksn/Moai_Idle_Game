using TMPro;
using UnityEngine;

// 소환 가능 횟수 UI 텍스트를 표시하는 스크립트
// (예: (1 / 3))
// 설정 변경 이벤트를 받아 자동으로 갱신됨
public class Spawntext : MonoBehaviour
{
    // 소환 횟수 표시용 텍스트
    private TextMeshProUGUI spawnCount;

    private void Awake()
    {
        // TextMeshProUGUI 컴포넌트 캐싱
        spawnCount = GetComponent<TextMeshProUGUI>();

        // 이벤트 중복 등록 방지를 위해 먼저 제거 후 다시 등록
        ShopButton.GameEvents.OnSettingsChanged -= Refresh;
        ShopButton.GameEvents.OnSettingsChanged += Refresh;
    }

    private void OnEnable()
    {
        // 패널 활성화 시 즉시 갱신
        Refresh();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 해제 (메모리 누수 / 중복 호출 방지)
        ShopButton.GameEvents.OnSettingsChanged -= Refresh;
    }

    // 소환 횟수 텍스트 갱신
    public void Refresh()
    {
        GameData data = SaveManager.Load();

        int num = data.settings.clickNum; // 현재 소환 가능 횟수
        int max = data.settings.clickMax; // 최대 소환 가능 횟수

        spawnCount.text = $"({num} / {max})";
    }
}