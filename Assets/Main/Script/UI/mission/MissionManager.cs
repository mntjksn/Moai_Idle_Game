using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    public GameObject missionPrefab;
    public Transform contentParent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (contentParent == null || missionPrefab == null)
        {
            Debug.LogError("[MissionManager] Prefab 또는 Parent가 비어있습니다.");
            return;
        }

        // 미션 7개 생성
        for (int i = 0; i < 7; i++)
        {
            var go = Instantiate(missionPrefab, contentParent);

            var item = go.GetComponent<Mission>();
            if (item != null)
                item.Setup(i);    // index 전달
        }
    }
}