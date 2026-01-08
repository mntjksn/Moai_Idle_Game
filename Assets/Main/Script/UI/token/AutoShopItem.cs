using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoShopItem : MonoBehaviour
{
    public enum AutoType { AutoSpawn, AutoMerge }
    public AutoType type;

    public int cost;   // 최초 구매 비용

    [SerializeField] private TextMeshProUGUI costText; // 가격/구매완료 전부 담당
    [SerializeField] private Button button;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        var data = SaveManager.Load();
        bool purchased = IsPurchased(data);

        if (purchased)
        {
            // 구매완료 상태
            costText.text = "구매 완료";
            button.interactable = false; // 구매 후 완전히 OFF
            return;
        }

        // 아직 구매 안함 → 가격 표시
        costText.text = $"{cost} 토큰";

        // 쿨타임 중이면 버튼 OFF
        float remain = GetRemain(data);
        button.interactable = (remain <= 0);
    }

    private bool IsPurchased(GameData data)
    {
        return (type == AutoType.AutoSpawn)
            ? data.settings.autoSpawnPurchased
            : data.settings.autoMergePurchased;
    }

    private float GetRemain(GameData data)
    {
        return (type == AutoType.AutoSpawn)
            ? data.settings.autoSpawnRemain
            : data.settings.autoMergeRemain;
    }

    public void OnClick()
    {
        var data = SaveManager.Load();

        bool purchased = IsPurchased(data);
        if (purchased) return;

        // 비용 체크
        if (data.currency.token < cost)
        {
            AppearTextManager.Instance.Show("토큰이 부족합니다!");
            return;
        }

        // 구매!
        data.currency.token -= cost;
        PlaySFX();

        if (type == AutoType.AutoSpawn)
        {
            data.settings.autoSpawnPurchased = true;
            data.settings.autoSpawnActive = true;
            data.settings.autoSpawnRemain = 300f;
        }
        else
        {
            data.settings.autoMergePurchased = true;
            data.settings.autoMergeActive = true;
            data.settings.autoMergeRemain = 300f;
        }

        SaveManager.Save(data);
        AutoTimeManager.Instance.ForceUpdateUI(); // UI 즉시 반영
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }
}