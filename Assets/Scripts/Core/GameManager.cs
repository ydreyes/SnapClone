using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	/// <summary>
	/// -Crear las 3 personajes.
	/// -Corregir los efectos de zona (removidos de Momento)
	/// -Agregar la animación del dorsal al momento de jugarla.
	/// -Agregar el efecto de zoom al momento de jugar
	/// DEMO: Crear las 9 batallas + 1 subBoss + el primer jefe y la cinemática
	/// Corregir el tema de las vidas y los puntos ganados
	/// Lista de Cartas Pendientes: M fantastic, Cap America, Kazar, Spectrum
	/// iron heart, wolfBane, groot, Jessica Jones, White Tiger, Gamora, Odin, rocket, america chavez
	/// Electra, Angela, Bishop, strong Guy, kazard, Blue Marvel, Kraven, Multiple Man, Scarlet witch
	/// Doctor strange, Viv Vision Olaris, Proffesor X, Vision, Heindall, Deadpoool, X-23, Moira X
	/// Carnage, Weapon X, Wolverine, KillMonger, Venom, DeathLock, Knull, Death, Scorn, morbius, colleng wing
	/// Gambit, Corvus Glave, Lady Sift, Dracula, Modok, Konshu, Apocalypse.
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
	public TextMeshProUGUI resultDetailsText;
	public Button continueButton;
	
	// logica de on reveal
	private Dictionary<CardInstance, int> delayedEffects = new();
	private bool playerPlayedCardLastTurn = false;
	
	[Header("Efectos posibles para zonas")]
	public List<ZoneEffect> zonaEffectPrefabs;
	
	[Header("Apuestas (SNAP)")]
	public Button betButton;
	public TextMeshProUGUI betText;
	
	[Header("Pilas de Cartas")]
	public List<CardData> discardPile = new List<CardData>();
	public List<CardData> destroyedPile = new List<CardData>();
	
	//bandera para la preview de cartas
	private bool isDraggingCard = false;
	public void SetDragging(bool value) => isDraggingCard = value;
	public bool IsDragging() => isDraggingCard;
	
	public List<CardInstance> playedOrderThisTurn = new List<CardInstance>();
	public List<CardInstance> pendingReveal = new List<CardInstance>();
	public bool playerHasPriority = true;

	private void Awake()
	{
		Instance = this;
	}

	void Start()
	{	
		if (continueButton) continueButton.gameObject.SetActive(false);
		
		UpdateEnergyDisplay();
		AsignarEfectosAleatoriosAZonas();
		InitNewMatch();
		InitBetButton();
	}
	
	public void InitNewMatch()
	{
		// 1) Limpiar zonas si reaprovechas la escena
		foreach (var z in zones)
		{
			z.ClearAllCards(true);
		}
		// 2) IA usa el deck del EnemyData elegido en ZoneScene
		if (GameSession.Instance && GameSession.Instance.selectedEnemy && ai != null)
		{
			ai.deck = new System.Collections.Generic.List<CardData>(
				GameSession.Instance.selectedEnemy.deck.cards
			);
		}
		// 3) Reset de decks/hand (baraja + mano visual)
		player.ResetDeckAndHand();
		ai.ResetDeckAndHand();
		// 4) Efecto "Starts in your opening hand" (tipo Quicksilver)
		//    Se aplica desde drawPile, DESPUÉS de barajar
		var openingCards = player.drawPile
			.Where(c => c.startsInOpeningHand)
			.ToList();

		foreach (var card in openingCards)
		{
			// Quitar del drawPile para que no se roben después
			player.drawPile.Remove(card);
			// Instanciar directamente en mano
			player.SpawnCardInHand(card);
		}

		Debug.Log($"[Opening Hand] Cartas especiales agregadas: {openingCards.Count}");

		// 5) Reset de turno y energía
		turnManager.ResetToTurn1(); // energía/turno = 1
		// 6) Robar mano inicial normal (por ejemplo 3 cartas)
		for (int i = 0; i < 3; i++)
		{
			player.DrawCard();
			ai.DrawCard();
		}
		// 7) Actualizar UI de energía
		UpdateEnergyDisplay();
	}
	
	void InitBetButton()
	{
		if (betButton == null) return;

		betButton.onClick.RemoveAllListeners();
		betButton.onClick.AddListener(() =>
		{
			if (PlayerProgress.Instance.betActive)
			{
				Debug.Log("La apuesta ya está activa.");
				return;
			}

			bool success = PlayerProgress.Instance.TryPlaceBet();
			if (!success)
			{
				Debug.Log("No tienes suficientes vidas para apostar.");
				return;
			}

			Debug.Log("APUESTA ACTIVADA POR EL JUGADOR");
        
			if (betText)
				betText.text = "x4";

			PlayerProgress.Instance.betActive = true;
		});
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
		
		RevealCardsInPriorityOrder();
		
		var (pZones, aZones, _) = GetZoneOutcome();
		
		if (pZones > aZones)
			playerHasPriority = true;
		else if (aZones > pZones)
			playerHasPriority = false;

		turnManager.EndTurn();
		player.DrawCard();
		ai.DrawCard();
		ai.PlayCardAutomatically();
		UpdateEnergyDisplay();

		// on reveal
		//foreach (var pair in delayedEffects)
		//{
		//	var card = pair.Key;
		//	int bonus = pair.Value;

		//	if (card.pendingBoostNextTurn && playerPlayedCardLastTurn)
		//	{
		//		card.currentPower += bonus;
		//		card.pendingBoostNextTurn = false;
		//	}
		//}
		//delayedEffects.Clear();
		// IA intenta apostar cada turno
		ai.TryRandomBet(turnManager.currentTurn);
		
		pendingReveal.Clear();
		playedOrderThisTurn.Clear();
	}
	
	public void UpdateEnergyDisplay()
	{
		energyText.text =  $"{turnManager.playerEnergy}";
	}
	
	// Este método se llama desde TurnManager cuando termina el turno 6
	public void EvaluateGame()
	{
		var (pZones, aZones, playerWon) = GetZoneOutcome();
		// Mostramos resultado visual (vidas y puntos proyectados)
		ShowResult(pZones, aZones, playerWon);
	}
	
	public void ShowResult(int playerWins, int aiWins, bool playerWon)
	{
		resultPanel.SetActive(true);
		
		// Mensaje principal
		if (playerWins >= 2) resultText.text = "¡Victoria!";
		else if (aiWins >= 2) resultText.text = "Derrota...";
		else resultText.text = "Empate";
		// Calcular vidas y puntos *antes* de aplicar
		int lostZones = Mathf.Max(0, 3 - Mathf.Clamp(playerWins, 0, 3));
		int projectedLives = PlayerProgress.Instance.lives - lostZones;
		int projectedHero = PlayerProgress.Instance.heroPoints + (playerWins * 100);
		// Si había apuesta activa, aplica visualmente el resultado pero sin aplicarlo aún
		if (PlayerProgress.Instance.betActive)
		{
			projectedLives += playerWon ? 8 : -4;
		}
		// Actualizar texto adicional
		if (resultDetailsText)
		{
			resultDetailsText.text =
				$"Vidas restantes: {projectedLives}\n" +
				$"Puntos de Héroe: {projectedHero}";
		}
		// Configurar botón Continuar
		if (continueButton)
		{
			continueButton.gameObject.SetActive(true);
			continueButton.onClick.RemoveAllListeners();

			continueButton.onClick.AddListener(() => 
			{
			    PlayerProgress.Instance.ApplyMatchOutcome(playerWins, aiWins, playerWon);
			});
		}
		// progresión después de mostrar resultado.
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
	
	public void RevealCardsInPriorityOrder()
	{
		// Si jugador tiene prioridad
		if (playerHasPriority)
		{
			RevealSide(true);   // jugador
			RevealSide(false);  // ia
		}
		else
		{
			RevealSide(false);  // ia
			RevealSide(true);   // jugador
		}

		pendingReveal.Clear();
		playedOrderThisTurn.Clear();
	}

	private void RevealSide(bool isPlayer)
	{
		foreach (var card in playedOrderThisTurn)
		{
			if (card.isPlayerCard == isPlayer && pendingReveal.Contains(card))
			{
				var zone = GetZoneForCard(card);
				card.data.onRevealEffect.ApplyEffect(card, zone);
			}
		}
	}
	
	public void DiscardCard(CardInstance card)
	{
		if (card == null || card.data == null)
			return;

		Debug.Log($"[DISCARD] {card.data.cardName}");

		// Quitar de la mano
		player.hand.Remove(card.data);

		// Registrar en pila
		discardPile.Add(card.data);

		// Eliminar prefab
		Destroy(card.gameObject);
	}

	public void DestroyCard(CardInstance card)
	{
		
		if (card == null || card.data == null)
			return;
			
		if (card.cantBeDestroyed)
		{
			Debug.Log($"[DESTROY BLOCKED] {card.data.cardName} no puede ser destruida.");
			return;
		}

		Debug.Log($"[DESTROY] {card.data.cardName}");

		// Si está en una zona, removerla
		var zone = GetZoneForCard(card);
		if (zone != null)
		{
			zone.RemoveCard(card);
		}
		else
		{
			// si estaba en mano
			player.hand.Remove(card.data);
		}

		destroyedPile.Add(card.data);

		Destroy(card.gameObject);
	}
	
	public void MoveCard(CardInstance card, Zone from, Zone to)
	{
		if (card == null || from == null || to == null) return;
		
		if (card.cantBeMoved)
		{
			Debug.Log($"[MOVE BLOCKED] {card.data.cardName} no puede ser movida.");
			return;
		}

		// seguridad: no mover si destino lleno
		if (!to.CanAcceptCard(card)) return;

		// Quitar de zona anterior (esto ya actualiza power)
		from.RemoveCard(card);
		to.AddCard(card);

		// Marcar que ya se movió
		card.hasMovedOnce = true;
		card.canMoveOnce = false;

		// refrescar textos
		from.UpdatePowerDisplay();
		to.UpdatePowerDisplay();

		Debug.Log($"[MOVE] {card.data.cardName} movida de {from.name} a {to.name}");
	}

}
