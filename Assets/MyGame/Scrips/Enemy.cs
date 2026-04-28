using UnityEngine;

public class Enemy : MonoBehaviour, IcanTakeDamage
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Animation Settings")]
    public Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        if (anim == null) anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Trien khai ham tu interface IcanTakeDamage
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} nhan {damage} damage. Mau con: {currentHealth}");

        // Trigger hit animation
        if (anim != null)
        {
            anim.SetTrigger("isHit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Enemy da chet!");

        // Trigger death animation
        if (anim != null)
        {
            anim.SetTrigger("isDead");
        }

        // Tat va cham
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Xoa object sau khi animation ket thuc
        Destroy(gameObject, 3f);
    }
}
