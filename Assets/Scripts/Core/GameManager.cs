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
	/// crear 4 personajes con sus barajas - Listo
	/// Se debe mostrar la vida y los puntos ganados en la ZoneScene - Listo
	/// Crear save system (para el prototipo usar el de unity pero luego usar el de steam)
	/// Crear una pantalla de tienda - Listo
	/// Crear una pantalla de listado de todas las cartas adquiridas. - Listo
	/// Crear una pantalla de creación de barajas - Listo
	/// crear escena de construccion de baraja - Listo
	/// Crear una pantalla de progreso. - En proceso.
	/// Cambiar todo a resolución para PC.
	/// Correcciones:
	/// -Se debe mostrar las vidas y los puntos en la selección de zona
	/// -Se debe traer la baraja del personaje seleccionado en la gameScene
	/// -Corregir la preview del deck en la pantalla de selección de persona
	/// -corregir el bug de que las cartas regresen a su posicion original si no pueden jugarse
	
	/// NOTA: terminar lo anterior antes de pasar a esta parte
	/// pulir la demo: agregarle jugo, efectos, canciones, sonidos, etc.
	/// agregar efectos ON ACTIVATE - La habilidad solo se debe activar una vez
	/// Agregar hechizos (cartas que desaparecen al ser jugadas)
	///  Modificar las barajas inciales y agregarles un Héroe Lider.(Basarte en el modo arena)
	/// Agregar el jefe final (galactus)
	/// que las cartas se juegen al mismo tiempo
	/// crear efectos de zona - que las zonas se vayan revelando al turno 1 dos y tres
	/// agregar la prioridad del juego y de las cartas
	/// Crear un modo landScape.
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
		
		if (continueButton) continueButton.gameObject.SetActive(false);
		UpdateEnergyDisplay();
		AsignarEfectosAleatoriosAZonas();
		InitNewMatch();
	}
	
	public void InitNewMatch()
	{
		// Limpia zonas/manos si reaprovechas la escena
		foreach (var z in zones)
		{
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
		}

		turnManager.ResetToTurn1(); // energía/turno = 1

		for (int i = 0; i < 3; i++) { player.DrawCard(); ai.DrawCard(); }

		UpdateEnergyDisplay();
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
				// Aplicar reglas y volver a ZoneScene
				PlayerProgress.Instance.ApplyMatchOutcome(playerWins, aiWins, playerWon);
			});
		}
		Debug.Log($"Resultado: Jugador {playerWins} zonas vs IA {aiWins} zonas");
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

}
