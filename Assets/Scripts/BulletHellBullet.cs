using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHellBullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private RadialBullets radialBullets;
    private float bulletSpeed = 0;
    private bool useTheCurve;
    private BulletPatternTemplate bulletPattern;
    float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        radialBullets = FindObjectOfType<RadialBullets>();

    }
    private void Awake() {
        radialBullets = FindObjectOfType<RadialBullets>();
    }
    private void OnEnable() {
        Invoke(nameof(NoLongerStart), .01f);
        //Invoke(nameof(DisableObj), 20f);
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(bulletPattern != null){
            loopCurve();
            if(useTheCurve) rb.velocity = transform.right * bulletSpeed * bulletPattern.curve.Evaluate(time);
            else rb.velocity = transform.right * bulletSpeed;
            bulletSpeed += bulletPattern.acceleration;
        }

    }
    void loopCurve(){
        if(time < 1) time += Time.deltaTime;
        if(time > 1) time = 0;
    }
    private void OnCollisionEnter2D(Collision2D other) {
        if(other.collider.CompareTag("Ground") || other.collider.CompareTag("Player")){
            gameObject.SetActive(false);
        }
        
    }
    private void DisableObj(){
        if(gameObject.activeInHierarchy){
            gameObject.SetActive(false);
        }
    }
    private void NoLongerStart(){
        if(radialBullets != null && radialBullets.enemy.currentPattern != null){
            bulletPattern = radialBullets.enemy.currentPattern;
            bulletSpeed = bulletPattern.projectileSpeed;
            useTheCurve = bulletPattern.useCurve; 
        }

    }
}
