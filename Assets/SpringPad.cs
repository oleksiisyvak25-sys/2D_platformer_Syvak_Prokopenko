using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpringPad : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private Sprite springNormalSprite;
    [SerializeField] private Sprite springCompressedSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isBouncing = false;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBouncing) return;

        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerMovement>();
            if (player != null && player.VerticalVelocity <= 0.1f)
            {
                StartCoroutine(BounceRoutine(player));
            }
        }
    }

    private IEnumerator BounceRoutine(PlayerMovement player)
    {
        isBouncing = true;

        if (springCompressedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = springCompressedSprite;

        player.Bounce(bounceForce);

        yield return new WaitForSeconds(0.25f);

        if (springNormalSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = springNormalSprite;

        isBouncing = false;
    }
}