using System.Collections;
using UnityEngine;

public class SpringPad : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite springUpSprite;
    [SerializeField] private Sprite springDownSprite;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private float compressedDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip bounceSound;
    private AudioSource audioSource;

    private SpriteRenderer spriteRenderer;
    private bool isBouncing = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (springUpSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = springUpSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBouncing)
        {
            var rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);

                StartCoroutine(BounceRoutine());

                if (bounceSound != null)
                {
                    if (audioSource != null) audioSource.PlayOneShot(bounceSound);
                    else AudioSource.PlayClipAtPoint(bounceSound, transform.position);
                }
            }
        }
    }

    private IEnumerator BounceRoutine()
    {
        isBouncing = true;

        if (spriteRenderer != null && springDownSprite != null)
        {
            spriteRenderer.sprite = springDownSprite;
        }

        yield return new WaitForSeconds(compressedDuration);

        if (spriteRenderer != null && springUpSprite != null)
        {
            spriteRenderer.sprite = springUpSprite;
        }

        isBouncing = false;
    }
}