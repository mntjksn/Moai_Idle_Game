using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 자동 합치기(오토 머지) 시스템
// - 일정 주기로 현재 배치된 캐릭터들을 스캔해서
//   같은 레벨(같은 iN) 2개를 찾으면 합치기 연출 후 다음 레벨을 생성한다.
// - AutoTimeManager(타이머)에서 RunAutoMerge()를 호출해도 되고,
//   이 스크립트 자체가 OnEnable 시 루프를 돌면서 자동 실행도 한다.
public class AutoMerger : MonoBehaviour
{
    // 캐릭터가 배치되는 부모 Transform
    private Transform chp;

    // 머지 결과 생성(merge.itemCreate) 호출을 위해 Merge 참조
    private Merge merge;

    // Update 대신 루프 코루틴
    private Coroutine mergeRoutine;

    // 싱글톤(간단 버전)
    public static AutoMerger Instance;

    [Header("Merge FX")]
    public float mergeMoveDuration = 0.28f;   // 두 오브젝트가 중앙으로 모이는 이동 연출 시간
    public float mergeScaleDown = 0.15f;      // 이동 중 스케일 다운 비율(0이면 스케일 다운 연출 끔)

    private void Awake()
    {
        // 싱글톤 세팅(중복 제거는 안 함: 필요하면 Instance 중복 방어 추가 가능)
        Instance = this;

        // 씬 내 오브젝트 참조 캐싱
        chp = GameObject.FindGameObjectWithTag("chp")?.transform;
        merge = FindObjectOfType<Merge>();
    }

    private void OnEnable()
    {
        // 패널/오브젝트가 켜질 때 자동 루프 시작
        if (mergeRoutine == null)
            mergeRoutine = StartCoroutine(AutoMergeLoop());
    }

    private void OnDisable()
    {
        // 꺼질 때 루프 정지(코루틴 누수 방지)
        if (mergeRoutine != null)
            StopCoroutine(mergeRoutine);

        mergeRoutine = null;
    }

    // 외부에서 "지금 한 번 자동 합치기 실행"하고 싶을 때 호출
    // - AutoTimeManager에서 타이머 끝났을 때 호출하는 용도
    public void RunAutoMerge()
    {
        var data = SaveManager.Load();

        // 자동 합치기 기능이 ON 상태인지, 현재 작업(active) 상태인지 체크
        if (!data.settings.autoMergeEnabled) return;
        if (!data.settings.autoMergeActive) return;

        TryMerge();
    }

    // Update 대체 루프
    // - 일정 주기(0.5초)로 합칠 수 있는 쌍이 있는지 검사
    // - 다른 자동 시스템(오토 소환)과 충돌 방지용 락 사용
    private IEnumerator AutoMergeLoop()
    {
        while (true)
        {
            GameData data = SaveManager.Load();

            // 구입 안했거나, 비활성/쿨타임 상태면 가볍게 대기
            if (!data.settings.autoMergePurchased ||
                !data.settings.autoMergeEnabled ||
                !data.settings.autoMergeActive)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // 오토 소환 중이면 합치기 대기(충돌 방지)
            if (AutoSystemLock.isAutoSpawning)
            {
                yield return null;
                continue;
            }

            // 지금은 오토 머지 동작 중이라고 표시(다른 시스템이 건드리지 않게)
            AutoSystemLock.isAutoMerging = true;

            TryMerge();

            AutoSystemLock.isAutoMerging = false;

            // 너무 자주 스캔하면 비용 증가 → 0.5초 간격으로 체크
            yield return new WaitForSeconds(0.5f);
        }
    }

    // 현재 chp 아래 활성화된 캐릭터들을 스캔해서
    // 같은 레벨(iN) 2개 이상인 그룹이 있으면 1쌍을 합친다.
    private void TryMerge()
    {
        // 참조가 없으면 동작 불가
        if (chp == null || merge == null) return;

        int count = chp.childCount;
        if (count < 2) return;

        // 레벨(iN)별로 MergeItem을 그룹핑
        Dictionary<int, List<MergeItem>> levelGroups = new Dictionary<int, List<MergeItem>>();

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            var item = child.GetComponent<MergeItem>();
            if (item == null) continue;

            // 현재 합치는 연출 중인 애는 제외(중복 합치기 방지)
            if (item.isMerging) continue;

            int lv = item.iN;

            if (!levelGroups.ContainsKey(lv))
                levelGroups[lv] = new List<MergeItem>();

            levelGroups[lv].Add(item);
        }

        // 2개 이상인 레벨 그룹을 찾으면 1쌍만 합치고 종료
        foreach (var kv in levelGroups)
        {
            if (kv.Value.Count >= 2)
            {
                // 즉시 삭제 대신 연출 코루틴 실행
                StartCoroutine(MergePairRoutine(kv.Value[0], kv.Value[1]));
                return;
            }
        }
    }

    // 실제 합치기 연출 + 풀 반환 + 다음 레벨 생성
    // - a,b를 중간 지점(mid)으로 모은 후 풀로 되돌리고,
    // - merge.itemCreate(level+1)로 새 캐릭터 생성
    private IEnumerator MergePairRoutine(MergeItem a, MergeItem b)
    {
        if (a == null || b == null) yield break;

        // 다른 합치기 로직이 이 둘을 다시 잡지 못하게 잠금
        a.isMerging = true;
        b.isMerging = true;

        int level = a.iN;

        // 시작 위치 / 중간 지점 계산
        Vector3 startA = a.transform.position;
        Vector3 startB = b.transform.position;
        Vector3 mid = (startA + startB) * 0.5f;

        // 시작 스케일 저장(연출 후 복원/풀 반환 대비)
        Vector3 scaleA0 = a.transform.localScale;
        Vector3 scaleB0 = b.transform.localScale;

        float t = 0f;
        float dur = Mathf.Max(0.01f, mergeMoveDuration);

        // -------------------------
        // 중앙으로 모이는 연출
        // -------------------------
        while (t < 1f)
        {
            // 도중에 파괴/비활성화되는 경우 방어
            if (a == null || b == null) yield break;

            t += Time.deltaTime / dur;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // easeOutCubic

            // 위치 이동
            a.transform.position = Vector3.Lerp(startA, mid, eased);
            b.transform.position = Vector3.Lerp(startB, mid, eased);

            // (선택) 스케일 다운 연출(빨려들듯)
            if (mergeScaleDown > 0f)
            {
                float s = Mathf.Lerp(1f, Mathf.Max(0.01f, mergeScaleDown), eased);
                a.transform.localScale = scaleA0 * s;
                b.transform.localScale = scaleB0 * s;
            }

            yield return null;
        }

        // -------------------------
        // 기존 두 개는 풀로 반환
        // -------------------------
        if (a != null)
        {
            a.isMerging = false; // 반환 전에 플래그 복원
            ObjectPool.Instance.ReturnToPool(a.iN, a.gameObject);
        }

        if (b != null)
        {
            b.isMerging = false;
            ObjectPool.Instance.ReturnToPool(b.iN, b.gameObject);
        }

        // -------------------------
        // 다음 레벨 생성
        // - Merge가 생성 위치를 objPosition1로 쓰도록 mid를 전달
        // -------------------------
        merge.SetMergeSpawnPos(mid);
        merge.itemCreate(level + 1);
    }
}