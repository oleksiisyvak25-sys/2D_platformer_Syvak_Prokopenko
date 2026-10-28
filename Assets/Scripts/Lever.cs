using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Lever : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private int stateCount = 2;
    [SerializeField] private int startState = 0;
    [SerializeField] private Sprite[] stateSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private AudioClip clickSound;

    public int State { get; private set; }
    public bool Locked { get; private set; }

    public event Action<Lever> Changed;

    private bool _playerNear;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        State = Mathf.Clamp(startState, 0, Mathf.Max(2, stateCount) - 1);
        ApplyVisual();

        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!_playerNear || Locked) return;
        if (PauseController.IsPaused) return;

        if (Input.GetKeyDown(interactKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        State = (State + 1) % Mathf.Max(2, stateCount);
        ApplyVisual();

        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }

        Changed?.Invoke(this);
    }

    public void SetLocked(bool locked)
    {
        Locked = locked;
        if (interactPrompt != null) interactPrompt.SetActive(_playerNear && !Locked);
    }

    private void ApplyVisual()
    {
        if (spriteRenderer == null) return;

        if (stateSprites != null && State < stateSprites.Length && stateSprites[State] != null)
        {
            spriteRenderer.sprite = stateSprites[State];
        }
        else
        {
            spriteRenderer.flipX = (State % 2 == 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerNear = true;
        if (interactPrompt != null) interactPrompt.SetActive(!Locked);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerNear = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }
}