using UnityEngine;

public class HeatZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<FreezingSystem>(out FreezingSystem player))
        {
            player.AddCampFire(1);
        }


        if (collision.TryGetComponent<FrozenEnemy>(out FrozenEnemy enemy))
        {
            enemy.Unfreeze();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<FreezingSystem>(out FreezingSystem player))
        {
            player.AddCampFire(-1);
        }

    }
}
