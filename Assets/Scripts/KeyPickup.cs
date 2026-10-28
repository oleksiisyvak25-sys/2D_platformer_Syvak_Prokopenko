using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    private enum State { Idle, Following, Consumed }

    [Header("Key")]
    [SerializeField] private KeyColor color = KeyColor.Red;
    [SerializeField] private Sprite hudIcon;
    [SerializeField] private AudioClip pickupSound;

    [Header("Idle / bobbing")]
    [SerializeField] private float bobAmplitude = 0.12f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Following the player")]
    [SerializeField] private Vector2 followOffset = new Vector2(0.9f, 0.8f);
    [SerializeField] private float followSmoothTime = 0.15f;
    [SerializeField] private float teleportDistance = 10f;

    public KeyColor KeyType => color;

    public Sprite IconSprite
    {
        get
        {
            if (hudIcon != null) return hudIcon;
            return _spriteRenderer != null ? _spriteRenderer.sprite : null;
        }
    }

    private State _state = State.Idle;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Vector3 _startPosition;
    private Vector3 _velocity;
    private float _phase;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _startPosition = transform.position;
        _phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        if (_state != State.Idle) return;

        transform.position = _startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + _phase) * bobAmplitude);
    }

    private void LateUpdate()
    {
        if (_state != State.Following) return;

        KeyInventory inventory = KeyInventory.Instance;
        if (inventory == null) return;

        Transform target = inventory.GetFollowTarget(this);
        if (target == null) return;

        bool followsPlayer = target == inventory.Player;
        float offsetY = followsPlayer ? followOffset.y : 0f;

        Vector3 desired = target.position + new Vector3(-inventory.PlayerFacing * followOffset.x, offsetY, 0f);
        desired.y += Mathf.Sin(Time.time * bobSpeed + _phase) * bobAmplitude;

        if ((desired - transform.position).sqrMagnitude > teleportDistance * teleportDistance)
        {
            transform.position = desired;
            _velocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, followSmoothTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_state != State.Idle || !other.CompareTag("Player")) return;

        _state = State.Following;
        _collider.enabled = false;
        transform.SetParent(null);

        KeyInventory.GetOrCreate().AddKey(this);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    public void FlyToAndConsume(Vector3 targetPosition)
    {
        if (_state == State.Consumed) return;

        _state = State.Consumed;
        StartCoroutine(FlyRoutine(targetPosition));
    }

    private IEnumerator FlyRoutine(Vector3 targetPosition)
    {
        const float duration = 0.45f;

        Vector3 from = transform.position;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(from, targetPosition, eased);
            transform.localScale = startScale * (1f - 0.6f * t);
            yield return null;
        }

        Destroy(gameObject);
    }
}