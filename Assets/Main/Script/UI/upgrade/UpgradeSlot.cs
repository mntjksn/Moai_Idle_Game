using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 업그레이드 슬롯 1칸 UI 컨트롤러
// - index(캐릭터 레벨)에 해당하는 업그레이드 구매 UI를 표시
// - ★중요: Instantiate 직후 OnEnable이 먼저 실행될 수 있어서,
//          index가 세팅되기 전(0) Refresh가 돌면 index-1=-1 문제가 생김
// - 해결: Setup() 완료(initialized=true) 이후에만 Refresh() 수행
public class UpgradeSlot : MonoBehaviour
{
    [Header("Data")]
    public int index; // 캐릭터 레벨 인덱스 (1부터 시작한다고 가정)

    // Instantiate 직후 OnEnable 방어용
    private bool initialized = false;

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

    // 현재 슬롯이 바라보는 캐릭터 데이터
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
        // ★ Instantiate 직후(Setup 전)에는 Refresh하지 않는다
        if (!initialized) return;

        // 슬롯이 켜질 때마다 최신 상태로 1회 갱신
        Refresh();
    }

    // UpgradeManager가 Instantiate 후 index를 세팅한 다음 꼭 호출해줘야 함.
    public void Setup(int idx)
    {
        index = idx;
        initialized = true;

        // 세팅 완료 즉시 1회 갱신
        Refresh();
    }

    // 슬롯 UI/상태를 최신 데이터로 갱신
    // - Update 대신 외부(Manager)에서 필요할 때 호출하는 방식 권장
    public void Refresh()
    {
        // ★ 방어: index가 1 미만이면 잘못된 슬롯이므로 숨김
        if (index < 1)
        {
            SetVisible(false);
            return;
        }

        // 최신 데이터 참조 확보
        item = CharacterManager.Instance?.GetItem(index);
        if (item == null)
        {
            SetVisible(false);
            return;
        }

        // 텍스트/이미지 반영
        RefreshTexts(item);

        // 패널/버튼 상태 반영
        UpdateSlotState(item);
    }

    //     UI: Text / Image
    private void RefreshTexts(CharacterItem it)
    {
        if (icon != null)
            icon.sprite = it.itemimg;

        if (nameText != null)
            nameText.text = $"{index}. {it.name} 업그레이드";

        if (goldText != null)
            goldText.text = $"{it.itemgold:N0} 개 ⇒ {(it.itemgold * 2):N0} 개";

        if (buyText != null)
            buyText.text = $"{GetUpgradeCost(index):N0} 다이아";
    }

    //   UI: Panel / Button State
    private void UpdateSlotState(CharacterItem it)
    {
        // 1) 스폰 전이면 숨김
        if (!it.spawncheck)
        {
            SetVisible(false);
            return;
        }

        // 2) 이전 캐릭터 업그레이드 조건
        // - index==1은 이전 캐릭터가 없으므로 무조건 통과
        // - index>1은 index-1이 유효한지 체크 후 upgrade 여부 확인
        bool prevUnlocked = true;

        if (index > 1)
        {
            var prev = CharacterManager.Instance?.GetItem(index - 1);

            // prev가 null이면(데이터 부족/인덱스 꼬임) 안전하게 숨김
            if (prev == null)
            {
                SetVisible(false);
                return;
            }

            prevUnlocked = prev.upgrade;
        }

        if (!prevUnlocked)
        {
            SetVisible(false);
            return;
        }

        // 3) 여기부터는 표시 대상
        if (mainPanel != null) mainPanel.SetActive(true);

        bool upgraded = it.upgrade;

        if (endPanel != null) endPanel.SetActive(upgraded);

        if (upgradeBtn != null)
            upgradeBtn.interactable = !upgraded;
    }

    private void SetVisible(bool visible)
    {
        // mainPanel이 켜져야 UI가 보임
        if (mainPanel != null) mainPanel.SetActive(visible);

        // 숨길 때는 완료 패널도 같이 꺼둠
        if (!visible && endPanel != null) endPanel.SetActive(false);

        // 숨길 때는 버튼도 비활성
        if (!visible && upgradeBtn != null) upgradeBtn.interactable = false;
    }

    //        Purchase
    public void OnUpgrade()
    {
        // ★ 안전: 구매 시점에도 다시 최신 참조
        item = CharacterManager.Instance?.GetItem(index);
        if (item == null) return;

        // 이미 업그레이드면 종료
        if (item.upgrade) return;

        GameData data = SaveManager.Load();
        int cost = GetUpgradeCost(index);

        // 재화 부족
        if (data.currency.dia < cost)
        {
            AppearTextManager.Instance.Show("다이아가 부족합니다!");
            return;
        }

        // SFX
        PlaySFX();

        // 재화 차감
        data.currency.dia -= cost;

        // 캐릭터 업그레이드 플래그 (데이터가 Scriptable/런타임 데이터라면 이 방식 OK)
        item.upgrade = true;

        // 현재 배치된 캐릭터들에 UC 반영
        ApplyUpgradeToSpawnedCharacters(index);

        // 카운트/미션 반영
        data.upgrades.upgrade++;
        data.missions.mission_8_value++;

        // 오프라인 캐시 즉시 최신화(추천)
        if (OfflineRewardSystem.Instance != null)
            data.offline.cachedGoldPerSec = OfflineRewardSystem.Instance.CalculateGoldPerTick();

        // 저장
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

    //          Util
    private int GetUpgradeCost(int idx)
    {
        // 기존 수식 그대로 함수화(중복 제거)
        return (idx * 15 + (int)Mathf.Pow(idx, 2)) + idx;
    }

    private void ApplyUpgradeToSpawnedCharacters(int levelIndex)
    {
        // 프로젝트에서 chp를 Tag로 쓰는 구조 그대로
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