using UnityEngine;

// 캐릭터 관련 오브젝트의 부모 역할을 하는 유지용 오브젝트
// 씬이 변경되어도 하나만 존재하도록 관리
public class chparent : MonoBehaviour
{
    private void Awake()
    {
        // 동일한 chparent가 여러 개 생성되는 것을 방지
        chparent[] objs = FindObjectsOfType<chparent>();

        if (objs.Length == 1)
        {
            // 최초 생성된 오브젝트는 씬 전환 시 유지
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 존재하는 경우 자신은 제거
            Destroy(gameObject);
            return;
        }
    }
}