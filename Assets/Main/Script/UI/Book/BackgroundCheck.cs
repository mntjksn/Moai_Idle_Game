using UnityEngine;

// 배경 해금 조건을 주기적으로 체크하는 스크립트
// - 특정 조건 달성 시 배경 해금
// - 해금 시 패널 표시 및 데이터 저장
public class BackgroundCheck : MonoBehaviour
{
    // 세이브 데이터
    private GameData data;

    // 체크 주기(초)
    private float checkInterval = 0.5f;
    private float timer = 0f;

    // 해금 패널을 띄울 UI 캔버스
    private Transform canvas2;

    private void Awake()
    {
        // 세이브 데이터 최초 로드
        // 이후에는 같은 인스턴스를 사용
        data = SaveManager.Load();

        // 배경 해금 패널이 올라갈 캔버스
        canvas2 = GameObject.Find("Canvas2")?.transform;
    }

    private void Update()
    {
        // 일정 시간마다만 체크
        timer += Time.deltaTime;
        if (timer < checkInterval)
            return;

        timer = 0f;
        CheckAll();
    }

    // 모든 배경 해금 조건 검사
    private void CheckAll()
    {
        // 소환 횟수 기반 해금
        CheckAndUnlock(1, data.background.spawn_check >= 8000);
        CheckAndUnlock(2, data.background.spawn_check >= 50000);

        // 머지 횟수 기반 해금
        CheckAndUnlock(3, data.background.merge_check >= 10000);
        CheckAndUnlock(4, data.background.merge_check >= 60000);

        // 출석 보상 기반 해금
        CheckAndUnlock(5, data.dailyReward.rewardCheck >= 14);
        CheckAndUnlock(6, data.dailyReward.rewardCheck >= 30);

        // 스테이지 진행도 기반 해금
        CheckAndUnlock(7, data.clickclick.stageLevel >= 30);

        // 기타 시스템 기반 해금
        CheckAndUnlock(8, data.background.box_check >= 1000);
        CheckAndUnlock(9, data.background.lotto_check >= 1500);
    }

    // 개별 배경 해금 처리
    private void CheckAndUnlock(int index, bool condition)
    {
        // 배경 아이템 가져오기
        var bgItem = BackgroundManager.Instance.GetItem(index);

        // 조건 미충족 / 이미 해금 / 데이터 없음이면 종료
        if (bgItem == null || bgItem.spawncheck || !condition)
            return;

        // 배경 해금 처리
        bgItem.spawncheck = true;
        BackgroundManager.Instance.SaveBackground();

        // 최근 해금된 배경 인덱스 저장
        data.upgrades.backgroundcheck = index;
        SaveManager.Save(data);

        // 해금 패널 생성 (1회)
        if (bgItem.panel != null && canvas2 != null)
            Instantiate(bgItem.panel, Vector3.zero, Quaternion.identity, canvas2);

        Debug.Log($"[BackgroundCheck] 배경 {bgItem.name} (#{index}) 해금");
    }
}