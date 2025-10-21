using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        transform.localScale = new Vector3(
            screenWidth / sr.bounds.size.x,
            screenHeight / sr.bounds.size.y,
            1);
    }
}
