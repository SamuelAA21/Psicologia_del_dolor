using System;
using UnityEngine;

[Serializable]
public class UnderstandApplyCard
{
    [TextArea(2, 4)]
    [SerializeField] private string text;
    [SerializeField] private UnderstandApplyCategory category;

    public string Text => text;
    public UnderstandApplyCategory Category => category;
}
