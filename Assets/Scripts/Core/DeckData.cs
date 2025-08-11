using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Deck")]
public class DeckData : ScriptableObject
{
	public string deckName;
	public Sprite deckIcon;
	public List<CardData> cards = new List<CardData>(12);
}
