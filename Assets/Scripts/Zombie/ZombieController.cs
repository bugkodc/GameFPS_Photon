using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering;
using System.Linq;

public class ZombieController : MonoBehaviourPunCallbacks
{
    public GameObject playerTarget;
    public List<GameObject> playerTargets;
    Vector3 target;
    float distanceToPlayer;
    NavMeshAgent navMeshAgent;

    ZombieManager zombieManager;

    //Animations
    [SerializeField]
    Animator animator;
    string isRunningBool = "isRunning", runningSpeed = "velocity",
    isAttackingTrigger = "isAttacking";



    [SerializeField] float rangeToMove = 16;

    //Stats
    [SerializeField]
    float attackDamage = 20;

    [SerializeField]
    float minMovementSpeed = 2, maxMovementSpeed = 4;

    [SerializeField]
    float minAttackSpeed = 2, maxAttackSpeed = 4;
    float attackSpeed;

    //Equals to the stopping distance of the nav mesh agent
    float rangeToAttack;

    float counterAttack;
    bool isInRangeToMove, isInRangeToAttack, isAlive;

    bool isAttacking;
    float maxSpeed;
    NavMeshPath path;
    [SerializeField] float armLength = 1.5f;
    private Vector3 currentPosition;

    // Start is called before the first frame update
    void Start()
    {
        zombieManager = gameObject.GetComponent<ZombieManager>();
        if (PhotonNetwork.InRoom)
            playerTargets = GameObject.FindGameObjectsWithTag("Player").ToList();
        else
            playerTarget = GameObject.FindGameObjectWithTag("Player");

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Random.Range(minMovementSpeed, maxMovementSpeed);
        maxSpeed = navMeshAgent.speed;
        path = new NavMeshPath();
        rangeToAttack = navMeshAgent.stoppingDistance + 0.1f;
        attackSpeed = Random.Range(minAttackSpeed, maxAttackSpeed);
        StartCoroutine("CheckMoveZombie");

    }

    // Update is called once per frame
    void Update()
    {
        //If we are online we check which player is the closest to the zombie
        if (PhotonNetwork.InRoom)
        {
            float minDistanceToPlayer = float.MaxValue;
            foreach (GameObject player in playerTargets)
            {
                if (player != null && player.GetComponent<PlayerManager>().isAlive)
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
        //In order to move the enemy even when the player is jumping, we set the target vector y to 0.
        target = new Vector3(playerTarget.transform.position.x, 0, playerTarget.transform.position.z);

        distanceToPlayer = Vector3.Distance(transform.position, target);
        isInRangeToMove = (distanceToPlayer < rangeToMove);
        isInRangeToAttack = distanceToPlayer <= rangeToAttack;
        isAlive = zombieManager.isAlive;

        Movement();
        Attack();

        counterAttack += Time.deltaTime;
    }

    IEnumerator CheckMoveZombie()
    {
        currentPosition = gameObject.transform.position;
        yield return new WaitForSeconds(5f);
        if (currentPosition == gameObject.transform.position && !isAttacking)
        {
            zombieManager.Die();
        }
        else
        {
            StartCoroutine("CheckMoveZombie");
        }

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
            isAttacking = false;
            if (counterAttack >= attackSpeed)
            {
                animator.SetTrigger(isAttackingTrigger);
                counterAttack = 0;
                isAttacking = true;
            }
           
        }
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
        playerTargets = GameObject.FindGameObjectsWithTag("Player").ToList();
    }
    public void RemovePlayer( GameObject playercurrent)
    {
        foreach (var player in playerTargets)
        {
           if(playercurrent == player)
            {
                playerTargets.Remove(player);
            }
        }
        playerTarget = playerTargets[Random.Range(0, playerTargets.Count)];
    }
}
