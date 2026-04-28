using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 20f;
    public GameObject hitEffectPrefab;

    private bool hasHit = false; // Tranh tinh trang 1 vien dan gay damage nhieu lan

    private void OnCollisionEnter(Collision collision)
    {
        ProcessHit(collision.gameObject, collision.contacts[0].point, collision.contacts[0].normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Doi voi Trigger thi lay vi tri hien tai cua dan lam diem va cham
        ProcessHit(other.gameObject, transform.position, -transform.forward);
    }

    private void ProcessHit(GameObject targetObj, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (hasHit) return;

        // Bo qua neu trung vao chinh Player ban ra
        if (targetObj.CompareTag("Player")) return;

        hasHit = true;
        Debug.Log("[Bullet] Va cham voi: " + targetObj.name);

        // Tim Interface IcanTakeDamage
        IcanTakeDamage target = targetObj.GetComponent<IcanTakeDamage>();
        if (target == null)
        {
            // Neu khong co tren chinh no, tim o cac lop cha (truong hop collider nam o xuong con)
            target = targetObj.GetComponentInParent<IcanTakeDamage>();
        }

        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log("<color=red><b>[HIT] BẮN TRÚNG KẺ ĐỊCH!</b></color> - Target: " + targetObj.name);
        }
        else
        {
            Debug.Log("[Bullet] Doi tuong nay khong co script Enemy hoac IcanTakeDamage.");
        }

        // Hieu ung va cham
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
        }

        // Huy vien dan
        Destroy(gameObject);
    }
}
