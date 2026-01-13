using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMerger : MonoBehaviour
{
    private Transform chp;
    private Merge merge;

    private Coroutine mergeRoutine;
    public static AutoMerger Instance;

    [Header("Merge FX")]
    public float mergeMoveDuration = 0.28f;   // 이동 연출 시간
    public float mergeScaleDown = 0.15f;      // 마지막에 살짝 줄어드는 느낌(0이면 끔)

    private void Awake()
    {
        Instance = this;
        chp = GameObject.Find("chp")?.transform;
        merge = FindObjectOfType<Merge>();
    }

    private void OnEnable()
    {
        if (mergeRoutine == null)
            mergeRoutine = StartCoroutine(AutoMergeLoop());
    }

    private void OnDisable()
    {
        if (mergeRoutine != null)
            StopCoroutine(mergeRoutine);

        mergeRoutine = null;
    }

    public void RunAutoMerge()
    {
        var data = SaveManager.Load();
        if (!data.settings.autoMergeEnabled) return;
        if (!data.settings.autoMergeActive) return;

        TryMerge();
    }

    private IEnumerator AutoMergeLoop()
    {
        while (true)
        {
            GameData data = SaveManager.Load();

            if (!data.settings.autoMergePurchased ||
                !data.settings.autoMergeEnabled ||
                !data.settings.autoMergeActive)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (AutoSystemLock.isAutoSpawning)
            {
                yield return null;
                continue;
            }

            AutoSystemLock.isAutoMerging = true;

            TryMerge();

            AutoSystemLock.isAutoMerging = false;

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void TryMerge()
    {
        if (chp == null || merge == null) return;

        int count = chp.childCount;
        if (count < 2) return;

        Dictionary<int, List<MergeItem>> levelGroups = new Dictionary<int, List<MergeItem>>();

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            var item = child.GetComponent<MergeItem>();
            if (item == null) continue;

            // 연출/합치는 중인 애는 제외
            if (item.isMerging) continue;

            int lv = item.iN;

            if (!levelGroups.ContainsKey(lv))
                levelGroups[lv] = new List<MergeItem>();

            levelGroups[lv].Add(item);
        }

        foreach (var kv in levelGroups)
        {
            if (kv.Value.Count >= 2)
            {
                // 즉시 삭제 대신 연출 코루틴
                StartCoroutine(MergePairRoutine(kv.Value[0], kv.Value[1]));
                return;
            }
        }
    }

    private IEnumerator MergePairRoutine(MergeItem a, MergeItem b)
    {
        if (a == null || b == null) yield break;

        // 다른 시스템이 다시 잡지 못하게
        a.isMerging = true;
        b.isMerging = true;

        int level = a.iN;

        Vector3 startA = a.transform.position;
        Vector3 startB = b.transform.position;
        Vector3 mid = (startA + startB) * 0.5f;

        Vector3 scaleA0 = a.transform.localScale;
        Vector3 scaleB0 = b.transform.localScale;

        float t = 0f;
        float dur = Mathf.Max(0.01f, mergeMoveDuration);

        // 부드럽게 중간지점으로 이동
        while (t < 1f)
        {
            if (a == null || b == null) yield break;

            t += Time.deltaTime / dur;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // easeOutCubic

            a.transform.position = Vector3.Lerp(startA, mid, eased);
            b.transform.position = Vector3.Lerp(startB, mid, eased);

            // (선택) 마지막에 살짝 빨려들듯 스케일 다운
            if (mergeScaleDown > 0f)
            {
                float s = Mathf.Lerp(1f, Mathf.Max(0.01f, mergeScaleDown), eased);
                a.transform.localScale = scaleA0 * s;
                b.transform.localScale = scaleB0 * s;
            }

            yield return null;
        }

        // 기존 두 개 삭제 대신 풀 반환
        if (a != null)
        {
            a.isMerging = false;
            ObjectPool.Instance.ReturnToPool(a.iN, a.gameObject);
        }
        if (b != null)
        {
            b.isMerging = false;
            ObjectPool.Instance.ReturnToPool(b.iN, b.gameObject);
        }

        // 새 캐릭터 생성
        merge.SetMergeSpawnPos(mid);
        merge.itemCreate(level + 1);
    }
}