using System.Collections.Generic;
using UnityEngine;

// 캐릭터 오브젝트 풀링 관리
// - id(캐릭터 단계/인덱스)별로 Queue를 만들어 재사용
// - Spawn 시 풀에서 꺼내고, Return 시 다시 넣음
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    // 생성할 기본 프리팹(모든 캐릭터가 동일 프리팹 기반이라고 가정)
    public GameObject itemPrefab;

    // 기본 풀 크기(0번만 예외로 더 작게 사용)
    public int initialPoolSize = 10;

    // id별 풀(Queue)
    private Dictionary<int, Queue<GameObject>> poolDict = new Dictionary<int, Queue<GameObject>>();

    // 풀 오브젝트들을 모아둘 부모
    private Transform poolParent;

    private void Awake()
    {
        // 싱글톤 유지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 풀 오브젝트를 담는 부모 생성
            poolParent = new GameObject("[ObjectPool]").transform;
            DontDestroyOnLoad(poolParent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기 풀 생성
        InitializePools();
    }

    // 캐릭터 개수만큼 id별 풀을 만들고 미리 오브젝트 생성
    private void InitializePools()
    {
        int count = CharacterManager.Instance.GetCount();

        for (int i = 0; i < count; i++)
        {
            if (!poolDict.ContainsKey(i))
                poolDict[i] = new Queue<GameObject>();

            // 0번 캐릭터는 풀 크기를 다르게 설정
            int poolSize = (i == 0) ? 2 : initialPoolSize;

            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = CreateNewItem(i);
                obj.SetActive(false);
                poolDict[i].Enqueue(obj);
            }
        }
    }

    // 새 오브젝트 생성 후 MergeItem 초기화
    private GameObject CreateNewItem(int id)
    {
        GameObject go = Instantiate(itemPrefab, poolParent);
        go.SetActive(false);

        // 현재 캐릭터 데이터 적용
        var itemData = CharacterManager.Instance.GetItem(id);
        var mergeItem = go.GetComponent<MergeItem>();

        if (mergeItem != null && itemData != null)
            mergeItem.InitItem(itemData);

        return go;
    }

    // 풀에서 오브젝트를 꺼내서 활성화 후 반환
    public GameObject SpawnFromPool(int id, Vector3 position, Quaternion rotation)
    {
        // id 풀 없으면 생성
        if (!poolDict.ContainsKey(id))
            poolDict[id] = new Queue<GameObject>();

        GameObject obj;

        // 풀에 있으면 꺼내고, 없으면 새로 생성
        if (poolDict[id].Count > 0)
            obj = poolDict[id].Dequeue();
        else
            obj = CreateNewItem(id);

        // 최신 캐릭터 데이터 재적용(업그레이드/스프라이트 변경 반영 목적)
        var itemData = CharacterManager.Instance.GetItem(id);
        var mergeItem = obj.GetComponent<MergeItem>();
        if (mergeItem != null && itemData != null)
            mergeItem.InitItem(itemData);

        // 부모 해제 후 위치/회전 세팅
        obj.transform.SetParent(null);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // 상태 리셋
        obj.transform.localScale = Vector3.one;

        var mi2 = obj.GetComponent<MergeItem>();
        if (mi2 != null)
            mi2.isMerging = false;

        // 활성화
        obj.SetActive(true);

        return obj;
    }

    // 오브젝트를 비활성화하고 풀로 반환
    public void ReturnToPool(int id, GameObject obj)
    {
        // 기본 상태 리셋
        obj.transform.localScale = Vector3.one;

        var mi = obj.GetComponent<MergeItem>();
        if (mi != null)
            mi.isMerging = false;

        // 비활성화 후 풀 부모로 이동
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);

        // 풀 부모 기준 위치 초기화
        obj.transform.localPosition = Vector3.zero;

        // 큐에 다시 추가
        poolDict[id].Enqueue(obj);
    }
}