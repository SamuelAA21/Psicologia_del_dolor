using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnderstandApplyDeck", menuName = "Psicologia del Dolor/MiniGames/Understand Apply Deck")]
public class UnderstandApplyDeck : ScriptableObject
{
    [SerializeField] private List<UnderstandApplyCard> cards = new List<UnderstandApplyCard>();

    public IReadOnlyList<UnderstandApplyCard> Cards => cards;
}
