using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Progress")]
    [SerializeField] private int order = 1;
    [SerializeField] private Transform respawnPoint;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite[] activeFrames;
    [SerializeField] private float framesPerSecond = 4f;
    [SerializeField] private AudioClip activateSound;

    public int Order => order;

    private bool _isActive;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        RespawnManager.CheckpointChanged += OnCheckpointChanged;
    }

    private void OnDisable()
    {
        RespawnManager.CheckpointChanged -= OnCheckpointChanged;
    }

    private void Update()
    {
        if (!_isActive || spriteRenderer == null || activeFrames == null || activeFrames.Length < 2) return;

        int frame = (int)(Time.time * framesPerSecond) % activeFrames.Length;
        if (activeFrames[frame] != null) spriteRenderer.sprite = activeFrames[frame];
    }

    private void Start()
    {
        int activeOrder = RespawnManager.Instance != null ? RespawnManager.Instance.ActiveOrder : int.MinValue;
        SetVisual(activeOrder == order);
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        RespawnManager manager = RespawnManager.Instance;
        if (manager == null) return;

        Vector3 position = respawnPoint != null ? respawnPoint.position : transform.position;

        if (manager.TryActivateCheckpoint(order, position) && activateSound != null)
        {
            AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }
    }

    private void OnCheckpointChanged(int activeOrder)
    {
        SetVisual(activeOrder == order);
    }

    private void SetVisual(bool active)
    {
        _isActive = active;
        if (spriteRenderer == null) return;

        if (active)
        {
            if (activeFrames != null && activeFrames.Length > 0 && activeFrames[0] != null)
            {
                spriteRenderer.sprite = activeFrames[0];
            }
        }
        else if (inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }
}