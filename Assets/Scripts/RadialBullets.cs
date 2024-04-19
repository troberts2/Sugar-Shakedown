using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadialBullets : MonoBehaviour
{

    internal BulletPatternTemplate currentPattern;

    private bool increase = true;
    public GameObject ProjectilePrefab; 
    public bool radial = false;        // Prefab to spawn.

    [Header("Private Variables")]
    private Vector3 startPoint;                 // Starting position of the bullet.
    private const float radius = 5F;          // Help us find the move direction.
    internal Enemy enemy;
    private void Start() {
        enemy = GetComponent<Enemy>();
    }



    // Update is called once per frame
    void Update()
    {
        if(currentPattern != null && currentPattern.rotateSpeedChangeRate > 0)   SpinSpeedChange();
    }



    public IEnumerator ShootBullets(BulletPatternTemplate currentPattern){
        this.currentPattern = currentPattern;
        
        float angleStep;
        float arrayAngleStep;

        float angle = 0f;
        float arrayAngle = 0f;

        if(radial){
            angleStep = 360 / currentPattern.numberOfProjectilesPerArray;
            arrayAngleStep = 360/ currentPattern.numOfArrays;
        }else{
            if(currentPattern.numberOfProjectilesPerArray > 1){
                angleStep = currentPattern.individualArraySpread/(currentPattern.numberOfProjectilesPerArray -1);
            }else{
                angleStep = currentPattern.individualArraySpread/currentPattern.numberOfProjectilesPerArray;
            }
            
            arrayAngleStep = currentPattern.totalArraySpread;
        }

        enemy.state = Enemy.EnemyState.attacking;
        if(currentPattern.attackRotateSpeed > 0){
            StartCoroutine(Rotate(currentPattern.attackRotateSpeed, currentPattern.rotateToAngle));
        }
        //do the attack
        for(int r = 0; r < currentPattern.repeatTimes; r++){
            //each new attack
            for(int x = 0; x < currentPattern.numOfArrays; x++){
                for(int i = 0; i < currentPattern.numberOfProjectilesPerArray; i++){
                    var proj = ObjectPool.instance.GetPooledObject();
                    //if(proj == null) yield break;
                    proj.transform.position = transform.position;
                    proj.transform.rotation = Quaternion.Euler(0, 0, angle -  arrayAngle - currentPattern.attackAngleOffset);
                    proj.transform.rotation *= transform.rotation;
                    proj.SetActive(true);
                    angle += angleStep;
                }
                arrayAngle += arrayAngleStep;
                angle = 0;
            } 
            yield return null;
            angle = 0f;
            arrayAngle = 0f;
            yield return new WaitForSeconds(currentPattern.fireRate);
        }
        enemy.state = Enemy.EnemyState.moving;

    }
    void SpinSpeedChange(){
        if(increase){
            currentPattern.attackRotateSpeed =Mathf.Lerp(currentPattern.attackRotateSpeed, currentPattern.maxSpinSpeed, currentPattern.rotateSpeedChangeRate * Time.deltaTime);
            if(currentPattern.attackRotateSpeed>= currentPattern.maxSpinSpeed - .05f) increase = false;
        }else{
            currentPattern.attackRotateSpeed =Mathf.Lerp(currentPattern.attackRotateSpeed, -currentPattern.maxSpinSpeed, currentPattern.rotateSpeedChangeRate * Time.deltaTime);
            if(currentPattern.attackRotateSpeed <= -currentPattern.maxSpinSpeed + .05f) increase = true;
        }
    }
    IEnumerator Rotate(float duration, float rotateDegree)
    {
        float startRotation = transform.eulerAngles.z;
        float endRotation = startRotation + 360.0f;
        float t = 0.0f;
        while ( t  < duration )
        {
            t += Time.deltaTime;
            float zRotation = Mathf.Lerp(startRotation, endRotation, t / duration) % rotateDegree;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, zRotation);
            yield return null;
        }
    }
}
public class BulletPatternTemplate{
    internal int numberOfProjectilesPerArray;  
    internal int individualArraySpread;  
    internal int numOfArrays;   
    internal int totalArraySpread;
    internal float projectileSpeed;               // Speed of the projectile.
    internal float acceleration;
    internal AnimationCurve curve;
    internal bool useCurve;
    internal float attackRotateSpeed;
    internal float rotateSpeedChangeRate;
    internal float rotateToAngle;
    internal float fireRate;
    internal float maxSpinSpeed;
    internal int repeatTimes;
    internal float attackAngleOffset;

    public BulletPatternTemplate(BulletPattern bulletPattern){
        numberOfProjectilesPerArray = bulletPattern.numberOfProjectilesPerArray;
        individualArraySpread = bulletPattern.individualArraySpread;
        numOfArrays = bulletPattern.numOfArrays;
        totalArraySpread = bulletPattern.totalArraySpread;
        projectileSpeed = bulletPattern.projectileSpeed;
        acceleration = bulletPattern.acceleration;
        curve = bulletPattern.curve;
        useCurve = bulletPattern.useCurve;
        attackRotateSpeed = bulletPattern.attackRotateSpeed;
        rotateSpeedChangeRate = bulletPattern.rotateSpeedChangeRate;
        rotateToAngle = bulletPattern.rotateToAngle;
        fireRate = bulletPattern.fireRate;
        maxSpinSpeed = bulletPattern.maxSpinSpeed;
        repeatTimes = bulletPattern.repeatTime;

        attackAngleOffset = bulletPattern.GetAngleOffset();
    }
}
