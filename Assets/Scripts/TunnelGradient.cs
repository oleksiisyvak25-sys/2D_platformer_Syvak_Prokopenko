using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class TunnelGradient : MonoBehaviour
{
    [SerializeField] private Color startColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color endColor = Color.black;
    [SerializeField, Range(0.3f, 4f)] private float curve = 1.5f;
    [SerializeField] private int sortingOrder = 20;

    private const int Size = 128;
    private Texture2D _tex;
    private Sprite _sprite;

    private void OnEnable() => Build();

    private void OnValidate()
    {
        if (isActiveAndEnabled) Build();
    }

    private void OnDisable() => Cleanup();

    private void Build()
    {
        Cleanup();

        _tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        for (int x = 0; x < Size; x++)
        {
            float t = Mathf.Pow(x / (Size - 1f), curve);
            Color c = Color.Lerp(startColor, endColor, t);
            for (int y = 0; y < Size; y++) _tex.SetPixel(x, y, c);
        }
        _tex.Apply();

        _sprite = Sprite.Create(_tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        _sprite.hideFlags = HideFlags.HideAndDontSave;

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = _sprite;
        sr.sortingOrder = sortingOrder;
    }

    private void Cleanup()
    {
        if (_sprite != null) DestroyImmediate(_sprite);
        if (_tex != null) DestroyImmediate(_tex);
    }
}