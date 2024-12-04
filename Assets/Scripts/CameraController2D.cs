using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController2D : MonoBehaviour
{
    [SerializeField] private Camera followCamera;
    private Vector2 viewportHalfSize;
    private float leftBoundaryLimit;
    private float rightBoundaryLimit;
    private float bottomBoundaryLimit;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothing = 5f;

    private Vector3 shakeOffset = Vector3.zero;

    void Start()
    {
        tilemap.CompressBounds();
        CalculateCameraBoundaries();
    }

    private void CalculateCameraBoundaries()
    {
        viewportHalfSize = new Vector2(followCamera.orthographicSize * followCamera.aspect, followCamera.orthographicSize);

        leftBoundaryLimit = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewportHalfSize.x;
        rightBoundaryLimit = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewportHalfSize.x;
        bottomBoundaryLimit = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewportHalfSize.y;
    }

    public void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Shake(2.5f, 3f);
        }

        Vector3 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z) + shakeOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));

        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, leftBoundaryLimit, rightBoundaryLimit);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, bottomBoundaryLimit, smoothedPosition.y);

        transform.position = smoothedPosition;
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            shakeOffset = Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
