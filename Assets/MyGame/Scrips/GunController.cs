using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("VFX Settings")]
    public ParticleSystem muzzleFlashVFX;

    // Tu dong tim GunFire trong cay cha
    private GunFire gunFire;

    void Start()
    {
        if (muzzleFlashVFX != null)
            muzzleFlashVFX.Stop();

        // Tim GunFire tren cung GameObject hoac cha cua no
        gunFire = GetComponent<GunFire>();
        if (gunFire == null)
            gunFire = GetComponentInParent<GunFire>();
        if (gunFire == null)
            gunFire = FindFirstObjectByType<GunFire>();

        if (gunFire == null)
            Debug.LogWarning("[GunController] Khong tim thay GunFire! Gan script GunFire vao cung object hoac cha.");
        else
            Debug.Log("[GunController] Da tim thay GunFire: " + gunFire.name);
    }

    // Ham nay duoc goi boi Animation Event
    // Gan event vao dung frame nhan vat ban trong clip "W1_Stand_Fire_Single"
    public void OnFireEvent()
    {
        // Tru dan - day la noi chac chan duoc goi khi ban
        if (gunFire != null)
            gunFire.TryShoot();

        // VFX
        if (muzzleFlashVFX != null)
        {
            muzzleFlashVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlashVFX.Play();
        }

        // Am thanh
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayGunShot();
    }

    // Gan event vao frame bat dau tieng reload trong clip "isReload"
    public void OnReloadEvent()
    {
        // Nap dan thuc su
        if (gunFire != null)
            gunFire.TryReload();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayReload();
    }
}
