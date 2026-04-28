using UnityEngine;
using System.Collections;

public class GunFire : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector References
    // ─────────────────────────────────────────────
    [Header("Bullet Settings")]
    [Tooltip("Prefab của viên đạn (có Rigidbody + Collider)")]
    public GameObject bulletPrefab;

    [Tooltip("Điểm bắn đạn ra (đầu nòng súng)")]
    public Transform firePoint;

    [Tooltip("Tốc độ bay của đạn (m/s)")]
    public float bulletSpeed = 100f;

    [Tooltip("Thời gian tồn tại của đạn trước khi tự hủy (giây)")]
    public float bulletLifetime = 3f;

    [Tooltip("Sát thương của mỗi viên đạn")]
    public float damage = 20f;

    [Header("Ammo Settings")]
    [Tooltip("Số đạn tối đa trong băng")]
    public int maxAmmo = 20;

    [Tooltip("Thời gian reload (giây)")]
    public float reloadTime = 2f;

    // ─────────────────────────────────────────────
    //  Runtime State (hien trong Inspector luc Play)
    // ─────────────────────────────────────────────
    [SerializeField] private int currentAmmo;
    [SerializeField] private bool isReloading = false;
    [SerializeField] private float currentReloadTimer = 0f; // Xem thoi gian dang cho reload

    // Tham chiếu đến PlayerMovement để đọc trạng thái isReloading animation
    private PlayerMovement playerMovement;

    // ─────────────────────────────────────────────
    //  Properties (đọc từ UI / PlayerMovement)
    // ─────────────────────────────────────────────
    public int CurrentAmmo  => currentAmmo;
    public int MaxAmmo      => maxAmmo;
    public bool IsReloading => isReloading;

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────
    void Start()
    {
        currentAmmo     = maxAmmo;
        playerMovement  = GetComponentInParent<PlayerMovement>();

        if (bulletPrefab == null)
            Debug.LogWarning("[GunFire] bulletPrefab chưa được gán!");
        if (firePoint == null)
            Debug.LogWarning("[GunFire] firePoint chưa được gán!");
    }

    // ─────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────

    /// <summary>
    /// Bắn 1 viên đạn. Trả về true nếu bắn thành công.
    /// Gọi từ PlayerMovement khi click chuột trái.
    /// </summary>
    public bool TryShoot()
    {
        Debug.Log("[GunFire] TryShoot called. Current Ammo: " + currentAmmo);
        if (isReloading)
        {
            Debug.Log("[GunFire] Đang reload, không thể bắn!");
            return false;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("[GunFire] Hết đạn! Nhấn Ctrl để reload.");
            return false;
        }

        if (bulletPrefab == null || firePoint == null)
            return false;

        // Spawn đạn
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Thêm Rigidbody nếu chưa có
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) rb = bullet.AddComponent<Rigidbody>();

        // Giup dan khong xuyen tuong khi bay nhanh
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Dam bao co script Bullet de gay damage
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript == null) bulletScript = bullet.AddComponent<Bullet>();
        
        // Truyen sat thuong tu sung sang dan
        bulletScript.damage = damage;
        
        rb.useGravity = false; 
        rb.linearVelocity = firePoint.forward * bulletSpeed;

        // Tự hủy sau bulletLifetime giây
        Destroy(bullet, bulletLifetime);

        currentAmmo--;
        Debug.Log($"[GunFire] Bắn! Đạn còn: {currentAmmo}/{maxAmmo}");

        // Hết đạn → tự động bắt đầu reload
        if (currentAmmo <= 0)
        {
            Debug.Log("[GunFire] Hết đạn, tự động reload...");
            StartCoroutine(ReloadRoutine());
        }

        return true;
    }

    /// <summary>
    /// Bắt đầu reload thủ công (nhấn Ctrl). Gọi từ PlayerMovement.
    /// </summary>
    public void TryReload()
    {
        if (isReloading || currentAmmo >= maxAmmo) return;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        currentReloadTimer = reloadTime;

        while (currentReloadTimer > 0)
        {
            currentReloadTimer -= Time.deltaTime;
            yield return null;
        }

        currentAmmo = maxAmmo;
        currentReloadTimer = 0;
        isReloading = false;
        Debug.Log("[GunFire] Reload xong! Dan: " + currentAmmo);
    }
}
