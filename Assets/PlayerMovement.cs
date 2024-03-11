using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Movement
    public float moveSpeed = 5f;
    public float walkSpeed = 5f;

    //Dash
    [SerializeField] private float dashCd = .5f;
    private float dashCdTimer;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashSpeedChangeFactor = 2f;
    [SerializeField] private float dashDuration = .35f;
    [SerializeField] private float dashForce = 20f;


    //References
    private Rigidbody2D rb;
    [SerializeField] private GameObject crossHair;

    //Input Control
    private PlayerControls playerControls;
    private InputAction move;
    private InputAction dodge;
    private InputAction look;

    //State Control
    public enum MovementState{
        moving,
        dodging,
        attacking,
        stunned
    }
    public MovementState state = MovementState.moving;
    private bool iFrames = false;

    private void OnEnable() {
        move = playerControls.Player.Move;
        move.Enable(); 

        dodge = playerControls.Player.Dodge;
        dodge.Enable();
        dodge.performed += Dodge;

        look = playerControls.Player.Look;
        look.Enable();
    }
    private void OnDisable() {
        move.Disable();
        dodge.Disable();
        look.Disable();
    }
    // Start is called before the first frame update
    void Awake()
    {
        playerControls = new PlayerControls();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        SpeedControl();
        StateHandler();
        if (dashCdTimer > 0) dashCdTimer -= Time.deltaTime;
    }

    private void FixedUpdate() {
        MovePlayer();
        CrossHairAim();
    }

    private void SpeedControl()
    {
        Vector2 flatVel = new Vector2(rb.velocity.x, rb.velocity.y);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector2 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector2(limitedVel.x, rb.velocity.y);
        }
    }
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private void StateHandler()
    {
        //Mode - Hit
        if(state == MovementState.stunned){
            desiredMoveSpeed = 0;
        }
        // Mode - Dodge
        else if (state == MovementState.dodging)
        {
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        //Mode = Attack
        else if(state == MovementState.attacking){
            
        }
        
        //Mode = Moving
        else{
            desiredMoveSpeed = walkSpeed;
        }


        if(iFrames){
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;
        }else {
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        }


        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dodging && dashCdTimer <= 0) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void MovePlayer(){
        //Do not move if dodging
        if(state == MovementState.dodging) return;

        Vector2 moveDirection = move.ReadValue<Vector2>();

        rb.MovePosition((Vector2)transform.position + moveDirection * moveSpeed * Time.deltaTime);
    }
    private void Dodge(InputAction.CallbackContext context)
    {
        if(dashCdTimer > 0) return;

        dashCdTimer = dashCd;

        state = MovementState.dodging;
        iFrames = true;

        Vector2 dashDir = move.ReadValue<Vector2>();
        Vector2 forceToApply = dashDir * dashForce;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector2 delayedForceToApply;
    private void DelayedDashForce()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(delayedForceToApply, ForceMode2D.Impulse);
    }

    private void ResetDash()
    {
        state = MovementState.moving;
        iFrames = false;
    }

    private void CrossHairAim(){
        Vector2 aim = look.ReadValue<Vector2>();
        if(GetComponent<PlayerInput>().currentControlScheme == "Gamepad"){
            if(aim != Vector2.zero){
                aim.Normalize();
                aim *= 6f;
                crossHair.transform.localPosition = aim;
                crossHair.SetActive(true);
            }
            else{
                crossHair.SetActive(false);
            }
        }else{
            aim = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            crossHair.transform.position = aim;
        }


    }
}
