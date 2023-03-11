using UnityEngine;

public class Hero : MonoBehaviour
{
    public static Hero S;   // �̱���

    // ���ּ��� �̵��� �����ϴ� �ʵ�
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;

    // ���ּ� ���� ����
    public float shieldLevel = 1;

    public bool _________________________;
    public Bounds bounds;
    private void Awake()
    {
        S = this;   // �̱����� ����
        bounds = Utils.CombineBoundsOfChildren(this.gameObject);
    }

    private void Update()
    {
        // Input Ŭ�����κ��� ������ ������
        // ���ӿ��� ���̽�ƽ�� �Է��� �޴� ��� WASD Ű�� �Է��� �޴� ���� ���� �ۼ����� �ʾƵ� ��
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        // �Է� ���� ������� transform.position�� ����
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        // ������ ������ �ֵ��� ���ּ��� ȸ��
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