using UnityEngine;

// Orquesta el flujo de turno de la batalla y la gestión de pilas (design.md D4).
// GameManager queda como bootstrap de la escena de batalla y delega aquí.
public class BattleDirector
{
	private readonly GameManager game;

	public BattleDirector(GameManager game)
	{
		this.game = game;
	}

	// ── Flujo de turno ─────────────────────────────────────────────────

	public void EndTurn()
	{
		// IA juega antes de terminar turno
		game.ai.PlayCardAutomatically();
		game.RecalculateHandSizeOngoing(false);
		// mostrar ocultar panel de info de zona
		foreach (var zone in game.zones)
		{
			zone.NotifyTurnEnd();
		}

		RevealCardsInPriorityOrder();

		var (pZones, aZones, _) = GetZoneOutcome();
		if (pZones > aZones)
			game.playerHasPriority = true;
		else if (aZones > pZones)
			game.playerHasPriority = false;

		game.turnManager.EndTurn();
		game.ResetPlaysThisTurn();

		if (game.pendingPlayerEnergy > 0)
		{
			game.turnManager.playerEnergy += game.pendingPlayerEnergy;
			game.pendingPlayerEnergy = 0;
		}

		if (game.pendingAIEnergy > 0)
		{
			game.turnManager.aiEnergy += game.pendingAIEnergy;
			game.pendingAIEnergy = 0;
		}

		foreach (var z in game.zones)
			z.NotifyTurnStart();

		game.player.DrawCard();
		game.ai.DrawCard();

		game.RecalculateHandSizeOngoing(true);

		game.UpdateEnergyDisplay();

		// IA intenta apostar cada turno (se retira junto al sistema SNAP en Fase 1)
		game.ai.TryRandomBet(game.turnManager.currentTurn);

		game.pendingReveal.Clear();
		game.playedOrderThisTurn.Clear();
	}

	public void RevealCardsInPriorityOrder()
	{
		// Si jugador tiene prioridad
		if (game.playerHasPriority)
		{
			RevealSide(true);   // jugador
			RevealSide(false);  // ia
		}
		else
		{
			RevealSide(false);  // ia
			RevealSide(true);   // jugador
		}

		game.pendingReveal.Clear();
		game.playedOrderThisTurn.Clear();
	}

	private void RevealSide(bool isPlayer)
	{
		foreach (var card in game.playedOrderThisTurn)
		{
			if (card.isPlayerCard != isPlayer) continue;
			if (!game.pendingReveal.Contains(card)) continue;

			var zone = game.GetZoneForCard(card);
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

	// ── Resultado / fin de partida ─────────────────────────────────────

	public (int playerZones, int aiZones, bool playerWon) GetZoneOutcome()
	{
		int p = 0, a = 0;
		foreach (var z in game.zones)
		{
			int pp = z.GetTotalPower(true);
			int ap = z.GetTotalPower(false);
			if (pp > ap) p++;
			else if (ap > pp) a++;
		}

		bool win = (p >= 2 && p > a);
		return (p, a, win);
	}

	public void EvaluateGame()
	{
		ResolveEndGameEffects();

		var (pZones, aZones, playerWon) = GetZoneOutcome();

		// Mostramos resultado visual (vidas y puntos proyectados)
		game.ShowResult(pZones, aZones, playerWon);
	}

	private void ResolveEndGameEffects()
	{
		foreach (var z in game.zones)
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

	// ── Gestión de pilas ───────────────────────────────────────────────

	public void DiscardCard(CardInstance card)
	{
		if (card == null || card.data == null)
			return;

		Debug.Log($"[DISCARD] {card.data.cardName}");
		// quitar de mano
		game.player.hand.Remove(card.data);
		// registrar en pila
		game.discardPile.Add(card.data);
		// disparar WHEN DISCARDED (antes de destruir el GO)
		if (card.data.onDiscardedEffect != null)
		{
			card.data.onDiscardedEffect.ApplyEffect(card, null);
		}
		// destruir GO
		Object.Destroy(card.gameObject);

		game.RecalculateHandSizeOngoing(true);
		game.RecalculateDiscardOngoing(false);
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

		// ON DESTROY EFFECT
		if (card.data.abilityType == AbilityType.OnDestroy && card.data.conditionalEffect != null)
		{
			card.data.conditionalEffect.ApplyEffect(card, game.GetZoneForCard(card));
		}

		// Si está en una zona, removerla
		var zone = game.GetZoneForCard(card);
		if (zone != null)
		{
			zone.RemoveCard(card);
		}
		else
		{
			// si estaba en mano
			game.player.hand.Remove(card.data);
		}

		game.destroyedPile.Add(card.data);

		Object.Destroy(card.gameObject);
		game.RecalculateDestroyedOngoing();
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
			game.SpawnCopyInZone(card, from);
		}
		to.AddCard(card);

		// trigger when a card move Here
		to.NotifyCardMovedHere(card, from);

		card.lastMoveTurn = game.turnManager.currentTurn;

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
}