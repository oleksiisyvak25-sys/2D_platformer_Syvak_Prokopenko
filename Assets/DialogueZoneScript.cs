using UnityEngine;

public class DialogueZoneScript : MonoBehaviour
{
    private bool _isInZone = false;
    
    [SerializeField]
    private GameObject _button;

    private NPC _npcScript;

    private Renderer _renderButton;
    private void Awake()
    {
        _renderButton = _button.GetComponent<Renderer>();
        _renderButton.enabled = false;
        _npcScript = GetComponent<NPC>();
    }

    private void Update()
    {
        if (_isInZone && Input.GetKeyDown(KeyCode.E))
        {
            _npcScript.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isInZone = true;
            SetButtonActive(_isInZone);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isInZone = false;
            SetButtonActive(_isInZone);
        }
    }

    private void SetButtonActive(bool activity)
    {
        _button.GetComponent<Renderer>().enabled = activity;
    }
}
