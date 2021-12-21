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
    NavMeshAgent navMeshAgent;
    public PlayerVitals vitals;
    public FieldOfView fov;
    Camera cam;
    Animator animator;

    float pickUpTime = .25f;
    float pickUpTimeElapsed;
    public Item pickUpTarget;
    public GameObject target;

    public float speed;
    float walkSpeed = 2;
    float runSpeed = 4;
    float crouchWalkSpeed = .75f;
    float acceleration = 50;
    float turnSpeedLow = 7;
    float turnSpeedHigh = 15;
    float grabDistance = 1.4f;
    float idleRadius = 1;
    float walkRadius = 5;
    float runRadius = 10;
    float crouchRadius = 1;
    float noiseSphereRadius;

    float rangedAttackCooldown;
    public float reloadTimeElapsed;
    public float aimTimeElapsed;
    public RangedWeapon rangedWeaponEquipped;
    bool roundChambered;
    public bool weaponChanged = false;
    public int pistolAmmo;
    public int rifleAmmo;

    float meleeAttackCooldown;
    public MeleeWeapon meleeWeaponEquipped;

    public List<RangedWeapon> rangedWeapons;
    public List<MeleeWeapon> meleeWeapons;
    public List<Item> items; //Make Food inherit Item class?

    public bool itemSelectionChanged;

    public Item itemSelected;
    public float eatingTimeElapsed;
    public float caloriesInInventory;
    public float millilitersInInventory;

    Vector2 input;
    Vector3 camForward;
    Vector3 camRight;
    Vector3 intent;
    Vector3 velocity;
    float turnSpeed;
    Vector3 currentPos;
    private bool isMoving = false;
    Vector3 lastPos;
    float pulseTime = .5f;

    float fovRadius = 4;
    float fovAngle = 250;

    public enum MovementState { Idle, Walking, Running, Crouching, CrouchWalking, Holding }
    public MovementState movementState;
    public enum ActionState { Idle, Reloading, Aiming, PickingUp, Eating }
    public ActionState actionState;

    private void Awake() { Instance = this; }

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        vitals = GetComponent<PlayerVitals>();
        fov = GetComponent<FieldOfView>();
        animator = GetComponentInChildren<Animator>();
        fov.radius = fovRadius;
        fov.angle = fovAngle;
        fov.targetMask = LayerMask.GetMask("Interactable");
        cam = Camera.main;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.acceleration = acceleration;

        movementState = MovementState.Idle;
        StartCoroutine(EmitNoisePulse());
    }

    private void Update()
    {
        CalculateIsMoving();
        CaptureInput();
        CalculateCamera();
        ActionStateMachine();
        MovementStateMachine();
        Animate();

        rangedAttackCooldown -= Time.deltaTime;
        meleeAttackCooldown -= Time.deltaTime;

        velocity = Vector3.Lerp(velocity, transform.forward * input.magnitude * speed, acceleration * Time.deltaTime);

        navMeshAgent.Move(velocity*Time.deltaTime);

        turnSpeed = Mathf.Lerp(turnSpeedHigh, turnSpeedLow, velocity.magnitude / 5);

        navMeshAgent.speed = speed;
    }

    private void Animate()
    {
        animator.SetBool("isWalking", (movementState == MovementState.Walking));
        animator.SetBool("isRunning", (movementState == MovementState.Running));
        animator.SetBool("isCrouching", (movementState == MovementState.Crouching));
        animator.SetBool("isCrouchWalking", (movementState == MovementState.CrouchWalking));
    }

    private void MovementStateMachine()
    {
        switch (movementState)
        {
            case MovementState.Idle:
                speed = walkSpeed;
                noiseSphereRadius = idleRadius;
                break;
            case MovementState.Walking:
                speed = walkSpeed;
                noiseSphereRadius = idleRadius + walkRadius * input.magnitude;
                break;
            case MovementState.Running:
                speed = runSpeed;
                noiseSphereRadius = idleRadius + runRadius * input.magnitude;
                break;
            case MovementState.Crouching:
                speed = crouchWalkSpeed;
                noiseSphereRadius = idleRadius;
                break;
            case MovementState.CrouchWalking:
                speed = crouchWalkSpeed;
                noiseSphereRadius = idleRadius + crouchRadius * input.magnitude;
                break;
            case MovementState.Holding:
                speed = 0;
                noiseSphereRadius = idleRadius;
                break;
        }
    }

    private void ActionStateMachine()
    {
        switch (actionState)
        {
            case ActionState.Idle:
                aimTimeElapsed = 0;
                reloadTimeElapsed = 0;
                pickUpTimeElapsed = 0;
                fov.radius = fovRadius;
                fov.angle = fovAngle;
                fov.targetMask = LayerMask.GetMask("Interactable");
                target = null;
                if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
                    MeleeAttack();
                if (Input.GetAxis("ChangeWeapon") < 0)
                {
                    if (weaponChanged == false)
                        ChangeMeleeWeapon();
                }
                else if (Input.GetAxis("ChangeWeapon") > 0)
                {
                    if (weaponChanged == false)
                    ChangeRangedWeapon();
                }
                else
                    weaponChanged = false;
                if (Input.GetAxis("Inventory") < 0)
                {
                    if (itemSelectionChanged == false)
                        ChangeItemSelectedDown();
                }
                else if (Input.GetAxis("Inventory") > 0)
                {
                    if (itemSelectionChanged == false)
                        ChangeItemSelectedUp();
                }
                else
                    itemSelectionChanged = false;
                if (Input.GetButtonDown("Eat"))
                    if (itemSelected)
                        if (itemSelected.GetComponent<Food>() != null)
                            actionState = ActionState.Eating;
                break;
            case ActionState.Reloading:
                aimTimeElapsed = 0;
                target = null;
                reloadTimeElapsed += Time.deltaTime;
                if (reloadTimeElapsed >= rangedWeaponEquipped.reloadTime)
                {
                    if (rangedWeaponEquipped.name == "Pistol" && pistolAmmo >= rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine)
                    {
                        pistolAmmo -= rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine;
                        rangedWeaponEquipped.inMagazine = rangedWeaponEquipped.magazineSize;
                    }
                    else if (rangedWeaponEquipped.name == "Pistol" && pistolAmmo < rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine)
                    {
                        rangedWeaponEquipped.inMagazine += pistolAmmo;
                        pistolAmmo = 0;
                    }
                    if (rangedWeaponEquipped.name == "Rifle" && rifleAmmo >= rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine)
                    {
                        rifleAmmo -= rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine;
                        rangedWeaponEquipped.inMagazine = rangedWeaponEquipped.magazineSize;
                    }
                    else if (rangedWeaponEquipped.name == "Rifle" && rifleAmmo < rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine)
                    {
                        rangedWeaponEquipped.inMagazine += rifleAmmo;
                        rifleAmmo = 0;
                    }
                    reloadTimeElapsed = 0;
                    actionState = ActionState.Idle;
                }
                break;
            case ActionState.Aiming:
                movementState = MovementState.Holding;
                pickUpTarget = null;
                pickUpTimeElapsed = 0;
                if (rangedWeaponEquipped)
                    fov.radius = rangedWeaponEquipped.rangedAttackRange;
                fov.angle = 45;
                if (Input.GetButtonDown("Reload"))
                    if (rangedWeaponEquipped.inMagazine < rangedWeaponEquipped.magazineSize)
                    {
                        if (rangedWeaponEquipped.name == "Pistol" && pistolAmmo > 0)
                            actionState = ActionState.Reloading;
                        else if (rangedWeaponEquipped.name == "Rifle" && rifleAmmo > 0)
                            actionState = ActionState.Reloading;
                    }
                if (fov.target)
                    if (fov.target.name != "Zombie")
                        fov.target = null;
                fov.targetMask = LayerMask.GetMask("Zombie");
                target = fov.target;
                if (rangedWeaponEquipped == null)
                    break;
                if (rangedWeaponEquipped.inMagazine > 0)
                {
                    if (target)
                        if (aimTimeElapsed < rangedWeaponEquipped.aimTime)
                            aimTimeElapsed += Time.deltaTime;
                }
                else
                {
                    aimTimeElapsed = 0;
                    if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
                        if (rangedWeaponEquipped.name == "Pistol" && pistolAmmo > 0)
                            actionState = ActionState.Reloading;
                        else if (rangedWeaponEquipped.name == "Rifle" && rifleAmmo > 0)
                            actionState = ActionState.Reloading;
                }
                if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
                {
                    if (target)
                        if (roundChambered || rangedWeaponEquipped.fullAuto)
                            RangedAttack(target);
                }
                else
                    roundChambered = true;
                if (Input.GetMouseButton(1))
                {
                    input = Vector2.zero;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, int.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
                        transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
                break;
            case ActionState.PickingUp:
                aimTimeElapsed = 0;
                reloadTimeElapsed = 0;
                movementState = MovementState.Holding;
                pickUpTimeElapsed += Time.deltaTime;
                if (pickUpTimeElapsed >= pickUpTime)
                {
                    if (pickUpTarget)
                        pickUpTarget.PickUp();
                    pickUpTarget = null;
                    pickUpTimeElapsed = 0;
                    CalculateFoodInInventory();
                    actionState = ActionState.Idle;
                }
                break;
            case ActionState.Eating:
                Food food = itemSelected as Food;
                bool edible;
                if (food.calories > food.milliliters)
                    edible = (vitals.calories < vitals.maxCalories - food.calories);
                else
                    edible = (vitals.milliliters < vitals.maxMilliliters - food.milliliters);
                if (edible == false)
                    break;
                aimTimeElapsed = 0;
                reloadTimeElapsed = 0;
                movementState = MovementState.Holding;
                pickUpTarget = null;
                target = null;
                eatingTimeElapsed += Time.deltaTime;
                if (eatingTimeElapsed >= food.eatingTime)
                {
                    if (itemSelected)
                    {
                        itemSelected.GetComponent<Food>().Eat();
                        int index = items.IndexOf(itemSelected);
                        items.Remove(itemSelected);
                        if (items.Count > 0)
                            itemSelected = items[0];
                        if (items.Count <= 0)
                            itemSelected = null;
                    }
                    CalculateFoodInInventory();
                    eatingTimeElapsed = 0;
                    actionState = ActionState.Idle;
                }
                break;
        }
    }

    private void ChangeMeleeWeapon()
    {
        if (meleeWeaponEquipped)
        {
            int i = meleeWeapons.IndexOf(meleeWeaponEquipped);
            if (i == meleeWeapons.Count - 1)
                i = -1;
            meleeWeaponEquipped = meleeWeapons[i + 1];
            weaponChanged = true;
        }
    }

    private void ChangeRangedWeapon()
    {
        if (rangedWeaponEquipped)
        {
            int i = rangedWeapons.IndexOf(rangedWeaponEquipped);
            if (i == rangedWeapons.Count - 1)
                i = -1;
            rangedWeaponEquipped = rangedWeapons[i + 1];
            weaponChanged = true;
        }
    }

    private void ChangeItemSelectedUp()
    {
        if (items.Count > 0)
        {
            int i = items.IndexOf(itemSelected);
            if (i == items.Count - 1)
                i = -1;
            itemSelected = items[i + 1];
            itemSelectionChanged = true;
        }
    }

    private void ChangeItemSelectedDown()
    {
        if (items.Count > 0)
        {
            int i = items.IndexOf(itemSelected);
            if (i == 0)
                i = items.Count;
            itemSelected = items[i - 1];
            itemSelectionChanged = true;
        }
    }

    private void MeleeAttack()
    {
        if (meleeWeaponEquipped && meleeAttackCooldown <= 0)
        {
            Collider[] hitZombies = Physics.OverlapSphere(transform.position + transform.forward, meleeWeaponEquipped.meleeAttackRange, 1 << LayerMask.NameToLayer("Zombie"));
            if (hitZombies.Length > 0)
            {
                foreach (Collider zombie in hitZombies)
                {
                    zombie.gameObject.GetComponent<IDamageable<float>>().TakeDamage(meleeWeaponEquipped.meleeAttackDamage);
                    zombie.gameObject.GetComponent<NavMeshAgent>().Move((zombie.transform.position - transform.position).normalized * meleeWeaponEquipped.meleeKnockback);
                }
                if (meleeWeaponEquipped.durability > 0)
                    meleeWeaponEquipped.durability -= 1;
                else
                {
                    meleeWeapons.Remove(meleeWeaponEquipped);
                    if (meleeWeapons.Count > 0)
                        meleeWeaponEquipped = meleeWeapons[0];
                    Destroy(meleeWeaponEquipped);
                }
            }
            EmitNoiseUnique(meleeWeaponEquipped.meleeAttackNoise);
            meleeAttackCooldown = meleeWeaponEquipped.meleeAttackSpeed;
        }
    }

    private void RangedAttack(GameObject target)
    {
        if (aimTimeElapsed >= rangedWeaponEquipped.aimTime && rangedAttackCooldown <= 0)
        {
            if (rangedWeaponEquipped.inMagazine > 0)
            {
                roundChambered = false;
                target.GetComponent<IDamageable<float>>().TakeDamage(rangedWeaponEquipped.rangedAttackDamage);
                target.GetComponent<NavMeshAgent>().Move((target.transform.position - transform.position).normalized * rangedWeaponEquipped.rangedKnockback);
                EmitNoiseUnique(rangedWeaponEquipped.rangedAttackNoise);
                rangedWeaponEquipped.inMagazine -= 1;
                rangedAttackCooldown = rangedWeaponEquipped.rangedAttackSpeed;
                if (rangedWeaponEquipped.boltAction)
                    aimTimeElapsed = 0;
            }
        }
    }

    private void CalculateIsMoving()
    {
        currentPos = transform.position;
        isMoving = (currentPos != lastPos);
        lastPos = currentPos;
    }

    public IEnumerator EmitNoisePulse()
    {
        Collider[] hitZombies = Physics.OverlapSphere(transform.position, noiseSphereRadius, 1 << LayerMask.NameToLayer("Zombie"));
        if (hitZombies.Length > 0)
        {
            foreach (Collider zombie in hitZombies)
                    zombie.gameObject.GetComponent<Zombie>().StartChase(gameObject);
        }
        yield return new WaitForSeconds(pulseTime);
        StartCoroutine(EmitNoisePulse());
    }

    private void EmitNoiseUnique(float volume)
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

        if (Input.GetMouseButton(1) || Input.GetAxis("Aim") > 0)
        {
            movementState = MovementState.Holding;
                if (actionState != ActionState.Reloading)
                    actionState = ActionState.Aiming;
        }
        else
        {
            if (actionState == ActionState.Aiming)
                actionState = ActionState.Idle;
            if (isMoving)
            {
                if (Input.GetButton("Run"))
                {
                    if (vitals.stamina > 1)
                        movementState = MovementState.Running;
                    else
                        movementState = MovementState.Walking;
                    actionState = ActionState.Idle;
                }
                else if (Input.GetButton("Crouch"))
                    movementState = MovementState.CrouchWalking;
                else
                    movementState = MovementState.Walking;
            }
            else if (Input.GetButton("Crouch"))
                movementState = MovementState.Crouching;
            else
                movementState = MovementState.Idle;
        }

         if (input.magnitude > 0)
        {
            navMeshAgent.ResetPath();
            pickUpTarget = null;
            intent = camForward * input.y + camRight * input.x;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(intent), turnSpeed * Time.deltaTime);
        }
        else
        {
            if (Input.GetButton("PickUp"))
            {
                if (fov.target && actionState != ActionState.Aiming)
                {
                    actionState = ActionState.Idle;
                    if (fov.target.name != "Door")
                        pickUpTarget = fov.target.GetComponent<Item>();
                }
                if (pickUpTarget)
                    navMeshAgent.destination = pickUpTarget.transform.position;
            }
        }

        if (Input.GetButtonDown("PickUp"))
            if (fov.target)
                if (fov.target.name == "Door")
                {
                    actionState = ActionState.Idle;
                    fov.target.GetComponent<Door>().Interact();
                }

        if (pickUpTarget)
            if (Vector3.Distance(transform.position, pickUpTarget.transform.position) < grabDistance)
                actionState = ActionState.PickingUp;
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
        PlayerPrefs.DeleteAll();
    }

    public void Eat(float cals)
    {
        vitals.calories += cals;
    }

    public void TakeDamage(float damage)
    {
        vitals.health -= damage;
        vitals.maxHealth -= damage / 10;
    }

    private void CalculateFoodInInventory()
    {
        caloriesInInventory = 0;
        millilitersInInventory = 0;
        if (items.Count > 0)
        {
            foreach (Food food in items)
            {
                caloriesInInventory += food.calories;
                millilitersInInventory += food.milliliters;
            }
        }
    }
}
