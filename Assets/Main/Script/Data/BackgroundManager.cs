using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class BackgroundItem
{
    public string name;
    public string sub;
    public string sub2;

    // Resources.Load 경로(확장자 제외)
    public string spritePath;
    public string panelPrefabPath;

    // 런타임 전용(저장 안됨)
    [NonSerialized] public Sprite itemimg;
    [NonSerialized] public GameObject panel;

    // 해금 여부
    public bool spawncheck;
}

[Serializable]
public class BackgroundItemListWrapper
{
    // BackgroundData.json의 최상위 키가 "backgrounds" 이므로 필드명도 동일해야 한다
    public List<BackgroundItem> backgrounds;
}

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    public List<BackgroundItem> backgrounds = new List<BackgroundItem>();

    public int SelectedIndex { get; private set; } = 0;

    public event Action<int> OnBackgroundSelected;

    public bool IsLoaded { get; private set; }

    private const string KEY_BG_INDEX = "bg_index";
    private const string JSON_NAME = "BackgroundData.json";

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

        LoadBackground();

        // 선택 인덱스 적용(범위 보정)
        SelectedIndex = Mathf.Clamp(
            PlayerPrefs.GetInt(KEY_BG_INDEX, 0),
            0,
            Mathf.Max(0, backgrounds.Count - 1)
        );

        // 잠긴 배경이면 첫 해금 배경으로 변경
        if (!IsUnlocked(SelectedIndex))
            SelectedIndex = FirstUnlockedIndex();

        OnBackgroundSelected?.Invoke(SelectedIndex);
    }

    public void SelectBackground(int index)
    {
        if (index < 0 || index >= backgrounds.Count)
            return;

        if (!IsUnlocked(index))
            return;

        SelectedIndex = index;

        PlayerPrefs.SetInt(KEY_BG_INDEX, SelectedIndex);
        PlayerPrefs.Save();

        SaveBackground();
        OnBackgroundSelected?.Invoke(index);
    }

    public bool IsUnlocked(int index)
    {
        return index >= 0 &&
               index < backgrounds.Count &&
               backgrounds[index].spawncheck;
    }

    public void UnlockBackground(int index, bool selectIfNone = false)
    {
        if (index < 0 || index >= backgrounds.Count)
            return;

        if (!backgrounds[index].spawncheck)
        {
            backgrounds[index].spawncheck = true;
            SaveBackground();
        }

        if (selectIfNone && FirstUnlockedIndex() == index)
            SelectBackground(index);
    }

    private int FirstUnlockedIndex()
    {
        for (int i = 0; i < backgrounds.Count; i++)
        {
            if (backgrounds[i].spawncheck)
                return i;
        }

        return 0;
    }

    private void LoadBackground()
    {
        IsLoaded = false;

        string persistentPath = Path.Combine(Application.persistentDataPath, JSON_NAME);

        // JSON이 없으면 StreamingAssets에서 복사
        EnsureJsonExists(persistentPath);

        if (!File.Exists(persistentPath))
        {
            backgrounds = new List<BackgroundItem>();
            IsLoaded = true;
            return;
        }

        string json = File.ReadAllText(persistentPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            backgrounds = new List<BackgroundItem>();
            IsLoaded = true;
            return;
        }

        json = json.TrimStart();

        BackgroundItemListWrapper wrapper = null;

        try
        {
            if (json.StartsWith("["))
            {
                // 배열만 있을 경우 "backgrounds"로 감싸서 파싱
                string wrapped = "{\"backgrounds\":" + json + "}";
                wrapper = JsonUtility.FromJson<BackgroundItemListWrapper>(wrapped);
            }
            else
            {
                // {"backgrounds":[...]} 형태면 그대로 파싱
                wrapper = JsonUtility.FromJson<BackgroundItemListWrapper>(json);
            }
        }
        catch
        {
            wrapper = null;
        }

        backgrounds = (wrapper != null && wrapper.backgrounds != null)
            ? wrapper.backgrounds
            : new List<BackgroundItem>();

        // 리소스 로드(1회)
        for (int i = 0; i < backgrounds.Count; i++)
        {
            BackgroundItem it = backgrounds[i];

            if (!string.IsNullOrEmpty(it.spritePath))
                it.itemimg = Resources.Load<Sprite>(it.spritePath);

            if (!string.IsNullOrEmpty(it.panelPrefabPath))
                it.panel = Resources.Load<GameObject>(it.panelPrefabPath);
        }

        // 전부 잠겨있으면 첫 배경 자동 해금
        if (!backgrounds.Exists(b => b.spawncheck) && backgrounds.Count > 0)
            backgrounds[0].spawncheck = true;

        IsLoaded = true;
    }

    private void EnsureJsonExists(string persistentPath)
    {
        if (File.Exists(persistentPath))
            return;

        string streamingPath = Path.Combine(Application.streamingAssetsPath, JSON_NAME);

#if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest req = UnityWebRequest.Get(streamingPath);
        req.SendWebRequest();
        while (!req.isDone) { }

        if (!req.isNetworkError && !req.isHttpError)
            File.WriteAllText(persistentPath, req.downloadHandler.text);
#else
        if (File.Exists(streamingPath))
            File.Copy(streamingPath, persistentPath, true);
#endif
    }

    public void SaveBackground()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, JSON_NAME);
            BackgroundItemListWrapper wrapper = new BackgroundItemListWrapper { backgrounds = backgrounds };
            File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));
        }
        catch
        {
            // 저장 실패는 무시
        }
    }

    public BackgroundItem GetItem(int index)
    {
        if (index < 0 || index >= backgrounds.Count)
            return null;

        return backgrounds[index];
    }

    public int GetCount()
    {
        return backgrounds.Count;
    }
}