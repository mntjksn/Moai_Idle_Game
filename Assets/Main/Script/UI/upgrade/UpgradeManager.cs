using System.Collections.Generic;
using UnityEngine;

// 업그레이드 슬롯 리스트 생성/갱신 매니저
// - 시작 시 캐릭터 수 기준으로 슬롯 생성
// - 필요 시 전체 슬롯 UI 갱신(UpdateAllSlots)
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI")]
    public GameObject slotPrefab;   // 슬롯 프리팹(UpgradeSlot 컴포넌트 포함)
    public Transform content;       // 슬롯이 붙을 부모(Content)

    [Header("Runtime Cache")]
    public List<UpgradeSlot> slots = new List<UpgradeSlot>(); // 생성된 슬롯 캐싱

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

        // CharacterManager 준비 안 됐으면 안전 종료
        if (CharacterManager.Instance == null || CharacterManager.Instance.characters == null)
        {
            Debug.LogError("[UpgradeManager] CharacterManager가 아직 준비되지 않았습니다.");
            return;
        }

        BuildSlots();
        RefreshAllSlots();
    }

    // 슬롯을 생성한다.
    // - 인덱스 1부터 생성(0은 기본 캐릭터라 제외한 듯한 설계)
    private void BuildSlots()
    {
        // 혹시 재호출될 수 있는 상황 대비(중복 생성 방지)
        slots.Clear();

        int max = CharacterManager.Instance.characters.Count;

        for (int i = 1; i < max; i++)
        {
            GameObject obj = Instantiate(slotPrefab, content);

            if (!obj.TryGetComponent(out UpgradeSlot slot))
            {
                Debug.LogError("[UpgradeManager] slotPrefab에 UpgradeSlot 컴포넌트가 없습니다!");
                Destroy(obj);
                continue;
            }

            slot.index = i;
            slots.Add(slot);
        }
    }

    // 모든 슬롯 UI를 갱신한다.
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