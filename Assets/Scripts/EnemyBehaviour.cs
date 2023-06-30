using Panda;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;




public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] AIBehaviour player;
    Vector3 playerDir;
    public Vector3 lastPlayerPos;
    [SerializeField] NavMeshAgent enemyAgent;
    bool awareOfPlayer;
    public bool alerted;
    [SerializeField] EnemyBehaviour patroller;
    [SerializeField] bool isStatic=false;
    [SerializeField] Transform staticPos;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireSpeed;
    [SerializeField] float rateOfFire;
    [SerializeField] LayerMask layer;
    [SerializeField] int health;
    public List<GameObject> Doors = new List<GameObject>();
    public List<EnemyBehaviour> Patrollers = new List<EnemyBehaviour>();



    [Task]
    bool IsMoving()
    {
        //bool moving = enemyAgent.remainingDistance <= enemyAgent.stoppingDistance;
        bool moving = enemyAgent.velocity.magnitude <= 0.1f ? false : true;
        return moving;
    }

    [Task]
    bool isAware()
    {
        return awareOfPlayer;
    }

    [Task]
    bool BecomeUnaware()
    {
        awareOfPlayer = false;
        return true;
    }

    [Task]
    bool Idle()
    {
        enemyAgent.SetDestination(staticPos.position);
      

        return true;
    }

    [Task]
    bool ReleaseDoor()
    {
        foreach(var door in Doors)
        {
            door.transform.parent.GetComponent<Animator>().SetBool("Open", true);
            door.GetComponent<Door>().isOpen = true;
            door.GetComponent<Door>().canBeOpenedByPlayer = true;
        }

        return true;
    }


    [Task]
    bool PatrolArea()
    {
        Vector3 RandomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        enemyAgent.SetDestination(RandomDir.normalized * 10 + transform.position );
        return true;
    }

    [Task]
    bool MustHaveBeenTheWind()
    {
        awareOfPlayer = false;
        Vector3 RandomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        enemyAgent.SetDestination(RandomDir.normalized * 1 + transform.position );
        return false;
    }

    [Task]
    bool ChasePlayer()
    {
        enemyAgent.SetDestination(lastPlayerPos);
        return true;
    }






    [Task]
    bool CanSeePlayer()
    {
        if (alerted) return true;
        return PlayerOnVision();
    }

    [Task]

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<AIBehaviour>();
        StartCoroutine(ShootRoutine(rateOfFire));
        if (isStatic)
        {
            foreach (var gameobject in GameObject.FindGameObjectsWithTag("Door"))
            {
                Doors.Add(gameobject);
            }
            foreach (var patroller in GameObject.FindGameObjectsWithTag("Patroller"))
            {
                Patrollers.Add(patroller.GetComponent<EnemyBehaviour>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        playerDir = player.transform.position - transform.position;
    }

    IEnumerator ShootRoutine(float rof)
    {
        yield return new WaitForSeconds(1/rof);
        RaycastHit playerHit;
        if (Physics.Raycast(transform.position, playerDir.normalized, out playerHit,  Mathf.Infinity,layer))
        {
            if (playerHit.collider.CompareTag("Player"))
            {
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Rigidbody>().AddForce((player.transform.position - transform.position).normalized * fireSpeed);
            }
        }
        StartCoroutine(ShootRoutine(rateOfFire));
    }

    bool PlayerOnVision()
    {
        RaycastHit playerHit;
        if (Physics.Raycast(transform.position, playerDir.normalized, out playerHit))
        {
            if (playerHit.collider.CompareTag("Player"))
            {
                awareOfPlayer = true;
                lastPlayerPos = player.transform.position;
                if (isStatic)
                {
                    foreach( var patroller in Patrollers)
                    {
                        patroller.alerted = true;
                        patroller.lastPlayerPos = player.transform.position;
                    }
                    foreach (var door in Doors)
                    {
                        if (door.GetComponent<Door>().isOpen == true)
                        {
                            door.transform.parent.GetComponent<Animator>().SetBool("Open", false);
                            door.GetComponent<Door>().isOpen = false;
                            door.GetComponent<Door>().canBeOpenedByPlayer = false;
                        }
                            
                    }
                    
                }
                
                return true;
            }
            else
            {
                foreach (var patroller in Patrollers)
                {
                    patroller.alerted = false;
                    
                }
            }

        }
        ;
        return false;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == ("PlayerBullet"))
        {
            ChangeHealth();
        }
    }

    void ChangeHealth()
    {
        health--;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
