using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIcon;

    [Header("Typing")]
    [SerializeField] private KeyCode advanceKey = KeyCode.E;
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private float sentencePause = 0.15f;
    [SerializeField] private float commaPause = 0.07f;

    [Header("Sound (необов'язково)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private int soundEveryNCharacters = 2;

    public bool IsActive { get; private set; }

    public int LastClosedFrame { get; private set; } = -1;

    private IList<DialogueLine> _lines;
    private string _speaker;
    private Sprite _avatar;
    private int _index;
    private bool _isTyping;
    private int _openedFrame = -1;
    private Coroutine _routine;
    private Action _onFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel == null)
        {
            Debug.LogError("DialogueUI: не призначено Panel. Створи вікно через GameObject > UI > Dialogue Window (NPC).", this);
            return;
        }

        if (panel == gameObject)
        {
            Debug.LogError("DialogueUI: скрипт не можна вішати на саму панель, бо вона ховається. Повісь його на батьківський об'єкт.", this);
            return;
        }

        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (IsActive) PauseController.SetPause(false);
    }

    private void Update()
    {
        if (!IsActive || Time.frameCount == _openedFrame) return;

        if (Input.GetKeyDown(advanceKey))
        {
            Advance();
        }
    }

    public bool StartDialogue(string speaker, Sprite avatar, IList<DialogueLine> lines, Action onFinished = null)
    {
        if (IsActive || lines == null || lines.Count == 0 || panel == null || dialogueText == null) return false;

        _speaker = speaker;
        _avatar = avatar;
        _lines = lines;
        _index = 0;
        _onFinished = onFinished;

        IsActive = true;
        _openedFrame = Time.frameCount;

        PauseController.SetPause(true);
        panel.SetActive(true);

        ShowLine();
        return true;
    }

    public void CloseImmediately()
    {
        if (!IsActive) return;

        _onFinished = null;
        EndDialogue();
    }


    private DialogueLine CurrentLine => _lines[_index];

    private void ShowLine()
    {
        DialogueLine line = CurrentLine;

        string speaker = string.IsNullOrEmpty(line.speakerOverride) ? _speaker : line.speakerOverride;
        Sprite avatar = line.avatarOverride != null ? line.avatarOverride : _avatar;

        if (nameText != null)
        {
            nameText.text = speaker;
            nameText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }

        if (avatarImage != null)
        {
            avatarImage.sprite = avatar;
            avatarImage.gameObject.SetActive(avatar != null);
        }

        StopRoutine();
        _routine = StartCoroutine(TypeRoutine(line));
    }

    private IEnumerator TypeRoutine(DialogueLine line)
    {
        _isTyping = true;
        if (continueIcon != null) continueIcon.SetActive(false);

        dialogueText.text = line.text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        int total = dialogueText.textInfo.characterCount;

        if (charactersPerSecond > 0f)
        {
            float delay = 1f / charactersPerSecond;

            for (int visible = 1; visible <= total; visible++)
            {
                dialogueText.maxVisibleCharacters = visible;
                PlayTypeSound(visible);

                float wait = delay + PunctuationPause(visible - 1);
                yield return new WaitForSecondsRealtime(wait);
            }
        }

        FinishTyping(line);
    }

    private void FinishTyping(DialogueLine line)
    {
        _isTyping = false;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        if (continueIcon != null) continueIcon.SetActive(true);

        if (line.autoAdvanceDelay > 0f)
        {
            _routine = StartCoroutine(AutoAdvanceRoutine(line.autoAdvanceDelay));
        }
        else
        {
            _routine = null;
        }
    }

    private IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        _routine = null;
        Advance();
    }

    private void Advance()
    {
        if (_isTyping)
        {
            StopRoutine();
            FinishTyping(CurrentLine);
            return;
        }

        StopRoutine();
        _index++;

        if (_index < _lines.Count) ShowLine();
        else EndDialogue();
    }

    private void EndDialogue()
    {
        StopRoutine();

        IsActive = false;
        _isTyping = false;
        LastClosedFrame = Time.frameCount;

        if (dialogueText != null) dialogueText.text = "";
        if (panel != null) panel.SetActive(false);

        PauseController.SetPause(false);

        Action callback = _onFinished;
        _onFinished = null;
        callback?.Invoke();
    }

    private void StopRoutine()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    private float PunctuationPause(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= dialogueText.textInfo.characterCount) return 0f;

        char c = dialogueText.textInfo.characterInfo[characterIndex].character;

        if (c == '.' || c == '!' || c == '?') return sentencePause;
        if (c == ',') return commaPause;
        return 0f;
    }

    private void PlayTypeSound(int visibleCount)
    {
        if (audioSource == null || typeSound == null) return;
        if (soundEveryNCharacters > 1 && visibleCount % soundEveryNCharacters != 0) return;

        audioSource.PlayOneShot(typeSound);
    }
}