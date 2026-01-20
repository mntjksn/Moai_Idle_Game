using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 로또 버튼 클릭 시
// - 티켓 소모
// - 확률에 따라 보상 지급
// - 결과 패널 표시
public class lottobutton : MonoBehaviour
{
    [Header("Result UI")]
    public GameObject result_panel;        // 결과 패널
    public TextMeshProUGUI title;           // 당첨 등수 텍스트
    public TextMeshProUGUI res;             // 보상 내용 텍스트
    public TextMeshProUGUI sub;             // 추가 메시지 텍스트

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource; // 로또 실행 효과음

    private void Awake()
    {
        // 멀티 터치 방지 (중복 클릭 방지 목적)
        Input.multiTouchEnabled = false;
    }

    // 결과 패널 닫기 + 버튼 다시 활성화
    public void text()
    {
        if (result_panel != null)
            result_panel.SetActive(false);

        // 다시 로또 버튼 클릭 가능하게
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = true;
    }

    // 로또 버튼 클릭 처리
    public void lotto_btn()
    {
        GameData data = SaveManager.Load();

        // 티켓 부족 시 실행 불가
        if (data.currency.ticket < 1)
        {
            if (AppearTextManager.Instance != null)
                AppearTextManager.Instance.Show("티켓이 부족합니다!");
            return;
        }

        // 효과음 재생
        if (audioSource != null && Setting.IsSFXOn())
            audioSource.Play();

        // 티켓 소모
        data.currency.ticket--;

        // 배경 해금 체크용 카운트 증가 (상한 유지)
        if (data.background.lotto_check <= 1500)
            data.background.lotto_check++;

        SaveManager.Save(data);

        // 실제 로또 결과 계산 및 보상 지급
        PlayLotto(data);

        // 버튼 연타 방지
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;

        // 일정 시간 후 다시 버튼 활성화
        Invoke(nameof(text), 1f);
    }

    // 로또 확률 계산 및 보상 처리
    private void PlayLotto(GameData data)
    {
        // 로또 관련 미션 카운트 증가
        data.missions.mission_6_value++;

        // 각 등수별 확률 값
        float p1 = data.lottos.lotto_1_value;
        float p2 = data.lottos.lotto_2_value;
        float p3 = data.lottos.lotto_3_value;
        float p4 = data.lottos.lotto_4_value;
        float p5 = data.lottos.lotto_5_value;

        // 전체 확률 합
        float total = p1 + p2 + p3 + p4 + p5;

        // 0 ~ total 범위 랜덤 값
        float rand = Random.Range(0f, total);

        string t; // 타이틀
        string r; // 결과 텍스트
        string s; // 서브 메시지

        // 1등
        if (rand < p1)
        {
            data.currency.token += data.lottos.lotto_1_reward;
            t = "***** 1등 당첨 *****";
            r = "토큰 " + data.lottos.lotto_1_reward + "개";
            s = "짝짝짝짝짝짝짝짝짝짝";
        }
        // 2등
        else if (rand < p1 + p2)
        {
            data.currency.token += data.lottos.lotto_2_reward;
            t = "*** 2등 당첨 ***";
            r = "토큰 " + data.lottos.lotto_2_reward + "개";
            s = "아쉬운데~";
        }
        // 3등
        else if (rand < p1 + p2 + p3)
        {
            data.currency.dia += data.lottos.lotto_3_reward;
            t = "** 3등 당첨 **";
            r = "다이아 " + data.lottos.lotto_3_reward + "개";
            s = "만족하시나요?";
        }
        // 4등
        else if (rand < p1 + p2 + p3 + p4)
        {
            data.currency.gold += data.lottos.lotto_4_reward;
            t = "* 4등 당첨 *";
            r = "돌멩이 " + data.lottos.lotto_4_reward.ToString("N0") + "개";
            s = "티끌 모아 태산";
        }
        // 5등(꽝)
        else
        {
            t = "5등 당첨";
            r = "꽝";
            s = "한 번 더 도전?!";
        }

        // 골드 상한 방지
        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        // 결과 UI 표시
        if (result_panel != null)
            result_panel.SetActive(true);

        if (title != null) title.text = t;
        if (res != null) res.text = r;
        if (sub != null) sub.text = s;
    }
}