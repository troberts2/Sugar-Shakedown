using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform target;
    [SerializeField] private float visionDistance = 10f;
    [SerializeField] private float activeDistance = 20f;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float attackCd = 2f;
    private float attackCdTimer;
    [SerializeField] private float maxHp = 3f;
    private float hp;
    private Color originalColor;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.stoppingDistance = maxDistance;
        hp = maxHp;
        originalColor = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position, visionDistance, ~ignoreLayers);

        if(attackCdTimer >= 0) attackCdTimer -= Time.deltaTime;

        if(hit && hit.collider.CompareTag("Player")){
            Attack();
        }
        else if(maxDistance != 0 && Vector2.Distance(transform.position, target.position) < maxDistance){
            transform.position = Vector2.MoveTowards(transform.position, target.position, -moveSpeed * Time.deltaTime);
        }
        else if(activeDistance != 0 && Vector2.Distance(transform.position, target.position) < activeDistance){
            agent.SetDestination(target.position);
        }
        
    }

    void Attack(){
        if(attackCdTimer > 0) return;
        attackCdTimer = attackCd;
        Debug.Log("Attack");
        GameObject bullet = Instantiate(enemyBullet, transform.position, Quaternion.identity);
        Vector2 bulletDir = (target.position - transform.position).normalized;
        bullet.GetComponent<Rigidbody2D>().AddForce(bulletDir * bulletSpeed, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collider){
        if(collider.collider.CompareTag("PlayerBullet")){
            Destroy(collider.gameObject);
            TakeDamage(1f);
        }
    }

    void TakeDamage(float amount){
        hp -= amount;
        if(hp < 1){
            Destroy(gameObject);
        }else{
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            Invoke(nameof(ResetColor), .1f);
        }
    }

    void ResetColor(){
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = originalColor;
    }
}
