using UnityEngine;

// 미션 UI들을 생성하고 초기화하는 매니저
public class MissionManager : MonoBehaviour
{
    // 전역 접근용 싱글톤
    public static MissionManager Instance;

    [Header("Prefabs & Parents")]
    public GameObject missionPrefab;   // 미션 UI 프리팹
    public Transform contentParent;     // ScrollView Content

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // 필수 참조 방어
        if (contentParent == null || missionPrefab == null)
        {
            Debug.LogError("[MissionManager] Prefab 또는 Parent가 비어있습니다.");
            return;
        }

        // 미션 7개 생성
        for (int i = 0; i < 7; i++)
        {
            GameObject go = Instantiate(missionPrefab, contentParent);

            // Mission 컴포넌트에 인덱스 전달
            Mission item = go.GetComponent<Mission>();
            if (item != null)
                item.Setup(i);
        }
    }
}