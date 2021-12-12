using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour, IDamageable<float>, INoiseEmittable
{
    NavMeshAgent navMeshAgent;
    PlayerVitals vitals;
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

    Vector2 input;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 intent;
    Vector3 velocity;
    float turnSpeed;
    Vector3 currentPos;
    Vector3 lastPos;
    GameObject pickingUp;
    float tickTime = .5f;
    float tick;
    float pulseTime = .5f;
    float pulse;

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
        cam = Camera.main;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.acceleration = acceleration;

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

        tick -= Time.deltaTime;
        if (tick <= 0)
        {
            NearestInteraction();
            tick = tickTime;
        }

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
                if (NearestInteraction())
                {
                    if (NearestInteraction().name != "Door")
                        pickingUp = NearestInteraction();
                }
                if (pickingUp)
                    navMeshAgent.destination = pickingUp.transform.position;
            }
        }

        if (Input.GetKeyDown(pickUpKey) || Input.GetKeyDown(pickUpButton))
        {
            if (NearestInteraction())
            {
                if (NearestInteraction().name == "Door")
                {
                    if (Vector3.Distance(transform.position, NearestInteraction().transform.position) < grabDistance)
                        NearestInteraction().GetComponent<Door>().Interact();
                }
                else
                    pickingUp = NearestInteraction();
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
            //target = NearestVisibleTarget();
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
                    return;
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

    private GameObject NearestInteraction()
    {
        Collider[] hitInteractions = Physics.OverlapSphere(transform.position, 5, 1 << LayerMask.NameToLayer("Interactable"));
        if (hitInteractions.Length > 0)
        {
            Collider closest = null;
            float closestDist = Mathf.Infinity;
            foreach (Collider interaction in hitInteractions)
            {
                float dist = Vector3.Distance(interaction.transform.position, transform.position);
                if (dist < closestDist)
                {
                    closest = interaction;
                    closestDist = dist;
                }
            }
            return closest.gameObject;
        }
        else
            return null;
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
