using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CharacterItem
{
    public string name;
    public string sub;

    public int itemNum;
    public int itemgold;

    // Resources.Load 경로(확장자 제외)
    public string spritePath;
    public string panelPrefabPath;

    // 런타임 전용(저장 안됨)
    [System.NonSerialized] public Sprite itemimg;
    [System.NonSerialized] public GameObject panel;

    public bool spawncheck;
    public bool upgrade;
}

[System.Serializable]
public class CharacterItemListWrapper
{
    // ItemData.json의 최상위 키가 "characters" 이므로 필드명도 동일해야 한다
    public List<CharacterItem> characters;
}

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    // 캐릭터 목록
    public List<CharacterItem> characters = new List<CharacterItem>();

    // 로드 완료 여부(다른 스크립트에서 접근 타이밍 방지용)
    public bool IsLoaded { get; private set; }

    private const string JSON_NAME = "ItemData.json";

    private void Awake()
    {
        // 싱글톤 유지
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        StartCoroutine(LoadCharactersRoutine());
    }

    private IEnumerator LoadCharactersRoutine()
    {
        IsLoaded = false;

        string targetPath = Path.Combine(Application.persistentDataPath, JSON_NAME);

        // JSON이 없으면 StreamingAssets에서 복사
        if (!File.Exists(targetPath))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, JSON_NAME);

#if UNITY_ANDROID && !UNITY_EDITOR
            UnityWebRequest req = UnityWebRequest.Get(streamingPath);
            yield return req.SendWebRequest();

            if (!req.isNetworkError && !req.isHttpError)
                File.WriteAllText(targetPath, req.downloadHandler.text);
#else
            if (File.Exists(streamingPath))
                File.Copy(streamingPath, targetPath, true);
#endif
        }

        // 파일이 여전히 없으면 빈 리스트
        if (!File.Exists(targetPath))
        {
            characters = new List<CharacterItem>();
            IsLoaded = true;
            yield break;
        }

        // JSON 읽기
        string json = File.ReadAllText(targetPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            characters = new List<CharacterItem>();
            IsLoaded = true;
            yield break;
        }

        json = json.TrimStart();

        // 배열 JSON / wrapper JSON 모두 대응
        CharacterItemListWrapper wrapper = null;

        try
        {
            if (json.StartsWith("["))
            {
                // 배열만 있을 경우 "characters"로 감싸서 파싱
                string wrapped = "{\"characters\":" + json + "}";
                wrapper = JsonUtility.FromJson<CharacterItemListWrapper>(wrapped);
            }
            else
            {
                // {"characters":[...]} 형태면 그대로 파싱
                wrapper = JsonUtility.FromJson<CharacterItemListWrapper>(json);
            }
        }
        catch
        {
            wrapper = null;
        }

        characters = (wrapper != null && wrapper.characters != null)
            ? wrapper.characters
            : new List<CharacterItem>();

        // 리소스 로드(1회)
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterItem item = characters[i];

            if (!string.IsNullOrEmpty(item.spritePath))
                item.itemimg = Resources.Load<Sprite>(item.spritePath);

            if (!string.IsNullOrEmpty(item.panelPrefabPath))
                item.panel = Resources.Load<GameObject>(item.panelPrefabPath);
        }

        IsLoaded = true;
        yield break;
    }

    // 리스트 인덱스로 가져오기
    public CharacterItem GetItem(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            Debug.LogError("[CharacterManager] 잘못된 인덱스 요청: " + index);
            return null;
        }

        return characters[index];
    }

    // itemNum으로 가져오기(세이브/로드 용)
    public CharacterItem GetItemByItemNum(int itemNum)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterItem item = characters[i];
            if (item != null && item.itemNum == itemNum)
                return item;
        }

        return null;
    }

    public int GetCount()
    {
        return characters.Count;
    }
}