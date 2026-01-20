using UnityEngine;

// 게임 종료 버튼 이벤트 처리용 스크립트
public class End : MonoBehaviour
{
    // 종료 버튼 클릭 시 호출
    public void but_event()
    {
#if UNITY_EDITOR
        // 에디터 실행 중일 경우 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드 환경에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}