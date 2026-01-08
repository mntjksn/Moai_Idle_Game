using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Item2
{
    public string name;
    public string sub;
    public string sub2;
    public string spritePath;
    public string panelPrefabPath;

    [NonSerialized] public Sprite itemimg;
    [NonSerialized] public GameObject panel;

    public bool spawncheck;
}

[Serializable]
public class Item2ListWrapper { public List<Item2> items2; }


public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    public List<Item2> backgrounds = new List<Item2>();

    public int SelectedIndex { get; private set; } = 0;

    public event Action<int> OnBackgroundSelected;

    const string KEY_BG_INDEX = "bg_index";
    const string JSON_NAME = "BackgroundData.json";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBackground();

        SelectedIndex = Mathf.Clamp(
            PlayerPrefs.GetInt(KEY_BG_INDEX, 0),
            0,
            Mathf.Max(0, backgrounds.Count - 1)
        );

        if (!IsUnlocked(SelectedIndex))
            SelectedIndex = FirstUnlockedIndex();

        OnBackgroundSelected?.Invoke(SelectedIndex);
    }

    public void SelectBackground(int index)
    {
        if (index < 0 || index >= backgrounds.Count || !IsUnlocked(index))
            return;

        SelectedIndex = index;
        PlayerPrefs.SetInt(KEY_BG_INDEX, SelectedIndex);
        PlayerPrefs.Save();

        SaveBackground();
        OnBackgroundSelected?.Invoke(index);
    }

    public bool IsUnlocked(int index)
        => index >= 0 && index < backgrounds.Count && backgrounds[index].spawncheck;

    public void UnlockBackground(int index, bool selectIfNone = false)
    {
        if (index < 0 || index >= backgrounds.Count) return;

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
            if (backgrounds[i].spawncheck)
                return i;

        return 0;
    }

    private void LoadBackground()
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, JSON_NAME);

        // JSON이 없으면 StreamingAssets → persistentDataPath로 복사
        if (!File.Exists(persistentPath))
        {
            string streaming = Path.Combine(Application.streamingAssetsPath, JSON_NAME);

#if UNITY_ANDROID && !UNITY_EDITOR
            UnityWebRequest req = UnityWebRequest.Get(streaming);
            req.SendWebRequest();
            while (!req.isDone) { }

            if (!req.isHttpError && !req.isNetworkError)
                File.WriteAllText(persistentPath, req.downloadHandler.text);
#else
            if (File.Exists(streaming))
                File.Copy(streaming, persistentPath, true);
#endif
        }

        // JSON 파일 로드
        string json = File.ReadAllText(persistentPath).TrimStart();

        Item2ListWrapper wrapper;

        try
        {
            if (json.StartsWith("["))
                wrapper = JsonUtility.FromJson<Item2ListWrapper>("{\"items2\":" + json + "}");
            else
                wrapper = JsonUtility.FromJson<Item2ListWrapper>(json);

            if (wrapper?.items2 == null)
            {
                backgrounds = new List<Item2>();
                return;
            }

            backgrounds = wrapper.items2;

            // 리소스 로드를 한 번만 수행
            foreach (var it in backgrounds)
            {
                if (!string.IsNullOrEmpty(it.spritePath))
                    it.itemimg = Resources.Load<Sprite>(it.spritePath);

                if (!string.IsNullOrEmpty(it.panelPrefabPath))
                    it.panel = Resources.Load<GameObject>(it.panelPrefabPath);
            }

            // 첫 배경 자동 해금
            if (!backgrounds.Exists(b => b.spawncheck) && backgrounds.Count > 0)
                backgrounds[0].spawncheck = true;
        }
        catch
        {
            backgrounds = new List<Item2>();
        }
    }

    public void SaveBackground()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, JSON_NAME);
            Item2ListWrapper wrapper = new Item2ListWrapper { items2 = backgrounds };
            File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));
        }
        catch { }
    }

    public Item2 GetItem(int index)
    {
        if (index < 0 || index >= backgrounds.Count)
            return null;

        return backgrounds[index];
    }

    public int GetCount() => backgrounds.Count;
}