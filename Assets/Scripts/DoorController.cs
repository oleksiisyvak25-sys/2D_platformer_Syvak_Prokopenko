using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    [Header("Opening")]
    [SerializeField] private bool requireKey = true;
    [SerializeField] private KeyColor keyColor = KeyColor.Red;
    [SerializeField] private float openRadius = 1.5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private bool hideWhenOpen = true;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorBool = "IsOpen";
    [SerializeField] private AudioClip openSound;

    [Header("Events")]
    [SerializeField] private UnityEvent onOpened;

    public bool IsOpen { get; private set; }

    private Collider2D[] _solidColliders;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        var solid = new System.Collections.Generic.List<Collider2D>();
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            if (!col.isTrigger) solid.Add(col);
        }
        _solidColliders = solid.ToArray();

        ApplyState(false, false);
    }

    private void Update()
    {
        if (IsOpen || !requireKey) return;

        KeyInventory inventory = KeyInventory.Instance;
        if (inventory == null || !inventory.Has(keyColor)) return;
        if (!PlayerIsNear(inventory.Player)) return;

        if (inventory.TryUseKey(keyColor, transform.position))
        {
            Open();
        }
    }

    public void Open()
    {
        if (IsOpen) return;

        ApplyState(true, true);
        onOpened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen) return;

        ApplyState(false, true);
    }

    private void ApplyState(bool open, bool animate)
    {
        IsOpen = open;

        foreach (var col in _solidColliders)
        {
            if (col != null) col.enabled = !open;
        }

        if (spriteRenderer != null)
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            if (open && openSprite == null && hideWhenOpen)
            {
                if (animate && fadeDuration > 0f) _fadeRoutine = StartCoroutine(FadeOutRoutine());
                else spriteRenderer.enabled = false;
            }
            else
            {
                spriteRenderer.enabled = true;

                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;

                Sprite sprite = open ? openSprite : closedSprite;
                if (sprite != null) spriteRenderer.sprite = sprite;
            }
        }

        if (animator != null && HasBoolParameter(animator, animatorBool))
        {
            animator.SetBool(animatorBool, open);
        }

        if (open && animate && openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        spriteRenderer.enabled = false;
        _fadeRoutine = null;
    }

    private bool PlayerIsNear(Transform player)
    {
        if (player == null) return false;

        Vector2 p = player.position;

        if (_solidColliders.Length == 0)
        {
            return Vector2.Distance(transform.position, p) <= openRadius;
        }

        foreach (var col in _solidColliders)
        {
            if (col == null) continue;
            if (Vector2.Distance(col.ClosestPoint(p), p) <= openRadius) return true;
        }

        return false;
    }

    private static bool HasBoolParameter(Animator target, string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName)) return false;

        foreach (var parameter in target.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, openRadius);
    }
}