using UnityEngine;

public class Hero : MonoBehaviour
{
    public static Hero S;   // 싱글톤

    // 우주선의 이동을 제어하는 필드
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;

    // 우주선 상태 정보
    public float shieldLevel = 1;

    public bool _________________________;
    public Bounds bounds;
    private void Awake()
    {
        S = this;   // 싱글톤을 설정
        bounds = Utils.CombineBoundsOfChildren(this.gameObject);
    }

    private void Update()
    {
        // Input 클래스로부터 정보를 가져옴
        // 게임에서 조이스틱의 입력을 받는 행과 WASD 키의 입력을 받는 행을 따로 작성하지 않아도 됨
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        // 입력 축을 기반으로 transform.position을 변경
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        // 동적인 느낌을 주도록 우주선을 회전
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        bounds.center = transform.position;

        Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.onScreen);
        if (off != Vector3.zero)
        {
            pos -= off;
            transform.position = pos;
        }
    }
}