using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    public GameObject itemPrefab;
    public int initialPoolSize = 10;

    private Dictionary<int, Queue<GameObject>> poolDict = new Dictionary<int, Queue<GameObject>>();
    private Transform poolParent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

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
        InitializePools();
    }

    private void InitializePools()
    {
        int count = CharacterManager.Instance.GetCount();

        for (int i = 0; i < count; i++)
        {
            if (!poolDict.ContainsKey(i))
                poolDict[i] = new Queue<GameObject>();

            int poolSize = (i == 0) ? 2 : initialPoolSize;

            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = CreateNewItem(i);
                obj.SetActive(false);
                poolDict[i].Enqueue(obj);
            }
        }
    }

    private GameObject CreateNewItem(int id)
    {
        GameObject go = Instantiate(itemPrefab, poolParent);
        go.SetActive(false);

        var itemData = CharacterManager.Instance.GetItem(id);
        var mergeItem = go.GetComponent<MergeItem>();

        if (mergeItem != null && itemData != null)
            mergeItem.InitItem(itemData);

        return go;
    }

    public GameObject SpawnFromPool(int id, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(id))
            poolDict[id] = new Queue<GameObject>();

        GameObject obj;

        if (poolDict[id].Count > 0)
            obj = poolDict[id].Dequeue();
        else
            obj = CreateNewItem(id);

        // 최신 데이터 재적용
        var itemData = CharacterManager.Instance.GetItem(id);
        var mergeItem = obj.GetComponent<MergeItem>();
        if (mergeItem != null && itemData != null)
            mergeItem.InitItem(itemData);

        obj.transform.SetParent(null);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        obj.transform.localScale = Vector3.one;
        var mi2 = obj.GetComponent<MergeItem>();
        if (mi2 != null) mi2.isMerging = false;

        obj.SetActive(true);

        return obj;
    }

    public void ReturnToPool(int id, GameObject obj)
    {
        // 상태 리셋 (중요)
        obj.transform.localScale = Vector3.one;

        var mi = obj.GetComponent<MergeItem>();
        if (mi != null) mi.isMerging = false;

        obj.SetActive(false);
        obj.transform.SetParent(poolParent);

        obj.transform.localPosition = Vector3.zero;

        poolDict[id].Enqueue(obj);
    }
}