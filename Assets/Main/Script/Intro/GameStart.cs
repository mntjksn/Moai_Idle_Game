using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 시작 오브젝트를 드래그로 이동시키고, 트리거에 닿으면 페이드 후 씬 전환
public class GameStart : MonoBehaviour, IDragHandler
{
    // 페이드 패널(알파값 조절)
    public Image FadePanel;

    // 이동할 씬 이름
    public string nextSceneName = "Main";

    // 씬 전환 중복 실행 방지
    private bool isTransitioning = false;

    // Camera.main은 비용이 있으므로 캐싱
    private Camera mainCam;

    // 페이드 시간(초)
    private float fadeDuration = 0.5f;

    private void Start()
    {
        // 메인 카메라 캐싱
        mainCam = Camera.main;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 전환 중이면 무시
        if (isTransitioning) return;

        // 특정 태그 오브젝트에 닿았을 때만 실행
        if (!collision.CompareTag("Start")) return;

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        isTransitioning = true;

        // 페이드 패널이 있으면 알파를 0 -> 1로 증가
        if (FadePanel != null)
        {
            FadePanel.gameObject.SetActive(true);

            Color c = FadePanel.color;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                c.a = Mathf.Clamp01(t);
                FadePanel.color = c;
                yield return null;
            }
        }

        // 전환 연출용 짧은 대기
        yield return new WaitForSeconds(0.05f);

        // 씬 로드
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 씬 전환 중 드래그 차단
        if (isTransitioning) return;

        // 카메라가 없으면 다시 찾기
        if (mainCam == null) mainCam = Camera.main;

        // 스크린 좌표를 월드 좌표로 변환해 위치 이동
        Vector3 screenPos = eventData.position;

        // 2D에서도 ScreenToWorldPoint는 z값이 필요할 수 있어 임의 값 사용
        screenPos.z = 10f;

        transform.position = mainCam.ScreenToWorldPoint(screenPos);
    }
}