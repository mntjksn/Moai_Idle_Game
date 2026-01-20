using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 선택된 배경을 실제 UI Image에 적용하는 역할
// - BackgroundManager의 선택 변경 이벤트를 구독
// - 배경 스프라이트를 즉시 반영
public class BackgroundApplier : MonoBehaviour
{
    // 배경을 표시할 Image 컴포넌트
    [SerializeField] private Image targetImage;

    // 배경 매니저 참조
    private BackgroundManager manager;

    private void Awake()
    {
        // 인스펙터에서 할당 안 됐으면 자신에게서 찾기
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        // 배경 이미지는 화면을 꽉 채워야 하므로 비율 유지 끔
        if (targetImage != null)
            targetImage.preserveAspect = false;
    }

    private void OnEnable()
    {
        // BackgroundManager가 준비될 때까지 대기 후 세팅
        StartCoroutine(SetupRoutine());
    }

    private IEnumerator SetupRoutine()
    {
        // BackgroundManager 싱글톤 생성 대기
        yield return new WaitUntil(() => BackgroundManager.Instance != null);

        manager = BackgroundManager.Instance;

        // 이벤트 중복 구독 방지
        manager.OnBackgroundSelected -= Apply;
        manager.OnBackgroundSelected += Apply;

        // 현재 선택된 배경을 즉시 적용
        Apply(manager.SelectedIndex);
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (manager != null)
            manager.OnBackgroundSelected -= Apply;
    }

    // 실제 배경 스프라이트를 적용하는 함수
    public void Apply(int index)
    {
        if (targetImage == null || manager == null)
            return;

        // 배경 데이터 가져오기
        var item = manager.GetItem(index);
        if (item == null || item.itemimg == null)
        {
            Debug.LogWarning($"[BackgroundApplier] index {index} 배경 스프라이트 없음");
            return;
        }

        // 스프라이트가 바뀐 경우에만 적용
        if (targetImage.sprite != item.itemimg)
        {
            targetImage.sprite = item.itemimg;

            // Canvas는 자동 갱신되므로 강제 리프레시는 필요 없음
        }
    }

    // 디버그용: 강제로 특정 배경 적용
    public void Debug_ForceSet(int index)
    {
        var item = BackgroundManager.Instance?.GetItem(index);
        if (item != null && item.itemimg != null)
            targetImage.sprite = item.itemimg;
    }
}