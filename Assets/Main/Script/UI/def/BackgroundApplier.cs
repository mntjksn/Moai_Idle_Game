using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundApplier : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    private BackgroundManager manager;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        // 배경은 화면에 꽉 차야 하므로 preserveAspect는 false
        if (targetImage != null)
            targetImage.preserveAspect = false;
    }

    private void OnEnable()
    {
        StartCoroutine(SetupRoutine());
    }

    IEnumerator SetupRoutine()
    {
        // BackgroundManager 초기화 대기
        yield return new WaitUntil(() => BackgroundManager.Instance != null);

        manager = BackgroundManager.Instance;

        // 안전 구독
        manager.OnBackgroundSelected -= Apply;
        manager.OnBackgroundSelected += Apply;

        // 현재 선택된 배경 즉시 적용
        Apply(manager.SelectedIndex);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.OnBackgroundSelected -= Apply;
    }

    /// <summary>
    /// 실제 배경 스프라이트를 적용하는 함수
    /// </summary>
    public void Apply(int index)
    {
        if (targetImage == null || manager == null) return;

        var item = manager.GetItem(index);
        if (item == null || item.itemimg == null)
        {
            Debug.LogWarning($"[BackgroundApplier] index {index} 의 배경 스프라이트가 null");
            return;
        }

        // 스프라이트 변경
        if (targetImage.sprite != item.itemimg)
        {
            targetImage.sprite = item.itemimg;

            // 강제 리프레시 필요 없음 (Canvas가 자동 갱신)
            // targetImage.enabled = false; targetImage.enabled = true;
        }

        // 디버그 로그 최소화
        // Debug.Log($"[BackgroundApplier] Applied {item.name} to {targetImage.name}");
    }

    // 디버그용
    public void Debug_ForceSet(int index)
    {
        var item = BackgroundManager.Instance?.GetItem(index);
        if (item != null && item.itemimg != null)
            targetImage.sprite = item.itemimg;
    }
}