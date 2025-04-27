using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> playGrounds;
    private GameObject currentPlayground;
    Camera myCamera;
    void Start()
    {
        myCamera = FindAnyObjectByType<Camera>();
        currentPlayground = playGrounds[0];
    }
    public void ChangeLevel()
    {
        int x = playGrounds.FindInstanceID(currentPlayground);
        x = (x + 1) % 2;
        SwitchPlayground(x);
    }

    void SwitchPlayground(int x)
    {
        currentPlayground = playGrounds[x];
        Vector3 targetPosition = new Vector3(x * 6, myCamera.transform.position.y, myCamera.transform.position.z);
        StartCoroutine(SmoothMove(myCamera.transform, targetPosition, 1f));

    }
    IEnumerator SmoothMove(Transform camTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = camTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            camTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        camTransform.position = targetPosition;
    }
}
