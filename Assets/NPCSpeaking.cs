using UnityEngine;

[CreateAssetMenu(fileName="JumpingDialogue", menuName ="NPC Dialogue")]
public class NPCSpeaking : ScriptableObject
{
    [SerializeField]
    private float _typingSpeed;
    [SerializeField]
    private string _npcName;
    [SerializeField]
    private Sprite _npcPortrait;
    
    [SerializeField]
    private string[] _dialogueLines;

    [SerializeField]
    private bool[] _autoProgressLine;
    
    [SerializeField]
    private float _autoProgressDelay;

    public Sprite npcPortrait => _npcPortrait;

    public string npcName => _npcName;

    public string[] dialogueLines => _dialogueLines;

    public float autoProgressDelay => _autoProgressDelay;

    public float typingSpeed => _typingSpeed;

    public bool[] autoProgressLine => _autoProgressLine;
}
