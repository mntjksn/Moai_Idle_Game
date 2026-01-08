using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GameData
{
    public Currency currency = new Currency();        // 재화
    public ClickClick clickclick = new ClickClick();  // 클릭 관련
    public DailyReward dailyReward = new DailyReward(); // 출석 보상용
    public Settings settings = new Settings();        // 설정값
    public Upgrades upgrades = new Upgrades();        // 업그레이드
    public Background background = new Background();        // 배경
    public Shop shops = new Shop();         // 상점
    public Lotto lottos = new Lotto();      // 로또
    public Mission missions = new Mission();// 미션

    [System.Serializable]
    public class Currency
    {
        public int gold = 0;
        public int dia = 0;
        public int ticket = 0;
        public int token = 0;
    }

    [System.Serializable]
    public class ClickClick
    {
        public int stageLevel = 0;
        public int hp = 100;
        public int maxHp = 100;
        public int damage = 1;
        public int damage_check = 1;
        public int damage_upgrade = 50;
        public int rewardGold = 200;
        public int rewardDia = 5;
        public int rewardTicket = 0;
    }

    [System.Serializable]
    public class DailyReward
    {
        public string lastRewardDate = "";
        public float playTimeToday = 0f;
        public int playTimeTodayMax = 30;
        public bool rewardGivenToday = false;
        public int rewardCheck = 0;
    }

    [System.Serializable]
    public class Settings
    {
        public int childMax = 5;
        public int clickMax = 1;
        public int clickNum = 1;
        public float getGoldTime = 5.0f;
        public float spawnTime = 5.0f;
        public int ChgetGold = 0;

        // --- 자동 기능 ---
        public bool autoSpawnPurchased = false;
        public bool autoMergePurchased = false;

        public bool autoSpawnActive = false;
        public bool autoMergeActive = false;

        public float autoSpawnRemain = 0f; // 남은 시간(초)
        public float autoMergeRemain = 0f;

        public float autoSpawnCooldown = 0f;
        public float autoMergeCooldown = 0f;

        public bool autoSpawnEnabled = true;
        public bool autoMergeEnabled = true;
    }

    [System.Serializable]
    public class Upgrades
    {
        public int chprefab = 0;
        public int upCh = 0;
        public int count = 0;
        public int upgrade = 0;
        public int booknum = 0;
        public int background = 0;
        public int backgroundcheck = 0;
    }

    [System.Serializable]
    public class Background
    {
        public int spawn_check = 0;
        public int merge_check = 0;
        public int box_check = 0;
        public int lotto_check = 0;
    }

    [System.Serializable]
    public class Shop
    {
        public int shop_1_price = 100;
        public int shop_2_price = 30;
        public int shop_3_price = 40;
        public int shop_4_price = 20;

        public int shop_1_level = 1;
        public int shop_2_level = 1;
        public int shop_3_level = 1;
        public int shop_4_level = 1;

        public int tokenshop_1_price = 1;
        public int tokenshop_2_price = 500;
        public int tokenshop_3_price = 1000;
    }

        [System.Serializable]
    public class Lotto
    {
        public float lotto_1_value = 0.1f;
        public int lotto_1_reward = 2;

        public float lotto_2_value = 1.4f;
        public int lotto_2_reward = 1;

        public float lotto_3_value = 16.5f;
        public int lotto_3_reward = 100;

        public float lotto_4_value = 32f;
        public int lotto_4_reward = 2000;

        public float lotto_5_value = 50f;
        public int lotto_5_reward = 0;
    }

    [System.Serializable]
    public class Mission
    {
        public int mission_2_value = 0;
        public int mission_2_max = 1;
        public int mission_2_reward = 5;
        public int mission_2_tic = 0;

        public int mission_3_value = 0;
        public int mission_3_max = 2;
        public int mission_3_reward = 20;
        public int mission_3_tic = 0;

        public int mission_4_value = 0;
        public int mission_4_max = 2;
        public int mission_4_reward = 30;
        public int mission_4_tic = 0;

        public int mission_5_value = 0;
        public int mission_5_max = 50;
        public int mission_5_reward = 20;
        public int mission_5_tic = 0;

        public int mission_6_value = 0;
        public int mission_6_max = 1;
        public int mission_6_reward = 10;
        public int mission_6_tic = 0;

        public int mission_7_value = 0;
        public int mission_7_max = 1;
        public int mission_7_reward = 10;
        public int mission_7_tic = 0;

        public int mission_8_value = 0;
        public int mission_8_max = 1;
        public int mission_8_reward = 1;
        public int mission_8_tic = 0;
    }
}

public static class SaveManager
{
    private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

    private static GameData cachedData;   // 로드된 데이터 캐싱
    private static bool isLoaded = false;

    private const bool DebugMode = false; // 로그 제거

    public static GameData Load()
    {
        if (isLoaded)
            return cachedData;

        if (!File.Exists(FilePath))
        {
            CopyDefaultFile();
        }

        string json = File.ReadAllText(FilePath);

        if (string.IsNullOrEmpty(json))
        {
            cachedData = new GameData();
            Save(cachedData);
        }
        else
        {
            cachedData = JsonUtility.FromJson<GameData>(json);

            if (cachedData == null)
                cachedData = new GameData();
        }

        isLoaded = true;

        return cachedData;
    }

    public static void Save(GameData data)
    {
        cachedData = data;   // 캐시 업데이트

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] 저장 실패: {ex.Message}");
        }
    }

    private static void CopyDefaultFile()
    {
        string streaming = Path.Combine(Application.streamingAssetsPath, "playerdata.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest req = UnityWebRequest.Get(streaming);
        req.SendWebRequest();
        while (!req.isDone) { }

        if (req.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(FilePath, req.downloadHandler.data);
        }
        else
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(new GameData(), true));
        }
#else
        if (File.Exists(streaming))
            File.Copy(streaming, FilePath, true);
        else
            File.WriteAllText(FilePath, JsonUtility.ToJson(new GameData(), true));
#endif
    }
}