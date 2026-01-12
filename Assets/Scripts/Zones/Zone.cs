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
	public List<CardInstance> cardsPlayedThisTurn = new List<CardInstance>();
	
	[Header("UI")]
	public Button infoButton;
	public GameObject effectInfoPanel;
	public Image effectImage;
	public TextMeshProUGUI effectInfoText;
	
	[Header("Bonos de ubicación")]
	public int playerLocationBonus = 0;
	public int aiLocationBonus = 0;
	
	// Track de si se jugó una carta aquí en X turno por cada bando
	private int lastTurnPlayerPlayedHere = -1;
	private int lastTurnAIPlayedHere = -1;
	
	// Pendientes: "si NO juegas aquí en el turno siguiente, +X a esta carta"
	[System.Serializable]
	private class PendingNoPlayBoost
	{
		public CardInstance target;
		public int amount;
		public int expiresTurn; // turno donde se evalúa
		public bool forPlayer;  // bando del target
	}

	private readonly List<PendingNoPlayBoost> pendingNoPlayBoosts = new List<PendingNoPlayBoost>();
	
	// Efecto: próxima carta jugada aquí activa buff en carta específica
	public CardInstance pendingBoostTargetCard = null; 
	public int pendingBoostAmount = 0;
	public int pendingBoostExpiresTurn = -1;
	
	// Efecto: tu poder total aquí es duplicado
	public bool doublePlayerPower = false;
	public bool doubleAIPower = false;
	
	// Efecto: Proffesor X
	[HideInInspector] public bool moveContext = false;
	

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
		if (HasOngoingMoveOnlyRuleActive() && !moveContext)
		{
			Debug.Log("[LOCK] Esta ubicación solo permite agregar/remover mediante MOVE.");
			if (card != null && card.isPlayerCard) card.ReturnToHand();
			return;
		}
				
		if (card == null) return;

		// max 4 cards per zone (por bando)
		var list = card.isPlayerCard ? playerCards : aiCards;
		if (list.Count >= 4)
		{
			Debug.Log("Zona llena");
			return;
		}

		// ✅ Registrar en colecciones SOLO si entra de verdad
		cardsInZone.Add(card);
		list.Add(card);

		int currentTurn = GameManager.Instance.turnManager.currentTurn;
		if (card.isPlayerCard) lastTurnPlayerPlayedHere = currentTurn;
		else lastTurnAIPlayedHere = currentTurn;

		cardsPlayedThisTurn.Add(card);

		// Aplica efectos de zona al jugar
		foreach (var effect in effects)
			effect.OnCardPlayed(card, this);

		// Trigger pendiente: si se juega otra carta aquí el turno siguiente
		if (pendingBoostTargetCard != null)
		{
			if (currentTurn == pendingBoostExpiresTurn && card != pendingBoostTargetCard)
			{
				CardInstance boostedCard = pendingBoostTargetCard;
				boostedCard.currentPower += pendingBoostAmount;

				Debug.Log($"[EFFECT] {boostedCard.data.cardName} gana +{pendingBoostAmount}!");

				pendingBoostTargetCard = null;
				pendingBoostAmount = 0;
				pendingBoostExpiresTurn = -1;

				boostedCard.UpdatePowerUI();
				UpdatePowerDisplay();
			}
		}

		// Recalcular ongoing (solo si aplica)
		foreach (var c in cardsInZone)
		{
			if (c != null && c.data != null && c.data.ongoingEffect is CardEffect_OngoingEnemyCount ongoing)
				ongoing.Recalculate(c, this);
		}

		// Parent UI
		Transform target = card.isPlayerCard ? playerRow : aiRow;
		card.transform.SetParent(target, false);
		card.transform.localScale = Vector3.one;

		UpdatePowerDisplay();
	}
	
	// Efectos de la zona
	public void NotifyTurnStart()
	{
		cardsPlayedThisTurn.Clear();

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
		
		// Evaluar "si NO jugó aquí este turno => buff"
		int ct = GameManager.Instance.turnManager.currentTurn;

		for (int i = pendingNoPlayBoosts.Count - 1; i >= 0; i--)
		{
			var p = pendingNoPlayBoosts[i];

			// Si ya no existe la carta (fue destruida), limpiar
			if (p.target == null)
			{
				pendingNoPlayBoosts.RemoveAt(i);
				continue;
			}

			if (p.expiresTurn != ct) continue;

			bool playedHereThisTurn = p.forPlayer
				? (lastTurnPlayerPlayedHere == ct)
				: (lastTurnAIPlayedHere == ct);

			// Si NO se jugó aquí este turno => aplicar buff
			if (!playedHereThisTurn)
			{
				p.target.permanentPowerBonus += p.amount;
				p.target.currentPower += p.amount;
				p.target.UpdatePowerUI();

				UpdatePowerDisplay();

				Debug.Log($"[NO-PLAY BUFF] {p.target.data.cardName} gana +{p.amount} (no jugó aquí en turno {ct})");
			}

			// Consumir siempre en el turno objetivo (haya o no haya buff)
			pendingNoPlayBoosts.RemoveAt(i);
		}
		
		// --- FIN DEL TURNO: resolver habilidades de cartas en esta zona ---
		ResolveEndTurnCardEffects();

		// Conditional: After you play a card here, +1 Power (al fin de turno)
		foreach (var source in cardsInZone)
		{
			if (source == null || source.data == null) continue;

			if (source.data.conditionalEffect is not ConditionalAfterYouPlayHerePlus1Effect effect)
				continue;

			// Contar cuántas cartas jugó SU MISMO bando aquí este turno (excluyendo a la propia carta)
			int count = 0;
			for (int i = 0; i < cardsPlayedThisTurn.Count; i++)
			{
				var played = cardsPlayedThisTurn[i];
				if (played == null) continue;

				if (played.isPlayerCard != source.isPlayerCard) continue;
				if (played == source) continue; // "other card"

				count++;
			}

			if (count <= 0) continue;

			int totalBonus = count * effect.bonusPerCard;

			// Bonus permanente + power actual
			source.permanentPowerBonus += totalBonus;
			source.currentPower += totalBonus;

			source.UpdatePowerUI();
			UpdatePowerDisplay();

			Debug.Log($"[COND] {source.data.cardName} gana +{totalBonus} (jugaste {count} otra(s) carta(s) aquí este turno)");
		}
	}
	
	private void ResolveEndTurnCardEffects()
	{
		// jugador
		foreach (var c in playerCards)
		{
			if (c == null || c.data == null) continue;

			// Usamos conditionalEffect como "EndTurn" (sin tocar tu arquitectura)
			if (c.data.conditionalEffect is EndTurnBuffHandIfWinningElseMoveEffect eff)
			{
				eff.ApplyEffect(c, this);
			}
		}

		// IA
		foreach (var c in aiCards)
		{
			if (c == null || c.data == null) continue;

			if (c.data.conditionalEffect is EndTurnBuffHandIfWinningElseMoveEffect eff)
			{
				eff.ApplyEffect(c, this);
			}
		}

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
		
		total += forPlayer ? playerLocationBonus : aiLocationBonus;
	
		return total;
	}
	
	public void UpdatePowerDisplay()
	{
		int playerTotal = GetTotalPower(true);
		int aiTotal = GetTotalPower(false);

		// Aplicar duplicadores Ongoing
		if (doublePlayerPower)
			playerTotal *= 2;

		if (doubleAIPower)
			aiTotal *= 2;

		playerPowerText.text = $"{playerTotal}";
		aiPowerText.text = $"{aiTotal}";
	}
	
	public void OnPointerClick(PointerEventData eventData)
	{
		GameManager.Instance.PlaySelectedCardInZone(this);
	}
	
	public void OnDrop(PointerEventData eventData)
	{
		var droppedCard = eventData.pointerDrag?.GetComponent<CardInstance>();
		if (droppedCard == null) return;

		// ¿La carta ya está en alguna zona? => entonces es MOVE
		var fromZone = GameManager.Instance.GetZoneForCard(droppedCard);

		if (fromZone != null)
		{
			// mover solo si está permitido y no es la misma zona
			if (!droppedCard.CanMoveNow()) { droppedCard.wasPlayedThisDrop = false; return; }
			if (fromZone == this) { droppedCard.wasPlayedThisDrop = false; return; }

			// validar espacio
			if (!CanAcceptCard(droppedCard))
			{
				Debug.Log("Zona llena, no se puede mover aquí.");
				return;
			}

			droppedCard.wasPlayedThisDrop = true; // para que no regrese
			GameManager.Instance.MoveCard(droppedCard, fromZone, this);
			return;
		}

		// Si NO está en zona, es jugar desde mano (tu lógica normal)
		if (!GameManager.Instance.PlayerCanPlay(droppedCard.data))
		{
			droppedCard.ReturnToHand();
			return;
		}

		droppedCard.wasPlayedThisDrop = true;
		GameManager.Instance.PlayCardFromDrag(droppedCard, this);
	}
	
	// Devuelve la lista de cartas de la zona según el bando
	public List<CardInstance> GetCards(bool forPlayer)
	{
		return forPlayer ? playerCards : aiCards;
	}

	// Quita una carta de la zona (listas + UI)
	public void RemoveCard(CardInstance card)
	{
		if (HasOngoingMoveOnlyRuleActive() && !moveContext)
		{
			Debug.Log("[LOCK] No puedes remover cartas de aquí salvo por MOVE.");
			return;
		}

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

		// Recalcular ongoing
		foreach (var c in cardsInZone)
		{
			if (c.data.ongoingEffect is CardEffect_OngoingEnemyCount ongoing)
			{
				ongoing.Recalculate(c, this);
			}
		}

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
	
	public bool CanAcceptCard(CardInstance card)
	{
		var list = card.isPlayerCard ? playerCards : aiCards;
		return list.Count < 4;
	}
	
	public void RecalculateZonePowers()
	{
		// 1) Reset: volver al poder base
		foreach (var c in playerCards)
		{
			if (c == null || c.data == null) continue;
			c.currentPower = c.data.power;
		}

		foreach (var c in aiCards)
		{
			if (c == null || c.data == null) continue;
			c.currentPower = c.data.power;
		}

		// 2) Aplicar auras Ongoing "other ongoing here +2"
		ApplyOngoingAura(playerCards);
		ApplyOngoingAura(aiCards);

		// 3) Refrescar UI de cartas + zona
		foreach (var c in playerCards) if (c != null) c.UpdatePowerUI();
		foreach (var c in aiCards) if (c != null) c.UpdatePowerUI();

		UpdatePowerDisplay();
	}

	private void ApplyOngoingAura(System.Collections.Generic.List<CardInstance> sideCards)
	{
		// Buscar todas las fuentes del aura en esta zona
		var sources = new System.Collections.Generic.List<(CardInstance source, int bonus)>();

		foreach (var c in sideCards)
		{
			if (c == null || c.data == null) continue;

			if (c.data.ongoingEffect is OngoingBuffOtherOngoingHereEffect aura)
				sources.Add((c, aura.bonus));
		}

		if (sources.Count == 0) return;

		// Por cada fuente, buffear a "otras" cartas ongoing aquí
		foreach (var (source, bonus) in sources)
		{
			foreach (var target in sideCards)
			{
				if (target == null || target.data == null) continue;

				// no se bufea a sí misma
				if (target == source) continue;

				// solo a cartas que tengan ongoingEffect
				if (target.data.ongoingEffect == null) continue;

				// bloquear reducción si tienes inmunidad
				// (aquí solo sumamos, así que está ok)
				target.currentPower += bonus;
			}
		}
	}
	
	public void RegisterNextTurnNoPlayBoostForCard(CardInstance card, int amount)
	{
		int currentTurn = GameManager.Instance.turnManager.currentTurn;

		pendingNoPlayBoosts.Add(new PendingNoPlayBoost
		{
			target = card,
			amount = amount,
			expiresTurn = currentTurn + 1,
			forPlayer = card.isPlayerCard
		});

		Debug.Log($"[Zone] Registrado NO-PLAY: {card.data.cardName} +{amount} si no juega aquí en turno {currentTurn + 1}");
	}
	
	public void NotifyCardMovedHere(CardInstance movedCard, Zone fromZone)
	{
		// La carta movida ya está dentro de playerCards/aiCards en este punto.

		// Revisar ambos bandos: cualquier carta en esta zona con este efecto gana +2
		ApplyMoveHereBuff(playerCards, movedCard);
		ApplyMoveHereBuff(aiCards, movedCard);

		UpdatePowerDisplay();
	}
	
	private void ApplyMoveHereBuff(List<CardInstance> list, CardInstance movedCard)
	{
		foreach (var c in list)
		{
			if (c == null || c.data == null) continue;

			if (!c.gainsPowerWhenCardMovesHere) continue;

			// opcional: si la misma carta se movió aquí, NO se bufea a sí misma
			if (c == movedCard) continue;

			c.currentPower += c.gainsPowerWhenCardMovesHereAmount;
			c.UpdatePowerUI();

			Debug.Log($"[MOVE-HERE BUFF] {c.data.cardName} gana +{c.gainsPowerWhenCardMovesHereAmount} porque una carta se movió a {name}");
		}
	}
	
	public void AddCardFromEffect(CardInstance card)
	{
		if (card == null) return;

		// validar espacio
		var list = card.isPlayerCard ? playerCards : aiCards;
		if (list.Count >= 4)
		{
			Debug.Log("[EFFECT SPAWN] Zona llena, no se puede crear copia aquí.");
			Destroy(card.gameObject);
			return;
		}

		// añadir a colecciones SIN marcar como "jugada este turno"
		cardsInZone.Add(card);
		list.Add(card);

		// parent correcto
		Transform target = card.isPlayerCard ? playerRow : aiRow;
		card.transform.SetParent(target, false);
		card.transform.localScale = Vector3.one;

		// recalcular ongoings específicos que dependan de conteo (como tu EnemyCount)
		foreach (var c in cardsInZone)
		{
			if (c.data != null && c.data.ongoingEffect is CardEffect_OngoingEnemyCount ongoing)
				ongoing.Recalculate(c, this);
		}

		// refrescar UI
		card.UpdatePowerUI();
		UpdatePowerDisplay();
	}

	private bool HasOngoingMoveOnlyRuleActive()
	{
		// Activo si existe una carta REVELADA en esta zona con ese ongoing
		for (int i = 0; i < cardsInZone.Count; i++)
		{
			var c = cardsInZone[i];
			if (c == null || c.data == null) continue;
			if (!c.isRevealed) continue;

			if (c.data.ongoingEffect is OngoingMoveOnlyAddRemoveHereEffect)
				return true;
		}
		return false;
	}

}
