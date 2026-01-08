using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMerger : MonoBehaviour
{
    private Transform chp;
    private Merge merge;

    private Coroutine mergeRoutine;
    public static AutoMerger Instance;

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

            // 자동 합치기 미구매 or 비활성화
            if (!data.settings.autoMergePurchased)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (!data.settings.autoMergeEnabled)
            {  
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (!data.settings.autoMergeActive)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // AutoSpawn이 동작 중이면 대기
            if (AutoSystemLock.isAutoSpawning)
            {
                yield return null;
                continue;
            }

            // 내가 작업 시작
            AutoSystemLock.isAutoMerging = true;

            // 한 번 합치기 시도
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

        // 같은 레벨 찾기
        Dictionary<int, List<MergeItem>> levelGroups = new Dictionary<int, List<MergeItem>>();

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            var item = child.GetComponent<MergeItem>();
            if (item == null) continue;

            int lv = item.iN;

            if (!levelGroups.ContainsKey(lv))
                levelGroups[lv] = new List<MergeItem>();

            levelGroups[lv].Add(item);
        }

        // 같은 레벨이 2개 이상이면 한 쌍만 합치고 종료
        foreach (var kv in levelGroups)
        {
            if (kv.Value.Count >= 2)
            {
                MergePair(kv.Value[0], kv.Value[1]);
                return;
            }
        }
    }

    private void MergePair(MergeItem a, MergeItem b)
    {
        int level = a.iN;

        // 두 개 중간 위치
        Vector3 pos = (a.transform.position + b.transform.position) * 0.5f;

        // 기존 두 개 삭제
        Destroy(a.gameObject);
        Destroy(b.gameObject);

        // Merge 시스템 사용
        merge.objPosition1 = pos;
        merge.itemCreate(level + 1);
    }
}