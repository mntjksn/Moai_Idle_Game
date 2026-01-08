using UnityEngine;

public class BackgroundCheck : MonoBehaviour
{
    private GameData data;
    private float checkInterval = 0.5f;
    private float timer = 0f;

    private Transform canvas2;

    private void Awake()
    {
        data = SaveManager.Load(); // 최초 1회만 로드
        canvas2 = GameObject.Find("Canvas2")?.transform;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval)
            return;

        timer = 0f; // 다음 체크까지 대기
        CheckAll();
    }

    private void CheckAll()
    {
        // 해금 조건
        CheckAndUnlock(1, data.background.spawn_check >= 8000);
        CheckAndUnlock(2, data.background.spawn_check >= 50000);
        CheckAndUnlock(3, data.background.merge_check >= 10000);
        CheckAndUnlock(4, data.background.merge_check >= 60000);
        CheckAndUnlock(5, data.dailyReward.rewardCheck >= 14);
        CheckAndUnlock(6, data.dailyReward.rewardCheck >= 30);
        CheckAndUnlock(7, data.clickclick.stageLevel >= 30);
        CheckAndUnlock(8, data.background.box_check >= 1000);
        CheckAndUnlock(9, data.background.lotto_check >= 1500);
    }

    private void CheckAndUnlock(int index, bool condition)
    {
        var bgItem = BackgroundManager.Instance.GetItem(index);

        if (bgItem == null || bgItem.spawncheck || !condition)
            return;

        // 해금 처리
        bgItem.spawncheck = true;
        BackgroundManager.Instance.SaveBackground();

        data.upgrades.backgroundcheck = index;
        SaveManager.Save(data);

        // 패널 생성 (한 번만)
        if (bgItem.panel != null && canvas2 != null)
            Instantiate(bgItem.panel, Vector3.zero, Quaternion.identity, canvas2);

        Debug.Log($"[BackgroundCheck] 배경 {bgItem.name} (#{index}) 해금");
    }
}