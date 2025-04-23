using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ZombieController: Điều khiển hành vi di chuyển, tấn công và kiểm tra trạng thái của zombie.
/// ZombieController: Controls zombie movement, attack behavior, and idle checks.
/// </summary>
public class ZombieController : MonoBehaviour
{
    [Header("Targeting")]
    public GameObject playerTarget;             
    public List<GameObject> playerTargets;       
    private Vector3 target;                       
    [Header("Movement Settings")]
    [SerializeField] private float rangeToMove = 16f;        
    [SerializeField] private float minMovementSpeed = 2f;   
    [SerializeField] private float maxMovementSpeed = 4f;  

    [Header("Attack Settings")]
    [SerializeField] private float minAttackSpeed = 2f;    
    [SerializeField] private float maxAttackSpeed = 4f;   
    [SerializeField] private float attackDamage = 20f;      
    [SerializeField] private float armLength = 1.5f;        

    [Header("Animations")]
    [SerializeField] private Animator animator;            
    private readonly string isRunningBool = "isRunning";
    private readonly string runningSpeedFloat = "velocity";
    private readonly string isAttackingTrigger = "isAttacking";

    [Header("Runtime Variables")]
    private NavMeshAgent navMeshAgent;       
    private NavMeshPath path;                
    private ZombieManager zombieManager;      
    private float rangeToAttack;              
    private float attackSpeed;               
    private float counterAttack;              
    private bool isInRangeToMove;            
    private bool isInRangeToAttack;           
    private bool isAttacking;                
    private bool isAlive;                  
    private float maxSpeed;                  
    private Vector3 currentPosition;          

    void Start()
    {
        zombieManager = GetComponent<ZombieManager>();
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Random.Range(minMovementSpeed, maxMovementSpeed);
        maxSpeed = navMeshAgent.speed;

        path = new NavMeshPath();
        rangeToAttack = navMeshAgent.stoppingDistance + 0.1f;
        attackSpeed = Random.Range(minAttackSpeed, maxAttackSpeed);

        StartCoroutine(CheckMoveZombie());
    }

    void Update()
    {
        // Cập nhật mục tiêu chỉ lấy XZ
        target = new Vector3(playerTarget.transform.position.x, 0f, playerTarget.transform.position.z);
        float distanceToPlayer = Vector3.Distance(transform.position, target);

        isInRangeToMove = distanceToPlayer < rangeToMove;
        isInRangeToAttack = distanceToPlayer <= rangeToAttack;
        isAlive = zombieManager.isAlive;

        Movement();
        Attack();

        counterAttack += Time.deltaTime;
    }


    IEnumerator CheckMoveZombie()
    {
        currentPosition = transform.position;
        yield return new WaitForSeconds(5f);

        if (transform.position == currentPosition && !isAttacking)
            zombieManager.Die();
        else
            StartCoroutine(CheckMoveZombie());
    }

    void Movement()
    {
        animator.SetFloat(runningSpeedFloat, navMeshAgent.velocity.magnitude / maxSpeed);

        navMeshAgent.CalculatePath(target, path);

        if (isAlive && isInRangeToMove && !isInRangeToAttack && path.status == NavMeshPathStatus.PathComplete)
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
        if (isAlive && isInRangeToAttack)
        {
            FaceTarget();
            if (counterAttack >= attackSpeed)
            {
                animator.SetTrigger(isAttackingTrigger);
                counterAttack = 0f;
                isAttacking = true;
            }
        }
    }

    void FaceTarget()
    {
        Vector3 dir = (target - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    public void MakeDamage()
    {
        float dist = Vector3.Distance(transform.position, target);
        if (dist <= rangeToAttack + armLength)
            playerTarget.GetComponent<PlayerManager>().TakeDamage(attackDamage);
    }

    public void RemovePlayer(GameObject playercurrent)
    {
        if (playerTargets.Contains(playercurrent))
            playerTargets.Remove(playercurrent);

        if (playerTargets.Count > 0)
            playerTarget = playerTargets[Random.Range(0, playerTargets.Count)];
    }
}
