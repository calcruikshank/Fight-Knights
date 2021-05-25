using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    float lastShake = 0f;

    private void Start()
    {
        lastShake = .1f;
    }
    private void Update()
    {
        lastShake += Time.deltaTime;
    }
    public IEnumerator Shake(float duration, float magnitude)
    {
        if (lastShake > .25f)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(x, y, originalPosition.z);
                elapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = originalPosition;

            lastShake = 0f;
        }
        
    }
}
