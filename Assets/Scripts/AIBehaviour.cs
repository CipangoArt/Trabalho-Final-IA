using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBehaviour : MonoBehaviour
{
    [SerializeField] Transform destsination;
    [SerializeField] NavMeshAgent ai;
    [SerializeField] Camera camera;
    [SerializeField] int health;
    [SerializeField] GameObject lose;
    [SerializeField] GameObject win;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireSpeed;
    [SerializeField] float rateOfFire;
    [SerializeField] CapsuleCollider cc;
    [SerializeField] float interactionRange;

    public  List<GameObject> Doors = new List<GameObject>();
    private bool canShoot = true;

    [SerializeField] OffMeshLink link;
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ROFCounter());
        foreach(var gameobject in GameObject.FindGameObjectsWithTag("Door"))
        {
            Doors.Add(gameobject);
        }
    }

    IEnumerator ROFCounter()
    {
        yield return new WaitForSeconds(1 / rateOfFire);
         canShoot = true;
        StartCoroutine(ROFCounter());
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 100f;
        mousePoint = camera.ScreenToWorldPoint(mousePoint);
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 dir = new Vector3(hit.point.x,transform.position.y,hit.point.z);
            Vector3 rotateDir = new Vector3(dir.x - transform.position.x, dir.y - transform.position.y, dir.z - transform.position.z);
            transform.forward = rotateDir;

        }

            if (Input.GetMouseButtonDown(0))
        {
            
            if(Physics.Raycast(ray, out hit))
            {
                ai.SetDestination(hit.point);
            }
        }
        if (ai.isOnOffMeshLink)
        {
            ai.CompleteOffMeshLink();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var door in Doors)
            {
                if (Vector3.Distance(door.gameObject.transform.position, transform.position) <= interactionRange&&door.GetComponent<Door>().canBeOpenedByPlayer==true)
                {
                    if (door.GetComponent<Door>().isOpen == false)
                    {
                        door.transform.parent.GetComponent<Animator>().SetBool("Open", true);
                        door.GetComponent<Door>().isOpen = true;
                    }
                    else
                    {
                        door.transform.parent.GetComponent<Animator>().SetBool("Open", false);
                        door.GetComponent<Door>().isOpen = false;
                    }
                    
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)&&canShoot)
        {

                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Rigidbody>().AddForce(transform.forward.normalized * fireSpeed);
            canShoot = false;
            
            
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == ("EnemyBullet"))
        {
            ChangeHealth();
        }
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("Win"))
        {
            win.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void ChangeHealth()
    {
        health--;
        if (health <= 0)
        {
            lose.SetActive(true);
            Time.timeScale = 0;
            Destroy(gameObject);
        }
    }

}
