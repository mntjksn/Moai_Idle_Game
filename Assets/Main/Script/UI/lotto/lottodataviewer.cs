using System.Xml;
using TMPro;
using UnityEngine;

public class lottodataviewer : MonoBehaviour
{
    public TextMeshProUGUI text_value, text_reward;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        GameData data = SaveManager.Load();

        text_value.text =
            $"1등 확률 : {data.lottos.lotto_1_value:F1}%\n" +
            $"2등 확률 : {data.lottos.lotto_2_value:F1}%\n" +
            $"3등 확률 : {data.lottos.lotto_3_value:F1}%\n" +
            $"4등 확률 : {data.lottos.lotto_4_value:F1}%\n" +
            $"5등 확률 : {data.lottos.lotto_5_value:F1}%";

        text_reward.text =
            $"***** 1등 : 토큰 {data.lottos.lotto_1_reward:N0}개 *****\n" +
            $"*** 2등 : 토큰 {data.lottos.lotto_2_reward:N0}개 ***\n" +
            $"** 3등 : 다이아 {data.lottos.lotto_3_reward:N0}개 **\n" +
            $"* 4등 : 돌멩이 {data.lottos.lotto_4_reward:N0}개 *\n" +
            $"5등 : 꽝";
    }
}