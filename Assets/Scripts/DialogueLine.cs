using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;

    public string speakerOverride;

    public Sprite avatarOverride;

    public float autoAdvanceDelay = 0f;
}