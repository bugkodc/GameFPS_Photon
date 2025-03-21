using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] GameManager gameManager;


    public IEnumerator Shake(float shakeDuration, float magnitude)
    {
        float elapsed = 0.0f;
        Quaternion originalRotation = transform.localRotation;

        while (elapsed < shakeDuration && gameManager.CurrentLocalGameState == GameState.inGame)
        {

            float xShake = Random.Range(-1, 1) * magnitude;
            float yShake = Random.Range(-1, 1) * magnitude;

            transform.localRotation = Quaternion.Euler(new Vector3(xShake, yShake, originalRotation.z));

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = originalRotation;

    }
}
