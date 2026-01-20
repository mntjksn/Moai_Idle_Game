using System.Collections;
using UnityEngine;

// 자동 소환 시스템
// - AutoTimeManager에서 타이밍 제어
// - 실제 소환 동작만 담당
// - AutoMerge와 충돌 방지용 Lock 사용
public class AutoSpawner : MonoBehaviour
{
    private ClickLimit clickLimit;   // 실제 소환 버튼을 가진 시스템
    private GameData data;

    private Coroutine spawnRoutine;
    public static AutoSpawner Instance;

    private void Awake()
    {
        // 싱글톤 참조
        Instance = this;

        // 소환 제한 관리 클래스 캐싱
        clickLimit = FindObjectOfType<ClickLimit>();
    }

    private void OnEnable()
    {
        // 자동 소환 루프 시작
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(AutoSpawnLoop());
    }

    private void OnDisable()
    {
        // 루프 정지 (중복 실행 방지)
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    // AutoTimeManager에서 호출하는 "자동 소환 1회 실행"
    // (타이머 종료 시점에만 호출됨)
    public void RunAutoSpawn()
    {
        data = SaveManager.Load();

        // 자동 소환이 꺼져있거나 비활성 상태면 실행 안 함
        if (!data.settings.autoSpawnEnabled) return;
        if (!data.settings.autoSpawnActive) return;

        // 실제 소환 가능 조건 확인
        if (clickLimit != null && clickLimit.IsSpawnable())
        {
            // 수동 클릭과 동일한 흐름을 타기 위해 버튼 이벤트 직접 호출
            clickLimit.btn.onClick.Invoke();
        }
    }

    // 자동 소환 상시 감시 루프
    // - Update 대신 Coroutine 사용
    // - 조건 만족 시에만 소환 시도
    private IEnumerator AutoSpawnLoop()
    {
        while (true)
        {
            // 프레임 단위 대기
            yield return null;

            data = SaveManager.Load();

            // 자동 소환 미구매 / 비활성 / 작업 중이 아니면 스킵
            if (!data.settings.autoSpawnPurchased) continue;
            if (!data.settings.autoSpawnEnabled) continue;
            if (!data.settings.autoSpawnActive) continue;

            // 자동 합치기 동작 중이면 충돌 방지
            if (AutoSystemLock.isAutoMerging) continue;

            // 소환 조건 미충족 시 스킵
            if (!CanSpawn()) continue;

            // ===== 소환 실행 =====

            // AutoMerge와 충돌 방지용 락
            AutoSystemLock.isAutoSpawning = true;

            // 실제 버튼 클릭 실행
            clickLimit.btn.onClick.Invoke();

            AutoSystemLock.isAutoSpawning = false;

            // 너무 빠른 연속 실행 방지
            yield return new WaitForSeconds(0.5f);
        }
    }

    // 소환 가능 여부 체크
    // - ClickLimit 내부 로직 그대로 사용
    private bool CanSpawn()
    {
        if (clickLimit == null) return false;
        return clickLimit.IsSpawnable();
    }
}