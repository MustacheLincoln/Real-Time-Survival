using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour, IDamageable<float>
{
    public static Player Instance { get; private set; }

    public Image reloadProgressBar;
    public Image aimProgressBar;
    public TMP_Text targetLabel;
    NavMeshAgent navMeshAgent;
    PlayerVitals vitals;
    FieldOfView fov;
    Camera cam;

    float pickUpTime = .25f;
    float pickUpTimeElapsed;

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
    public bool hasRangedWeapon;
    public float rangedAttackDamage = 100;
    public float rangedAttackSpeed = .01f;
    public float rangedAttackNoise = 20;
    public float rangedAttackRange = 20;
    public float rangedKnockback = .25f;
    public bool rangedAttackAutomatic = false;
    public int magazineSize = 5;
    public float reloadTime = 2;
    public float aimTime = 1;
    float rangedAttackCooldown;
    int inMagazine;
    float reloadTimeElapsed;
    public float aimTimeElapsed;
    bool reloading;
    bool chambered;
    public GameObject rangedWeaponEquipped;

    public bool hasMeleeWeapon;
    public float meleeAttackDamage = 50;
    public float meleeAttackSpeed = .5f;
    public float meleeAttackNoise = 6;
    public float meleeAttackRange = .5f;
    public float meleeKnockback = .5f;
    float meleeAttackCooldown;
    public GameObject meleeWeaponEquipped;

    public List<GameObject> rangedWeapons;
    public List<GameObject> meleeWeapons;

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
    float fovAngle = 250;

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

    private void Awake()
    {
        Instance = this;
    }

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
        hasMeleeWeapon = false;
        hasRangedWeapon = false;

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

        reloadProgressBar.transform.position = transform.position;
        reloadProgressBar.fillAmount = reloadTimeElapsed/reloadTime;

        if (fov.target)
        {
            targetLabel.text = fov.target.name;
            targetLabel.transform.position = fov.target.transform.position;
        }
        else
            targetLabel.text = null;

        if (isAiming)
        {
            if (target)
            {
                aimProgressBar.transform.position = target.transform.position;
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

        if (pickingUp)
        {
            if (isAiming == false)
            {
                if (reloading == false)
                {
                    if (Vector3.Distance(transform.position, pickingUp.transform.position) < grabDistance)
                    {
                        speed = 0;
                        pickUpTimeElapsed += Time.deltaTime;
                        if (pickUpTimeElapsed >= pickUpTime)
                        {
                            pickingUp.GetComponent<IPickUpable>().PickUp();
                            pickingUp = null;
                            pickUpTimeElapsed = 0;
                            //float cals = 10;
                            //if (vitals.calories < vitals.maxCalories - cals)
                            //Eat(cals);
                        }
                    }
                }
            }
        }
        else
            pickUpTimeElapsed = 0;

        if (Input.GetAxis("ChangeWeapon") > 0)
        {
            if (hasRangedWeapon)
                if (rangedWeaponEquipped != rangedWeapons[0])
                    rangedWeapons[0].GetComponent<RangedWeapon>().Equip();
        }

        if (Input.GetMouseButton(1) || Input.GetAxis("Aim") > 0)
        {
            if (hasRangedWeapon)
            {
                isAiming = true;
                pickingUp = null;
                fov.radius = rangedAttackRange;
                fov.angle = 45;

                if (fov.target)
                    if (fov.target.name != "Zombie")
                        fov.target = null;
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
        }
        else
        {
            isAiming = false;
            fov.radius = fovRadius;
            fov.angle = fovAngle;
            fov.targetMask = LayerMask.GetMask("Interactable");
            target = null;
            aimTimeElapsed = 0;

            if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
            {
                MeleeAttack();
            }
        }
    }

    private void MeleeAttack()
    {
        if (hasMeleeWeapon)
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
                        target.GetComponent<NavMeshAgent>().Move((target.transform.position - transform.position).normalized * rangedKnockback);
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
        if (hasRangedWeapon)
        {
            if (aimTimeElapsed < aimTime)
            {
                aimTimeElapsed += Time.deltaTime;
            }
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
                noiseSphereRadius = idleRadius + walkRadius * input.magnitude;
                break;
            case State.Running:
                noiseSphereRadius = idleRadius + runRadius * input.magnitude;
                break;
            case State.Crouching:
                noiseSphereRadius = idleRadius + crouchRadius * input.magnitude;
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
