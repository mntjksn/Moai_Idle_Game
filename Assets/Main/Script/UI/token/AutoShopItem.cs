using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 자동 기능(오토 소환 / 오토 합치기) 구매 아이템
// - 토큰으로 최초 1회 구매
// - 구매 후 버튼 비활성화
// - Update 사용 X (이벤트/직접 호출 방식)
public class AutoShopItem : MonoBehaviour
{
    public enum AutoType { AutoSpawn, AutoMerge }
    public AutoType type;

    [Header("Cost")]
    public int cost;   // 최초 구매 비용

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button button;
    [SerializeField] private AudioSource audioSource;

    private void OnEnable()
    {
        // 패널 열릴 때만 UI 갱신
        RefreshUI();
    }

    // UI 갱신 (필요한 타이밍에만 호출)
    public void RefreshUI()
    {
        GameData data = SaveManager.Load();
        bool purchased = IsPurchased(data);

        if (purchased)
        {
            costText.text = "구매 완료";
            button.interactable = false;
            return;
        }

        costText.text = $"{cost} 토큰";
        button.interactable = true;
    }

    // 구매 여부 확인
    private bool IsPurchased(GameData data)
    {
        return (type == AutoType.AutoSpawn)
            ? data.settings.autoSpawnPurchased
            : data.settings.autoMergePurchased;
    }

    // 구매 버튼 클릭
    public void OnClick()
    {
        GameData data = SaveManager.Load();

        if (IsPurchased(data))
            return;

        if (data.currency.token < cost)
        {
            AppearTextManager.Instance.Show("토큰이 부족합니다!");
            return;
        }

        // 구매 처리
        data.currency.token -= cost;
        PlaySFX();

        if (type == AutoType.AutoSpawn)
        {
            data.settings.autoSpawnPurchased = true;
            data.settings.autoSpawnEnabled = true;
            data.settings.autoSpawnActive = true;
            data.settings.autoSpawnRemain = 300f; // 5분
        }
        else
        {
            data.settings.autoMergePurchased = true;
            data.settings.autoMergeEnabled = true;
            data.settings.autoMergeActive = true;
            data.settings.autoMergeRemain = 300f; // 5분
        }

        SaveManager.Save(data);

        // UI 즉시 반영
        RefreshUI();

        // 자동 타이머 UI도 즉시 반영
        if (AutoTimeManager.Instance != null)
            AutoTimeManager.Instance.ForceUpdateUI();
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }
}