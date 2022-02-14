using UnityEngine;

public class SwingMeter : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    public void Clear() {
        lineRenderer.positionCount = 0;
        lineRenderer.SetPositions(new Vector3[] {});
    }

    public void Set(Vector2 startPosition, Vector2 endPosition) {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] {startPosition, endPosition});
    }
}
