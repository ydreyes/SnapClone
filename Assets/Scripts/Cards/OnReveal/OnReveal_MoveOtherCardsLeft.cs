using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_MoveOtherCardsLeft",
	menuName = "Cards/OnReveal/Move Other Cards Left"
)]
public class OnRevealMoveOtherCardsLeftEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		var gm = GameManager.Instance;
		if (gm == null || gm.zones == null || gm.zones.Count == 0) return;

		bool forPlayer = card.isPlayerCard;

		// Snapshot de "tus otras cartas" por zona (para no romper la iteración al mover)
		Dictionary<int, List<CardInstance>> snapshot = new Dictionary<int, List<CardInstance>>();

		for (int i = 0; i < gm.zones.Count; i++)
		{
			var z = gm.zones[i];
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;
			var copy = new List<CardInstance>();

			for (int k = 0; k < list.Count; k++)
			{
				var c = list[k];
				if (c == null) continue;
				if (c == card) continue; // "other cards"
				copy.Add(c);
			}

			if (copy.Count > 0)
				snapshot[i] = copy;
		}

		// Mover 1 a la izquierda: procesamos de izquierda a derecha (1 -> N-1),
		// así las zonas más cercanas se llenan primero.
		for (int fromIndex = 1; fromIndex < gm.zones.Count; fromIndex++)
		{
			if (!snapshot.TryGetValue(fromIndex, out var cardsToMove)) continue;

			Zone fromZone = gm.zones[fromIndex];
			Zone toZone = gm.zones[fromIndex - 1];

			// Intentar mover cada carta de esa zona
			for (int i = 0; i < cardsToMove.Count; i++)
			{
				var movingCard = cardsToMove[i];
				if (movingCard == null) continue;

				// Regla: si no puede moverse, skip
				if (movingCard.cantBeMoved) continue;

				// Regla: si destino lleno, skip (nota: se evalúa en tiempo real porque se van llenando)
				if (!toZone.CanAcceptCard(movingCard)) continue;

				// Move real (usa tus reglas internas: RemoveCard/AddCard + power updates + flags)
				gm.MoveCard(movingCard, fromZone, toZone);
			}
		}
	}
}
