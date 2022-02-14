using UnityEngine;

public class ResizeCamera : MonoBehaviour
{
    [SerializeField]
    private float width = 60f;

    private Vector2 lastScreenSize = Vector2.zero;

    void Update() {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height) {
            Camera.main.orthographicSize = width * Screen.height / Screen.width * 0.5f;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }
}
