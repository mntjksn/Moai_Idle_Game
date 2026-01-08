using TMPro;
using UnityEngine;

public class Spawntext : MonoBehaviour
{
    private TextMeshProUGUI spawnCount;

    private void Awake()
    {
        spawnCount = GetComponent<TextMeshProUGUI>();

        // 이벤트 등록
        ShopButton.GameEvents.OnSettingsChanged -= Refresh;
        ShopButton.GameEvents.OnSettingsChanged += Refresh;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        // 이벤트 제거 (중복 방지)
        ShopButton.GameEvents.OnSettingsChanged -= Refresh;
    }

    public void Refresh()
    {
        GameData data = SaveManager.Load();

        int num = data.settings.clickNum;
        int max = data.settings.clickMax;

        spawnCount.text = $"({num} / {max})";
    }
}