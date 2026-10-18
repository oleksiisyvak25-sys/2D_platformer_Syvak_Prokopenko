using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HeavyBomb : MonoBehaviour
{
    [SerializeField] private int damage = 2;
    [SerializeField] private float missDestroyDelay = 0.8f;

    private BossFightManager fightManager;
    private bool isDetonated = false;

    public void Initialize(BossFightManager manager)
    {
        fightManager = manager;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDetonated) return;

        if (collision.gameObject.CompareTag("Boss"))
        {
            isDetonated = true;

            var boss = collision.gameObject.GetComponent<BossArenaController>();
            if (boss != null)
            {
                boss.TakeBombDamage(damage);
            }

            if (fightManager != null)
            {
                fightManager.OnBossHitByBomb();
            }

            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(HandleMissedDrop());
        }
    }

    private IEnumerator HandleMissedDrop()
    {
        isDetonated = true;

        yield return new WaitForSeconds(missDestroyDelay);

        if (fightManager != null)
        {
            fightManager.OnBombMissed();
        }

        Destroy(gameObject);
    }
}