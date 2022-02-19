using UnityEngine;
using System.Collections;
using System;

public class GameCamera : MonoBehaviour
{
    [SerializeField]
    private float width = 60f;

    [SerializeField]
    private float transitionTime = 1.0f;

    [SerializeField]
    private Vector3 startPosition = new Vector3(0, 0, -10);

    [SerializeField]
    private Vector3 endPosition = new Vector3(0, 40, -10); 

    private Vector2 lastScreenSize = Vector2.zero;

    void Start() {
        transform.position = endPosition;
    }

    void Update() {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height) {
            Camera.main.orthographicSize = width * Screen.height / Screen.width * 0.5f;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }

    public IEnumerator TransitionCamera(Action callback, bool skipMoveUp = false) {
        // Move camera up and wait
        float startTime = Time.time;
        if (!skipMoveUp) {
            while (Time.time - startTime < transitionTime) {
                yield return new WaitForSeconds(.01f);
                float dTime = (Time.time - startTime) / transitionTime;
                transform.position = Vector3.Slerp(startPosition, endPosition, dTime);
            }
            yield return new WaitForSeconds(transitionTime);
        }
        
        // Callback
        callback();

        // Move camera down
        startTime = Time.time;
        while (Time.time - startTime < transitionTime) {
            yield return new WaitForSeconds(.01f);
            float dTime = (Time.time - startTime) / transitionTime;
            transform.position = Vector3.Slerp(endPosition, startPosition, dTime);
        }
        transform.position = startPosition;
    }

}
