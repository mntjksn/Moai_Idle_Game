using UnityEngine;
using UnityEngine.UI;

// 미션 완료 여부를 감지해서
// 미션 버튼의 색상을 변경해주는 역할
public class mission_check : MonoBehaviour
{
    private Button btn;

    // GameData 캐싱
    private GameData cachedData;

    // SaveManager.Load() 호출 빈도 제한용
    private float checkInterval = 0.15f;  // 0.15초마다 갱신
    private float timer = 0f;

    private void Start()
    {
        // 버튼 캐싱
        btn = GetComponent<Button>();

        // 시작 시 1회 데이터 로드
        cachedData = SaveManager.Load();
    }

    private void Update()
    {
        // 일정 주기로만 데이터 다시 읽기 (성능 최적화)
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            cachedData = SaveManager.Load();
            timer = 0f;
        }

        var data = cachedData;

        // 하나라도 완료 가능한 미션이 있는지 체크
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

        // 미션 완료 가능 시 강조 색상
        colors.normalColor = anyComplete
            ? new Color(1f, 0.6f, 0f, 1f)  // 주황색
            : Color.white;

        btn.colors = colors;
    }
}