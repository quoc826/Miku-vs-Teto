using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("VFX Settings")]
    public ParticleSystem muzzleFlashVFX;

    void Start()
    {
        if (muzzleFlashVFX != null)
            muzzleFlashVFX.Stop();
    }

    // Hàm này được gọi bởi Animation Event
    // Gắn event vào đúng frame nhân vật bắn trong clip "W1_Stand_Fire_Single"
    public void OnFireEvent()
    {
        if (muzzleFlashVFX != null)
        {
            muzzleFlashVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlashVFX.Play();
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayGunShot();
    }

    // Gắn event vào frame bắt đầu tiếng reload trong clip "isReload"
    public void OnReloadEvent()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayReload();
    }
}
