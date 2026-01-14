using UnityEngine;

public class OfflinePopupSpawner : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public GameObject offlinePopupPrefab; // Project의 Offline_Reward 프리팹
    public Transform parent;              // Canvas(Environment) 같은 부모

    private GameObject spawned;

    private void Start()
    {
        var sys = OfflineRewardSystem.Instance;
        if (sys == null || offlinePopupPrefab == null) return;

        sys.ComputePending();
        if (!sys.hasPendingReward) return;

        spawned = Instantiate(offlinePopupPrefab, parent);
        spawned.SetActive(true);

        PlaySFX();
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();
    }
}