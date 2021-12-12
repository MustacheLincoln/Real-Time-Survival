using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
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
    float tick = .5f;
    float t;

    Vector2 input;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 intent;
    Vector3 velocity;
    float turnSpeed;
    Vector3 currentPos;
    Vector3 lastPos;
    bool isMoving;
    public GameObject nearestPickUp;
    GameObject pickingUp;

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
        crouchButton = KeyCode.Joystick1Button1;

        state = State.Idle;
    }

    private void Update()
    {
        CaptureInput();
        CalculateCamera();

        t -= Time.deltaTime;
        if (t <= 0)
        {
            GetNearestInteraction();
            t = tick;
        }

        currentPos = transform.position;
        isMoving = (currentPos != lastPos);
        lastPos = currentPos;

        if (isMoving)
        {
            if (Input.GetKey(runKey) || Input.GetKey(runButton))
            {
                speed = runSpeed;
                state = State.Running;
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
                GetNearestInteraction();
                if (nearestPickUp)
                {
                    if (nearestPickUp.name != "Door")
                        pickingUp = nearestPickUp;
                }
                if (pickingUp)
                    navMeshAgent.destination = pickingUp.transform.position;
            }
        }

        if (Input.GetKeyDown(pickUpKey) || Input.GetKeyDown(pickUpButton))
        {
            GetNearestInteraction();
            if (nearestPickUp)
            {
                if (nearestPickUp.name == "Door")
                    nearestPickUp.GetComponent<Door>().Interact();
                else
                    pickingUp = nearestPickUp;
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
                    vitals.Eat(cals);
            }
        }
    }


    private void GetNearestInteraction()
    {
        Collider[] hitPickUps = Physics.OverlapSphere(transform.position, 5, 1 << LayerMask.NameToLayer("Interactable"));
        if (hitPickUps.Length > 0)
        {
            Transform tMin = null;
            float minDist = Mathf.Infinity;
            foreach (Collider pickUp in hitPickUps)
            {
                float dist = Vector3.Distance(pickUp.transform.position, transform.position);
                if (dist < minDist)
                {
                    tMin = pickUp.transform;
                    minDist = dist;
                }
                nearestPickUp = tMin.gameObject;
            }
        }
        else
            nearestPickUp = null;
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

}
