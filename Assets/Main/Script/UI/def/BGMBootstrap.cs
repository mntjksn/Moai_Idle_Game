using UnityEngine;

// BGM 재생 상태를 관리하는 부트스트랩 스크립트
// - PlayerPrefs에 저장된 BGM On/Off 값을 읽어 초기 상태 적용
// - 정적 메서드를 통해 어디서든 BGM 제어 가능
public class BGMBootstrap : MonoBehaviour
{
    // 실제 BGM을 재생하는 AudioSource
    private static AudioSource src;

    // BGM On/Off 상태 저장 키
    private const string PREF_BGM = "BGMOnOff";

    private void Awake()
    {
        // 싱글톤 설정
        if (src != null)
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 캐싱
        src = GetComponent<AudioSource>();
        if (src == null)
            return;

        // 저장된 BGM 설정 불러오기 (기본값: On)
        bool on = PlayerPrefs.GetInt(PREF_BGM, 1) == 1;

        // 초기 상태 적용
        Apply(on);
    }

    // 외부에서 BGM On/Off 설정
    public static void SetBGM(bool on)
    {
        if (src == null)
            return;

        // 설정 저장
        PlayerPrefs.SetInt(PREF_BGM, on ? 1 : 0);
        PlayerPrefs.Save();

        // 즉시 반영
        Apply(on);
    }

    // 실제 AudioSource에 상태 적용
    private static void Apply(bool on)
    {
        if (src == null)
            return;

        if (on)
        {
            // 음소거 해제 및 재생
            src.mute = false;

            if (!src.isPlaying && src.clip != null)
                src.Play();
        }
        else
        {
            // 음소거 + 재생 중지
            src.mute = true;
            src.Stop();
        }
    }
}