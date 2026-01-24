using System.Collections.Generic;
using UnityEngine;

// 업그레이드 슬롯 리스트 생성 / 전체 갱신 매니저
// - 게임 시작 시 캐릭터 수 기준으로 슬롯 생성
// - Update 사용 X
// - 필요할 때 RefreshAllSlots() 호출로 UI 갱신
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI")]
    public GameObject slotPrefab;   // UpgradeSlot 컴포넌트가 붙은 프리팹
    public Transform content;       // ScrollView Content 등 부모 Transform

    [Header("Runtime Cache")]
    public List<UpgradeSlot> slots = new List<UpgradeSlot>();

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 필수 참조 체크
        if (slotPrefab == null || content == null)
        {
            Debug.LogError("[UpgradeManager] slotPrefab 또는 content가 비어있습니다.");
            return;
        }

        // CharacterManager 준비 안 됐으면 슬롯 생성 불가
        if (CharacterManager.Instance == null ||
            CharacterManager.Instance.characters == null ||
            CharacterManager.Instance.characters.Count <= 1)
        {
            Debug.LogError("[UpgradeManager] CharacterManager가 아직 준비되지 않았거나 캐릭터 수가 부족합니다.");
            return;
        }

        BuildSlots();
        RefreshAllSlots();
    }

    //        Build Slots
    // 업그레이드 슬롯 생성
    // - index는 1부터 시작 (0번 캐릭터는 기본 캐릭터라 제외한 구조)
    private void BuildSlots()
    {
        // 혹시 재호출될 수 있는 상황 대비
        // (씬 재진입, 패널 재사용 등)
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
                Destroy(slots[i].gameObject);
        }

        slots.Clear();

        int max = CharacterManager.Instance.characters.Count;

        for (int i = 1; i < max; i++)
        {
            GameObject obj = Instantiate(slotPrefab, content);

            // UpgradeSlot 필수 체크
            if (!obj.TryGetComponent(out UpgradeSlot slot))
            {
                Debug.LogError("[UpgradeManager] slotPrefab에 UpgradeSlot 컴포넌트가 없습니다!");
                Destroy(obj);
                continue;
            }

            // ★ 핵심 포인트 ★
            // index 직접 세팅 금지
            // 반드시 Setup()을 통해 초기화해야
            // OnEnable / Refresh 타이밍 문제(index=0 → -1 접근) 방지됨
            slot.Setup(i);

            slots.Add(slot);
        }
    }

    //      Refresh All
    // 모든 슬롯 UI를 강제로 갱신
    // - 업그레이드 구매 후
    // - 캐릭터 해금 후
    // - 패널 다시 열 때 사용
    public void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot != null)
                slot.Refresh();
        }
    }
}