using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Item
{
    public string name;
    public string sub;
    public int itemNum;
    public int itemgold;
    public string spritePath;
    public string panelPrefabPath;

    [System.NonSerialized] public Sprite itemimg;
    [System.NonSerialized] public GameObject panel;

    public bool spawncheck;
    public bool upgrade;
}

[System.Serializable]
public class ItemListWrapper
{
    public List<Item> items;
}

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public List<Item> characters = new List<Item>();

    private const string JSON_NAME = "ItemData.json";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 로딩을 Awake에서 직접 하지 않고 코루틴으로 분리 → 프리즈 방지
        StartCoroutine(LoadCharactersRoutine());
    }

    private IEnumerator LoadCharactersRoutine()
    {
        string targetPath = Path.Combine(Application.persistentDataPath, JSON_NAME);

        // JSON이 없을 경우 StreamingAssets에서 복사
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

        // JSON 로드
        string json = File.ReadAllText(targetPath);
        string wrapped = "{\"items\":" + json + "}";
        ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(wrapped);

        characters = wrapper.items ?? new List<Item>();

        // 스프라이트 / 프리팹 로딩 최적화
        foreach (var item in characters)
        {
            if (!string.IsNullOrEmpty(item.spritePath))
                item.itemimg = Resources.Load<Sprite>(item.spritePath);

            if (!string.IsNullOrEmpty(item.panelPrefabPath))
                item.panel = Resources.Load<GameObject>(item.panelPrefabPath);
        }

        yield break;
    }

    public Item GetItem(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            Debug.LogError($"[CharacterManager] 잘못된 인덱스 요청: {index}");
            return null;
        }

        return characters[index];
    }

    public int GetCount() => characters.Count;
}