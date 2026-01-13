using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Merge : MonoBehaviour
{
    public chbool cb;

    // 자동머지/수동머지에서 “합친 지점” 생성용
    public Vector3 objPosition1;

    private Transform chp;
    private Transform canvas;
    private GameData data;

    private Camera cam;
    private bool isSceneLoading = false;

    [HideInInspector] public bool IsLoadingData = false;

    // 합치기 위치가 “유효하게 세팅됐는지” 플래그
    private bool hasMergePos = false;

    [Header("Merge Spawn FX")]
    public bool useMergeSpawnScaleFx = true;
    public float spawnPopDuration = 0.16f; // 팝 연출 시간
    public float spawnStartScale = 0.15f;  // 시작 스케일(=스케일 다운)

    private void Awake()
    {
        if (FindObjectsOfType<Merge>().Length == 1)
            DontDestroyOnLoad(gameObject);
        else
        {
            Destroy(gameObject);
            return;
        }

        chp = GameObject.Find("chp")?.transform;
        canvas = GameObject.Find("Canvas2")?.transform;
        cam = Camera.main;

        data = SaveManager.Load();
    }

    // AutoMerger(또는 수동 머지)에서 합치기 생성 위치를 세팅할 때 사용
    public void SetMergeSpawnPos(Vector3 pos)
    {
        objPosition1 = pos;
        hasMergePos = true;
    }

    public void itemCreate(int num)
    {
        // 데이터 최신화가 필요하면 여기서 Load를 다시 하는 게 안전할 수 있음(원하면 추가해줄게)
        int childMax = data.settings.childMax;
        int upCh = data.upgrades.count;
        int currentChildren = GetActiveChildCount();
        int listMax = CharacterManager.Instance.GetCount();

        bool needSave = false;
        var item = CharacterManager.Instance.GetItem(num);

        // 엔딩 조건
        if (num >= listMax)
        {
            if (!isSceneLoading)
            {
                isSceneLoading = true;
                data.missions.mission_2_value++;
                SaveManager.Save(data);
                SceneManager.LoadScene("End");
            }
            return;
        }

        // 동일 레벨 소환
        if (num == upCh && currentChildren < childMax)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-2.2f, 2.2f),
                Random.Range(-3.5f, 2.0f),
                0f
            );

            GameObject spawned = SpawnItem(num, randomPos);
            if (useMergeSpawnScaleFx && spawned != null)
                StartCoroutine(SpawnPopFx(spawned.transform));

            needSave = true;
        }
        else if (num < listMax && num != upCh)
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = 10f;
            Vector3 worldPos = cam != null ? cam.ScreenToWorldPoint(mouse) : Vector3.zero;

            // 합치기 위치가 세팅되어 있으면 그 위치 우선, 아니면 마우스 위치
            Vector3 spawnPos = hasMergePos ? objPosition1 : worldPos;
            hasMergePos = false; // 한 번 쓰고 리셋

            GameObject spawned = SpawnItem(num, spawnPos);
            if (useMergeSpawnScaleFx && spawned != null)
                StartCoroutine(SpawnPopFx(spawned.transform));

            // 로드 중일 때는 mission_4 증가 안 함
            if (!IsLoadingData)
            {
                if (data.background.merge_check <= 65000)
                    data.background.merge_check++;

                data.missions.mission_4_value++;
            }

            needSave = true;
        }

        // 도감 최초 해금
        if (!item.spawncheck)
        {
            item.spawncheck = true;
            data.missions.mission_2_value++;
            data.missions.mission_2_tic++;
            data.upgrades.chprefab = num;

            if (item.panel != null && canvas != null)
                Instantiate(item.panel, Vector3.zero, Quaternion.identity, canvas);

            needSave = true;
        }

        if (needSave)
            SaveManager.Save(data);
    }

    private int GetActiveChildCount()
    {
        int count = 0;
        int max = chp.childCount;

        for (int i = 0; i < max; i++)
            if (chp.GetChild(i).gameObject.activeSelf)
                count++;

        return count;
    }

    // 반환형 GameObject로 변경 (연출 코루틴 걸기 위해)
    private GameObject SpawnItem(int num, Vector3 pos)
    {
        GameObject go = ObjectPool.Instance.SpawnFromPool(num, pos, Quaternion.identity);

        if (go != null)
            go.transform.SetParent(chp, false);

        return go;
    }

    // 반환형 GameObject로 변경
    public GameObject SpawnItemDirect(int num, Vector3 pos)
    {
        GameObject go = ObjectPool.Instance.SpawnFromPool(num, pos, Quaternion.identity);
        if (go != null)
            go.transform.SetParent(chp, false);

        return go;
    }

    // “스케일 다운 → 팝” 연출
    private IEnumerator SpawnPopFx(Transform t)
    {
        if (t == null) yield break;

        Vector3 target = t.localScale; // 원래 스케일
        Vector3 start = target * Mathf.Clamp(spawnStartScale, 0.01f, 1f);

        t.localScale = start;

        float dur = Mathf.Max(0.01f, spawnPopDuration);
        float time = 0f;

        while (time < dur)
        {
            if (t == null) yield break;

            time += Time.deltaTime;
            float x = Mathf.Clamp01(time / dur);

            // 튕김이 너무 과하면 easeOutCubic로 교체 가능
            float eased = 1f + 1.70158f * Mathf.Pow(x - 1f, 3f) + 1.70158f * Mathf.Pow(x - 1f, 2f);

            t.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }

        t.localScale = target;
    }
}