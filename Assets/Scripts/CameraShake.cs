using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPosition;
    private bool _isShaking = false;

    void Start()
    {
        _originalPosition = transform.localPosition;
    }

    public void Shake(float duration = 0.3f, float magnitude = 0.2f)
    {
        if (!_isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        _isShaking = true;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(_originalPosition.x + x, _originalPosition.y + y, _originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        transform.localPosition = _originalPosition;
        _isShaking = false;
    }
}
