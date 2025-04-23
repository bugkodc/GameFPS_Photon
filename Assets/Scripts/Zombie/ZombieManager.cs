using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ZombieManager: Quản lý sinh lực, hiệu ứng UI, âm thanh và chết cho zombie hoặc boss.
/// ZombieManager: Manages health, UI effects, audio growls, and death behavior for zombies or bosses.
/// </summary>
public class ZombieManager : MonoBehaviour
{
    [Header("Health Settings")]
    public float currentHealth;               
    [SerializeField] public float maxHealth;  
    public bool isAlive;                     

    [Header("UI References")]
    [SerializeField] private Slider HPSlider; 

    [Header("AI Controllers")]
    private ZombieController zombieController; 
    private BossController bossController;     
    public bool isZoombie;                   

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] growlClips; 
    private AudioSource audioSource;                
    [SerializeField] private float CDGrowlTime = 2f;
    private float counter = 0f;                   

    [Header("References")]
    public GameManager gameManager;            
    private Collider _collider;                 

    [Header("Animation")]
    [SerializeField] private Animator animator; 
    private readonly string dieAnimationTrigger = "isDead";
    void Start()
    {
        if (isZoombie)
            zombieController = GetComponent<ZombieController>();
        else
            bossController = GetComponent<BossController>();

        currentHealth = maxHealth;
        isAlive = true;
        HPSlider.value = currentHealth / maxHealth;

        audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (gameManager.CurrentLocalGameState == GameState.inGame)
            GrowlAndRotateZombie();
    }

    private void GrowlAndRotateZombie()
    {
        var targetTransform = isZoombie
            ? zombieController.playerTarget.transform
            : bossController.playerTarget.transform;

        HPSlider.transform.LookAt(targetTransform);

        if (counter >= CDGrowlTime && !audioSource.isPlaying && isAlive)
            Growl();

        counter += Time.deltaTime;
    }
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        HPSlider.value = currentHealth / maxHealth;

        if (currentHealth <= 0f && isAlive)
            Die();
    }

    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        HPSlider.value = currentHealth / maxHealth;
    }

    public void Die()
    {
        isAlive = false;
        HPSlider.gameObject.SetActive(false);
        animator.SetTrigger(dieAnimationTrigger);
        _collider.enabled = false;
        gameManager.LookForEnemies();
        Destroy(gameObject, 3f);
    }

    private void Growl()
    {
        audioSource.clip = growlClips[Random.Range(0, growlClips.Length)];
        audioSource.Play();
        counter = 0f;
    }
}
