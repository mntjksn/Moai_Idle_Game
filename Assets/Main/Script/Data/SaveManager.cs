using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GameData
{
    // 전체 저장 데이터(각 기능별 데이터 묶음)
    public Currency currency = new Currency();            // 재화
    public ClickClick clickclick = new ClickClick();      // 클릭 전투 관련
    public DailyReward dailyReward = new DailyReward();   // 출석/일일 보상
    public OfflineData offline = new OfflineData();       // 오프라인 보상
    public Settings settings = new Settings();            // 설정값 및 자동 기능
    public Upgrades upgrades = new Upgrades();            // 업그레이드 관련
    public Background background = new Background();      // 배경 관련 체크
    public Shop shops = new Shop();                       // 상점
    public Lotto lottos = new Lotto();                    // 로또
    public Mission missions = new Mission();              // 미션

    [System.Serializable]
    public class Currency
    {
        // 기본 재화
        public int gold = 0;
        public int dia = 0;
        public int ticket = 0;
        public int token = 0;
    }

    [System.Serializable]
    public class ClickClick
    {
        // 스테이지 및 전투 관련
        public int stageLevel = 0;

        public int hp = 100;
        public int maxHp = 100;

        public int damage = 1;
        public int damage_check = 1;     // 데미지 계산 보정값(사용 방식 유지)
        public int damage_upgrade = 50;  // 데미지 업그레이드 비용 또는 기준값

        // 스테이지 보상
        public int rewardGold = 200;
        public int rewardDia = 5;
        public int rewardTicket = 0;
    }

    [System.Serializable]
    public class DailyReward
    {
        // 마지막 보상 받은 날짜(문자열)
        public string lastRewardDate = "";

        // 오늘 플레이 시간(분 또는 초 단위는 사용처 기준 유지)
        public float playTimeToday = 0f;
        public int playTimeTodayMax = 30;

        // 오늘 보상 지급 여부
        public bool rewardGivenToday = false;

        // 보상 단계/체크 값
        public int rewardCheck = 0;
    }

    [System.Serializable]
    public class OfflineData
    {
        // 마지막 종료/백그라운드 진입 시간(UTC ticks)
        public long lastQuitUtcTicks;

        // 마지막 저장 시점의 초당 수급량
        public double cachedGoldPerSec;
    }

    [System.Serializable]
    public class Settings
    {
        // 배치 및 클릭 관련
        public int childMax = 5;
        public int clickMax = 1;
        public int clickNum = 1;

        // 골드 획득 및 스폰 주기
        public float getGoldTime = 5.0f;
        public float spawnTime = 5.0f;

        // 캐릭터 전체 초당 골드 합산 값(UI 표시용 등)
        public int ChgetGold = 0;

        // 자동 기능 구매 여부
        public bool autoSpawnPurchased = false;
        public bool autoMergePurchased = false;

        // 자동 기능 활성 상태
        public bool autoSpawnActive = false;
        public bool autoMergeActive = false;

        // 자동 기능 남은 시간(초)
        public float autoSpawnRemain = 0f;
        public float autoMergeRemain = 0f;

        // 자동 기능 쿨타임(초)
        public float autoSpawnCooldown = 0f;
        public float autoMergeCooldown = 0f;

        // 자동 기능 사용 가능 여부(토글)
        public bool autoSpawnEnabled = true;
        public bool autoMergeEnabled = true;
    }

    [System.Serializable]
    public class Upgrades
    {
        // 업그레이드 관련 값(사용처 기준 유지)
        public int chprefab = 0;
        public int upCh = 0;
        public int count = 0;
        public int upgrade = 0;

        public int booknum = 0;

        // 배경 관련 업그레이드/체크
        public int background = 0;
        public int backgroundcheck = 0;
    }

    [System.Serializable]
    public class Background
    {
        // 각 기능 오픈 여부 또는 체크 값(사용처 기준 유지)
        public int spawn_check = 0;
        public int merge_check = 0;
        public int box_check = 0;
        public int lotto_check = 0;
    }

    [System.Serializable]
    public class Shop
    {
        // 골드 상점 가격
        public int shop_1_price = 100;
        public int shop_2_price = 30;
        public int shop_3_price = 40;
        public int shop_4_price = 20;

        // 골드 상점 레벨
        public int shop_1_level = 1;
        public int shop_2_level = 1;
        public int shop_3_level = 1;
        public int shop_4_level = 1;

        // 토큰 상점 가격
        public int tokenshop_1_price = 1;
        public int tokenshop_2_price = 500;
        public int tokenshop_3_price = 1000;
    }

    [System.Serializable]
    public class Lotto
    {
        // 로또 확률/보상(사용처 기준 유지)
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
        // 각 미션 진행도/목표/보상(사용처 기준 유지)
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
    // 저장 파일 경로
    private static readonly string FilePath =
        Path.Combine(Application.persistentDataPath, "playerdata.json");

    // 로드된 데이터 캐싱
    private static GameData cachedData;
    private static bool isLoaded = false;

    public static GameData Load()
    {
        // 이미 로드되어 있으면 캐시 반환
        if (isLoaded && cachedData != null)
            return cachedData;

        try
        {
            // 파일이 없으면 기본 파일 복사 또는 기본 데이터 생성
            if (!File.Exists(FilePath))
            {
                CopyDefaultFile();
            }

            // 파일이 여전히 없으면 새 데이터 생성
            if (!File.Exists(FilePath))
            {
                cachedData = new GameData();
                Save(cachedData);
                isLoaded = true;
                return cachedData;
            }

            // 파일 읽기
            string json = File.ReadAllText(FilePath);

            // 공백/개행만 있는 경우도 빈 데이터로 처리
            if (string.IsNullOrWhiteSpace(json))
            {
                cachedData = new GameData();
                Save(cachedData);
                isLoaded = true;
                return cachedData;
            }

            // 역직렬화 시도
            cachedData = JsonUtility.FromJson<GameData>(json);

            // 역직렬화 실패 방어
            if (cachedData == null)
            {
                cachedData = new GameData();
                Save(cachedData);
            }

            isLoaded = true;

            return cachedData;
        }
        catch (System.Exception ex)
        {
            // 파일이 깨졌거나 읽기 실패 등 예외 발생 시 복구
            Debug.LogError("[SaveManager] 로드 실패: " + ex.Message);

            cachedData = new GameData();

            // 복구 저장은 실패할 수도 있으니 별도 try-catch
            try
            {
                Save(cachedData);
            }
            catch
            {
                // 저장 실패는 여기서 더 처리하지 않음
            }

            isLoaded = true;
            return cachedData;
        }
    }

    public static void Save(GameData data)
    {
        cachedData = data;

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SaveManager] 저장 실패: " + ex.Message);
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
            if (DebugMode) Debug.Log("[SaveManager] 기본 파일 복사 완료(안드로이드)");
        }
        else
        {
            // 기본 파일을 못 읽으면 새 데이터 생성
            File.WriteAllText(FilePath, JsonUtility.ToJson(new GameData(), true));
            if (DebugMode) Debug.Log("[SaveManager] 기본 파일 복사 실패로 새 데이터 생성(안드로이드)");
        }
#else
        if (File.Exists(streaming))
        {
            File.Copy(streaming, FilePath, true);
        }
        else
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(new GameData(), true));
        }
#endif
    }
}