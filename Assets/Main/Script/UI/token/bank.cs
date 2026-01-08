using UnityEngine;
using UnityEngine.UI;

public class bank : MonoBehaviour
{
    public enum BankType { ticket, token }
    public BankType bankType;

    [SerializeField] private AudioSource audioSource;
    private Button button;

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        button = GetComponent<Button>();
    }

    public void but_event()
    {
        GameData data = SaveManager.Load();

        int dia = data.currency.dia;
        int ticket = data.currency.ticket;
        int token = data.currency.token;

        bool purchased = false;  // 구매 성공 여부
        string failMsg = "";     // 실패 메시지 저장

        switch (bankType)
        {
            case BankType.ticket:
                purchased = TryBuy(ref dia, 1000, ref ticket, 1);
                if (!purchased) failMsg = "다이아가 부족합니다!";
                break;

            case BankType.token:
                purchased = TryBuy(ref dia, 10000, ref token, 1);
                if (!purchased) failMsg = "다이아가 부족합니다!";
                break;
        }

        // 구매 실패 처리
        if (!purchased)
        {
            AppearTextManager.Instance.Show(failMsg);
            return; // 아래 저장 안 하고 종료
        }

        // 구매 성공 시 사운드 재생
        PlaySFX();

        data.currency.dia = dia;
        data.currency.ticket = ticket;
        data.currency.token = token;

        SaveManager.Save(data);
    }

    /// <summary>
    /// 통합 구매 함수
    /// required: 필요 재화
    /// give: 지급 재화
    /// </summary>
    private bool TryBuy(ref int costType, int required, ref int rewardType, int give)
    {
        if (costType < required)
            return false;

        costType -= required;
        rewardType += give;
        return true;
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }
}