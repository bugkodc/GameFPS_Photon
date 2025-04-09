using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Heal");
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager playerManager = other.gameObject.GetComponent<PlayerManager>();
            if (playerManager.currentHealth < playerManager.maximumHealth)
            {
                playerManager.Heal(20);
                Destroy(gameObject);
            }
        }
    }
}
