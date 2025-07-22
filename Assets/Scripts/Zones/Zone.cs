using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class Zone : MonoBehaviour, IPointerClickHandler
{
	public List<CardInstance> playerCards = new List<CardInstance>();
	public List<CardInstance> aiCards = new List<CardInstance>();
	
	public Transform playerRow;
	public Transform aiRow;
	
	public TextMeshProUGUI playerPowerText;
	public TextMeshProUGUI aiPowerText;

	public void AddCard(CardInstance card)
	{
		// max 4 cards per zone
		var list = card.isPlayerCard ? playerCards:aiCards;
		
		if (list.Count >= 4)
		{
			Debug.Log("Zona llena");
			return;
		}
		
		list.Add(card);
		
		Transform target = card.isPlayerCard ? playerRow:aiRow;
		card.transform.SetParent(target, false);
		card.transform.localScale = Vector3.one;
		UpdatePowerDisplay();
	}

	public void RegisterOngoing(CardInstance card)
	{
		// Aquí puedes activar efectos como: +1 poder a todas las cartas en esta zona
		foreach (var c in playerCards)
			c.data.power += 1;
		foreach (var c in aiCards)
			c.data.power += 1;
			
		UpdatePowerDisplay();	
	}

	public int GetTotalPower(bool forPlayer)
	{
		int total = 0;
		var list = forPlayer ? playerCards : aiCards;
		foreach (var card in list)
			total += card.currentPower;
		return total;
	}
	
	public void UpdatePowerDisplay()
	{
		playerPowerText.text = $"Jugador: {GetTotalPower(true)}";
		aiPowerText.text = $"IA: {GetTotalPower(false)}";		
	}
	
	public void OnPointerClick(PointerEventData eventData)
	{
		GameManager.Instance.PlaySelectedCardInZone(this);
	}
	
	// Efectos Ongoing
	//public void RegisterOnGoing(CardInstance card)
	//{
	//	if (card.effectApplied == false) {
	//		return;
	//	}
		
	//	if (card.data.cardName == "Swarm Booster") //colocar el nombre para aplicar el efecto
	//	{
	//		int totalCards = playerCards.Count + aiCards.Count;
	//		card.currentPower += totalCards;
	//		card.effectApplied = true;
	//	}
	//	UpdatePowerDisplay();
	//}
}
