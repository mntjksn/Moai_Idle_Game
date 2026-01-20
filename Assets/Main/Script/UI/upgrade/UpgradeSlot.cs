using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 업그레이드 슬롯 1칸 UI 컨트롤러
// - index(캐릭터 레벨)에 해당하는 업그레이드 구매 UI를 표시
// - Update 없이 Refresh() 호출로만 갱신(권장)
public class UpgradeSlot : MonoBehaviour
{
    [Header("Data")]
    public int index; // 캐릭터 레벨 인덱스

    [Header("Panels")]
    public GameObject mainPanel; // 기본 패널(구매 UI)
    public GameObject endPanel;  // 구매 완료 표시 패널

    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI buyText;
    public Image icon;
    public Button upgradeBtn;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;

    // 현재 슬롯이 바라보는 캐릭터 데이터(필요 시 Refresh에서 다시 가져옴)
    private CharacterItem item;

    private void Awake()
    {
        // 버튼 리스너는 1회만
        if (upgradeBtn != null)
        {
            upgradeBtn.onClick.RemoveListener(OnUpgrade);
            upgradeBtn.onClick.AddListener(OnUpgrade);
        }
    }

    private void OnEnable()
    {
        // 슬롯이 켜질 때마다 최신 상태로 1회 갱신
        Refresh();
    }

    // 슬롯 UI/상태를 최신 데이터로 갱신
    // - Update 대신 외부(Manager)에서 필요할 때 호출하는 방식 권장
    public void Refresh()
    {
        // 최신 데이터 참조 확보
        item = CharacterManager.Instance?.GetItem(index);
        if (item == null)
        {
            SetVisible(false);
            return;
        }

        RefreshTexts(item);
        UpdateSlotState(item);
    }

    // -----------------------------
    // 텍스트/이미지 반영
    // -----------------------------
    private void RefreshTexts(CharacterItem it)
    {
        if (icon != null) icon.sprite = it.itemimg;

        if (nameText != null)
            nameText.text = $"{index}. {it.name} 업그레이드";

        if (goldText != null)
            goldText.text = $"{it.itemgold:N0} 개 ⇒ {(it.itemgold * 2):N0} 개";

        if (buyText != null)
            buyText.text = $"{GetUpgradeCost(index):N0} 다이아";
    }

    // -----------------------------
    // 상태(패널/버튼) 반영
    // -----------------------------
    private void UpdateSlotState(CharacterItem it)
    {
        bool spawned = it.spawncheck;
        if (!spawned)
        {
            // 스폰 전이면 숨김
            SetVisible(false);
            return;
        }

        // 첫 슬롯(1번)은 spawncheck만 만족하면 표시
        // 나머지는 "이전 캐릭터 업그레이드 완료"가 선행 조건
        bool prevUnlocked = (index == 1) || (CharacterManager.Instance?.GetItem(index - 1)?.upgrade ?? false);
        if (!prevUnlocked)
        {
            SetVisible(false);
            return;
        }

        // 여기부터는 표시 대상
        mainPanel?.SetActive(true);

        bool upgraded = it.upgrade;
        endPanel?.SetActive(upgraded);

        if (upgradeBtn != null)
            upgradeBtn.interactable = !upgraded;
    }

    private void SetVisible(bool visible)
    {
        if (mainPanel != null) mainPanel.SetActive(visible);
        if (endPanel != null) endPanel.SetActive(false); // 숨길 때는 완료 패널도 같이 꺼둠

        if (upgradeBtn != null)
            upgradeBtn.interactable = false;
    }

    // -----------------------------
    // 구매 처리
    // -----------------------------
    public void OnUpgrade()
    {
        // 최신 데이터 다시 확보(안전)
        item = CharacterManager.Instance?.GetItem(index);
        if (item == null) return;

        // 이미 업그레이드면 무시
        if (item.upgrade) return;

        GameData data = SaveManager.Load();
        int cost = GetUpgradeCost(index);

        if (data.currency.dia < cost)
        {
            AppearTextManager.Instance.Show("다이아가 부족합니다!");
            return;
        }

        PlaySFX();

        // 재화 차감 + 업그레이드 적용
        data.currency.dia -= cost;
        item.upgrade = true;

        // 현재 배치된 캐릭터들에 UC 반영
        ApplyUpgradeToSpawnedCharacters(index);

        // 카운트/미션 반영
        data.upgrades.upgrade++;
        data.missions.mission_8_value++;

        // 오프라인 캐시 즉시 최신화(추천)
        if (OfflineRewardSystem.Instance != null)
            data.offline.cachedGoldPerSec = OfflineRewardSystem.Instance.CalculateGoldPerTick();

        SaveManager.Save(data);

        // 내 슬롯 즉시 갱신 + 전체 슬롯도 갱신
        Refresh();
        UpgradeManager.Instance?.RefreshAllSlots();
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }

    // -----------------------------
    // 유틸
    // -----------------------------
    private int GetUpgradeCost(int idx)
    {
        // 기존 수식 그대로 함수화(중복 제거)
        return (idx * 15 + (int)Mathf.Pow(idx, 2)) + idx;
    }

    private void ApplyUpgradeToSpawnedCharacters(int levelIndex)
    {
        Transform chp = GameObject.FindGameObjectWithTag("chp")?.transform;
        if (chp == null) return;

        for (int i = 0; i < chp.childCount; i++)
        {
            var child = chp.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            var mi = child.GetComponent<MergeItem>();
            if (mi == null) continue;

            // 이 레벨의 캐릭터면 UC true 반영
            if (mi.iN == levelIndex)
                mi.UC = true;
        }
    }
}