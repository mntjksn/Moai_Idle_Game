using UnityEngine;

// 상점 탭(골드 / 다이아 / 토큰) 전환을 담당하는 스크립트
public class ShopChange : MonoBehaviour
{
    // 각 상점 패널
    public GameObject gold_panel;
    public GameObject dia_panel;
    public GameObject token_panel;

    // 패널이 활성화될 때 기본 탭 설정
    private void OnEnable()
    {
        // 기본은 골드 상점
        SetActivePanel(gold_panel);
    }

    // 버튼 클릭으로 상점 탭 전환
    // name 값은 버튼에서 문자열로 전달됨 ("gold", "dia", "token")
    public void ClickShop(string name)
    {
        switch (name)
        {
            case "gold":
                SetActivePanel(gold_panel);
                break;

            case "dia":
                SetActivePanel(dia_panel);
                break;

            case "token":
                SetActivePanel(token_panel);
                break;
        }
    }

    // 하나만 켜고 나머지는 끄는 공통 처리
    private void SetActivePanel(GameObject active)
    {
        if (gold_panel != null)
            gold_panel.SetActive(active == gold_panel);

        if (dia_panel != null)
            dia_panel.SetActive(active == dia_panel);

        if (token_panel != null)
            token_panel.SetActive(active == token_panel);
    }
}