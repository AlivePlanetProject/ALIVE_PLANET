using UnityEngine;

// 이 열거형은 Utils 클래스 외부에 위치함
public enum BoundsTest
{
    center,     // 게임 오브젝트의 중심이 화면 안에 있음
    onScreen,   // 경계가 완전히 화면 안에 있음
    offScreen   // 경계가 완전히 화면 밖에 있음
}

public class Utils : MonoBehaviour
{
    // ================== 경계 함수 ================== \\

    // 전달된 두 경계를 캡슐화하는 경계를 생성
    public static Bounds BoundsUnion(Bounds b0, Bounds b1)
    {
        // 경계 중 하나의 크기가 Vector3.zero인 경우 이를 무시
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

        // b1.min 및 b1.max를 포함하도록 b0을 확장
        b0.Encapsulate(b1.min);
        b0.Encapsulate(b1.max);
        return b0;
    }

    public static Bounds CombineBoundsOfChildren(GameObject go)
    {
        // 빈 Bounds b를 생성
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);

        // 이 게임 오브젝트에 Renderer 컴포넌트가 있는 경우
        if (go.GetComponent<Renderer>() != null)
        {
            // Renderer의 경계를 포함하도록 b를 확장
            b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
        }

        // 이 게임 오브젝트에 Collider 컴포넌트가 있는 경우
        if (go.GetComponent<Collider>() != null)
        {
            // Collider의 경계를 포함하도록 b를 확장
            b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
        }

        // 현재 gameObject.transform의 각 하위 게임오브젝트를 상대로 재귀 반복
        foreach (Transform t in go.transform)
        {
            // 하위 게임 오브젝트의 경계를 포함하도록 b를 확장
            b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
        }

        return b;
    }

    // camBounds라는 읽기 전용인 static 속성을 만듦
    public static Bounds camBounds
    {
        get
        {
            // _camBounds가 아직 설정되지 않은 경우
            if (_camBounds.size == Vector3.zero)
            {
                // 기본 카메라를 사용해 SetCameraBounds 호출
                SetCameraBounds();
            }
            return _camBounds;
        }
    }
    // camBounds가 사용하는 private 정적 필드
    // 컴파일러는 코드를 해석하기 전에 Utils 클래스에 선언된 모든 항목을 먼저 읽음
    // 따라서 변수를 선언하는 순서는 중요하지 않음
    private static Bounds _camBounds;

    // camBounds가 _camBounds를 설정하기 위해 사용하는 함수이며
    // 직접 호출할 수 있음
    public static void SetCameraBounds(Camera cam = null)
    {
        // 카메라가 전달되지 않은 경우 메인 카메라를 사용
        if (cam == null) cam = Camera.main;
        /*
            여기에서는 카메라에 대한 두 가지 중요한 가정을 한 상태
            1. 직교 카메라이다.
            2. 카메라 회전이 R:[0,0,0]이다.
        */

        // 화면 좌표의 toLeft와 bottomRight에 Vector3를 만듦
        Vector3 topLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(Screen.width, Screen.height, 0);

        // 이 좌표를 월드 좌표로 변환
        Vector3 boundTLN = cam.ScreenToWorldPoint(topLeft);
        Vector3 boundBRF = cam.ScreenToWorldPoint(bottomRight);

        // 좌표의 z 값을 근거리 및 원거리 카메라 클리핑 평면으로 조정
        boundTLN.z += cam.nearClipPlane;
        boundBRF.z += cam.farClipPlane;

        // Bounds의 center를 찾음
        Vector3 center = (boundTLN + boundBRF) / 2f;
        _camBounds = new Bounds(center, Vector3.zero);

        // 범위를 포함하도록 _camBounds를 확장
        _camBounds.Encapsulate(boundTLN);
        _camBounds.Encapsulate(boundBRF);
    }
    // Bounds bnd가 camBounds 안에 포함되는지 확인
    public static Vector3 ScreenBoundsCheck(Bounds bnd, BoundsTest test = BoundsTest.center)
    {
        return BoundsInBoundsCheck(camBounds, bnd, test);
    }

    // Bounds lilB가 Bounds bigB 안에 포함되는지 확인
    public static Vector3 BoundsInBoundsCheck(Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.onScreen)
    {
        // 이 함수의 동작은 선택한 BoundsTest에 따라 달라짐

        // lilB의 중심을 얻음
        Vector3 pos = lilB.center;

        // 오프셋을 [0,0,0]로 초기화
        Vector3 off = Vector3.zero;

        switch (test)
        {
            // center 테스트는 lilB의 중심을 bigB 안쪽으로
            // 옮기기 위해 적용해야 하는 off(오프셋)을 얻음
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

            // onScreen 테스트는 lilB 전체를 bigB 안쪽으로
            // 옮기기 위해 적용해야 하는 off를 얻음
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

            // offScreen 테스트는 lilB의 극히 일부를 bigB 안쪽으로
            // 옮기기 위해 적용해야 하는 off를 얻음
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