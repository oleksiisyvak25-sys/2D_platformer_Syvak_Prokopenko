using UnityEngine;

public class HazardSpikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Kill();
            }
            else if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerGameOver();
            }
        }
    }
}