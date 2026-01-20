using System.Collections;
using TMPro;
using UnityEngine;

// 화면에 잠깐 나타났다 사라지는 안내 텍스트를 관리하는 매니저
// - 텍스트 표시 후 페이드 아웃
// - 중복 호출 시 이전 코루틴 정리
public class AppearTextManager : MonoBehaviour
{
    public static AppearTextManager Instance;

    // 표시할 텍스트
    [SerializeField] private TextMeshProUGUI appearText;

    // 페이드 시간
    [SerializeField] private float fadeTime = 0.5f;

    // 현재 실행 중인 코루틴
    private Coroutine runCoroutine;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 인스펙터에서 할당 안 됐을 경우 자동 탐색
        if (appearText == null)
            appearText = GameObject.Find("appearText")?.GetComponent<TextMeshProUGUI>();

        // 필수 참조 방어
        if (appearText == null)
        {
            Debug.LogError("[AppearTextManager] appearText 할당 안됨");
            return;
        }

        // 초기 상태 리셋
        ResetText();
        appearText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 텍스트 상태 복구
        ResetText();

        // 실행 중인 코루틴 정지
        if (runCoroutine != null)
            StopCoroutine(runCoroutine);
    }

    // 텍스트 알파 값을 항상 1로 되돌림
    private void ResetText()
    {
        if (appearText == null)
            return;

        Color c = appearText.color;
        c.a = 1f;
        appearText.color = c;
    }

    // 외부에서 호출하는 텍스트 표시 함수
    public void Show(string msg)
    {
        if (appearText == null)
            return;

        // 이전 상태로 리셋(반투명 누적 방지)
        ResetText();
        appearText.text = msg;

        // 기존 코루틴 중지
        if (runCoroutine != null)
            StopCoroutine(runCoroutine);

        // 활성화 후 페이드 시작
        appearText.gameObject.SetActive(true);
        runCoroutine = StartCoroutine(FadeRoutine());
    }

    // 텍스트 페이드 연출 코루틴
    private IEnumerator FadeRoutine()
    {
        Color origin = appearText.color;
        Color fade = new Color(origin.r, origin.g, origin.b, 0f);

        // 2회 반복 연출
        for (int i = 0; i < 2; i++)
        {
            float t = 0f;

            // 페이드 아웃
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                appearText.color = Color.Lerp(origin, fade, t / fadeTime);
                yield return null;
            }

            // 잠깐 대기 후 원래 색으로 복구
            yield return new WaitForSeconds(0.1f);
            appearText.color = origin;
        }

        // 종료 시 완전 초기화
        ResetText();
        appearText.gameObject.SetActive(false);

        runCoroutine = null;
    }
}