using UnityEngine;

public class move : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer sprite;
    private Camera cam;

    private float xSpeed;
    private float ySpeed;

    private const float xMin = 0.075f;
    private const float xMax = 0.925f;
    private const float yMin = 0.195f;
    private const float yMax = 0.75f;

    private float thinkTimer = 0f;
    private float thinkInterval = 5f;

    private float clampTimer = 0f;
    private const float clampInterval = 0.02f;   // 더 부드럽게 체크

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        GenerateMoveVector();
    }

    private void Update()
    {
        clampTimer += Time.deltaTime;
        if (clampTimer >= clampInterval)
        {
            clampTimer = 0f;
            SmoothClampToScreen();   // ← 움직임 꺼져도 항상 실행됨!
        }

        if (!Setting.IsMOVEOn())
        {
            rigid.velocity = Vector2.zero; // 이동만 멈춤
            return; // 여기서 끝내도 괜찮음 (clamp는 이미 위에서 실행됨)
        }

        // 이동 ON일 때만 실행
        rigid.velocity = new Vector2(xSpeed, ySpeed);

        thinkTimer += Time.deltaTime;
        if (thinkTimer >= thinkInterval)
        {
            thinkTimer = 0f;
            GenerateMoveVector();
        }
    }

    private void GenerateMoveVector()
    {
        xSpeed = Random.Range(-0.075f, 0.075f);
        ySpeed = Random.Range(-0.075f, 0.075f);

        if (xSpeed != 0)
            sprite.flipX = xSpeed < 0;
    }

    private void SmoothClampToScreen()
    {
        if (cam == null) return;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        // 목표 위치(부드럽게 보정)
        Vector3 targetVp = vp;

        if (vp.x < xMin) targetVp.x = xMin;
        if (vp.x > xMax) targetVp.x = xMax;
        if (vp.y < yMin) targetVp.y = yMin;
        if (vp.y > yMax) targetVp.y = yMax;

        // ★ 부드럽게 Lerp 보정
        Vector3 smoothVp = Vector3.Lerp(vp, targetVp, Time.deltaTime * 35f);
        transform.position = cam.ViewportToWorldPoint(smoothVp);
    }
}