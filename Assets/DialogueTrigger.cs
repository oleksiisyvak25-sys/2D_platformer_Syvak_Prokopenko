using UnityEngine;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogue Content")]
    [TextArea(3, 6)]
    [SerializeField] private string npcMessage = "Hello travaler!\nThere are monsters who tried to take our game.\nEnter the castle (E) and stop them!";

    private void Awake()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialogueText != null)
            {
                dialogueText.text = npcMessage;
            }

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }
    }
}