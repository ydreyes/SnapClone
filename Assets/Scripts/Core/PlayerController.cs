using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public List<CardData> deck;
	public List<CardData> hand = new List<CardData>();
	public List<CardData> drawPile = new();
	public Transform handArea;
	public GameObject cardPrefab;

	public void ShuffleDeck()
	{
		for (int i = 0; i < deck.Count; i++)
		{
			var temp = deck[i];
			int randomIndex = Random.Range(i, deck.Count);
			deck[i] = deck[randomIndex];
			deck[randomIndex] = temp;
		}
	}

	public void DrawCard()
	{
		if (deck.Count == 0) return;

		var card = deck[0];
		deck.RemoveAt(0);
		hand.Add(card);
		InstantiateCard(card);
	}
	
	public void ResetDeckAndHand()
	{
		hand.Clear();
		drawPile.Clear();
		drawPile.AddRange(deck);
		// Si tienes GOs instanciados de la mano, destrúyelos aquí
	}

	void InstantiateCard(CardData cardData)
	{
		Debug.Log($"Instanciando carta: {cardData.cardName}");
		Debug.Log($"cardPrefab: {cardPrefab}, handArea: {handArea}");
		
		GameObject cardGO = Instantiate(cardPrefab, handArea);
		CardInstance instance = cardGO.GetComponent<CardInstance>();
		instance.data = cardData;
		instance.isPlayerCard = true;
	}
}
