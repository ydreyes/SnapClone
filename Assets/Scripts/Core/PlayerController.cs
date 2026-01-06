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
		if (drawPile.Count == 0) return;

		CardData card = drawPile[0];
		drawPile.RemoveAt(0);
		hand.Add(card);
		InstantiateCard(card);
	}
	
	public void ResetDeckAndHand()
	{
		hand.Clear();
		// destruir cartas visuales si reutiliza la escena
		if (handArea != null)
		{
			for (int i = handArea.childCount - 1; i >= 0; i--)
			{
				Destroy(handArea.GetChild(i).gameObject);
			}
		}
		// restaurar drawPile como nueva copia del deck base
		drawPile = new List<CardData>(deck);
		// mezclar el drawPile
		for (int i = 0; i < drawPile.Count; i++)
		{
			var temp = drawPile[i];
			int randomIndex = Random.Range(i, drawPile.Count);
			drawPile[i] = drawPile[randomIndex];
			drawPile[randomIndex] = temp;
		}
	}

	void InstantiateCard(CardData cardData)
	{
		GameObject cardGO = Instantiate(cardPrefab, handArea);
		CardInstance instance = cardGO.GetComponent<CardInstance>();
		instance.Init(handArea);
		
		instance.data = cardData;
		//instance.currentPower = cardData.power;
		instance.currentPower = cardData.power + cardData.permanentPowerBonus;
		instance.UpdatePowerUI();
		instance.isPlayerCard = true;
		// mover la carta
		instance.canMoveOnce = cardData.canMoveOnce;
		// inmunidades
		instance.cantBeDestroyed = cardData.cantBeDestroyed;
		instance.cantBeMoved = cardData.cantBeMoved;
		instance.cantHavePowerReduced = cardData.cantHavePowerReduced;
		
		instance.GetComponent<CardView>().SetUp(cardData); // inicializar visuales
	}

	public void SpawnCardInHand(CardData cardData)
	{
		// 1. Agregarla a la lista lógica de la mano (si aún no está)
		if (!hand.Contains(cardData))
			hand.Add(cardData);

		// 2. Instanciar en UI
		GameObject cardGO = Instantiate(cardPrefab, handArea);
		CardInstance instance = cardGO.GetComponent<CardInstance>();

		instance.Init(handArea);
		instance.data = cardData;
		instance.currentPower = cardData.power;
		instance.isPlayerCard = true;
		// mover la carta
		instance.canMoveOnce = cardData.canMoveOnce;
		// inmunidades
		instance.cantBeDestroyed = cardData.cantBeDestroyed;
		instance.cantBeMoved = cardData.cantBeMoved;
		instance.cantHavePowerReduced = cardData.cantHavePowerReduced;

		// 3. Inicializar visualmente
		instance.GetComponent<CardView>().SetUp(cardData);
	}
	
}
