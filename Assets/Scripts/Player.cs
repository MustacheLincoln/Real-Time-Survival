using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour, IDamageable<float>
{
    public static Player Instance { get; private set; }
    GameManager gameManager;
    NavMeshAgent navMeshAgent;
    public PlayerVitals vitals;
    public FieldOfView fov;
    Camera cam;
    Animator animator;
    public Transform backpackAttachPoint;
    public Transform rightHandHoldPoint;
    public Transform leftHandHoldPoint;
    public Transform largeMeleeWeaponAttachPoint;
    public Transform largeRangedWeaponAttachPoint;
    public Transform smallMeleeWeaponAttachPoint;
    public Transform smallRangedWeaponAttachPoint;
    public Backpack backpackEquipped;

    float pickUpTime = .25f;
    float interactTimeElapsed;

    public float danger = 0;
    public float speed;
    float walkSpeed = 1.5f;
    float runSpeed = 4;
    float crouchWalkSpeed = .75f;
    float acceleration = 50;
    float turnSpeedLow = 7;
    float turnSpeedHigh = 15;
    float interactDistance = 1.4f;
    float idleRadius = 1;
    float walkRadius = 2;
    float runRadius = 6;
    float crouchRadius = 1;
    float noiseSphereRadius = 1;
    public int inventorySize = 4;
    public bool meleeAttacking = false;
    bool inventoryPressed = false;
    bool pickUpPressed;
    public float searchTimeElapsed;

    float rangedAttackCooldown;
    public float reloadTimeElapsed;
    public float aimTimeElapsed;
    public RangedWeapon rangedWeaponEquipped;
    bool roundChambered;

    float meleeAttackCooldown;
    public MeleeWeapon meleeWeaponEquipped;

    public List<Item> items;

    public int pistolAmmo;
    public int rifleAmmo;

    public Food eating;
    public Item inspecting;
    public Item interacting;
    public GameObject navigatingTo;
    public RangedWeapon reloading;
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
    public bool isMoving;
    public bool isAiming;
    public bool isHoldingLargeRanged;
    public bool isHoldingLargeMelee;
    Vector3 lastPos;
    float pulseTime = .5f;

    float fovRadius = 4;
    float fovAngle = 250;

    public enum MovementState { Idle, Walking, Running, Crouching, CrouchWalking, Holding }
    public MovementState movementState;

    private void Awake()
    {
        Instance = this;
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

        Load();

        currentPos = transform.position;
        lastPos = currentPos;

        movementState = MovementState.Idle;
        StartCoroutine(EmitNoisePulse());
    }

    private void Load()
    {
        navMeshAgent.Warp(ES3.Load("playerPosition", Vector3.zero));
        transform.rotation = ES3.Load("playerRotation", Quaternion.identity);
        danger = ES3.Load("playerDanger", 0f);
        rangedWeaponEquipped = ES3.Load("playerRangedWeaponEquipped", rangedWeaponEquipped);
        roundChambered = ES3.Load("playerRoundChambered", roundChambered);
        meleeWeaponEquipped = ES3.Load("playerMeleeWeaponEquipped", meleeWeaponEquipped);
        items = ES3.Load("playerItems", items);
        itemSelected = ES3.Load("playerItemSelected", itemSelected);
        backpackEquipped = ES3.Load("backpackEquipped", backpackEquipped);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        CalculateFoodInInventory();
        CalculateAmmoInInventory();
    }

    private void Update()
    {
        CalculateIsMoving();
        CaptureInput();
        CalculateCamera();
        MovementStateMachine();
        Animate();

        rangedAttackCooldown -= Time.deltaTime;

        velocity = Vector3.Lerp(velocity, transform.forward * input.magnitude * speed, acceleration * Time.deltaTime);

        navMeshAgent.Move(velocity * Time.deltaTime);

        turnSpeed = Mathf.Lerp(turnSpeedHigh, turnSpeedLow, velocity.magnitude / 5);

        navMeshAgent.speed = speed;
    }

    private void Animate()
    {
        animator.SetBool("isHoldingLargeRanged", isHoldingLargeRanged);
        animator.SetBool("isHoldingLargeMelee", isHoldingLargeMelee);
        animator.SetBool("isWalking", movementState == MovementState.Walking);
        animator.SetBool("isRunning", movementState == MovementState.Running);
        animator.SetBool("isCrouching", movementState == MovementState.Crouching);
        animator.SetBool("isCrouchWalking", movementState == MovementState.CrouchWalking);
        animator.SetBool("isAiming", isAiming);
        switch (movementState)
        {
            case MovementState.Idle:
                animator.speed = 1;
                break;
            case MovementState.Crouching:
                animator.speed = 1;
                break;
            case MovementState.Walking:
                animator.speed = input.magnitude;
                break;
            case MovementState.Running:
                animator.speed = input.magnitude;
                break;
            case MovementState.CrouchWalking:
                animator.speed = input.magnitude;
                break;
            case MovementState.Holding:
                animator.speed = 1;
                break;
        }
        if (input.magnitude == 0)
            animator.speed = 1;
        if (isAiming)
            animator.speed = 1;
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

    public IEnumerator Reloading(RangedWeapon weapon, Ammo ammo = null)
    {
        if (ammo == null)
            foreach (Item item in items)
                if (item.GetComponent<Ammo>())
                    if (item.GetComponent<Ammo>().ammoType == weapon.ammoType)
                    {
                        ammo = item as Ammo;
                        reloading = null;
                        break;
                    }
        if (ammo == null || weapon.inMagazine >= weapon.magazineSize || reloading)
        {
            reloading = null;
            yield break;
        }
        if (ammo.ammoType != weapon.ammoType)
        {
            reloading = null;
            yield break;
        }
        reloading = weapon;
        while (reloadTimeElapsed < weapon.reloadTime)
        {
            aimTimeElapsed = 0;
            reloadTimeElapsed += Time.deltaTime;
            yield return null;
        }
        int reloadAmount = Mathf.Min((rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine), ammo.amount);
        ammo.amount -= reloadAmount;
        rangedWeaponEquipped.inMagazine += reloadAmount;
        reloading = null;
        if (rangedWeaponEquipped.magazineSize - rangedWeaponEquipped.inMagazine > 0)
        {
            StartCoroutine(Reloading(weapon));
            yield break;
        }
        reloadTimeElapsed = 0;
        CalculateAmmoInInventory();
    }

    private void RefillAmmo(Ammo ammo)
    {
        Ammo inventoryAmmo = null;
        foreach (Item item in items)
            if (item.GetComponent<Ammo>())
                if (item.GetComponent<Ammo>().ammoType == ammo.ammoType && item.GetComponent<Ammo>().amount < item.GetComponent<Ammo>().maxAmount)
                {
                    inventoryAmmo = item as Ammo;
                    break;
                }
        if (inventoryAmmo == null)
            return;
        int refillAmount = Mathf.Min((inventoryAmmo.maxAmount - inventoryAmmo.amount), ammo.amount);
        ammo.amount -= refillAmount;
        inventoryAmmo.amount += refillAmount;
        CalculateAmmoInInventory();
        if (ammo.amount > 0)
        {
            RefillAmmo(ammo);
        }
        else
            ammo.Destroy();
    }

    private IEnumerator Interacting(Item item, string method)
    {
        if (interacting)
            yield break;
        interacting = item;
        HolsterWeapon();
        aimTimeElapsed = 0;
        reloadTimeElapsed = 0;
        movementState = MovementState.Holding;
        interactTimeElapsed = 0;
        while (interactTimeElapsed < pickUpTime)
        {
            interactTimeElapsed += Time.deltaTime;
            yield return null;
            if (isMoving)
            {
                interactTimeElapsed = 0;
                interacting = null;
                movementState = MovementState.Idle;
                yield break;
            }
        }
        if (method == "pickUp")
            PickUp(item);
        else if (method == "inspect")
            StartCoroutine(Inspect(item));
        else if (method == "use")
            Use(item);
        movementState = MovementState.Idle;
        interactTimeElapsed = 0;
        interacting = null;
    }

    public IEnumerator Eat(Food food)
    {
        bool edible;
        if (food.calories > food.milliliters)
            edible = (vitals.calories < vitals.maxCalories - food.calories);
        else
            edible = (vitals.milliliters < vitals.maxMilliliters - food.milliliters);
        if (edible == false)
            yield break;
        if (eating)
            yield break;
        eating = food;
        HolsterWeapon();
        aimTimeElapsed = 0;
        reloadTimeElapsed = 0;
        while (eatingTimeElapsed < food.eatingTime)
        {
            eatingTimeElapsed += Time.deltaTime;
            yield return null;
            if (isMoving)
            {
                eatingTimeElapsed = 0;
                eating = null;
                yield break;
            }
        }
        vitals.calories += food.calories;
        vitals.milliliters += food.milliliters;
        RemoveItem(food, false);
        food.Destroy();
        CalculateFoodInInventory();
        eating = null;
        eatingTimeElapsed = 0;
    }

    internal void RemoveItem(Item item, bool isReplacingEquipment)
    {
        int indexModifier = 0;
        if (isReplacingEquipment)
            indexModifier = -1;
        if (items.Contains(item))
        {
            int index = items.IndexOf(item) + indexModifier;
            items.Remove(item);
            if (items.Count > 0)
            {
                if (index < items.Count - 1)
                    itemSelected = items[Mathf.Max(0, index)];
                else
                    itemSelected = items[items.Count - 1];
            }
            else
                itemSelected = null;
        }

    }

    public IEnumerator Inspect(Item target)
    {
        if (inspecting)
            yield break;
        inspecting = target;
        fov.target = null;
        HolsterWeapon();
        while (inspecting)
        {
            yield return null;
            if (isMoving)
            {
                inspecting = null;
                yield break;
            }
        }
    }

    public void HolsterWeapon()
    {
        isHoldingLargeRanged = false;
        isHoldingLargeMelee = false;
        if (rangedWeaponEquipped)
        {
            if (rangedWeaponEquipped.large)
            {
                rangedWeaponEquipped.transform.position = largeRangedWeaponAttachPoint.position;
                rangedWeaponEquipped.transform.rotation = largeRangedWeaponAttachPoint.rotation;
                rangedWeaponEquipped.transform.parent = largeRangedWeaponAttachPoint;
            }
            else
            {
                rangedWeaponEquipped.transform.position = smallRangedWeaponAttachPoint.position;
                rangedWeaponEquipped.transform.rotation = smallRangedWeaponAttachPoint.rotation;
                rangedWeaponEquipped.transform.parent = smallRangedWeaponAttachPoint;
            }
        }
        if (meleeWeaponEquipped)
        {
            if (meleeWeaponEquipped.large)
            {
                meleeWeaponEquipped.transform.position = largeMeleeWeaponAttachPoint.position;
                meleeWeaponEquipped.transform.rotation = largeMeleeWeaponAttachPoint.rotation;
                meleeWeaponEquipped.transform.parent = largeMeleeWeaponAttachPoint;
            }
            else
            {
                meleeWeaponEquipped.transform.position = smallMeleeWeaponAttachPoint.position;
                meleeWeaponEquipped.transform.rotation = smallMeleeWeaponAttachPoint.rotation;
                meleeWeaponEquipped.transform.parent = smallMeleeWeaponAttachPoint;
            }
        }
    }

    public void DrawWeapon(Item weapon)
    {
        HolsterWeapon();
        weapon.transform.position = rightHandHoldPoint.position;
        weapon.transform.rotation = rightHandHoldPoint.rotation;
        weapon.transform.parent = rightHandHoldPoint;
    }

    private void LeaveBehind(Item item)
    {
        //Broken
        if (item.transform.parent)
            if (item.transform.parent.gameObject.GetComponent<Container>())
                item.transform.parent.gameObject.GetComponent<Container>().NextItem();
        inspecting = null;
    }

    private void ChangeItemSelectedUp()
    {
        if (items.Count > 0)
        {
            if (itemSelected == null)
            {
                itemSelected = items[0];
            }
            else
            {
                int i = items.IndexOf(itemSelected);
                if (i == items.Count - 1)
                    i = -1;
                itemSelected = items[i + 1];
            }
        }
    }

    private void ChangeItemSelectedDown()
    {
        if (items.Count > 0)
        {
            if (itemSelected == null)
            {
                itemSelected = items[items.Count - 1];
            }
            else
            {
                int i = items.IndexOf(itemSelected);
                if (i == 0)
                    i = items.Count;
                itemSelected = items[i - 1];
            }
        }
    }

    private IEnumerator MeleeAttack()
    {
        if (meleeWeaponEquipped && meleeAttacking == false)
        {
            meleeAttacking = true;
            DrawWeapon(meleeWeaponEquipped);
            isHoldingLargeMelee = meleeWeaponEquipped.large;
            animator.SetTrigger("Melee");
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
                    meleeWeaponEquipped.Break(this);
                EmitNoiseUnique(meleeWeaponEquipped.meleeAttackNoise);
            }
            meleeAttackCooldown = 0;
            while (meleeAttackCooldown < meleeWeaponEquipped.meleeAttackSpeed)
            {
                movementState = MovementState.Holding;
                meleeAttackCooldown += Time.deltaTime;
                yield return null;
            }
            movementState = MovementState.Idle;            
            meleeAttacking = false;
        }
    }

    private void RangedAttack(GameObject target)
    {
        if (rangedWeaponEquipped.inMagazine > 0)
        {
            animator.SetTrigger("Fire");
            roundChambered = false;
            target.GetComponent<IDamageable<float>>().TakeDamage(rangedWeaponEquipped.rangedAttackDamage);
            target.GetComponent<NavMeshAgent>().Move((target.transform.position - transform.position).normalized * rangedWeaponEquipped.rangedKnockback);
            EmitNoiseUnique(rangedWeaponEquipped.rangedAttackNoise);
            rangedWeaponEquipped.inMagazine -= 1;
            rangedAttackCooldown = rangedWeaponEquipped.rangedAttackSpeed;
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

    private void PickUp(Item item)
    {
        int storage = 0;
        if (backpackEquipped)
            storage = backpackEquipped.storage;
        if (items.Count < inventorySize + storage)
        {
            if (item.transform.parent)
                if (item.transform.parent.gameObject.GetComponent<Container>())
                    item.transform.parent.gameObject.GetComponent<Container>().NextItem();
            item.AddToInventory();
        }
        else
            if (item.GetComponent<Ammo>())
                RefillAmmo(item as Ammo);
        inspecting = null;
        fov.target = null;
        CalculateFoodInInventory();
        CalculateAmmoInInventory();
    }

    public void Drop(Item item)
    {
        StartCoroutine(item.Drop());
        RemoveItem(item, false);
        CalculateFoodInInventory();
        CalculateAmmoInInventory();
    }

    private void Use(Item item)
    {
        inspecting = null;
        fov.target = null;
        item.Use(this);
        //if (item.transform.parent)
            //if (item.transform.parent.gameObject.GetComponent<Container>())
                //item.transform.parent.gameObject.GetComponent<Container>().NextItem();
    }

    private void CaptureInput()
    {
        if (inspecting)
        {
            if (Input.GetButtonDown("PickUp"))
            {
                pickUpPressed = true;
                PickUp(inspecting);
            }
            else if (Input.GetButtonDown("Use"))
            {
                Use(inspecting);
            }
            else if (Input.GetButtonDown("Cancel"))
            {
                LeaveBehind(inspecting);
            }
            return;
        }

        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);

        if (Input.GetMouseButton(1) || Input.GetAxis("Aim") > 0)
        {
            if (reloading == null)
            {
                StartCoroutine(Aiming());
                if (Input.GetButtonDown("Reload"))
                {
                    pickUpPressed = true;
                    StartCoroutine(Reloading(rangedWeaponEquipped));
                }
                if (Input.GetMouseButton(1))
                {
                    input = Vector2.zero;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, int.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
                        transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
            }
        }
        else
        {
            isAiming = false;
            if (movementState != MovementState.Holding)
            {
                if (isMoving)
                {
                    if (Input.GetButton("Run"))
                    {
                        if (vitals.stamina > 1 && input.magnitude >= .99f)
                            movementState = MovementState.Running;
                        else
                            movementState = MovementState.Walking;
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
            if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
                StartCoroutine(MeleeAttack());
            if (Input.GetAxis("InventoryHorizontal") < 0)
            {
                if (inventoryPressed == false)
                {
                    inventoryPressed = true;
                    ChangeItemSelectedDown();
                } 
            }
            else if (Input.GetAxis("InventoryHorizontal") > 0)
            {
                if (inventoryPressed == false)
                {
                    inventoryPressed = true;
                    ChangeItemSelectedUp();
                }
            }
            else if (Input.GetAxis("InventoryVertical") > 0)
            {
                if (inventoryPressed == false)
                {
                    inventoryPressed = true;
                    if (itemSelected)
                        itemSelected.Use(this);
                }

            }
            else if (Input.GetAxis("InventoryVertical") < 0)
            {
                if (inventoryPressed == false)
                {
                    inventoryPressed = true;
                    if (itemSelected)
                        Drop(itemSelected);
                }
            }
            else
                inventoryPressed = false;
        }
        if (input.magnitude > 0)
        {
            navMeshAgent.ResetPath();
            intent = camForward * input.y + camRight * input.x;
            if (intent != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(intent), turnSpeed * Time.deltaTime);
        }
        else
        {
            if (fov.target && isAiming == false)
            {
                if (Input.GetButton("PickUp") && pickUpPressed == false)
                        StartCoroutine(Approach(fov.target, "pickUp"));
                else if (Input.GetButtonDown("Use"))
                        StartCoroutine(Approach(fov.target, "use"));
                else if (Input.GetButtonDown("Inspect"))
                        StartCoroutine(Approach(fov.target, "inspect"));
            }
        }

        if (Input.GetButtonDown("PickUp"))
            if (fov.target)
                if (fov.target.name == "Door")
                {
                    pickUpPressed = true;
                    fov.target.GetComponent<Door>().Interact();
                }

        if (Input.GetButtonUp("PickUp"))
            pickUpPressed = false; 
    }

    private IEnumerator Approach(GameObject target, string method)
    {
        if (navigatingTo)
            yield break;
        navigatingTo = target;
        navMeshAgent.destination = navigatingTo.transform.position;
        while (Vector3.Distance(transform.position, navigatingTo.transform.position) >= interactDistance)
        {
            yield return null;
            if (input.magnitude > 0)
            {
                navigatingTo = null;
                yield break;
            }
        }
        navMeshAgent.ResetPath();
        while (isMoving)
            yield return null;
        if (target.GetComponent<Item>())
            StartCoroutine(Interacting(target.GetComponent<Item>(), method));
        if (target.GetComponent<Container>())
            if (Input.GetButton("PickUp"))
                    Search(target.GetComponent<Container>());
        navigatingTo = null;
    }
    private IEnumerator Aiming()
    {
        if (isAiming)
            yield break;
        isAiming = true;
        if (rangedWeaponEquipped == null)
        {
            isAiming = false;
            yield break;
        }
        DrawWeapon(rangedWeaponEquipped);
        isHoldingLargeRanged = rangedWeaponEquipped.large;
        interactTimeElapsed = 0;
        aimTimeElapsed = 0;
        while (isAiming)
        {
            movementState = MovementState.Holding;
            fov.radius = rangedWeaponEquipped.rangedAttackRange;
            fov.angle = 45;
            if (fov.target)
                if (fov.target.name != "Zombie")
                    fov.target = null;
            fov.targetMask = LayerMask.GetMask("Zombie");
            if (rangedWeaponEquipped.inMagazine > 0)
            {
                if (fov.target)
                    if (aimTimeElapsed < rangedWeaponEquipped.aimTime)
                        aimTimeElapsed += Time.deltaTime;
            }
            else
            {
                aimTimeElapsed = 0;
                if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0 || Input.GetKey(KeyCode.Tab))
                    StartCoroutine(Reloading(rangedWeaponEquipped));
            }
            if (Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0)
            {
                if (fov.target)
                    if (roundChambered || rangedWeaponEquipped.gunType == RangedWeapon.GunType.FullAuto)
                        if (aimTimeElapsed >= rangedWeaponEquipped.aimTime && rangedAttackCooldown <= 0)
                        {
                            RangedAttack(fov.target);
                            if (rangedWeaponEquipped.gunType == RangedWeapon.GunType.BoltAction)
                                aimTimeElapsed = 0;
                        }
            }
            else
                roundChambered = true;
            yield return null;
        }
        movementState = MovementState.Idle;
        fov.radius = fovRadius;
        fov.angle = fovAngle;
        fov.targetMask = LayerMask.GetMask("Interactable");
        isAiming = false;
    }

    private void Search(Container container)
    {
        if (container.searched == false)
        {
            searchTimeElapsed += Time.deltaTime;
            if (searchTimeElapsed >= container.searchTime)
            {
                container.Open();
                searchTimeElapsed = 0;
            }
        }
        if (container.searched == true && container.contents.Count > 0)
        {
            container.Open();
        }
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
            foreach (Item item in items)
            {
                if (item.GetComponent<Food>())
                {
                    Food food = item as Food;
                    caloriesInInventory += food.calories;
                    millilitersInInventory += food.milliliters;
                }
            }
        }
    }

    private void CalculateAmmoInInventory()
    {
        pistolAmmo = 0;
        rifleAmmo = 0;
        if (items.Count > 0)
        {
            foreach (Item item in items)
            {
                if (item.GetComponent<Ammo>())
                {
                    Ammo ammo = item as Ammo;
                    if (ammo.ammoType == Ammo.AmmoType.Pistol)
                        pistolAmmo += ammo.amount;
                    else if (ammo.ammoType == Ammo.AmmoType.Rifle)
                        rifleAmmo += ammo.amount;
                }
            }
            int tempPistolAmmo = pistolAmmo;
            int tempRifleAmmo = rifleAmmo;
            List<Ammo> emptyBoxesToDestroy = new List<Ammo>();
            foreach (Item item in items)
            {
                if (item.GetComponent<Ammo>())
                {
                    Ammo ammo = item as Ammo;
                    if (ammo.ammoType == Ammo.AmmoType.Pistol)
                    {
                        ammo.amount = Mathf.Min(tempPistolAmmo, ammo.maxAmount);
                        tempPistolAmmo -= ammo.amount;
                    }
                    else if (ammo.ammoType == Ammo.AmmoType.Rifle)
                    {
                        ammo.amount = Mathf.Min(tempRifleAmmo, ammo.maxAmount);
                        tempRifleAmmo -= ammo.amount;
                    }
                    if (ammo.amount <= 0)
                    {
                        emptyBoxesToDestroy.Add(ammo);
                    }
                }
            }
            if (emptyBoxesToDestroy.Count > 0)
                foreach (Ammo ammo in emptyBoxesToDestroy)
                {
                    RemoveItem(ammo, false);
                    ammo.Destroy();
                }
        }
    }

    private void OnApplicationQuit()
    {
        CheckDanger();
        Save();
    }

    private void Save()
    {
        ES3.Save("playerDanger", danger);
        ES3.Save("playerPosition", transform.position);
        ES3.Save("playerRotation", transform.rotation);
        ES3.Save("playerRangedWeaponEquipped", rangedWeaponEquipped);
        ES3.Save("playerRoundChambered", roundChambered);
        ES3.Save("playerMeleeWeaponEquipped", meleeWeaponEquipped);
        ES3.Save("playerItems", items);
        ES3.Save("playerItemSelected", itemSelected);
        ES3.Save("backpackEquipped", backpackEquipped);
    }

    private void CheckDanger()
    {
        float dangerRadius = 5;
        Collider[] hitZombies = Physics.OverlapSphere(transform.position, dangerRadius, 1 << LayerMask.NameToLayer("Zombie"));
        if (hitZombies.Length > 0)
        {
            foreach (Collider zombie in hitZombies)
                danger += 10;
        }
    }
}
