using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Vector2 input;

    float speed = 10;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);

        if (input != Vector2.zero)
            navMeshAgent.ResetPath();

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward = camForward.normalized;
        camRight = camRight.normalized;
        

        navMeshAgent.Move((camForward*input.y + camRight*input.x)*Time.deltaTime*speed);


        if (Input.GetButtonDown("Jump"))
        {
            navMeshAgent.destination = Vector3.zero;
        }
    }
}
