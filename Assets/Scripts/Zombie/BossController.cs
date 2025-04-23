using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BossController: Controls the behavior of the boss enemy, including movement, attack, spawning, and animation.
/// BossController: Điều khiển hành vi của boss như di chuyển, tấn công, triệu hồi quái và điều khiển animation.
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("References")]
    public GameObject playerTarget;
    public GameObject[] playerTargets;
    public GameManager gameManager;
    public GameObject[] spawnEnemy;
    public ParticleSystem VFXHealth;

    private NavMeshAgent navMeshAgent;
    private ZombieManager zombieManager;

    [Header("AI State")]
    private Vector3 target;
    private float distanceToPlayer;
    private bool isInRangeToMove;
    private bool isInRangeToAttack;
    private bool isAlive;
    private bool isSpawn = false;
    private float counterAttack;
    private float maxSpeed;

    [Header("Combat Settings")]
    [SerializeField] private float rangeToMove = 16f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float minMovementSpeed = 2f;
    [SerializeField] private float maxMovementSpeed = 4f;
    [SerializeField] private float minAttackSpeed = 2f;
    [SerializeField] private float maxAttackSpeed = 4f;
    [SerializeField] private float armLength = 1.5f;

    private float rangeToAttack;
    private float attackSpeed;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    private string isRunningBool = "isRunning";
    private string runningSpeed = "velocity";
    private string isAttackingTrigger = "isAttacking";

    [Header("Spawn Settings")]
    private NavMeshPath path;
    private int _randomNumberSpawn;
    private int _randomNumberHealth;

    void Start()
    {
        VFXHealth.Stop();
        zombieManager = GetComponent<ZombieManager>();
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Random.Range(minMovementSpeed, maxMovementSpeed);
        maxSpeed = navMeshAgent.speed;

        path = new NavMeshPath();
        rangeToAttack = navMeshAgent.stoppingDistance + 0.1f;
        attackSpeed = Random.Range(minAttackSpeed, maxAttackSpeed);

        _randomNumberSpawn = Random.Range(1, 10);
        Invoke("SpawnEnemy", _randomNumberSpawn);
    }

    void Update()
    {
        target = new Vector3(playerTarget.transform.position.x, 0, playerTarget.transform.position.z);
        distanceToPlayer = Vector3.Distance(transform.position, target);
        isInRangeToMove = distanceToPlayer < rangeToMove;
        isInRangeToAttack = distanceToPlayer <= rangeToAttack;
        isAlive = zombieManager.isAlive;

        if (!isSpawn)
        {
            Movement();
        }

        Attack();
        counterAttack += Time.deltaTime;
    }

    void Movement()
    {
        animator.SetFloat(runningSpeed, navMeshAgent.velocity.magnitude / maxSpeed);
        navMeshAgent.CalculatePath(target, path);

        if (isInRangeToMove && path.status == NavMeshPathStatus.PathComplete && !isInRangeToAttack && isAlive)
        {
            animator.SetBool(isRunningBool, true);
            navMeshAgent.SetDestination(target);
        }
        else
        {
            animator.SetBool(isRunningBool, false);
            navMeshAgent.SetDestination(transform.position);
        }
    }

    void Attack()
    {
        if (isInRangeToAttack && isAlive)
        {
            FaceTarget();

            if (counterAttack >= attackSpeed)
            {
                animator.SetTrigger(isAttackingTrigger);
                counterAttack = 0;
            }
        }
    }

    void SpawnEnemy()
    {
        int numberSpawn = Random.Range(2, 5);

        for (int i = 0; i < numberSpawn; i++)
        {
            int randomSpawn = Random.Range(0, spawnEnemy.Length);
            spawnEnemy[randomSpawn].SetActive(true);
            zombieManager.gameManager.InstantiateZombieEnenmy(true, randomSpawn, spawnEnemy);
        }

        StartCoroutine(WaitSpawn());
    }

    IEnumerator WaitSpawn()
    {
        isSpawn = true;
        _randomNumberHealth = Random.Range(0, 10);

        if (_randomNumberHealth > 7)
        {
            zombieManager.AddHealth(zombieManager.maxHealth / 5);
            StartCoroutine(WaitHeat());
        }

        animator.SetBool(isRunningBool, false);
        navMeshAgent.SetDestination(transform.position);

        yield return new WaitForSeconds(_randomNumberSpawn);

        foreach (GameObject spawnPoint in spawnEnemy)
        {
            spawnPoint.SetActive(false);
        }

        isSpawn = false;
        _randomNumberSpawn = Random.Range(1, 10);
        Invoke("SpawnEnemy", _randomNumberSpawn);
    }

    IEnumerator WaitHeat()
    {
        VFXHealth.Play();
        yield return new WaitForSeconds(3f);
        VFXHealth.Stop();
    }

    void FaceTarget()
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void MakeDamage()
    {
        if (distanceToPlayer <= rangeToAttack + armLength)
        {
            playerTarget.GetComponent<PlayerManager>().TakeDamage(attackDamage);
        }
    }
}
