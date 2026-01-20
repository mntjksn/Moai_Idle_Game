using UnityEngine;

// 도감 UI에서 캐릭터 도감 / 배경 도감 패널 전환을 담당
public class BookChange : MonoBehaviour
{
    // 캐릭터 도감 패널
    public GameObject ch_panel;

    // 배경 도감 패널
    public GameObject bg_panel;

    private void OnEnable()
    {
        // 도감 화면이 열릴 때 기본 상태
        // 캐릭터 도감 ON, 배경 도감 OFF
        ch_panel.SetActive(true);
        bg_panel.SetActive(false);
    }

    // 도감 탭 버튼 클릭 시 호출
    // name 값에 따라 패널 전환
    public void ClickBook(string name)
    {
        if (name == "ch")
        {
            // 캐릭터 도감 선택
            ch_panel.SetActive(true);
            bg_panel.SetActive(false);
        }
        else if (name == "bg")
        {
            // 배경 도감 선택
            ch_panel.SetActive(false);
            bg_panel.SetActive(true);
        }
        else
        {
            // 정의되지 않은 값은 무시
            return;
        }
    }
}