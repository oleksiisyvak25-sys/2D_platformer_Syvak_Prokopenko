using UnityEngine;

public class BossButton : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite targetSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Position Offset When Pressed")]
    [Tooltip("На скільки посунути кнопку вглиб до основи/стіни при натисканні")]
    [SerializeField] private float pressedOffset = 0.12f;

    public bool IsTarget { get; private set; }
    public bool IsPressed { get; private set; }

    private BossFightManager fightManager;
    private Vector3 initialLocalPos;

    private void Awake()
    {
        initialLocalPos = transform.localPosition;
    }

    public void Initialize(BossFightManager manager)
    {
        fightManager = manager;
        SetState(false, false);
    }

    public void SetState(bool isTarget, bool isPressed)
    {
        IsTarget = isTarget;
        IsPressed = isPressed;

        if (spriteRenderer == null) return;

        if (isPressed && pressedSprite != null)
        {
            spriteRenderer.sprite = pressedSprite;
            transform.localPosition = initialLocalPos - transform.up * pressedOffset;
        }
        else
        {
            transform.localPosition = initialLocalPos;

            if (isTarget && targetSprite != null)
                spriteRenderer.sprite = targetSprite;
            else if (normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTarget && !IsPressed && collision.CompareTag("BossFire"))
        {
            SetState(false, true);
            if (fightManager != null)
            {
                fightManager.OnButtonPressed(this);
            }
            Destroy(collision.gameObject);
        }
    }
}