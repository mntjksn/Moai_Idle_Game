using TMPro;
using UnityEngine;

// 로또 확률 및 보상 정보를 UI 텍스트로 표시하는 뷰어
// - 버튼이나 계산 로직 없음
// - GameData에 저장된 값만 읽어서 보여주는 역할
public class lottodataviewer : MonoBehaviour
{
    // 확률 표시 텍스트
    public TextMeshProUGUI text_value;

    // 보상 내용 표시 텍스트
    public TextMeshProUGUI text_reward;

    private void OnEnable()
    {
        // 패널이 열릴 때마다 최신 데이터로 갱신
        Refresh();
    }

    // 로또 확률 / 보상 정보 갱신
    private void Refresh()
    {
        GameData data = SaveManager.Load();

        // 각 등수별 확률 표시
        // (소수점 1자리까지 표기)
        if (text_value != null)
        {
            text_value.text =
                $"1등 확률 : {data.lottos.lotto_1_value:F1}%\n" +
                $"2등 확률 : {data.lottos.lotto_2_value:F1}%\n" +
                $"3등 확률 : {data.lottos.lotto_3_value:F1}%\n" +
                $"4등 확률 : {data.lottos.lotto_4_value:F1}%\n" +
                $"5등 확률 : {data.lottos.lotto_5_value:F1}%";
        }

        // 각 등수별 보상 표시
        if (text_reward != null)
        {
            text_reward.text =
                $"***** 1등 : 토큰 {data.lottos.lotto_1_reward:N0}개 *****\n" +
                $"*** 2등 : 토큰 {data.lottos.lotto_2_reward:N0}개 ***\n" +
                $"** 3등 : 다이아 {data.lottos.lotto_3_reward:N0}개 **\n" +
                $"* 4등 : 돌멩이 {data.lottos.lotto_4_reward:N0}개 *\n" +
                $"5등 : 꽝";
        }
    }
}