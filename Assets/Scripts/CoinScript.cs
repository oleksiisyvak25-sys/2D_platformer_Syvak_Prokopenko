using UnityEngine;

public class CoinScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<CollectingCoins>(out CollectingCoins collector))
        {
            collector.IncreaseNumberOfCoins(1);
            Destroy(gameObject);
        }
    }
}
