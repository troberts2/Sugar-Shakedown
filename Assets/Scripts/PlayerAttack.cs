using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    //Attacking
    private float attackCd;
    [SerializeField] private float attackCdTimer = .1f;
    [SerializeField] private float bulletSpeed = 50f;
    //references
    private PlayerMovement pm;
    [SerializeField] private Transform crossHair;
    [SerializeField] private GameObject bulletPrefab;

    //Player Input
    private PlayerControls playerControls;
    private InputAction fire;

    private void Start() {
        pm = GetComponent<PlayerMovement>(); 
    }

    private void Awake() {
        playerControls = new PlayerControls();
    }
    private void OnEnable() {
        fire = playerControls.Player.Fire;
        fire.Enable();
    }
    private void OnDisable() {
        fire.Disable();
    }

    private void Update() {
        if(attackCd >= 0) attackCd -= Time.deltaTime;
        else Attack();
        
    }

    private void Attack(){
        if(fire.IsPressed()){
            attackCd = attackCdTimer;

            Vector2 fireDirection = (crossHair.position - transform.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            StartCoroutine(DestoryBullet(bullet));
            bullet.GetComponent<Rigidbody2D>().AddForce(fireDirection * bulletSpeed, ForceMode2D.Impulse);
            
        }
    }
    private IEnumerator DestoryBullet(GameObject bullet){
        yield return new WaitForSeconds(5f);
        if(bullet != null){
            Destroy(bullet);
        }
    }
}
