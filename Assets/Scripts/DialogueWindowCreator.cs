#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DialogueWindowCreator
{
    [MenuItem("GameObject/UI/Dialogue Window (NPC)", false, 20)]
    private static void Create()
    {
        if (Object.FindFirstObjectByType<DialogueUI>() != null)
        {
            bool createAnyway = EditorUtility.DisplayDialog(
                "Dialogue Window",
                "На сцені вже є DialogueUI. Створити ще одне вікно?",
                "Створити", "Скасувати");

            if (!createAnyway) return;
        }

        Transform canvas = GetCanvas();

        GameObject root = CreateUIObject("DialogueWindow", canvas);
        SetStretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        GameObject panel = CreateUIObject("Panel", root.transform);
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.09f, 0.88f);
        SetRect(panel.GetComponent<RectTransform>(),
            new Vector2(0.08f, 0f), new Vector2(0.92f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 24f), new Vector2(0f, 200f));

        GameObject avatar = CreateUIObject("Avatar", panel.transform);
        var avatarImage = avatar.AddComponent<Image>();
        avatarImage.preserveAspect = true;
        avatarImage.raycastTarget = false;
        SetRect(avatar.GetComponent<RectTransform>(),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(20f, 0f), new Vector2(160f, 160f));

        TextMeshProUGUI nameText = CreateText("NameText", panel.transform, "Ім'я", 30, FontStyles.Bold,
            new Color(1f, 0.85f, 0.4f), TextAlignmentOptions.MidlineLeft);
        SetStretch(nameText.rectTransform, new Vector2(200f, -52f), new Vector2(-20f, -14f), true);

        TextMeshProUGUI dialogueText = CreateText("DialogueText", panel.transform, "Текст діалогу...", 28,
            FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft);
        SetStretch(dialogueText.rectTransform, new Vector2(200f, 16f), new Vector2(-20f, -58f));

        TextMeshProUGUI hint = CreateText("ContinueHint", panel.transform, "[E]", 24, FontStyles.Normal,
            new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.MidlineRight);
        SetRect(hint.rectTransform,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-24f, 12f), new Vector2(80f, 36f));

        var ui = root.AddComponent<DialogueUI>();
        var serialized = new SerializedObject(ui);
        serialized.FindProperty("panel").objectReferenceValue = panel;
        serialized.FindProperty("avatarImage").objectReferenceValue = avatarImage;
        serialized.FindProperty("nameText").objectReferenceValue = nameText;
        serialized.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        serialized.FindProperty("continueIcon").objectReferenceValue = hint.gameObject;
        serialized.ApplyModifiedProperties();

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(root.scene);
    }

    private static Transform GetCanvas()
    {
        Canvas canvas = null;

        if (Selection.activeGameObject != null)
        {
            canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = Object.FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        return canvas.transform;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);

        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;
        return go;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string content, float size,
        FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = CreateUIObject(name, parent);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static void SetStretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax, bool topAnchored = false)
    {
        rect.anchorMin = topAnchored ? new Vector2(0f, 1f) : Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
#endif