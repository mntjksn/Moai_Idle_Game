using UnityEngine;

// 버튼 클릭에 따라 패널을 표시 / 숨김 / 제거하는 간단한 UI 이벤트 스크립트
public class btnEvent : MonoBehaviour
{
    // 제어할 패널 오브젝트
    public GameObject panel;

    // 패널 활성화
    public void panel_show()
    {
        if (panel == null)
            return;

        panel.SetActive(true);
    }

    // 패널 비활성화
    public void panel_off()
    {
        if (panel == null)
            return;

        panel.SetActive(false);
    }

    // 패널 오브젝트 완전 제거
    public void destroy_panel()
    {
        if (panel == null)
            return;

        Destroy(panel);
    }
}