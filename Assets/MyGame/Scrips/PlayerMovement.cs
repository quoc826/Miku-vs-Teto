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

        // Kiểm tra animation isReload đã xong chưa
        if (isReloading)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("isReload") || stateInfo.normalizedTime >= 1f)
                isReloading = false;
        }

        // Khóa di chuyển trong suốt thời gian bắn + cooldown 0.5s
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
                    isShooting = true;
                    lastShootTime = Time.time;
                    anim.SetTrigger("isShoot");
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