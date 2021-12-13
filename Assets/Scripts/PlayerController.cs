using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour, IDamageable<float>, INoiseEmittable
{
    NavMeshAgent navMeshAgent;
    PlayerVitals vitals;
    FieldOfView fov;
    Camera cam;

    float speed;
    float walkSpeed = 3;
    float runSpeed = 6;
    float crouchSpeed = 1.5f;
    float acceleration = 50;
    float turnSpeedLow = 7;
    float turnSpeedHigh = 15;
    float grabDistance = 1.5f;
    float idleRadius = 2;
    float walkRadius = 5;
    float runRadius = 10;
    float crouchRadius = 3;
    float noiseSphereRadius;

    bool isAiming;
    float attackDamage = 100;
    float attackSpeed = .01f;
    float attackCooldown;
    bool chambered;


    Vector2 input;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 intent;
    Vector3 velocity;
    float turnSpeed;
    Vector3 currentPos;
    Vector3 lastPos;
    GameObject pickingUp;
    GameObject target;
    float pulseTime = .5f;
    float pulse;

    float fovRadius = 4;
    float fovAngle = 200;

    KeyCode pickUpKey;
    KeyCode pickUpButton;
    KeyCode runKey;
    KeyCode runButton;
    KeyCode crouchKey;
    KeyCode crouchButton;

    public enum State { Idle, Walking, Running, Crouching }
    public State state;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        vitals = GetComponent<PlayerVitals>();
        fov = GetComponent<FieldOfView>();
        fov.radius = fovRadius;
        fov.angle = fovAngle;
        fov.targetMask = LayerMask.GetMask("Interactable");
        cam = Camera.main;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.acceleration = acceleration;
        isAiming = false;
        chambered = true;

        pickUpKey = KeyCode.Space;
        pickUpButton = KeyCode.Joystick1Button0;
        runKey = KeyCode.LeftShift;
        runButton = KeyCode.Joystick1Button5;
        crouchKey = KeyCode.LeftControl;
        crouchButton = KeyCode.Joystick1Button4;

        state = State.Idle;
    }

    private void Update()
    {
        CaptureInput();
        CalculateCamera();

        attackCooldown -= Time.deltaTime;

        MovementType();

        velocity = Vector3.Lerp(velocity, transform.forward * input.magnitude * speed, acceleration * Time.deltaTime);

        navMeshAgent.Move(velocity*Time.deltaTime);

        turnSpeed = Mathf.Lerp(turnSpeedHigh, turnSpeedLow, velocity.magnitude / 5);

        navMeshAgent.speed = speed;

        if (input.magnitude > 0)
        {
            navMeshAgent.ResetPath();
            pickingUp = null;
            intent = camForward * input.y + camRight * input.x;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(intent), turnSpeed * Time.deltaTime);
        }
        else
        {
            if (Input.GetKey(pickUpKey) || Input.GetKey(pickUpButton))
            {
                if (fov.target)
                {
                    if (fov.target.name != "Door")
                        pickingUp = fov.target;
                }
                if (pickingUp)
                    navMeshAgent.destination = pickingUp.transform.position;
            }
        }

        if (Input.GetKeyDown(pickUpKey) || Input.GetKeyDown(pickUpButton))
        {
            if (fov.target)
            {
                if (fov.target.name == "Door")
                {
                    if (Vector3.Distance(transform.position, fov.target.transform.position) < grabDistance)
                        fov.target.GetComponent<Door>().Interact();
                }
                else
                    pickingUp = fov.target;
            }
        }

        if (pickingUp)
        {
            if (Vector3.Distance(transform.position, pickingUp.transform.position) < grabDistance)
            {
                Destroy(pickingUp);
                pickingUp = null;
                float cals = 10;
                if (vitals.calories < vitals.maxCalories - cals)
                    Eat(cals);
            }
        }

        pulse -= 1 * Time.deltaTime;
        if (pulse <= 0)
        {
            EmitNoise();
            pulse = pulseTime;
        }

        if (Input.GetMouseButton(1) || Input.GetAxis("Aim") > 0)
        {
            isAiming = true;
            fov.radius = 20;
            fov.angle = 120;
            fov.targetMask = LayerMask.GetMask("Zombie");
            target = fov.target;
            if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
            {
                if (target)
                    if (chambered)
                    {
                        Attack(target);
                        chambered = false;
                    }
            }
            else
                chambered = true;
        }
        else
        {
            isAiming = false;
            fov.radius = fovRadius;
            fov.angle = fovAngle;
            fov.targetMask = LayerMask.GetMask("Interactable");
            target = null;
        }
    }

    private void Attack(GameObject target)
    {
        if (attackCooldown <= 0)
        {
            target.GetComponent<IDamageable<float>>().TakeDamage(attackDamage);
            attackCooldown = attackSpeed;
        }
    }

    private void MovementType()
    {
        if (IsMoving())
        {
            if (Input.GetKey(runKey) || Input.GetKey(runButton))
            {
                if (vitals.stamina > 1)
                {
                    speed = runSpeed;
                    state = State.Running;
                }
                else
                {
                    speed = walkSpeed;
                    state = State.Walking;
                }
            }
            else if (Input.GetKey(crouchKey) || Input.GetKey(crouchButton))
            {
                speed = crouchSpeed;
                state = State.Crouching;
            }
            else
            {
                speed = walkSpeed;
                state = State.Walking;
            }
        }
        else
        {
            speed = walkSpeed;
            state = State.Idle;
        }
    }

    private bool IsMoving()
    {
        currentPos = transform.position;
        bool isMoving = (currentPos != lastPos);
        lastPos = currentPos;
        return isMoving;
    }

    public void EmitNoise()
    {
        switch (state)
        {
            case State.Idle:
                noiseSphereRadius = idleRadius;
                break;
            case State.Walking:
                noiseSphereRadius = walkRadius;
                break;
            case State.Running:
                noiseSphereRadius = runRadius;
                break;
            case State.Crouching:
                noiseSphereRadius = crouchRadius;
                break;
        }
        Collider[] hitZombies = Physics.OverlapSphere(transform.position, noiseSphereRadius, 1 << LayerMask.NameToLayer("Zombie"));
        if (hitZombies.Length > 0)
        {
            foreach (Collider zombie in hitZombies)
                    zombie.gameObject.GetComponent<Zombie>().StartChase(gameObject);
        }
    }

    private void CaptureInput()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);
    }

    private void CalculateCamera()
    {
        camForward = cam.transform.forward;
        camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void Eat(float cals)
    {
        vitals.calories += cals;
    }

    public void TakeDamage(float damage)
    {
        vitals.health -= damage;
    }

}
