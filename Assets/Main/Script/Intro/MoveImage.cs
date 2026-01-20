using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// UI 이미지가 일정 거리 이동하면서 페이드아웃 -> 대기 -> 원위치에서 페이드인 반복
public class MoveImage : MonoBehaviour
{
    // 이동 거리(가로 방향)
    public float moveDistance = 100f;

    // 이동 및 페이드 시간(초)
    public float duration = 1f;

    // 각 단계 사이 대기 시간(초)
    public float waitTime = 0.5f;

    private RectTransform rectTransform;
    private Image image;

    // 시작 위치와 이동 후 위치
    private Vector2 originalPos;
    private Vector2 endPos;

    // 시작 색상과 끝 색상(알파 0)
    private Color startColor;
    private Color endColor;

    // 반복 실행 여부
    private bool isRunning = true;

    private void Start()
    {
        // 컴포넌트 캐싱
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        // 기준 위치 저장
        originalPos = rectTransform.anchoredPosition;

        // 이동 목표 위치 계산(오른쪽으로 이동)
        endPos = originalPos + new Vector2(moveDistance, 0);

        // 색상 값 저장(끝 색상은 알파만 0으로)
        startColor = image.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        // 반복 코루틴 시작
        StartCoroutine(FadeMoveLoop());
    }

    private IEnumerator FadeMoveLoop()
    {
        float t;

        while (isRunning)
        {
            // 이동 + 페이드 아웃
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float lerp = Mathf.Clamp01(t);

                rectTransform.anchoredPosition = Vector2.Lerp(originalPos, endPos, lerp);
                image.color = Color.Lerp(startColor, endColor, lerp);

                yield return null;
            }

            // 대기
            yield return new WaitForSeconds(waitTime);

            // 위치 리셋 후 페이드 인
            rectTransform.anchoredPosition = originalPos;

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float lerp = Mathf.Clamp01(t);

                image.color = Color.Lerp(endColor, startColor, lerp);

                yield return null;
            }

            // 대기
            yield return new WaitForSeconds(waitTime);
        }
    }

    // 외부에서 반복 효과 중지
    public void StopEffect()
    {
        isRunning = false;
    }
}