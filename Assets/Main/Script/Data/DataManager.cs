using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX; y = rY; z = rZ;
    }

    public static implicit operator Vector3(SerializableVector3 v)
        => new Vector3(v.x, v.y, v.z);

    public static implicit operator SerializableVector3(Vector3 v)
        => new SerializableVector3(v.x, v.y, v.z);
}

[System.Serializable]
public class moaiData
{
    public List<int> itemNum1 = new List<int>();
    public List<int> itemNum2 = new List<int>();
    public List<int> itemNum3 = new List<int>();
    public List<SerializableVector3> Pos1 = new List<SerializableVector3>();
    public int chCount;
    public bool save = true;
}

public class DataManager : MonoBehaviour
{
    public MergeItem mg;
    public Merge mg1;
    public chbool cb;
    public moaiData data = new moaiData();

    private string filePath;
    private Transform chp;   // 캐싱

    private void Start()
    {
        // 중복 방지
        if (FindObjectsOfType<DataManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, "moaiData2.json");

        chp = GameObject.Find("chp")?.transform;
        mg1 = mg1 ?? GameObject.Find("ItemData")?.GetComponent<Merge>();

        // 무거운 작업은 코루틴으로
        StartCoroutine(LoadGameDataRoutine());
    }

    private IEnumerator LoadGameDataRoutine()
    {
        // 파일 없으면 초기화 후 저장
        if (!File.Exists(filePath))
        {
            data = new moaiData();
            SaveGameData();
            yield break;
        }

        string json = File.ReadAllText(filePath);

        try
        {
            data = JsonUtility.FromJson<moaiData>(json);
        }
        catch
        {
            data = new moaiData();
        }

        // CharacterManager 상태 복원
        if (CharacterManager.Instance != null)
        {
            foreach (int idx in data.itemNum2)
            {
                var item = CharacterManager.Instance.GetItem(idx);
                if (item != null) item.spawncheck = true;
            }

            foreach (int idx in data.itemNum3)
            {
                var item = CharacterManager.Instance.GetItem(idx);
                if (item != null) item.upgrade = true;
            }
        }

        mg1.IsLoadingData = true;

        // 캐릭터 복원(지연 로딩으로 렉 방지)
        for (int i = 0; i < data.itemNum1.Count; i++)
        {
            cb.save = true;
            mg1.objPosition1 = data.Pos1[i];
            mg1.itemCreate(data.itemNum1[i]);

            // 한 번에 50~200개 생성하면 렉 → 분산 처리
            if (i % 5 == 0)
                yield return null;
        }

        mg1.IsLoadingData = false;
    }

    public void SaveGameData()
    {
        if (chp == null) return;

        data.itemNum1.Clear();
        data.itemNum2.Clear();
        data.itemNum3.Clear();
        data.Pos1.Clear();

        int count = chp.childCount;

        for (int i = 0; i < count; i++)
        {
            var child = chp.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            var item = child.GetComponent<MergeItem>();
            if (item != null)
            {
                data.itemNum1.Add(item.iN);
                data.Pos1.Add(child.position);
            }
        }

        if (CharacterManager.Instance != null)
        {
            int total = CharacterManager.Instance.GetCount();
            for (int i = 0; i < total; i++)
            {
                var item = CharacterManager.Instance.GetItem(i);
                if (item == null) continue;

                if (item.spawncheck) data.itemNum2.Add(item.itemNum);
                if (item.upgrade) data.itemNum3.Add(item.itemNum);
            }
        }

        data.chCount = data.itemNum1.Count;
        data.save = true;

        File.WriteAllText(filePath, JsonUtility.ToJson(data, true));
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveGameData();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}