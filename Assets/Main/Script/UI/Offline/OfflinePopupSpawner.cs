using UnityEngine;

// 오프라인 보상이 존재할 경우
// 시작 시 팝업을 생성해주는 스폰 전용 스크립트
public class OfflinePopupSpawner : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;   // 오프라인 보상 효과음

    public GameObject offlinePopupPrefab;   // 오프라인 보상 팝업 프리팹
    public Transform parent;                // 팝업이 붙을 부모(Canvas 등)

    private GameObject spawned;              // 생성된 팝업 참조

    private void Start()
    {
        // 오프라인 보상 시스템 접근
        var sys = OfflineRewardSystem.Instance;

        // 필수 참조 방어
        if (sys == null || offlinePopupPrefab == null)
            return;

        // 오프라인 보상 계산
        sys.ComputePending();

        // 받을 보상이 없으면 종료
        if (!sys.hasPendingReward)
            return;

        // 팝업 생성
        spawned = Instantiate(offlinePopupPrefab, parent);
        spawned.SetActive(true);

        // 효과음 재생
        PlaySFX();
    }

    // SFX 설정이 켜져 있을 때만 효과음 재생
    private void PlaySFX()
    {
        if (audioSource == null)
            return;

        if (Setting.IsSFXOn())
            audioSource.Play();
    }
}