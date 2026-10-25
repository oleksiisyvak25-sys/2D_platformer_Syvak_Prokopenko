using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    [SerializeField] private Transform destinationPoint;
    [SerializeField] private bool requireInteractKey = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInside = false;
    private GameObject playerObj;

    private void Update()
    {
        if (isPlayerInside && requireInteractKey && Input.GetKeyDown(interactKey))
        {
            PerformTeleport(playerObj);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerObj = collision.gameObject;
            isPlayerInside = true;

            if (!requireInteractKey)
            {
                PerformTeleport(playerObj);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerObj = null;
        }
    }

    private void PerformTeleport(GameObject player)
    {
        if (destinationPoint != null && player != null)
        {
            if (ScreenFadeTransition.Instance != null)
            {
                ScreenFadeTransition.Instance.TeleportWithFade(player, destinationPoint);
            }
            else
            {
                player.transform.position = destinationPoint.position;
                RespawnManager.HasEnteredCastle = true;
            }
        }
    }
}