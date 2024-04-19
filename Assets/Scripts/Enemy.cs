using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private Transform target;
    [SerializeField] private float visionDistance = 10f;
    [SerializeField] private float activeDistance = 20f;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private GameObject sugar;
    private float attackCdTimer;
    [SerializeField] private float maxHp = 3f;
    private float hp;
    private Color originalColor;

    public enum EnemyState{
        moving,
        attacking,
        stunned
    }

    public EnemyState state = EnemyState.moving;
    [SerializeField] internal EnemySettings enemySettings;
    internal BulletPatternTemplate currentPattern;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.stoppingDistance = maxDistance;
        agent.speed = moveSpeed;
        maxHp = enemySettings.enemyMaxHp;
        hp = maxHp;
        originalColor = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        target = FindObjectOfType<PlayerMovement>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position, visionDistance, ~ignoreLayers);

        if(attackCdTimer >= 0) attackCdTimer -= Time.deltaTime;

        if(state != EnemyState.attacking){
            FollowPlayerOrientation();
            if(maxDistance != 0 && Vector2.Distance(transform.position, target.position) < maxDistance){
                transform.position = Vector2.MoveTowards(transform.position, target.position, -moveSpeed * Time.deltaTime);
            }
            else if(hit && hit.collider.CompareTag("Player")){
                Attack();
            }
            else if(activeDistance != 0 && Vector2.Distance(transform.position, target.position) < activeDistance){
                agent.SetDestination(target.position);
            } 
        }

        
    }
    
    void FollowPlayerOrientation(){
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x -transform.position.x ) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Attack(){
        if(attackCdTimer > 0) return;

        if(enemySettings.bulletPatterns.Length != 0){
            currentPattern = new BulletPatternTemplate(enemySettings.bulletPatterns[Random.Range(0, enemySettings.bulletPatterns.Length)]);
        }

        attackCdTimer = (currentPattern.repeatTimes * currentPattern.fireRate) + enemySettings.secondsBetweenAttacks;
        
        if(GetComponent<RadialBullets>() != null){
            StartCoroutine(GetComponent<RadialBullets>().ShootBullets(currentPattern));
        }
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
            //Enemy dies
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            Instantiate(sugar, transform.position, Quaternion.identity);
            Destroy(gameObject, .1f);
        }else{
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            Invoke(nameof(ResetColor), .1f);
        }
    }

    void ResetColor(){
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = originalColor;
    }
}
