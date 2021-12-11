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

    float speed = 10;
    float acceleration = 12.5f;
    float turnSpeedLow = 7;
    float turnSpeedHigh = 15;

    Vector2 input;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 intent;
    Vector3 velocity;
    float turnSpeed;

    KeyCode eatKey;
    KeyCode eatButton;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        vitals = GetComponent<PlayerVitals>();
        cam = Camera.main;
        navMeshAgent.speed = speed;
        navMeshAgent.acceleration = acceleration;

        eatKey = KeyCode.Space;
        eatButton = KeyCode.Joystick1Button0;
    }

    private void Update()
    {
        CaptureInput();
        CalculateCamera();

        intent = camForward * input.y + camRight * input.x;

        turnSpeed = Mathf.Lerp(turnSpeedHigh, turnSpeedLow, velocity.magnitude / 5);

        if (input.magnitude > 0)
        {
            navMeshAgent.ResetPath();
            Quaternion rot = Quaternion.LookRotation(intent);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }

        velocity = Vector3.Lerp(velocity, transform.forward * input.magnitude * speed, acceleration * Time.deltaTime);

        navMeshAgent.Move(velocity*Time.deltaTime);


        if (Input.GetKeyDown(eatKey) || Input.GetKeyDown(eatButton))
        {
            float cals = 10;
            if (vitals.calories < vitals.maxCalories - cals)
                vitals.Eat(cals);
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

}
