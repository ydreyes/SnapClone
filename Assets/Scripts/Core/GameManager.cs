using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public PlayerController player;
	public AIController ai;
	public List<Zone> zones;
	public TurnManager turnManager;
	
	public TextMeshProUGUI energyText;
	
	public CardInstance selectedCard;
	
	[Header("Resultado")]
	public GameObject resultPanel;
	public TextMeshProUGUI resultText;
	
	// logica de on reveal
	private Dictionary<CardInstance, int> delayedEffects = new();
	//private List<CardInstance> delayedEffectCards = new List<CardInstance>();
	private bool playerPlayedCardLastTurn = false;

	private void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		player.ShuffleDeck();
		ai.ShuffleDeck();

		for (int i = 0; i < 3; i++)
		{
			player.DrawCard();
			ai.DrawCard();
		}
		
		UpdateEnergyDisplay();
	}
	
	public bool PlayerCanPlay(CardData card)
	{
		return card.energyCost <= turnManager.playerEnergy;
	}
	
	public void SelectCardToPlay(CardInstance card)
	{
		selectedCard = card;
		Debug.Log($"Carta seleccionada: {card.data.cardName}. Elige zona.");
	}
	
	public void PlaySelectedCardInZone(Zone zone)
	{
		if (selectedCard == null) return;

		// Quitar energía
		turnManager.playerEnergy -= selectedCard.data.energyCost;

		// Remover visualmente de la mano
		player.hand.Remove(selectedCard.data);
		selectedCard.transform.SetParent(null);
		selectedCard.PlayCard(zone);

		// on reveal
		playerPlayedCardLastTurn = true;

		selectedCard = null;
		
		UpdateEnergyDisplay();
	}

	public void EndTurn()
	{
		ai.PlayCardAutomatically();
		// on reveal
		foreach (var pair in delayedEffects)
		{
			var card = pair.Key;
			int bonus = pair.Value;

			if (card.pendingBoostNextTurn && playerPlayedCardLastTurn)
			{
				card.currentPower += bonus;
				card.pendingBoostNextTurn = false;
			}
		}
		delayedEffects.Clear();
		
		turnManager.EndTurn();
		player.DrawCard();
		ai.DrawCard();
		UpdateEnergyDisplay();
	}
	
	public void UpdateEnergyDisplay()
	{
		energyText.text =  $"Energía: {turnManager.playerEnergy}";
	}
	
	// Este método se llama desde TurnManager cuando termina el turno 6
	public void EvaluateGame()
	{
		int playerWins = 0;
		int aiWins = 0;
		
		foreach(var zone in zones)
		{
			int playerPower = zone.GetTotalPower(true);
			int aiPower = zone.GetTotalPower(false);
			
			if (playerPower > aiPower)
			{
				playerWins++;
			}
			else if (aiPower > playerPower)
			{
				aiWins++;
			}
		}
		
		ShowResult(playerWins, aiWins);
	}
	
	public void ShowResult(int playerWins, int aiWins)
	{
		resultPanel.SetActive(true);
		
		if (playerWins >= 2) {
			resultText.text = "¡Victoria!";
		}
		else if (aiWins >= 2){
			resultText.text = "Derrota...";
		}
		else {
			resultText.text = "Empate";
		}

		Debug.Log($"Resultado: Jugador {playerWins} zonas vs IA {aiWins} zonas");
	}
	
	public void RegisterDelayedEffect(CardInstance card, int bonus)
	{
		delayedEffects[card] = bonus;
	}
}
