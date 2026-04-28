using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;  
    public float runSpeed = 6f;   
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Animation Settings")]
    public Animator anim;

    [Header("Gun Settings")]
    public GameObject gunObject; // Kéo GameObject cây súng vào đây
    public GunFire gunFire;      // Kéo script GunFire (gắn trên gun model) vào đây

    private Rigidbody rb;
    private bool isGrounded;
    private bool isAiming = false;
    private bool isShooting = false;
    private bool isReloading = false;
    private float lastShootTime = -1f;
    private const float shootCooldown = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; 

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        // Tu dong tim GunFire neu chua keo vao Inspector
        if (gunFire == null)
        {
            gunFire = GetComponentInChildren<GunFire>();
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0, z).normalized;
        
        bool isMoving = move.magnitude >= 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Kiểm tra animation isShoot đã xong chưa
        if (isShooting)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("W1_Stand_Fire_Single") || stateInfo.normalizedTime >= 1f)
                isShooting = false;
        }

        // Kiểm tra trạng thái reload
        if (isReloading)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
            // Đợi ít nhất 0.1s để animation kịp chuyển sang trạng thái "isReload"
            bool isCurrentlyAnimatingReload = stateInfo.IsName("isReload");
            bool animFinished = isCurrentlyAnimatingReload && stateInfo.normalizedTime >= 0.95f;
            
            // Script GunFire báo đã nạp xong đạn
            bool scriptFinished = (gunFire == null) || !gunFire.IsReloading;

            // Chỉ cho phép kết thúc reload khi cả Script và Anim đều xong
            if (scriptFinished && (animFinished || !isCurrentlyAnimatingReload))
            {
                // Thêm một khoảng nghỉ nhỏ để tránh spam
                isReloading = false;
            }
        }

        // Khóa di chuyển trong suốt thời gian bắn + reload + cooldown
        bool isInShootSequence = isShooting || isReloading || (Time.time < lastShootTime + shootCooldown);

        if (!isInShootSequence)
        {
            if (isMoving)
            {
                float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                rb.linearVelocity = new Vector3(moveDir.x * currentSpeed, rb.linearVelocity.y, moveDir.z * currentSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
        else
        {
            // Đứng yên trong suốt shoot + cooldown
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            // Trigger jump animation
            if (anim != null)
            {
                anim.SetTrigger("isJump");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isAiming = !isAiming;
            if (anim != null)
                anim.SetBool("isAim", isAiming);
        }

        if (Input.GetMouseButtonDown(0) && !isShooting && !isReloading && Time.time >= lastShootTime + shootCooldown)
        {
            if (anim != null)
            {
                if (!isAiming)
                {
                    isAiming = true;
                    anim.SetBool("isAim", true);
                }
                else
                {
                    // Kiểm tra GunFire còn đạn không trước khi trigger animation
                    bool canFire = (gunFire == null) || (!gunFire.IsReloading && gunFire.CurrentAmmo > 0);
                    if (canFire)
                    {
                        isShooting = true;
                        lastShootTime = Time.time;
                        anim.SetTrigger("isShoot");

                        // Bắn đạn thật (Đã chuyển sang OnFireEvent trong GunController để khớp animation)
                        // if (gunFire != null) gunFire.TryShoot();
                    }
                    else if (gunFire != null && gunFire.CurrentAmmo <= 0 && !gunFire.IsReloading && !isReloading)
                    {
                        // Hết đạn → tự động reload
                        isReloading = true;
                        anim.SetTrigger("isReload");
                        gunFire.TryReload();
                        Debug.Log("[Player] Tu dong reload do het dan.");
                    }
                }
            }
        }

        // Nhấn Ctrl để reload (chỉ khi đang cầm súng / isAiming)
        if (Input.GetKeyDown(KeyCode.LeftControl) && isAiming && !isShooting && !isReloading)
        {
            if (anim != null)
            {
                isReloading = true;
                anim.SetTrigger("isReload");

                // Trigger reload vật lý (nạp lại đạn)
                if (gunFire != null)
                    gunFire.TryReload();
            }
        }

        // Ẩn/hiện súng: chỉ hiện khi Aim, Shoot hoặc Reload
        bool shouldShowGun = isAiming || isShooting || isReloading;
        if (gunObject != null)
            gunObject.SetActive(shouldShowGun);

        if (anim != null)
        {
            if (isAiming)
            {
                // Đang Aim + di chuyển → bật isMoveAim (chỉ vào được qua isAim)
                anim.SetBool("isMoveAim", isMoving);

                // Tắt walk/run khi đang Aim
                anim.SetBool("isWalk", false);
                anim.SetBool("isRun", false);
            }
            else
            {
                // Không Aim → tắt isMoveAim
                anim.SetBool("isMoveAim", false);

                // Walk / Run bình thường
                anim.SetBool("isWalk", isMoving);
                anim.SetBool("isRun", isMoving && isRunning);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}