using UnityEngine;

public class KillZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerHealth>(out PlayerHealth health))
        {
            health.Kill();
        }
    }
}

