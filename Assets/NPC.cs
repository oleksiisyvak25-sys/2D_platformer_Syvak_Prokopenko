using Assets;
using System.Collections;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField]
    private NPCSpeaking _dialogueData;

    [SerializeField]
    private GameObject _dialoguePanel;
    [SerializeField]
    private TMP_Text _dialogueText, _nameText;

    [SerializeField]
    private Image _portraitImage;

    private int _dialogueIndex;
    private bool _isTyping, _isDialogueActive;

    public bool CanInteract()
    {
        return !_isDialogueActive;
    }

    public void Interact()
    {
        if (_dialogueData == null || (PauseController.IsPaused && _isDialogueActive))
        {
            return;
        }

        if (_isDialogueActive)
        {
            NextLine();
        }
        else 
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        _isDialogueActive = true;
        _dialogueIndex = 0;

        _nameText.SetText(_dialogueData.npcName);
        _portraitImage.sprite = _dialogueData.npcPortrait;

        _dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    private void NextLine()
    {
        if (_isTyping)
        {
            StopAllCoroutines();
            _dialogueText.SetText(_dialogueData.dialogueLines[_dialogueIndex]);
            _isTyping = false;
        }
        else if (++_dialogueIndex < _dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else 
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine()
    {
        _isTyping = true;
        _dialogueText.SetText("");
        foreach (var letter in _dialogueData.dialogueLines[_dialogueIndex])
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(_dialogueData.typingSpeed);
        }
        _isTyping = false;
        if (_dialogueData.autoProgressLine.Length > _dialogueIndex && _dialogueData.autoProgressLine[_dialogueIndex])
        { 
            yield return new WaitForSeconds(_dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        _isDialogueActive = false;
        _dialogueText.SetText("");
        _dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
    }
}

