using System.Collections;
using UnityEngine;

public class AutoSpawner : MonoBehaviour
{
    private ClickLimit clickLimit;
    private GameData data;

    private Coroutine spawnRoutine;
    public static AutoSpawner Instance;

    private void Awake()
    {
        Instance = this;
        clickLimit = FindObjectOfType<ClickLimit>();
    }

    private void OnEnable()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(AutoSpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void RunAutoSpawn()
    {
        data = SaveManager.Load();
        if (!data.settings.autoSpawnEnabled) return;
        if (!data.settings.autoSpawnActive) return;

        // 실제 자동 소환 1회 수행
        if (clickLimit != null && clickLimit.IsSpawnable())
        {
            clickLimit.btn.onClick.Invoke();
        }
    }

    private IEnumerator AutoSpawnLoop()
    {
        while (true)
        {
            yield return null;

            data = SaveManager.Load();

            // 자동 소환 미구매 or 비활성화 상태
            if (!data.settings.autoSpawnPurchased) continue;
            if (!data.settings.autoSpawnEnabled) continue;
            if (!data.settings.autoSpawnActive) continue;

            // 자동 합치기 동작 중이면 대기
            if (AutoSystemLock.isAutoMerging) continue;

            // 소환 가능한 조건 확인
            if (!CanSpawn()) continue;

            // 락 설정 (AutoMerge와 충돌 방지)
            AutoSystemLock.isAutoSpawning = true;

            // 실제 클릭 버튼 실행 (UI/로직 전체 동일)
            clickLimit.btn.onClick.Invoke();

            AutoSystemLock.isAutoSpawning = false;

            // 너무 빠르면 문제 → 0.05초 휴식
            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool CanSpawn()
    {
        if (clickLimit == null) return false;

        // 네가 ClickLimit 안에 이미 만들어 둔 IsSpawnable() 그대로 씀
        return clickLimit.IsSpawnable();
    }
}