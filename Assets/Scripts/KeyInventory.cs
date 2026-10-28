using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory Instance { get; private set; }

    [Header("HUD (біля сердечок)")]
    [SerializeField] private RectTransform hudContainer;
    [SerializeField] private Vector2 iconSize = new Vector2(48f, 48f);
    [SerializeField] private float iconSpacing = 6f;

    private readonly List<KeyPickup> _held = new List<KeyPickup>();
    private readonly Dictionary<KeyPickup, GameObject> _icons = new Dictionary<KeyPickup, GameObject>();
    private Transform _player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    public static KeyInventory GetOrCreate()
    {
        if (Instance != null) return Instance;

        var go = new GameObject("KeyInventory");
        return go.AddComponent<KeyInventory>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public Transform Player
    {
        get
        {
            if (_player == null)
            {
                var movement = FindFirstObjectByType<PlayerMovement>();
                if (movement != null) _player = movement.transform;
            }
            return _player;
        }
    }

    public float PlayerFacing
    {
        get
        {
            Transform player = Player;
            return (player != null && player.lossyScale.x < 0f) ? -1f : 1f;
        }
    }

    public bool Has(KeyColor color)
    {
        foreach (var key in _held)
        {
            if (key != null && key.KeyType == color) return true;
        }
        return false;
    }

    public void AddKey(KeyPickup key)
    {
        if (key == null || _held.Contains(key)) return;

        _held.Add(key);
        CreateIcon(key);
    }

    public bool TryUseKey(KeyColor color, Vector3 doorPosition)
    {
        for (int i = 0; i < _held.Count; i++)
        {
            KeyPickup key = _held[i];
            if (key == null || key.KeyType != color) continue;

            _held.RemoveAt(i);

            if (_icons.TryGetValue(key, out GameObject icon))
            {
                if (icon != null) Destroy(icon);
                _icons.Remove(key);
            }

            key.FlyToAndConsume(doorPosition);
            return true;
        }

        return false;
    }

    public Transform GetFollowTarget(KeyPickup key)
    {
        int index = _held.IndexOf(key);
        if (index <= 0) return Player;

        KeyPickup previous = _held[index - 1];
        return previous != null ? previous.transform : Player;
    }

    private void CreateIcon(KeyPickup key)
    {
        if (hudContainer == null) return;

        EnsureLayout();

        var go = new GameObject("KeyIcon_" + key.KeyType, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(hudContainer, false);

        var rect = (RectTransform)go.transform;
        rect.sizeDelta = iconSize;

        var image = go.GetComponent<Image>();
        image.sprite = key.IconSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        _icons[key] = go;
    }

    private void EnsureLayout()
    {
        if (hudContainer.GetComponent<LayoutGroup>() != null) return;

        var layout = hudContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = iconSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }
}