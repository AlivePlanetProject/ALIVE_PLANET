using UnityEngine;

// �� �������� Utils Ŭ���� �ܺο� ��ġ��
public enum BoundsTest
{
    center,     // ���� ������Ʈ�� �߽��� ȭ�� �ȿ� ����
    onScreen,   // ��谡 ������ ȭ�� �ȿ� ����
    offScreen   // ��谡 ������ ȭ�� �ۿ� ����
}

public class Utils : MonoBehaviour
{
    // ================== ��� �Լ� ================== \\

    // ���޵� �� ��踦 ĸ��ȭ�ϴ� ��踦 ����
    public static Bounds BoundsUnion(Bounds b0, Bounds b1)
    {
        // ��� �� �ϳ��� ũ�Ⱑ Vector3.zero�� ��� �̸� ����
        if (b0.size == Vector3.zero && b1.size != Vector3.zero)
        {
            return b1;
        }
        else if (b0.size != Vector3.zero && b1.size == Vector3.zero)
        {
            return b0;
        }
        else if (b0.size == Vector3.zero && b1.size == Vector3.zero)
        {
            return b0;
        }

        // b1.min �� b1.max�� �����ϵ��� b0�� Ȯ��
        b0.Encapsulate(b1.min);
        b0.Encapsulate(b1.max);
        return b0;
    }

    public static Bounds CombineBoundsOfChildren(GameObject go)
    {
        // �� Bounds b�� ����
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);

        // �� ���� ������Ʈ�� Renderer ������Ʈ�� �ִ� ���
        if (go.GetComponent<Renderer>() != null)
        {
            // Renderer�� ��踦 �����ϵ��� b�� Ȯ��
            b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
        }

        // �� ���� ������Ʈ�� Collider ������Ʈ�� �ִ� ���
        if (go.GetComponent<Collider>() != null)
        {
            // Collider�� ��踦 �����ϵ��� b�� Ȯ��
            b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
        }

        // ���� gameObject.transform�� �� ���� ���ӿ�����Ʈ�� ���� ��� �ݺ�
        foreach (Transform t in go.transform)
        {
            // ���� ���� ������Ʈ�� ��踦 �����ϵ��� b�� Ȯ��
            b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
        }

        return b;
    }

    // camBounds��� �б� ������ static �Ӽ��� ����
    public static Bounds camBounds
    {
        get
        {
            // _camBounds�� ���� �������� ���� ���
            if (_camBounds.size == Vector3.zero)
            {
                // �⺻ ī�޶� ����� SetCameraBounds ȣ��
                SetCameraBounds();
            }
            return _camBounds;
        }
    }
    // camBounds�� ����ϴ� private ���� �ʵ�
    // �����Ϸ��� �ڵ带 �ؼ��ϱ� ���� Utils Ŭ������ ����� ��� �׸��� ���� ����
    // ���� ������ �����ϴ� ������ �߿����� ����
    private static Bounds _camBounds;

    // camBounds�� _camBounds�� �����ϱ� ���� ����ϴ� �Լ��̸�
    // ���� ȣ���� �� ����
    public static void SetCameraBounds(Camera cam = null)
    {
        // ī�޶� ���޵��� ���� ��� ���� ī�޶� ���
        if (cam == null) cam = Camera.main;
        /*
            ���⿡���� ī�޶� ���� �� ���� �߿��� ������ �� ����
            1. ���� ī�޶��̴�.
            2. ī�޶� ȸ���� R:[0,0,0]�̴�.
        */

        // ȭ�� ��ǥ�� toLeft�� bottomRight�� Vector3�� ����
        Vector3 topLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(Screen.width, Screen.height, 0);

        // �� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 boundTLN = cam.ScreenToWorldPoint(topLeft);
        Vector3 boundBRF = cam.ScreenToWorldPoint(bottomRight);

        // ��ǥ�� z ���� �ٰŸ� �� ���Ÿ� ī�޶� Ŭ���� ������� ����
        boundTLN.z += cam.nearClipPlane;
        boundBRF.z += cam.farClipPlane;

        // Bounds�� center�� ã��
        Vector3 center = (boundTLN + boundBRF) / 2f;
        _camBounds = new Bounds(center, Vector3.zero);

        // ������ �����ϵ��� _camBounds�� Ȯ��
        _camBounds.Encapsulate(boundTLN);
        _camBounds.Encapsulate(boundBRF);
    }
    // Bounds bnd�� camBounds �ȿ� ���ԵǴ��� Ȯ��
    public static Vector3 ScreenBoundsCheck(Bounds bnd, BoundsTest test = BoundsTest.center)
    {
        return BoundsInBoundsCheck(camBounds, bnd, test);
    }

    // Bounds lilB�� Bounds bigB �ȿ� ���ԵǴ��� Ȯ��
    public static Vector3 BoundsInBoundsCheck(Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.onScreen)
    {
        // �� �Լ��� ������ ������ BoundsTest�� ���� �޶���

        // lilB�� �߽��� ����
        Vector3 pos = lilB.center;

        // �������� [0,0,0]�� �ʱ�ȭ
        Vector3 off = Vector3.zero;

        switch (test)
        {
            // center �׽�Ʈ�� lilB�� �߽��� bigB ��������
            // �ű�� ���� �����ؾ� �ϴ� off(������)�� ����
            case BoundsTest.center:
                if (bigB.Contains(pos))
                {
                    return Vector3.zero;
                }

                if (pos.x > bigB.max.x)
                {
                    off.x = pos.x - bigB.max.x;
                }
                else if (pos.x < bigB.min.x)
                {
                    off.x = pos.x - bigB.min.x;
                }

                if (pos.y > bigB.max.y)
                {
                    off.y = pos.y - bigB.max.y;
                }
                else if (pos.y < bigB.min.y)
                {
                    off.y = pos.y - bigB.min.y;
                }

                if (pos.z > bigB.max.z)
                {
                    off.z = pos.z - bigB.max.z;
                }
                else if (pos.z < bigB.min.z)
                {
                    off.z = pos.z - bigB.min.z;
                }

                return off;

            // onScreen �׽�Ʈ�� lilB ��ü�� bigB ��������
            // �ű�� ���� �����ؾ� �ϴ� off�� ����
            case BoundsTest.onScreen:
                if (bigB.Contains(lilB.min) && bigB.Contains(lilB.max))
                {
                    return Vector3.zero;
                }

                if (lilB.max.x > bigB.max.x)
                {
                    off.x = lilB.max.x - bigB.max.x;
                }
                else if (lilB.min.x < bigB.min.x)
                {
                    off.x = lilB.min.x - bigB.min.x;
                }

                if (lilB.max.y > bigB.max.y)
                {
                    off.y = lilB.max.y - bigB.max.y;
                }
                else if (lilB.min.y < bigB.min.y)
                {
                    off.y = lilB.min.y - bigB.min.y;
                }

                if (lilB.max.z > bigB.max.z)
                {
                    off.z = lilB.max.z - bigB.max.z;
                }
                else if (lilB.min.z < bigB.min.z)
                {
                    off.z = lilB.min.z - bigB.min.z;
                }

                return off;

            // offScreen �׽�Ʈ�� lilB�� ���� �Ϻθ� bigB ��������
            // �ű�� ���� �����ؾ� �ϴ� off�� ����
            case BoundsTest.offScreen:
                bool cMin = bigB.Contains(lilB.min);
                bool cMax = bigB.Contains(lilB.max);

                if (cMin || cMax)
                {
                    return Vector3.zero;
                }

                if (lilB.min.x > bigB.max.x)
                {
                    off.x = lilB.min.x - bigB.max.x;
                }
                else if (lilB.max.x < bigB.min.x)
                {
                    off.x = lilB.max.x - bigB.min.x;
                }

                if (lilB.min.y > bigB.max.y)
                {
                    off.y = lilB.min.y - bigB.max.y;
                }
                else if (lilB.max.y < bigB.min.y)
                {
                    off.y = lilB.max.y - bigB.min.y;
                }

                if (lilB.min.z > bigB.max.z)
                {
                    off.z = lilB.min.z - bigB.max.z;
                }
                else if (lilB.max.z < bigB.min.z)
                {
                    off.z = lilB.max.z - bigB.min.z;
                }

                return off;
        }
        return Vector3.zero;
    }
}