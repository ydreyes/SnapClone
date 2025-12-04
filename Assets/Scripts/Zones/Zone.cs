using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Zone : MonoBehaviour, IPointerClickHandler, IDropHandler
{
	public List<CardInstance> playerCards = new List<CardInstance>();
	public List<CardInstance> aiCards = new List<CardInstance>();
	
	public Transform playerRow;
	public Transform aiRow;
	
	public TextMeshProUGUI playerPowerText;
	public TextMeshProUGUI aiPowerText;
	
	public List<CardInstance> cardsInZone = new List<CardInstance>();
	public List<ZoneEffect> effects = new List<ZoneEffect>();
	
	[Header("UI")]
	public Button infoButton;
	public GameObject effectInfoPanel;
	public Image effectImage;
	public TextMeshProUGUI effectInfoText;
	
	// Efecto: próxima carta jugada aquí activa buff en carta específica
	public CardInstance pendingBoostTargetCard = null; 
	public int pendingBoostAmount = 0;
	public int pendingBoostExpiresTurn = -1;
	
	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start()
	{
		effects = GetComponents<ZoneEffect>().ToList();
		
		if (infoButton != null)
		{
			infoButton.onClick.AddListener(ShowEffectPanel);
		}
		
		if (effectInfoPanel != null)
		{
			effectInfoPanel.SetActive(false);
		}
	}
	
	public void ShowEffectPanel()
	{
		if (effectInfoPanel != null && effectInfoText != null)
		{
			string fullText = "";
			foreach (var effect in effects)
			{
				fullText += $"<b>{effect.effectName}</b>\n{effect.description}\n\n";
			}
			
			effectInfoText.text = fullText.Trim();
			effectInfoPanel.SetActive(true);
		}
	}
	
	public void HideEffectPanel()
	{
		if (effectInfoPanel != null)
		{
			effectInfoPanel.SetActive(false);
		}
	}

	public void AddCard(CardInstance card)
	{
		// Efecto de la zona
		cardsInZone.Add(card);
		
		// Aplica efectos al agregar la carta
		foreach(var effect in effects)
		{
			effect.OnCardPlayed(card, this);
		}
		
		// Si existe un trigger pendiente: requiere que NO sea la misma carta
		if (pendingBoostTargetCard != null)
		{
			int currentTurn = GameManager.Instance.turnManager.currentTurn;
			// Solo en el turno correcto Y la carta NO es la que tiene el efecto
			if (currentTurn == pendingBoostExpiresTurn && card != pendingBoostTargetCard)
			{
				CardInstance boostedCard = pendingBoostTargetCard;
				// Aplicar buff a la carta con el efecto
				boostedCard.currentPower += pendingBoostAmount;
				Debug.Log($"[EFFECT] {boostedCard.data.cardName} gana +{pendingBoostAmount}!");
				// Consumir el efecto
				pendingBoostTargetCard = null;
				pendingBoostAmount = 0;
				pendingBoostExpiresTurn = -1;
				boostedCard.UpdatePowerUI();
				UpdatePowerDisplay();
			}
		}

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
	
	// Efectos de la zona
	public void NotifyTurnStart()
	{
		foreach (var effect in effects)
		{
			effect.OnTurnStart(this);
		}
	}
	
	public void NotifyTurnEnd()
	{
		foreach(var effect in effects)
		{
			effect.OnTurnEnd(this);
		}
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
		playerPowerText.text = $"{GetTotalPower(true)}";
		aiPowerText.text = $"{GetTotalPower(false)}";
	}
	
	public void OnPointerClick(PointerEventData eventData)
	{
		GameManager.Instance.PlaySelectedCardInZone(this);
	}
	
	public void OnDrop(PointerEventData eventData)
	{
		var droppedCard = eventData.pointerDrag?.GetComponent<CardInstance>();
		
		if (droppedCard == null) return;
		
		// Si no se puede jugar la carta, regresa a la mano inmediatamente
		if (!GameManager.Instance.PlayerCanPlay(droppedCard.data))
		{
			droppedCard.ReturnToHand();
			return;
		}
		
		if (droppedCard.isPlayerCard)
		{
			droppedCard.wasPlayedThisDrop = true;
			GameManager.Instance.PlayCardFromDrag(droppedCard, this);
		}
	}
	
	// Devuelve la lista de cartas de la zona según el bando
	public List<CardInstance> GetCards(bool forPlayer)
	{
		return forPlayer ? playerCards : aiCards;
	}

	// Quita una carta de la zona (listas + UI)
	public void RemoveCard(CardInstance card)
	{
		if (card == null) return;

		// Remover de las colecciones
		cardsInZone.Remove(card);
		if (card.isPlayerCard)
			playerCards.Remove(card);
		else
			aiCards.Remove(card);

		// Desparentar por seguridad (evita que quede colgada en la fila)
		if (card.transform && (card.transform.parent == playerRow || card.transform.parent == aiRow))
			card.transform.SetParent(null, false);

		// Refrescar poder mostrado
		UpdatePowerDisplay();
	}

	// Limpia toda la zona (útil para reiniciar combate/partida)
	public void ClearAllCards(bool destroyGameObjects = true)
	{
		// Copias para no modificar la colección mientras iteramos
		var all = playerCards.Concat(aiCards).ToList();
		foreach (var c in all)
		{
			RemoveCard(c);
			if (destroyGameObjects && c) Destroy(c.gameObject);
		}
	}
	
	public void RegisterNextTurnBoostForCard(CardInstance card, int amount)
	{
		pendingBoostTargetCard = card;
		pendingBoostAmount = amount;
		pendingBoostExpiresTurn = GameManager.Instance.turnManager.currentTurn + 1;

		Debug.Log($"[Zone] {card.data.cardName} recibirá +{amount} si el jugador juega otra carta aquí en el turno {pendingBoostExpiresTurn}");
	}

}
