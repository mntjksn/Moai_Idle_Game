using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    // Unity Vector3 <-> 저장용 Vector3 변환
    public static implicit operator Vector3(SerializableVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static implicit operator SerializableVector3(Vector3 v)
    {
        return new SerializableVector3(v.x, v.y, v.z);
    }
}

[System.Serializable]
public class CharacterData
{
    // 생성할 캐릭터(또는 아이템) 번호
    public int nowspawn;

    // 저장된 위치
    public SerializableVector3 position;
}

[System.Serializable]
public class moaiData
{
    // 배치된 캐릭터 목록(번호 + 위치)
    public List<CharacterData> characters = new List<CharacterData>();

    // 해금된 캐릭터 번호 목록
    public List<int> spawncheck = new List<int>();

    // 강화된 캐릭터 번호 목록
    public List<int> upgrade = new List<int>();

    // 저장 유효 플래그(사용 방식 유지)
    public bool save = true;
}

public class DataManager : MonoBehaviour
{
    public MergeItem mg;
    public Merge mg1;
    public chbool cb;

    public moaiData data = new moaiData();

    private string filePath;
    private Transform chp;

    private void Start()
    {
        // 씬에 중복 생성 방지
        if (FindObjectsOfType<DataManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // 저장 파일 경로
        filePath = Path.Combine(Application.persistentDataPath, "moaiData2.json");

        // 캐릭터 부모 오브젝트(자식들을 순회하면서 저장)
        chp = GameObject.Find("chp")?.transform;

        // Merge 참조(없으면 씬에서 찾기)
        if (mg1 == null)
            mg1 = GameObject.Find("ItemData")?.GetComponent<Merge>();

        // 데이터 로드(코루틴으로 프레임 분산)
        StartCoroutine(LoadGameDataRoutine());
    }

    private IEnumerator LoadGameDataRoutine()
    {
        // 파일이 없으면 기본값 생성 후 저장
        if (!File.Exists(filePath))
        {
            CreateDefaultData();
            SaveGameData();
            yield break;
        }

        string json = File.ReadAllText(filePath);

        // 파일이 비어있으면 기본값 생성 후 저장
        if (string.IsNullOrWhiteSpace(json))
        {
            CreateDefaultData();
            SaveGameData();
            yield break;
        }

        // 파싱 시도
        try
        {
            data = JsonUtility.FromJson<moaiData>(json);

            // JsonUtility가 간혹 null을 주거나 내부 리스트가 null일 수 있어 방어
            if (data == null || data.characters == null || data.spawncheck == null || data.upgrade == null)
            {
                CreateDefaultData();
                SaveGameData();
                yield break;
            }
        }
        catch
        {
            CreateDefaultData();
            SaveGameData();
            yield break;
        }

        // 캐릭터 해금/강화 상태 복원
        if (CharacterManager.Instance != null)
        {
            for (int i = 0; i < data.spawncheck.Count; i++)
            {
                int itemNum = data.spawncheck[i];
                var item = CharacterManager.Instance.GetItemByItemNum(itemNum);
                if (item != null) item.spawncheck = true;
            }

            for (int i = 0; i < data.upgrade.Count; i++)
            {
                int itemNum = data.upgrade[i];
                var item = CharacterManager.Instance.GetItemByItemNum(itemNum);
                if (item != null) item.upgrade = true;
            }
        }

        // Merge 로딩 플래그 ON
        if (mg1 != null)
            mg1.IsLoadingData = true;

        // 배치된 캐릭터 생성 및 위치 복원
        for (int i = 0; i < data.characters.Count; i++)
        {
            if (cb != null)
                cb.save = true;

            if (mg1 != null)
            {
                mg1.objPosition1 = data.characters[i].position;
                mg1.itemCreate(data.characters[i].nowspawn);
            }

            // 프레임 분산
            if (i % 5 == 0)
                yield return null;
        }

        // Merge 로딩 플래그 OFF
        if (mg1 != null)
            mg1.IsLoadingData = false;
    }

    private void CreateDefaultData()
    {
        data = new moaiData
        {
            characters = new List<CharacterData>(),
            spawncheck = new List<int>(),
            upgrade = new List<int>(),
            save = true
        };
    }

    public void SaveGameData()
    {
        // 저장 대상(부모)이 없으면 저장 불가
        if (chp == null)
            return;

        // 기존 데이터 초기화
        data.characters.Clear();
        data.spawncheck.Clear();
        data.upgrade.Clear();

        // 배치된 캐릭터(자식) 저장
        int count = chp.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);
            if (!child.gameObject.activeSelf)
                continue;

            MergeItem item = child.GetComponent<MergeItem>();
            if (item == null)
                continue;

            CharacterData character = new CharacterData
            {
                nowspawn = item.iN,
                position = child.position
            };

            data.characters.Add(character);
        }

        // 해금/강화 상태 저장
        if (CharacterManager.Instance != null)
        {
            int total = CharacterManager.Instance.GetCount();

            for (int i = 0; i < total; i++)
            {
                var item = CharacterManager.Instance.GetItem(i);
                if (item == null)
                    continue;

                // itemNum을 저장하고 있으므로, 로드 시에도 itemNum으로 찾는 함수가 있으면 더 안전
                if (item.spawncheck) data.spawncheck.Add(item.itemNum);
                if (item.upgrade) data.upgrade.Add(item.itemNum);
            }
        }

        data.save = true;

        // 파일 저장
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