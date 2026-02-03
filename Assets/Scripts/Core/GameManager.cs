using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

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
	
	public int playerPlaysThisTurn = 0;
	public int aiPlaysThisTurn = 0;
	
	// Energía pendiente para el próximo turno
	private int pendingPlayerEnergy = 0;
	private int pendingAIEnergy = 0;
	
	// lógica para cartas descartadas
	private Dictionary<CardData, int> pendingPowerByCard = new Dictionary<CardData, int>();

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
		RecalculateHandSizeOngoing(true);
		RecalculateHandSizeOngoing(false);
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

	public void ReplaceZoneEffect(Zone zone)
	{
		if (zone == null) return;
		if (zonaEffectPrefabs == null || zonaEffectPrefabs.Count == 0) return;

		// 1) Guardar tipo actual (si existe)
		var currentEffects = zone.GetComponents<ZoneEffect>();
		System.Type currentType = null;

		for (int i = 0; i < currentEffects.Length; i++)
		{
			if (currentEffects[i] != null)
			{
				currentType = currentEffects[i].GetType();
				break;
			}
		}

		// 2) Elegir un nuevo efecto (intenta que sea distinto)
		ZoneEffect chosen = null;

		for (int tries = 0; tries < 10; tries++)
		{
			var candidate = zonaEffectPrefabs[Random.Range(0, zonaEffectPrefabs.Count)];
			if (candidate == null) continue;

			if (currentType == null || candidate.GetType() != currentType)
			{
				chosen = candidate;
				break;
			}
		}

		if (chosen == null)
			chosen = zonaEffectPrefabs[Random.Range(0, zonaEffectPrefabs.Count)];

		// 3) Destruir efectos actuales
		for (int i = 0; i < currentEffects.Length; i++)
		{
			if (currentEffects[i] != null)
				Destroy(currentEffects[i]);
		}

		// 4) Agregar el nuevo efecto
		System.Type tipo = chosen.GetType();
		var nuevo = zone.gameObject.AddComponent(tipo) as ZoneEffect;

		// Copiar base
		nuevo.effectName = chosen.effectName;
		nuevo.description = chosen.description;

		// Copiar campos específicos (los que ya vienes usando)
		if (chosen is ZoneBuffAllCards buff)
		((ZoneBuffAllCards)nuevo).amount = buff.amount;
		else if (chosen is ZoneWeakenAllCards debuff)
		((ZoneWeakenAllCards)nuevo).amount = debuff.amount;
		else if (chosen is BonusPowerZoneEffect bonus)
		((BonusPowerZoneEffect)nuevo).bonusAmount = bonus.bonusAmount;

		// 5) Refrescar la lista de efectos de la zona (filtrando los “destroyed”)
		zone.effects = zone.GetComponents<ZoneEffect>()
			.Where(e => e != null)
			.ToList();

		Debug.Log($"[ZONE] {zone.name}: nuevo efecto => {nuevo.effectName}");
	}
	
	public bool PlayerCanPlay(CardData card)
	{
		int cost = GetEffectiveEnergyCost(card);
		return cost <= turnManager.playerEnergy;
	}
	
	public void SelectCardToPlay(CardInstance card)
	{
		selectedCard = card;
	}
	
	public void PlaySelectedCardInZone(Zone zone)
	{
		if (selectedCard == null) return;

		// Quitar energía
		//turnManager.playerEnergy -= selectedCard.data.energyCost;  
		int cost = GetEffectiveEnergyCost(selectedCard.data);  //<----Revertir si genera bugs 
		turnManager.playerEnergy -= cost;

		// Remover visualmente de la mano
		player.hand.Remove(selectedCard.data);
		selectedCard.transform.SetParent(null);
		selectedCard.PlayCard(zone);
		
		RecalculateHandSizeOngoing(true);

		// on reveal
		playerPlayedCardLastTurn = true;

		selectedCard = null;
		
		UpdateEnergyDisplay();
	}

	public void EndTurn()
	{
		// IA juega antes de terminar turno
		ai.PlayCardAutomatically();
		RecalculateHandSizeOngoing(false);
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
		ResetPlaysThisTurn();
		
		if (pendingPlayerEnergy > 0) 
		{
			turnManager.playerEnergy += pendingPlayerEnergy;
			pendingPlayerEnergy = 0;
		}
		
		if (pendingAIEnergy > 0)
		{
			turnManager.aiEnergy += pendingAIEnergy;
			pendingAIEnergy = 0;
		}

		foreach (var z in zones)
			z.NotifyTurnStart();

		player.DrawCard();
		ai.DrawCard();
		
		RecalculateHandSizeOngoing(true);

		UpdateEnergyDisplay();

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
		ResolveEndGameEffects();
		
		var (pZones, aZones, playerWon) = GetZoneOutcome();
		
		// Mostramos resultado visual (vidas y puntos proyectados)
		ShowResult(pZones, aZones, playerWon);
	}
	
	private void ResolveEndGameEffects()
	{
		foreach (var z in zones)
		{
			if (z == null) continue;

			// Jugador
			foreach (var c in z.playerCards)
			{
				if (c == null || c.data == null) continue;
				if (c.endGameEffectApplied) continue;
				if (c.data.endGameEffect == null) continue;

				c.data.endGameEffect.ApplyEffect(c, z);
				c.endGameEffectApplied = true;
			}

			// IA
			foreach (var c in z.aiCards)
			{
				if (c == null || c.data == null) continue;
				if (c.endGameEffectApplied) continue;
				if (c.data.endGameEffect == null) continue;

				c.data.endGameEffect.ApplyEffect(c, z);
				c.endGameEffectApplied = true;
			}

			z.UpdatePowerDisplay();
		}
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
		//turnManager.playerEnergy -= card.data.energyCost;
		int cost = GetEffectiveEnergyCost(card.data);
		turnManager.playerEnergy -= cost;

		player.hand.Remove(card.data);
		
		RecalculateHandSizeOngoing(true);

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
			if (card.isPlayerCard != isPlayer) continue;
			if (!pendingReveal.Contains(card)) continue;

			var zone = GetZoneForCard(card);
			if (zone == null) continue;

			// On Reveal (1 vez)
			if (card.data.onRevealEffect != null)
				card.data.onRevealEffect.ApplyEffect(card, zone);

			// Marcar revelada
			card.isRevealed = true;

			// Ongoing (activar al revelarse)
			if (card.data.ongoingEffect != null && !card.effectApplied)
			{
				card.data.ongoingEffect.ApplyEffect(card, zone);
				card.effectApplied = true;
			}
			
			if (card.data.ongoingEffect is OngoingDiscardCountPowerEffect eff)
			{
				eff.Recalculate(card);
			}

			zone.UpdatePowerDisplay();
		}
	}
	
	public void DiscardCard(CardInstance card)
	{
		if (card == null || card.data == null)
			return;

		Debug.Log($"[DISCARD] {card.data.cardName}");
		// quitar de mano
		player.hand.Remove(card.data);
		// registrar en pila
		discardPile.Add(card.data);
		// disparar WHEN DISCARDED (antes de destruir el GO)
		if (card.data.onDiscardedEffect != null)
		{
			card.data.onDiscardedEffect.ApplyEffect(card, null);
		}
		// destruir GO
		Destroy(card.gameObject);

		RecalculateHandSizeOngoing(true);
		RecalculateDiscardOngoing(false);
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
		
		// 🔹 ON DESTROY EFFECT
		if (card.data.abilityType == AbilityType.OnDestroy && card.data.conditionalEffect != null)
		{
			card.data.conditionalEffect.ApplyEffect(card, GetZoneForCard(card));
		}

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
		RecalculateDestroyedOngoing();
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
		if (card.spawnCopyOnMove) 
		{
			SpawnCopyInZone(card, from);
		}
		to.AddCard(card);
		
		//trigger when a card move Here
		to.NotifyCardMovedHere(card, from);
		
		card.lastMoveTurn = turnManager.currentTurn;

		if (!card.canMoveEachTurn)
		{
			card.hasMovedOnce = true;
			card.canMoveOnce = false;
		}

		// refrescar textos
		from.UpdatePowerDisplay();
		to.UpdatePowerDisplay();

		Debug.Log($"[MOVE] {card.data.cardName} movida de {from.name} a {to.name}");
	}
	
	public void RecalculateAdjacentLocationBonuses()
	{
		// 1) Reset bonuses
		for (int i = 0; i < zones.Count; i++)
		{
			zones[i].playerLocationBonus = 0;
			zones[i].aiLocationBonus = 0;
		}

		// 2) Recalcular según cartas con el efecto
		for (int i = 0; i < zones.Count; i++)
		{
			var z = zones[i];

			// Jugador
			foreach (var c in z.playerCards)
			{
				if (c?.data?.ongoingEffect is OngoingAdjacentLocationsBonusEffect eff)
				{
					ApplyAdjacentBonus(i, true, eff.bonusPower);
				}
			}

			// IA
			foreach (var c in z.aiCards)
			{
				if (c?.data?.ongoingEffect is OngoingAdjacentLocationsBonusEffect eff)
				{
					ApplyAdjacentBonus(i, false, eff.bonusPower);
				}
			}
		}

		// 3) Refrescar UI
		foreach (var z in zones)
			z.UpdatePowerDisplay();
	}

	private void ApplyAdjacentBonus(int index, bool forPlayer, int amount)
	{
		// izquierda
		int left = index - 1;
		if (left >= 0)
		{
			if (forPlayer) zones[left].playerLocationBonus += amount;
			else zones[left].aiLocationBonus += amount;
		}

		// derecha
		int right = index + 1;
		if (right < zones.Count)
		{
			if (forPlayer) zones[right].playerLocationBonus += amount;
			else zones[right].aiLocationBonus += amount;
		}
	}

	public void RecalculateGlobalOngoingBuffs()
	{
		// 1) Reset: todas las cartas del tablero vuelven a su base power
		foreach (var z in zones)
		{
			if (z == null) continue;

			foreach (var c in z.playerCards)
			{
				if (c == null || c.data == null) continue;
				//c.currentPower = c.data.power;
				c.currentPower = c.data.power + c.permanentPowerBonus;
			}

			foreach (var c in z.aiCards)
			{
				if (c == null || c.data == null) continue;
				//c.currentPower = c.data.power;
				c.currentPower = c.data.power + c.permanentPowerBonus;
			}
		}

		// kazar
		// 2) Contar cuántas fuentes existen por bando
		int playerSources = CountSources(true);
		int aiSources = CountSources(false);

		// 3) Aplicar buffs por bando
		if (playerSources > 0)
			ApplyBuffTo1Cost(true, playerSources);

		if (aiSources > 0)
			ApplyBuffTo1Cost(false, aiSources);
		
		// Blue Marvel Effect
		int playerOtherSources = CountOtherCardsSources(true);
		int aiOtherSources = CountOtherCardsSources(false);

		if (playerOtherSources > 0)
			ApplyBuffToOtherCards(true, playerOtherSources);

		if (aiOtherSources > 0)
			ApplyBuffToOtherCards(false, aiOtherSources);

		// 4) Refrescar UI
		foreach (var z in zones)
		{
			if (z == null) continue;

			foreach (var c in z.playerCards) if (c != null) c.UpdatePowerUI();
			foreach (var c in z.aiCards) if (c != null) c.UpdatePowerUI();

			z.UpdatePowerDisplay();
		}
	}

	private int CountSources(bool forPlayer)
	{
		int count = 0;

		foreach (var z in zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;
			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;
				if (c.data.ongoingEffect is OngoingBuffYour1CostCardsEffect)
					count++;
			}
		}

		return count;
	}

	private void ApplyBuffTo1Cost(bool forPlayer, int sources)
	{
		int totalBonus = sources * 1; // cada fuente da +1

		foreach (var z in zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;

			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;

				// solo cartas de costo 1
				if (c.data.energyCost != 1) continue;

				// no auto-buff: en Snap normalmente sí buffea también si es costo 1
				// Si quieres que NO se buffee a sí misma, dímelo y lo ajusto.
				c.currentPower += totalBonus;
			}
		}
	}
	
	public void RegisterPlayThisTurn(bool forPlayer)
	{
		if (forPlayer) playerPlaysThisTurn++;
		else aiPlaysThisTurn++;
	}

	public void ResetPlaysThisTurn()
	{
		playerPlaysThisTurn = 0;
		aiPlaysThisTurn = 0;
	}

	// Se llama al final del turno para aplicar "After you play a card, this gains +1 Power"
	private void ResolveAfterPlayGainsPowerThisTurn()
	{
		int ct = turnManager.currentTurn;

		foreach (var z in zones)
		{
			if (z == null) continue;

			// jugador
			foreach (var c in z.playerCards)
			{
				if (c == null) continue;
				if (!c.gainPowerAfterYouPlay) continue;

				int plays = playerPlaysThisTurn;

				// si esta carta fue jugada este turno, no se cuenta a sí misma
				if (c.playedTurn == ct) plays = Mathf.Max(0, plays - 1);

				if (plays <= 0) continue;

				c.currentPower += plays * c.gainPowerAmount;
				c.UpdatePowerUI();
			}

			// IA
			foreach (var c in z.aiCards)
			{
				if (c == null) continue;
				if (!c.gainPowerAfterYouPlay) continue;

				int plays = aiPlaysThisTurn;

				if (c.playedTurn == ct) plays = Mathf.Max(0, plays - 1);

				if (plays <= 0) continue;

				c.currentPower += plays * c.gainPowerAmount;
				c.UpdatePowerUI();
			}

			z.UpdatePowerDisplay();
		}
	}
	
	public void RecalculateHandSizeOngoing(bool forPlayer)
	{
		foreach (var z in zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;

			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;

				if (c.data.ongoingEffect is OngoingHandSizePowerBonusEffect eff)
					eff.Recalculate(c);
			}
		}
	}
	
	private int CountOtherCardsSources(bool forPlayer)
	{
		int count = 0;

		foreach (var z in zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;
			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;
				if (c.data.ongoingEffect is OngoingBuffYourOtherCardsPlus1Effect)
					count++;
			}
		}

		return count;
	}
	
	private void ApplyBuffToOtherCards(bool forPlayer, int sources)
	{
		foreach (var z in zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;

			// 1) fuentes en esta zona (para excluirlas)
			var sourcesInThisZone = new List<CardInstance>();
			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;
				if (c.data.ongoingEffect is OngoingBuffYourOtherCardsPlus1Effect)
					sourcesInThisZone.Add(c);
			}

			// 2) buff a las otras cartas (excluyendo cada fuente)
			foreach (var target in list)
			{
				if (target == null || target.data == null) continue;

				// excluir “other”: no bufear a ninguna carta que sea fuente de este efecto
				if (sourcesInThisZone.Contains(target)) continue;

				target.currentPower += sources; // +1 por cada fuente
			}
		}
	}
	
	public void SpawnCopyInZone(CardInstance source, Zone targetZone)
	{
		if (source == null || source.data == null || targetZone == null) return;

		// si la zona no acepta (por bando)
		if (!targetZone.CanAcceptCard(source)) return;

		// elegir prefab según bando
		GameObject prefab = source.isPlayerCard ? player.cardPrefab : ai.cardPrefab;
		if (prefab == null)
		{
			Debug.LogError("[COPY] No hay prefab asignado para crear la copia.");
			return;
		}

		GameObject go = Instantiate(prefab);
		CardInstance copy = go.GetComponent<CardInstance>();

		// set data base
		copy.data = source.data;
		copy.isPlayerCard = source.isPlayerCard;

		// IMPORTANTE: mantener el poder TOTAL actual
		copy.currentPower = source.currentPower;

		// si manejas permanentPowerBonus, cópialo también (si existe en tu CardInstance)
		copy.permanentPowerBonus = source.permanentPowerBonus;

		// copiar flags/runtime importantes (para que "mantenga el mismo efecto" ya aplicado)
		CopyRuntimeState(source, copy);

		// set visuals
		var view = go.GetComponent<CardView>();
		if (view != null) view.SetUp(copy.data);

		// meter a zona sin PlayCard()
		targetZone.AddCardFromEffect(copy);
	}

	public void CopyRuntimeState(CardInstance src, CardInstance dst)
	{
		// Copia lo que YA tengas en tu CardInstance. Aquí incluyo los que he visto en tu proyecto.

		dst.spawnCopyOnMove = src.spawnCopyOnMove;

		dst.cantBeDestroyed = src.cantBeDestroyed;
		dst.cantBeMoved = src.cantBeMoved;
		dst.cantHavePowerReduced = src.cantHavePowerReduced;

		dst.canMoveOnce = src.canMoveOnce;
		dst.hasMovedOnce = src.hasMovedOnce;

		dst.gainPowerAfterYouPlay = src.gainPowerAfterYouPlay;
		dst.gainPowerAmount = src.gainPowerAmount;
		dst.playedTurn = src.playedTurn;

		dst.gainsPowerWhenCardMovesHere = src.gainsPowerWhenCardMovesHere;
		dst.gainsPowerWhenCardMovesHereAmount = src.gainsPowerWhenCardMovesHereAmount;
		

		// Si tienes más flags de efectos, los copias aquí.
	}
	
	public void RegisterPendingPowerForCardData(CardData data, int amount)
	{
		if (data == null) return;
		if (!pendingPowerByCard.ContainsKey(data)) pendingPowerByCard[data] = 0;
		pendingPowerByCard[data] += amount;
	}
	
	public void AddPendingPower(CardData card, int amount)
	{
		if (card == null || amount == 0) return;

		if (!pendingPowerByCard.ContainsKey(card))
			pendingPowerByCard[card] = 0;

		pendingPowerByCard[card] += amount;
	}
	
	public void PutCardBackToHand(CardData card, bool forPlayer)
	{
		if (card == null) return;

		if (forPlayer)
		{
			player.SpawnCardInHand(card); // esto ya te aplica el pending power porque llamas TryConsumePendingPower
		}
		else
		{
			// IA no tiene UI de mano, pero la devuelve a su mano lógica
			if (!ai.hand.Contains(card))
				ai.hand.Add(card);
		}
	}
	
	public bool TryConsumePendingPower(CardData data, out int amount)
	{
		amount = 0;
		if (data == null) return false;

		if (!pendingPowerByCard.TryGetValue(data, out amount)) return false;
		pendingPowerByCard.Remove(data);
		return amount != 0;
	}
	
	public void RecalculateDiscardOngoing(bool forPlayer)
	{
		foreach (var z in zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;

			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;

				if (c.data.ongoingEffect is OngoingDiscardCountPowerEffect eff)
				{
					eff.Recalculate(c);
				}
			}
		}
	}

	public CardData CreateRuntimeCopy(CardData source, int overrideCost)
	{
		if (source == null) return null;

		// Clona el ScriptableObject en runtime (no modifica el asset original)
		CardData copy = Instantiate(source);

		// Ajuste de costo
		copy.energyCost = overrideCost;

		// Opcional: para diferenciar en UI (no obligatorio)
		copy.cardName = source.cardName + " (0)";

		return copy;
	}
	
	public void AddPendingEnergyNextTurn(bool forPlayer, int amount)
	{
		if (amount <= 0) return;
		
		if (forPlayer)
		{
			pendingPlayerEnergy += amount;
		}
		else 
		{
			pendingAIEnergy += amount;
		}

		Debug.Log($"[ENERGY NEXT TURN] {(forPlayer ? "Player" : "AI")} +{amount}");
	}
	
	public void RecalculateDestroyedOngoing()
	{
		foreach (var z in zones)
		{
			if (z == null) continue;
			
			foreach (var c in z.playerCards)
			{
				if (c?.data?.ongoingEffect is OngoingDestroyedCardsTotalPowerEffect eff)
				{
					eff.Recalculate(c);
				}
			}

			foreach (var c in z.aiCards)
			{
				if (c?.data?.ongoingEffect is OngoingDestroyedCardsTotalPowerEffect eff)
				{
					eff.Recalculate(c);
				}
			}
		}
	}

	public int GetEffectiveEnergyCost(CardData card)
	{
		if (card == null) return 0;

		int baseCost = card.energyCost;

		// ¿Tiene el efecto?
		if (card.ongoingEffect is OngoingCostReductionPerDestroyedEffect eff)
		{
			int destroyedCount = destroyedPile.Count;
			int reduction = destroyedCount * eff.costReductionPerCard;

			baseCost -= reduction;
		}

		return Mathf.Max(0, baseCost);
	}	

}
