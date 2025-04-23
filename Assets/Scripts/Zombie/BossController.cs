using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

public class BossController : MonoBehaviourPunCallbacks
{

    public GameObject playerTarget;
    public GameObject[] playerTargets;
    public GameManager gameManager;
    public GameObject[] spawnEnemy;
    public ParticleSystem VFXHealth;

    private Vector3 target;
    private float distanceToPlayer;
    private NavMeshAgent navMeshAgent;
    private ZombieManager zombieManager;

    //Animations
    [SerializeField] private Animator animator;
    private string isRunningBool = "isRunning";
    private string runningSpeed = "velocity";
    private string isAttackingTrigger = "isAttacking";

    [SerializeField] private float rangeToMove = 16;
    [SerializeField] private float attackDamage = 20;
    [SerializeField] private float minMovementSpeed = 2;
    [SerializeField] private float maxMovementSpeed = 4;
    [SerializeField] private float minAttackSpeed = 2;
    [SerializeField] private float maxAttackSpeed = 4;
    [SerializeField] private float armLength = 1.5f;

    private float attackSpeed;
    private float rangeToAttack;
    private float counterAttack;
    private bool isInRangeToMove;
    private bool isInRangeToAttack;
    private bool isAlive;
    private float maxSpeed;
    private NavMeshPath path;
    private int _randomNumberSpawn;
    private int _randomNumberHealth;
    private bool isSpawn = false;

    void Start()
    {
        VFXHealth.Stop();
        zombieManager = gameObject.GetComponent<ZombieManager>();
        if (PhotonNetwork.InRoom)
            playerTargets = GameObject.FindGameObjectsWithTag("Player");
        else
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
        if (PhotonNetwork.InRoom)
        {
            float minDistanceToPlayer = float.MaxValue;
            foreach (GameObject player in playerTargets)
            {
                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (distance < minDistanceToPlayer)
                    {
                        minDistanceToPlayer = distance;
                        playerTarget = player;
                    }
                }
            }
        }

        target = new Vector3(playerTarget.transform.position.x, 0, playerTarget.transform.position.z);
        distanceToPlayer = Vector3.Distance(transform.position, target);
        isInRangeToMove = (distanceToPlayer < rangeToMove);
        isInRangeToAttack = distanceToPlayer <= rangeToAttack;
        isAlive = zombieManager.isAlive;
        if (isSpawn == false)
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
        if (isInRangeToMove && path.status == NavMeshPathStatus.PathComplete &&
        !isInRangeToAttack && isAlive)
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
        int NumberSpawn = Random.Range(2, 5);
        for (int i = 0; i < NumberSpawn; i++)
        {
            int ramdomspawn = Random.Range(0, spawnEnemy.Length);
            spawnEnemy[ramdomspawn].SetActive(true);
            zombieManager.gameManager.InstantiateZombieEnenmy(PhotonNetwork.InRoom, ramdomspawn, spawnEnemy);
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
        transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    public void MakeDamage()
    {
        if (distanceToPlayer <= rangeToAttack + armLength)
        {
            if (PhotonNetwork.InRoom)
                playerTarget.GetComponent<PlayerManager>().photonView.RPC("TakeDamage", RpcTarget.All,
                attackDamage);
            else
                playerTarget.GetComponent<PlayerManager>().TakeDamage(attackDamage);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerTargets = GameObject.FindGameObjectsWithTag("Player");
    }
}
