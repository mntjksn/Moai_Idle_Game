using System;
using UnityEngine;

public class OfflineRewardSystem : MonoBehaviour
{
    public static OfflineRewardSystem Instance;

    [Header("Offline Settings")]
    [SerializeField] public int maxOfflineHours = 12;          // 최대 누적 시간(시간)

    [Header("Minute-based Rewards")]
    [SerializeField] public int diaPerMinutes = 2;             // 2분당 다이아 1개
    [SerializeField] public int ticketPerMinutes = 30;         // 30분당 티켓 1개

    [Header("Rate Settings (0.5 -> 0.05)")]
    [SerializeField] public bool useCachedTickGold = true;     // cachedGoldPerSec 사용(틱당합 캐시)
    [SerializeField] public float maxRate = 0.50f;             // 초반 배율
    [SerializeField] public float minRate = 0.05f;             // 극후반 배율
    [SerializeField] public float pivotGps = 3000f;            // 감쇠 시작점(초중반 튜닝)
    [SerializeField] public float maxGps = 300000f;            // 극후반 기준(여기서 minRate 수렴)

    [Header("Gold Cap")]
    [SerializeField] public long offlineGoldCap = 5_000_000;   // 오프라인 골드 상한(500만)

    [Header("Debug")]
    [SerializeField] public bool debugLog = false;

    // 상태
    public bool hasPendingReward { get; private set; }
    public OfflineRewardResult pending { get; private set; }

    private Transform chp;

    [Serializable]
    public struct OfflineRewardResult
    {
        public double elapsedSeconds;   // 실제 경과(참고용)
        public double usedSeconds;      // 계산에 사용된 시간(분단위로 정규화)
        public float tickInterval;
        public long ticks;

        public double goldPerTick;
        public float goldPerSec;
        public float rateApplied;

        public long goldReward;
        public int usedMinutes;         // 분 단위 사용 시간
        public int diaReward;
        public int ticketReward;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void EnsureRefs()
    {
        if (chp == null) chp = GameObject.FindGameObjectWithTag("chp")?.transform;
    }

    // 초당 골드(GPS)에 따라 오프라인 배율(0.5 -> 0.05)을 계산
    public float GetOfflineRate(float gps)
    {
        float MAX_RATE = Mathf.Clamp(maxRate, 0f, 1f);
        float MIN_RATE = Mathf.Clamp(minRate, 0f, 1f);

        float PIVOT = Mathf.Max(1f, pivotGps);
        float MAX_GPS = Mathf.Max(PIVOT + 1f, maxGps);

        float t = Mathf.Log10(1f + gps / PIVOT) / Mathf.Log10(1f + MAX_GPS / PIVOT);
        t = Mathf.Clamp01(t);

        return Mathf.Lerp(MAX_RATE, MIN_RATE, t);
    }

    // 현재 배치된 캐릭터들 기준 "틱당 골드 합" 계산 (업그레이드 2배 반영)
    public double CalculateGoldPerTick()
    {
        EnsureRefs();
        if (chp == null) return 0;

        double sum = 0;
        int count = chp.childCount;

        for (int i = 0; i < count; i++)
        {
            var child = chp.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            var mi = child.GetComponent<MergeItem>();
            if (mi == null) continue;

            var itemData = CharacterManager.Instance.GetItem(mi.iN);
            if (itemData == null) continue;

            int baseGold = itemData.itemgold;
            int earned = mi.UC ? baseGold * 2 : baseGold;

            sum += earned;
        }

        return sum;
    }

    // 오프라인 보상 계산(팝업 띄우기 전에 1회 호출)
    public void ComputePending()
    {
        hasPendingReward = false;

        var data = SaveManager.Load();

        // 첫 실행이면 기준만 세팅
        if (data.offline.lastQuitUtcTicks <= 0)
        {
            data.offline.lastQuitUtcTicks = DateTime.UtcNow.Ticks;
            data.offline.cachedGoldPerSec = CalculateGoldPerTick();
            SaveManager.Save(data);
            return;
        }

        // 실제 경과 시간(초)
        double elapsedSeconds =
            (DateTime.UtcNow - new DateTime(data.offline.lastQuitUtcTicks, DateTimeKind.Utc)).TotalSeconds;

        // 최소 1분
        if (elapsedSeconds < 60) return;

        // 캡(최대 12시간)
        double capSeconds = maxOfflineHours * 3600.0;
        double cappedSeconds = Math.Min(elapsedSeconds, capSeconds);

        // UI/계산 정합: 분 단위로 내림
        int usedMinutes = (int)Math.Floor(cappedSeconds / 60.0);
        double usedSeconds = usedMinutes * 60.0;

        float tickInterval = Mathf.Max(0.1f, data.settings.getGoldTime);
        long ticks = (long)Math.Floor(usedSeconds / tickInterval);

        // tick당 골드(캐시 or 실시간)
        double goldPerTick = useCachedTickGold ? data.offline.cachedGoldPerSec : CalculateGoldPerTick();
        if (goldPerTick <= 0) goldPerTick = CalculateGoldPerTick();

        // getGoldTime 업그레이드 반영: 초당 골드(GPS)로 배율 계산
        float gps = (float)(goldPerTick / tickInterval);
        float rate = GetOfflineRate(gps);

        long goldReward = (long)Math.Floor(ticks * goldPerTick * rate);

        // 오프라인 골드 상한(500만)
        if (goldReward > offlineGoldCap) goldReward = offlineGoldCap;

        // 분 단위 보상
        int diaReward = (diaPerMinutes > 0) ? (usedMinutes / diaPerMinutes) : 0;
        int ticketReward = (ticketPerMinutes > 0) ? (usedMinutes / ticketPerMinutes) : 0;

        pending = new OfflineRewardResult
        {
            elapsedSeconds = elapsedSeconds,
            usedSeconds = usedSeconds,
            tickInterval = tickInterval,
            ticks = ticks,
            goldPerTick = goldPerTick,
            goldPerSec = gps,
            rateApplied = rate,
            goldReward = goldReward,
            usedMinutes = usedMinutes,
            diaReward = diaReward,
            ticketReward = ticketReward
        };

        if (debugLog)
        {
            Debug.Log(
                "[OFFLINE] usedMin=" + usedMinutes +
                ", ticks=" + ticks +
                ", goldPerTick=" + goldPerTick +
                ", gps=" + gps.ToString("F1") +
                ", rate=" + rate.ToString("F3") +
                ", gold=" + goldReward +
                ", dia=" + diaReward +
                ", ticket=" + ticketReward
            );
        }

        if (goldReward > 0 || diaReward > 0 || ticketReward > 0)
            hasPendingReward = true;
    }

    // 보상 수령(확인 버튼에서 호출)
    public void ClaimPending()
    {
        if (!hasPendingReward) return;

        var data = SaveManager.Load();

        // 골드 int 범위 보호
        long addGold = pending.goldReward;
        if (addGold < 0) addGold = 0;

        long newGold = (long)data.currency.gold + addGold;
        if (newGold > int.MaxValue) newGold = int.MaxValue;

        data.currency.gold = (int)newGold;
        data.currency.dia += pending.diaReward;
        data.currency.ticket += pending.ticketReward;

        // 중복 수령 방지 및 기준 갱신
        data.offline.lastQuitUtcTicks = DateTime.UtcNow.Ticks;
        data.offline.cachedGoldPerSec = CalculateGoldPerTick();

        SaveManager.Save(data);

        // 상태 초기화
        hasPendingReward = false;
        pending = default;
    }

    // 앱 종료/백그라운드 진입 시점에 호출해두면 오프라인이 정확해짐
    public void SaveOfflineSnapshot()
    {
        var data = SaveManager.Load();
        data.offline.lastQuitUtcTicks = DateTime.UtcNow.Ticks;
        data.offline.cachedGoldPerSec = CalculateGoldPerTick();
        SaveManager.Save(data);

        if (debugLog)
            Debug.Log("[OFFLINE] Snapshot saved. tickGold=" + data.offline.cachedGoldPerSec);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveOfflineSnapshot();
    }

    private void OnApplicationQuit()
    {
        SaveOfflineSnapshot();
    }
}