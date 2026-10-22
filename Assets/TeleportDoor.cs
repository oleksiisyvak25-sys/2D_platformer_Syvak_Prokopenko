using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    [SerializeField] private Transform destinationPoint;

    [SerializeField] private bool requireInteractKey = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInside = false;
    private GameObject playerObj;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerObj = collision.gameObject;

            if (!requireInteractKey)
            {
                Teleport();
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

    private void Update()
    {
        if (isPlayerInside && requireInteractKey && Input.GetKeyDown(interactKey))
        {
            Teleport();
        }
    }

    private void Teleport()
    {
        if (destinationPoint == null || playerObj == null) return;

        var rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        playerObj.transform.position = destinationPoint.position;

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(destinationPoint.position.x, destinationPoint.position.y, -10f);
        }
    }
}