using UnityEngine;

// 오브젝트가 화면 안에서 랜덤 방향으로 천천히 떠다니도록 이동
// - 이동 ON/OFF는 Setting.IsMOVEOn()으로 제어
// - 화면 밖으로 나가면 부드럽게 화면 안으로 보정
public class move : MonoBehaviour
{
    // 컴포넌트 참조
    private Rigidbody2D rigid;
    private SpriteRenderer sprite;
    private Camera cam;

    // 현재 이동 속도
    private float xSpeed;
    private float ySpeed;

    // 화면 내 허용 범위(뷰포트 좌표 기준 0~1)
    private const float xMin = 0.075f;
    private const float xMax = 0.925f;
    private const float yMin = 0.195f;
    private const float yMax = 0.75f;

    // 일정 시간마다 이동 방향 재계산
    private float thinkTimer = 0f;
    private float thinkInterval = 5f;

    // 화면 밖 보정(Clamp) 체크 주기
    private float clampTimer = 0f;
    private const float clampInterval = 0.02f;

    private void Awake()
    {
        // 컴포넌트 캐싱
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        // 초기 이동 방향 생성
        GenerateMoveVector();
    }

    private void Update()
    {
        // 일정 주기로 화면 밖 보정 실행
        clampTimer += Time.deltaTime;
        if (clampTimer >= clampInterval)
        {
            clampTimer = 0f;

            // 이동이 꺼져도 화면 밖으로 나가면 안 되므로 항상 실행
            SmoothClampToScreen();
        }

        // 이동 옵션이 꺼져 있으면 이동만 멈추고 종료
        if (!Setting.IsMOVEOn())
        {
            rigid.velocity = Vector2.zero;
            return;
        }

        // 이동 ON 상태일 때 속도 적용
        rigid.velocity = new Vector2(xSpeed, ySpeed);

        // 일정 시간마다 이동 방향을 새로 뽑음
        thinkTimer += Time.deltaTime;
        if (thinkTimer >= thinkInterval)
        {
            thinkTimer = 0f;
            GenerateMoveVector();
        }
    }

    // 랜덤 이동 방향 생성
    private void GenerateMoveVector()
    {
        // 랜덤 속도 생성
        xSpeed = Random.Range(-0.075f, 0.075f);
        ySpeed = Random.Range(-0.075f, 0.075f);

        // 이동 방향에 따라 스프라이트 뒤집기
        if (xSpeed != 0)
            sprite.flipX = xSpeed < 0;
    }

    // 화면 밖으로 나가면 부드럽게 화면 안으로 보정
    private void SmoothClampToScreen()
    {
        if (cam == null) return;

        // 현재 위치를 뷰포트 좌표(0~1)로 변환
        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        // 목표 뷰포트 좌표(범위 밖이면 범위 안으로)
        Vector3 targetVp = vp;

        if (vp.x < xMin) targetVp.x = xMin;
        if (vp.x > xMax) targetVp.x = xMax;
        if (vp.y < yMin) targetVp.y = yMin;
        if (vp.y > yMax) targetVp.y = yMax;

        // 현재 -> 목표로 부드럽게 보정
        Vector3 smoothVp = Vector3.Lerp(vp, targetVp, Time.deltaTime * 35f);

        // 다시 월드 좌표로 변환하여 적용
        transform.position = cam.ViewportToWorldPoint(smoothVp);
    }
}