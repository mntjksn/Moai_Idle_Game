using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// 캐릭터 생성 및 합치기(머지)를 담당하는 매니저
public class Merge : MonoBehaviour
{
    // 자동 머지 / 수동 머지 시 합쳐진 위치에서 생성하기 위한 좌표
    public Vector3 objPosition1;

    // 캐릭터가 배치되는 부모 오브젝트
    private Transform chp;

    // 최초 해금 패널을 띄울 UI 캔버스
    private Transform canvas;

    // 세이브 데이터
    private GameData data;

    // 화면 좌표를 월드 좌표로 변환하기 위한 카메라
    private Camera cam;

    // 엔딩 씬 중복 로드 방지용
    private bool isSceneLoading = false;

    // 데이터 로드 중 생성에서는 미션 카운트 등을 증가시키지 않기 위한 플래그
    [HideInInspector] public bool IsLoadingData = false;

    // 합치기 위치가 정상적으로 세팅되었는지 여부
    private bool hasMergePos = false;

    [Header("Merge Spawn FX")]
    public bool useMergeSpawnScaleFx = true; // 생성 시 스케일 팝 연출 사용 여부
    public float spawnPopDuration = 0.16f;  // 팝 연출 시간
    public float spawnStartScale = 0.15f;   // 시작 스케일 비율

    private void Awake()
    {
        // 싱글톤 유지
        if (FindObjectsOfType<Merge>().Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기 참조 캐싱
        // 씬 전환 시 null이 될 수 있으므로 itemCreate에서 다시 확인함
        chp = GameObject.FindGameObjectWithTag("chp")?.transform;
        canvas = GameObject.Find("Canvas2")?.transform;
        cam = Camera.main;

        // 세이브 데이터 로드
        data = SaveManager.Load();
    }

    // 자동 머지 또는 수동 머지에서
    // 합쳐진 위치를 기준으로 생성하고 싶을 때 호출
    public void SetMergeSpawnPos(Vector3 pos)
    {
        objPosition1 = pos;
        hasMergePos = true;
    }

    // 캐릭터 생성 또는 머지 결과 생성
    public void itemCreate(int num)
    {
        // 항상 최신 세이브 데이터를 사용
        data = SaveManager.Load();

        // 씬 전환 후 참조가 끊겼을 수 있으므로 재확인
        if (chp == null) chp = GameObject.Find("chp")?.transform;
        if (canvas == null) canvas = GameObject.Find("Canvas2")?.transform;
        if (cam == null) cam = Camera.main;

        // 필수 싱글톤 및 참조 방어
        if (CharacterManager.Instance == null || !CharacterManager.Instance.IsLoaded)
            return;

        if (ObjectPool.Instance == null)
            return;

        if (chp == null)
            return;

        // 현재 최대 배치 수와 강화 단계
        int childMax = data.settings.childMax;
        int upCh = data.upgrades.count;

        // 현재 배치된 캐릭터 수
        int currentChildren = GetActiveChildCount();

        // 캐릭터 리스트 최대 개수
        int listMax = CharacterManager.Instance.GetCount();

        bool needSave = false;

        // 엔딩 조건: 존재하지 않는 단계 생성 시
        if (num >= listMax)
        {
            if (!isSceneLoading)
            {
                isSceneLoading = true;

                // 엔딩 관련 미션 카운트 증가
                data.missions.mission_2_value++;

                SaveManager.Save(data);
                SceneManager.LoadScene("End");
            }
            return;
        }

        // 유효한 캐릭터 데이터 가져오기
        var item = CharacterManager.Instance.GetItem(num);
        if (item == null)
            return;

        // 동일 레벨 소환(현재 강화 단계와 같은 레벨)
        if (num == upCh && currentChildren < childMax)
        {
            // 랜덤 위치 생성
            Vector3 randomPos = new Vector3(
                Random.Range(-2.2f, 2.2f),
                Random.Range(-3.5f, 2.0f),
                0f
            );

            GameObject spawned = SpawnItem(num, randomPos);

            // 생성 팝 연출
            if (useMergeSpawnScaleFx && spawned != null)
                StartCoroutine(SpawnPopFx(spawned.transform));

            needSave = true;
        }
        // 머지 결과 생성(강화 단계보다 낮은 레벨)
        else if (num < listMax && num != upCh)
        {
            Vector3 spawnPos = Vector3.zero;

            // 합치기 위치가 지정되어 있으면 해당 위치 사용
            if (hasMergePos)
            {
                spawnPos = objPosition1;
            }
            else
            {
                // 마우스 위치 기준 생성
                if (cam != null)
                {
                    Vector3 mouse = Input.mousePosition;
                    mouse.z = 10f;
                    spawnPos = cam.ScreenToWorldPoint(mouse);
                }
            }

            // 위치 사용 후 리셋
            hasMergePos = false;

            GameObject spawned = SpawnItem(num, spawnPos);

            // 생성 팝 연출
            if (useMergeSpawnScaleFx && spawned != null)
                StartCoroutine(SpawnPopFx(spawned.transform));

            // 데이터 로드 중이 아닐 때만 미션 카운트 증가
            if (!IsLoadingData)
            {
                if (data.background.merge_check <= 65000)
                    data.background.merge_check++;

                data.missions.mission_4_value++;
            }

            needSave = true;
        }

        // 도감 최초 해금 처리
        if (!item.spawncheck)
        {
            item.spawncheck = true;

            data.missions.mission_2_value++;
            data.missions.mission_2_tic++;

            // 최근 해금된 캐릭터 번호 저장
            data.upgrades.chprefab = num;

            // 최초 해금 패널 표시
            if (item.panel != null && canvas != null)
                Instantiate(item.panel, Vector3.zero, Quaternion.identity, canvas);

            needSave = true;
        }

        // 변경 사항이 있으면 저장
        if (needSave)
            SaveManager.Save(data);
    }

    // 현재 활성화된 캐릭터 수 계산
    private int GetActiveChildCount()
    {
        if (chp == null)
            return 0;

        int count = 0;
        int max = chp.childCount;

        for (int i = 0; i < max; i++)
        {
            if (chp.GetChild(i).gameObject.activeSelf)
                count++;
        }

        return count;
    }

    // 오브젝트 풀에서 캐릭터 생성 후 부모 설정
    private GameObject SpawnItem(int num, Vector3 pos)
    {
        GameObject go = ObjectPool.Instance.SpawnFromPool(num, pos, Quaternion.identity);

        if (go != null)
            go.transform.SetParent(chp, false);

        return go;
    }

    // 외부에서 직접 생성이 필요할 때 사용
    public GameObject SpawnItemDirect(int num, Vector3 pos)
    {
        if (ObjectPool.Instance == null || chp == null)
            return null;

        GameObject go = ObjectPool.Instance.SpawnFromPool(num, pos, Quaternion.identity);

        if (go != null)
            go.transform.SetParent(chp, false);

        return go;
    }

    // 생성 시 스케일 다운 -> 팝업 연출
    private IEnumerator SpawnPopFx(Transform t)
    {
        if (t == null)
            yield break;

        // 원래 스케일
        Vector3 target = t.localScale;

        // 시작 스케일
        Vector3 start = target * Mathf.Clamp(spawnStartScale, 0.01f, 1f);

        t.localScale = start;

        float dur = Mathf.Max(0.01f, spawnPopDuration);
        float time = 0f;

        while (time < dur)
        {
            if (t == null)
                yield break;

            time += Time.deltaTime;
            float x = Mathf.Clamp01(time / dur);

            // 살짝 튀는 이징 연출
            float eased =
                1f +
                1.70158f * Mathf.Pow(x - 1f, 3f) +
                1.70158f * Mathf.Pow(x - 1f, 2f);

            t.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }

        // 최종 스케일 보정
        t.localScale = target;
    }
}