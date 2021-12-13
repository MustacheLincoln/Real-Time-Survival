using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable<float>
{
    public Image reloadProgressBar;
    public Image aimProgressBar;
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
    float idleRadius = 1;
    float walkRadius = 5;
    float runRadius = 10;
    float crouchRadius = 2;
    float noiseSphereRadius;

    bool isAiming;
    float rangedAttackDamage = 100;
    float rangedAttackSpeed = .01f;
    float rangedAttackNoise = 20;
    float rangedAttackCooldown;
    float rangedAttackRange = 20;
    float rangedKnockback = .25f;
    bool rangedAttackAutomatic;
    int magazineSize = 5;
    int inMagazine;
    float reloadTime = 2;
    float reloadTimeElapsed;
    float aimTime = 1;
    public float aimTimeElapsed;
    bool reloading;
    bool chambered;

    float meleeAttackDamage = 50;
    float meleeAttackSpeed = .5f;
    float meleeAttackNoise = 6;
    float meleeAttackCooldown;
    float meleeAttackRange = .5f;
    float meleeKnockback = .5f;


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
    KeyCode reloadKey;
    KeyCode reloadButton;

    Coroutine reload;

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
        reloading = false;
        inMagazine = magazineSize;

        pickUpKey = KeyCode.Space;
        pickUpButton = KeyCode.Joystick1Button0;
        runKey = KeyCode.LeftShift;
        runButton = KeyCode.Joystick1Button5;
        crouchKey = KeyCode.LeftControl;
        crouchButton = KeyCode.Joystick1Button4;
        reloadKey = KeyCode.Space;
        reloadButton = KeyCode.Joystick1Button0;

        state = State.Idle;
        StartCoroutine(EmitNoise());
    }

    private void Update()
    {
        for (int action = (int)KeyCode.Backspace; action <= (int)KeyCode.Joystick8Button19; action++)
        {
            if (Input.GetKeyDown((KeyCode)action) && ((KeyCode)action).ToString().Contains("Joystick"))
            {
                string controllerNumber = ((KeyCode)action).ToString().Substring(8, 2);
                if (controllerNumber.EndsWith("B"))
                {
                    controllerNumber = controllerNumber.Substring(0, 1);
                }
                Debug.Log("This is Joystick Number " + controllerNumber);
            }
        }

        var pos = transform.position;
        reloadProgressBar.transform.position = pos;
        reloadProgressBar.fillAmount = reloadTimeElapsed/reloadTime;

        if (isAiming)
        {
            if (target)
            {
                var targetPos = target.transform.position;
                aimProgressBar.transform.position = targetPos;
                aimProgressBar.fillAmount = aimTimeElapsed / aimTime;
            }
            else
                aimProgressBar.fillAmount = 0;
        }
        else
            aimProgressBar.fillAmount = 0;


        CaptureInput();
        CalculateCamera();

        rangedAttackCooldown -= Time.deltaTime;
        meleeAttackCooldown -= Time.deltaTime;

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
                if (isAiming == false)
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
        }

        if (Input.GetKeyDown(pickUpKey) || Input.GetKeyDown(pickUpButton))
        {
            if (isAiming == false)
            {
                if (fov.target)
                {
                    if (fov.target.name == "Door")
                    {
                        fov.target.GetComponent<Door>().Interact();
                    }
                    else
                        pickingUp = fov.target;
                }
            }
        }

        if (pickingUp)
        {
            if (isAiming == false)
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
            fov.radius = rangedAttackRange;
            fov.angle = 45;
            fov.targetMask = LayerMask.GetMask("Zombie");

            target = fov.target;

            if (target)
                if (reloading == false)
                    Aim();
            if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
            {
                if (target && chambered)
                {
                    RangedAttack(target);
                    chambered = false;
                }
            }
            else
                chambered = true;

            if (Input.GetKeyDown(reloadKey) || Input.GetKeyDown(reloadButton))
                if (inMagazine < magazineSize)
                    reload = StartCoroutine(Reload());
        }
        else
        {
            isAiming = false;
            fov.radius = fovRadius;
            fov.angle = fovAngle;
            fov.targetMask = LayerMask.GetMask("Interactable");
            target = null;

            if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
            {
                MeleeAttack();
            }
        }
    }

    private void MeleeAttack()
    {
        if (meleeAttackCooldown <= 0)
        {
            InterruptReload();
            Collider[] hitZombies = Physics.OverlapSphere(transform.position + transform.forward, meleeAttackRange, 1 << LayerMask.NameToLayer("Zombie"));
            if (hitZombies.Length > 0)
            {
                foreach (Collider zombie in hitZombies)
                {
                    zombie.gameObject.GetComponent<IDamageable<float>>().TakeDamage(meleeAttackDamage);
                    zombie.gameObject.GetComponent<NavMeshAgent>().Move((zombie.transform.position - transform.position).normalized * meleeKnockback);
                }
            }
            EmitUniqueNoise(meleeAttackNoise);
            meleeAttackCooldown = meleeAttackSpeed;
        }
    }

    private void RangedAttack(GameObject target)
    {
        if (aimTimeElapsed >= aimTime)
        {
            if (rangedAttackCooldown <= 0)
            {
                if (inMagazine > 0)
                {
                    if (reloading == false)
                    {
                        target.GetComponent<IDamageable<float>>().TakeDamage(rangedAttackDamage);
                        target.gameObject.GetComponent<NavMeshAgent>().Move((target.transform.position - transform.position).normalized * rangedKnockback);
                        EmitUniqueNoise(rangedAttackNoise);
                        inMagazine -= 1;
                        rangedAttackCooldown = rangedAttackSpeed;
                        aimTimeElapsed = 0;

                    }
                }
                else
                {
                    reload = StartCoroutine(Reload()); //Or Click??
                }

            }
        }
    }

    private IEnumerator Reload()
    {
        if (reloading == false)
        {
            reloading = true;
            reloadTimeElapsed = 0;
            aimTimeElapsed = 0;
            while (reloadTimeElapsed < reloadTime)
            {
                reloadTimeElapsed += Time.deltaTime;
                yield return null;
            }
            inMagazine = magazineSize;
            reloadTimeElapsed = 0;
            reloading = false;
        }
    }

    public void InterruptReload()
    {
        if (reloading)
            StopCoroutine(reload);
        reloading = false;
        reloadTimeElapsed = 0;
    }

    private void Aim()
    {
        if (aimTimeElapsed < aimTime)
        {
            aimTimeElapsed += Time.deltaTime;
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
                InterruptReload();
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

        if (isAiming)
            speed = 0;

    }

    private bool IsMoving()
    {
        currentPos = transform.position;
        bool isMoving = (currentPos != lastPos);
        lastPos = currentPos;
        return isMoving;
    }

    public IEnumerator EmitNoise()
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
        yield return new WaitForSeconds(pulseTime);
        StartCoroutine(EmitNoise());
    }

    private void EmitUniqueNoise(float volume)
    {
        Collider[] hitZombies = Physics.OverlapSphere(transform.position, volume, 1 << LayerMask.NameToLayer("Zombie"));
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
