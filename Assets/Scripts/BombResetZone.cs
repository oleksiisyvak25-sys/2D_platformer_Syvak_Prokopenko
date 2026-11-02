using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BombResetZone : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HeavyBomb bomb = other.GetComponent<HeavyBomb>();
        if (bomb == null) bomb = other.GetComponentInParent<HeavyBomb>();

        if (bomb != null)
        {
            bomb.RespawnAtTop();
        }
    }

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}