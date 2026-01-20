using System;
using UnityEngine;

// 오프라인 보상 계산 + 지급을 담당하는 시스템
public class OfflineRewardSystem : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;  // 오프라인 보상 수령 효과음

    public static OfflineRewardSystem Instance;

    [Header("Offline Settings")]
    [SerializeField] public int maxOfflineHours = 5;   // 최대 누적 시간(시간)

    [Header("Minute-based Rewards")]
    [SerializeField] public int diaPerMinutes = 2;     // 2분당 다이아 1개
    [SerializeField] public int ticketPerMinutes = 30; // 30분당 티켓 1개

    [Header("Rate Settings (0.5 -> 0.05)")]
    [SerializeField] public bool useCachedTickGold = true; // 저장된 캐시값(틱 골드) 사용 여부
    [SerializeField] public float maxRate = 0.5f;          // 초반 배율
    [SerializeField] public float minRate = 0.025f;        // 후반 배율
    [SerializeField] public float pivotGps = 50f;          // 감쇠 시작점(GPS 기준)
    [SerializeField] public float maxGps = 5000f;          // 이 값에 가까워질수록 minRate에 수렴

    [Header("Gold Cap")]
    [SerializeField] public long offlineGoldCap = 10_000_000; // 오프라인 골드 상한

    [Header("Debug")]
    [SerializeField] public bool debugLog = false;

    // 현재 오프라인 보상이 있는지 여부
    public bool hasPendingReward { get; private set; }

    // 계산된 오프라인 보상 결과(팝업에서 읽음)
    public OfflineRewardResult pending { get; private set; }

    // 배치된 캐릭터들이 들어있는 부모(태그 chp)
    private Transform chp;

    [Serializable]
    public struct OfflineRewardResult
    {
        public double elapsedSeconds;   // 실제 경과 시간(초)
        public double usedSeconds;      // 계산에 반영된 시간(초) - 분 단위로 정규화됨
        public float tickInterval;      // getGoldTime(초)
        public long ticks;              // 반영된 틱 수

        public double goldPerTick;      // 틱당 골드 합
        public float goldPerSec;        // 초당 골드(GPS)
        public float rateApplied;       // 적용된 오프라인 배율

        public long goldReward;         // 지급할 골드
        public int usedMinutes;         // 반영된 시간(분)
        public int diaReward;           // 지급할 다이아
        public int ticketReward;        // 지급할 티켓
    }

    private void Awake()
    {
        // 싱글톤 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // chp 참조가 끊겼을 수 있으니 필요할 때만 찾아서 캐싱
    private void EnsureRefs()
    {
        if (chp == null)
            chp = GameObject.FindGameObjectWithTag("chp")?.transform;
    }

    // 초당 골드(GPS)에 따라 오프라인 배율(초반 maxRate -> 후반 minRate)을 계산
    // gps가 커질수록 배율이 줄어드는 구조(후반 인플레 억제)
    public float GetOfflineRate(float gps)
    {
        float MAX_RATE = Mathf.Clamp(maxRate, 0f, 1f);
        float MIN_RATE = Mathf.Clamp(minRate, 0f, 1f);

        float PIVOT = Mathf.Max(1f, pivotGps);
        float MAX_GPS = Mathf.Max(PIVOT + 1f, maxGps);

        // 로그 스케일로 완만하게 감쇠
        float t = Mathf.Log10(1f + gps / PIVOT) / Mathf.Log10(1f + MAX_GPS / PIVOT);
        t = Mathf.Clamp01(t);

        return Mathf.Lerp(MAX_RATE, MIN_RATE, t);
    }

    // 현재 배치된 캐릭터 기준 "틱당 골드 합" 계산(업그레이드 2배 반영)
    public double CalculateGoldPerTick()
    {
        EnsureRefs();

        // 배치 부모가 없으면 계산 불가
        if (chp == null)
            return 0;

        // CharacterManager 준비 전이면 안전하게 0 처리
        if (CharacterManager.Instance == null)
            return 0;

        double sum = 0;
        int count = chp.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);
            if (!child.gameObject.activeSelf)
                continue;

            MergeItem mi = child.GetComponent<MergeItem>();
            if (mi == null)
                continue;

            // 아이템 데이터 조회
            var itemData = CharacterManager.Instance.GetItem(mi.iN);
            if (itemData == null)
                continue;

            int baseGold = itemData.itemgold;

            // 업그레이드 시 2배(현재는 MergeItem.UC를 사용하고 있음)
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

        // 첫 실행이면 기준 시간만 세팅하고 종료
        if (data.offline.lastQuitUtcTicks <= 0)
        {
            data.offline.lastQuitUtcTicks = DateTime.UtcNow.Ticks;
            data.offline.cachedGoldPerSec = CalculateGoldPerTick(); // 변수명은 cachedGoldPerSec지만 실사용은 "틱당 골드"로 쓰고 있음
            SaveManager.Save(data);
            return;
        }

        // 실제 경과 시간(초)
        double elapsedSeconds =
            (DateTime.UtcNow - new DateTime(data.offline.lastQuitUtcTicks, DateTimeKind.Utc)).TotalSeconds;

        // 최소 1분 이상일 때만 보상
        if (elapsedSeconds < 60)
            return;

        // 최대 누적 시간 캡 적용
        double capSeconds = maxOfflineHours * 3600.0;
        double cappedSeconds = Math.Min(elapsedSeconds, capSeconds);

        // UI/계산 정합을 위해 "분" 단위로 내림(정수 분만 반영)
        int usedMinutes = (int)Math.Floor(cappedSeconds / 60.0);
        double usedSeconds = usedMinutes * 60.0;

        // 틱 간격(getGoldTime) 방어(0이면 분모 문제)
        float tickInterval = Mathf.Max(0.1f, data.settings.getGoldTime);

        // 반영할 틱 수(정수)
        long ticks = (long)Math.Floor(usedSeconds / tickInterval);

        // 틱당 골드(캐시값 or 실시간 계산)
        double goldPerTick = useCachedTickGold ? data.offline.cachedGoldPerSec : CalculateGoldPerTick();

        // 캐시가 0이거나 비정상일 경우 실시간 계산으로 보정
        if (goldPerTick <= 0)
            goldPerTick = CalculateGoldPerTick();

        // GPS(초당 골드)로 변환해서 배율 계산
        float gps = (float)(goldPerTick / tickInterval);
        float rate = GetOfflineRate(gps);

        // 최종 골드 보상
        long goldReward = (long)Math.Floor(ticks * goldPerTick * rate);

        // 오프라인 골드 상한 적용
        if (goldReward > offlineGoldCap)
            goldReward = offlineGoldCap;

        // 분 단위 보상(정수 분 기반)
        int diaReward = (diaPerMinutes > 0) ? (usedMinutes / diaPerMinutes) : 0;
        int ticketReward = (ticketPerMinutes > 0) ? (usedMinutes / ticketPerMinutes) : 0;

        // 결과 저장(팝업에서 표시)
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

        // 하나라도 보상이 있으면 팝업 대상
        if (goldReward > 0 || diaReward > 0 || ticketReward > 0)
            hasPendingReward = true;
    }

    // 보상 수령(팝업 확인 버튼에서 호출)
    public void ClaimPending()
    {
        // 받을 보상이 없으면 아무것도 하지 않음
        if (!hasPendingReward)
            return;

        // 수령 시점에만 효과음 재생
        PlaySFX();

        var data = SaveManager.Load();

        // 골드 int 범위 보호
        long addGold = pending.goldReward;
        if (addGold < 0) addGold = 0;

        long newGold = (long)data.currency.gold + addGold;
        if (newGold > int.MaxValue)
            newGold = int.MaxValue;

        data.currency.gold = (int)newGold;
        data.currency.dia += pending.diaReward;
        data.currency.ticket += pending.ticketReward;

        // 중복 수령 방지 + 기준 갱신
        data.offline.lastQuitUtcTicks = DateTime.UtcNow.Ticks;
        data.offline.cachedGoldPerSec = CalculateGoldPerTick();

        SaveManager.Save(data);

        // 상태 초기화
        hasPendingReward = false;
        pending = default;
    }

    // 앱 종료/백그라운드 진입 시점 저장(오프라인 기준 시간 갱신)
    public void SaveOfflineSnapshot()
    {
        var data = SaveManager.Load();

        data.offline.lastQuitUtcTicks = DateTime.UtcNow.Ticks;
        data.offline.cachedGoldPerSec = CalculateGoldPerTick();

        SaveManager.Save(data);

        if (debugLog)
            Debug.Log("[OFFLINE] Snapshot saved. tickGold=" + data.offline.cachedGoldPerSec);
    }

    // 백그라운드 진입 시 스냅샷 저장
    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveOfflineSnapshot();
    }

    // 앱 종료 시 스냅샷 저장
    private void OnApplicationQuit()
    {
        SaveOfflineSnapshot();
    }

    // 오프라인 보상 수령 효과음
    private void PlaySFX()
    {
        if (audioSource == null)
            return;

        if (Setting.IsSFXOn())
            audioSource.Play();
    }
}