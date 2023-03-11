using UnityEngine;

public class Shield : MonoBehaviour
{
    public float rotationsPerSecond = 0.1f;
    public bool ____________________;
    public int levelShown = 0;

    void Update()
    {
        // Hero 싱글톤에서 현재 방어막 레벨을 얻음
        int currLevel = Mathf.FloorToInt(Hero.S.shieldLevel);
        // levelShown 값과 다른 경우
        if (levelShown != currLevel)
        {
            levelShown = currLevel;
            Material mat = this.GetComponent<Renderer>().material;

            // 텍스쳐 오프셋을 조정해 다른 방어막 레벨을 표시
            mat.mainTextureOffset = new Vector2(0.2f * levelShown, 0);
        }
        // 방어막을 약간씩 회전
        float rZ = (rotationsPerSecond * Time.time * 360) % 360f;
        transform.rotation = Quaternion.Euler(0, 0, rZ);
    }
}