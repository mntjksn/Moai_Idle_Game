using UnityEngine;
using UnityEngine.UI;

public class mission_check : MonoBehaviour
{
    private Button btn;

    // 캐싱용
    private GameData cachedData;
    private float checkInterval = 0.15f;  // 0.15초마다 데이터 새로 읽기
    private float timer = 0f;

    void Start()
    {
        btn = GetComponent<Button>();
        cachedData = SaveManager.Load();   // 시작 시 1회 로드
    }

    void Update()
    {
        // 일정 주기로만 SaveManager.Load() 수행
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            cachedData = SaveManager.Load();
            timer = 0f;
        }

        var data = cachedData;

        // 미션 완료 여부 판단
        bool anyComplete =
            data.missions.mission_2_value >= data.missions.mission_2_max ||
            data.missions.mission_3_value >= data.missions.mission_3_max ||
            data.missions.mission_4_value >= data.missions.mission_4_max ||
            data.missions.mission_5_value >= data.missions.mission_5_max ||
            data.missions.mission_6_value >= data.missions.mission_6_max ||
            data.missions.mission_7_value >= data.missions.mission_7_max ||
            data.missions.mission_8_value >= data.missions.mission_8_max;

        // 버튼 색상 변경
        var colors = btn.colors;
        colors.normalColor = anyComplete
            ? new Color(1f, 0.6f, 0f, 1f)  // 주황
            : Color.white;

        btn.colors = colors;
    }
}