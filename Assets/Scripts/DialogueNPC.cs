using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DialogueNPC : MonoBehaviour
{
    [Header("Хто говорить")]
    [SerializeField] private string speakerName = "";
    [SerializeField] private Sprite avatar;

    [Header("Репліки")]
    [SerializeField] private DialogueLine[] lines;
    [SerializeField] private DialogueLine[] repeatLines;
    [SerializeField] private NPCSpeaking legacyData;

    [Header("Взаємодія")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Події")]
    [SerializeField] private UnityEvent onDialogueStarted;
    [SerializeField] private UnityEvent onDialogueFinished;

    private bool _playerNear;
    private bool _talkedBefore;
    private bool _promptVisible;
    private DialogueLine[] _legacyLines;

    private void Awake()
    {
        SetPrompt(false);
    }

    private void Start()
    {
        if (DialogueUI.Instance == null)
        {
            Debug.LogWarning("DialogueNPC: на сцені немає DialogueUI. Створи вікно через GameObject > UI > Dialogue Window (NPC).", this);
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        DialogueUI ui = DialogueUI.Instance;
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("[NPC] E pressed. playerNear = " + _playerNear +
                    ", uiFound = " + (ui != null) +
                    ", uiActive = " + (ui != null && ui.IsActive), this);
        }

        bool canTalk = _playerNear && ui != null && !ui.IsActive && !(oneTimeOnly && _talkedBefore);

        SetPrompt(canTalk);

        if (!canTalk) return;
        if (ui.LastClosedFrame == Time.frameCount) return;

        if (Input.GetKeyDown(interactKey))
        {
            Begin(ui);
        }
    }

    private void OnDisable()
    {
        _playerNear = false;
        SetPrompt(false);
    }

    private void Begin(DialogueUI ui)
    {
        DialogueLine[] chosen = GetLines();
        if (chosen == null || chosen.Length == 0)
        {
            Debug.LogWarning("DialogueNPC: немає жодної репліки у " + name, this);
            return;
        }

        if (ui.StartDialogue(ResolveSpeaker(), ResolveAvatar(), chosen, OnFinished))
        {
            onDialogueStarted?.Invoke();
        }
    }

    private void OnFinished()
    {
        _talkedBefore = true;
        onDialogueFinished?.Invoke();
    }

    private DialogueLine[] GetLines()
    {
        if (_talkedBefore && repeatLines != null && repeatLines.Length > 0)
        {
            return repeatLines;
        }

        if (lines != null && lines.Length > 0)
        {
            return lines;
        }

        return BuildLegacyLines();
    }

    private string ResolveSpeaker()
    {
        if (!string.IsNullOrEmpty(speakerName)) return speakerName;
        return legacyData != null ? legacyData.npcName : "";
    }

    private Sprite ResolveAvatar()
    {
        if (avatar != null) return avatar;
        return legacyData != null ? legacyData.npcPortrait : null;
    }

    private DialogueLine[] BuildLegacyLines()
    {
        if (legacyData == null || legacyData.dialogueLines == null) return null;
        if (_legacyLines != null) return _legacyLines;

        string[] source = legacyData.dialogueLines;
        bool[] auto = legacyData.autoProgressLine;

        _legacyLines = new DialogueLine[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            bool isAuto = auto != null && i < auto.Length && auto[i];

            _legacyLines[i] = new DialogueLine
            {
                text = source[i],
                autoAdvanceDelay = isAuto ? legacyData.autoProgressDelay : 0f
            };
        }

        return _legacyLines;
    }

    private void SetPrompt(bool visible)
    {
        if (interactPrompt == null || _promptVisible == visible && interactPrompt.activeSelf == visible) return;

        _promptVisible = visible;
        interactPrompt.SetActive(visible);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[NPC] Trigger enter: " + other.name + ", tag = " + other.tag, this);
        if (other.CompareTag("Player")) _playerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNear = false;
    }
}