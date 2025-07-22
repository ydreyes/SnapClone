using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
	public List<CardData> deck;
	public List<CardData> hand = new List<CardData>();
	public GameObject cardPrefab;
	public Transform aiZoneArea;

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
	}

	public void PlayCardAutomatically()
	{
		// IA juega una carta aleatoria si tiene energía
		if (hand.Count == 0) return;

		var turnEnergy = GameManager.Instance.turnManager.aiEnergy;

		for (int i = 0; i < hand.Count; i++)
		{
			var card = hand[i];
			if (card.energyCost <= turnEnergy)
			{
				// Instanciar carta en zona aleatoria
				GameObject cardGO = Instantiate(cardPrefab);
				CardInstance instance = cardGO.GetComponent<CardInstance>();
				instance.data = card;
				instance.isPlayerCard = false;

				// Elegir zona aleatoria
				int zoneIndex = Random.Range(0, GameManager.Instance.zones.Count);
				Zone selectedZone = GameManager.Instance.zones[zoneIndex];
				instance.PlayCard(selectedZone);

				hand.RemoveAt(i);
				break;
			}
		}
	}
}
