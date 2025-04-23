using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CameraShake: Handles camera shake effect based on input and duration, useful for adding effects during specific game events.
/// CameraShake: Điều khiển hiệu ứng rung camera dựa trên thời gian và độ mạnh, hữu ích cho việc thêm hiệu ứng trong các sự kiện trong game.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Game Management")]
    [SerializeField] private GameManager gameManager;
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
