using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
	/// <summary>
	/// pediente:
	/// usar drag and drop para jugar las cartas - Listo
	/// crear preview de carta al tocarla - Listo
	/// cear escena de menu principal - Listo
	/// crear escena de seleccion de personaje - Listo
	/// crear 4 personajes con sus barajas (Basarte en el modo arena)
	/// crear escena que muestre la ruta a seguir para llegar al jefe
	/// crear escena de construccion de baraja
	/// agregar efectos ON ACTIVATE - La habilidad solo se debe activar una vez
	/// Agregar hechizos (cartas que desaparecen al ser jugadas)
	/// que las cartas se juegen al mismo tiempo
	/// crear efectos de zona - que las zonas se vayan revelando al turno 1 dos y tres
	/// agregar la prioridad del juego y de las cartas
	/// pulir la demo: agregarle jugo, efectos, canciones, sonidos, etc.
	/// </summary>
	
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
	private bool playerPlayedCardLastTurn = false;
	
	[Header("Efectos posibles para zonas")]
	public List<ZoneEffect> zonaEffectPrefabs;
	
	//bandera para la preview de cartas
	private bool isDraggingCard = false;
	public void SetDragging(bool value) => isDraggingCard = value;
	public bool IsDragging() => isDraggingCard;


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
		AsignarEfectosAleatoriosAZonas();
		InitNewMatch();
	}
	
	public void InitNewMatch()
	{
		// Limpia zonas/manos si reaprovechas la escena
		foreach (var z in zones)
		{
			//foreach (var c in z.playerCards.ToArray()) { z.RemoveCard(c); if (c) Destroy(c.gameObject); }
			//foreach (var c in z.aiCards.ToArray())     { z.RemoveCard(c); if (c) Destroy(c.gameObject); }
			//z.UpdatePowerDisplay();
			z.ClearAllCards(true);
		}

		// Reset de decks/hand
		player.ResetDeckAndHand(); // ver §4.4
		ai.ResetDeckAndHand();

		// IA usa el deck del EnemyData elegido en ZoneScene
		if (GameSession.Instance && GameSession.Instance.selectedEnemy && ai != null)
		{
			ai.deck = new System.Collections.Generic.List<CardData>(
				GameSession.Instance.selectedEnemy.deck.cards
			);
			// si tu AI usa thinkDelay u otros parámetros, configúralos aquí
			// ai.thinkDelay = GameSession.Instance.selectedEnemy.aiThinkDelay;
		}

		turnManager.ResetToTurn1(); // energía/turno = 1

		for (int i = 0; i < 3; i++) { player.DrawCard(); ai.DrawCard(); }

		UpdateEnergyDisplay();
		// AsignarEfectosAleatoriosAZonas(); // si quieres re-randomizar por combate
	}
	
	public (int playerZones, int aiZones, bool playerWon) GetZoneOutcome()
	{
		int p = 0, a = 0;
		foreach (var z in zones)
		{
			int pp = z.GetTotalPower(true);
			int ap = z.GetTotalPower(false);
			if (pp > ap) p++;
			else if (ap > pp) a++;
		}
		bool win = (p >= 2 && p > a);
		return (p, a, win);
	}
	
	// Random zone effects
	void AsignarEfectosAleatoriosAZonas()
	{
		foreach (var zona in zones)
		{
			if (zonaEffectPrefabs.Count == 0) return;

			int index = Random.Range(0, zonaEffectPrefabs.Count);
			var efectoBase = zonaEffectPrefabs[index];

			// Agregar componente dinámicamente del tipo del efecto
			System.Type tipo = efectoBase.GetType();
			var nuevoEfecto = zona.gameObject.AddComponent(tipo) as ZoneEffect;

			// Copiar campos básicos
			nuevoEfecto.effectName = efectoBase.effectName;
			nuevoEfecto.description = efectoBase.description;

			// Si tiene campos específicos como "amount", se deben copiar también manualmente
			if (efectoBase is ZoneBuffAllCards buff)
			((ZoneBuffAllCards)nuevoEfecto).amount = buff.amount;

			else if (efectoBase is ZoneWeakenAllCards debuff)
			((ZoneWeakenAllCards)nuevoEfecto).amount = debuff.amount;

			else if (efectoBase is BonusPowerZoneEffect bonus)
			((BonusPowerZoneEffect)nuevoEfecto).bonusAmount = bonus.bonusAmount;
		}
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
		// mostrar ocultar panel de info de zona
		foreach (var zone in zones)
		{
			zone.NotifyTurnEnd();
		}
		
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
		//int playerWins = 0;
		//int aiWins = 0;
		
		//foreach(var zone in zones)
		//{
		//	int playerPower = zone.GetTotalPower(true);
		//	int aiPower = zone.GetTotalPower(false);
			
		//	if (playerPower > aiPower)
		//	{
		//		playerWins++;
		//	}
		//	else if (aiPower > playerPower)
		//	{
		//		aiWins++;
		//	}
		//}
		
		//ShowResult(playerWins, aiWins);
		var (pZones, aZones, playerWon) = GetZoneOutcome();

		// Aplica reglas: -1 vida por zona no controlada, apuesta, +100 puntos/ zona ganada
		PlayerProgress.Instance.ApplyMatchOutcome(pZones, aZones, playerWon);

		// Ya tenías ShowResult(p, a). Manténlo y agrega un botón “Continuar”
		ShowResult(pZones, aZones); // deja visible resultPanel
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
	
	public void PlayCardFromDrag(CardInstance card, Zone targetZone)
	{
		turnManager.playerEnergy -= card.data.energyCost;

		player.hand.Remove(card.data);
		card.transform.SetParent(null);
		card.PlayCard(targetZone);

		UpdateEnergyDisplay();
	}
	
	public Zone GetZoneForCard(CardInstance card)
	{
		foreach (var zone in zones)
		{
			if (zone.playerCards.Contains(card) || zone.aiCards.Contains(card))
				return zone;
		}
		return null;
	}

}
