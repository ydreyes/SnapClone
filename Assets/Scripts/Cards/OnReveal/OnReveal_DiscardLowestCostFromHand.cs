using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_DiscardLowestCostFromHand",
	menuName = "Cards/OnReveal/Discard Lowest Cost From Hand"
)]
public class OnRevealDiscardLowestCostFromHandEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		// Este efecto solo debe afectar al que jugó la carta (jugador o IA)
		if (card.isPlayerCard)
			DiscardLowestFromPlayerHand();
		else
			DiscardLowestFromAIHand();
	}

	private void DiscardLowestFromPlayerHand()
	{
		var gm = GameManager.Instance;
		var player = gm.player;

		if (player == null || player.hand == null || player.hand.Count == 0)
			return;

		// Encontrar costo mínimo en la mano
		int minCost = int.MaxValue;
		for (int i = 0; i < player.hand.Count; i++)
			if (player.hand[i] != null)
				minCost = Mathf.Min(minCost, player.hand[i].energyCost);

		if (minCost == int.MaxValue) return;

		// Entre las cartas con minCost, escoger una (primera o aleatoria)
		List<CardData> candidates = new();
		for (int i = 0; i < player.hand.Count; i++)
		{
			var cd = player.hand[i];
			if (cd != null && cd.energyCost == minCost)
				candidates.Add(cd);
		}
		if (candidates.Count == 0) return;

		CardData chosen = candidates[Random.Range(0, candidates.Count)];

		// Encontrar el CardInstance visual correspondiente en handArea y descartarlo
		var handArea = player.handArea;
		if (handArea == null) return;

		CardInstance targetInstance = null;
		for (int i = 0; i < handArea.childCount; i++)
		{
			var inst = handArea.GetChild(i).GetComponent<CardInstance>();
			if (inst != null && inst.data == chosen)
			{
				targetInstance = inst;
				break;
			}
		}

		if (targetInstance == null) return;

		gm.DiscardCard(targetInstance);
	}

	private void DiscardLowestFromAIHand()
	{
		// IA actualmente no instancía UI de mano, solo lista lógica.
		// descartamos desde la lista lógica y registramos pila.
		var gm = GameManager.Instance;
		var ai = gm.ai;

		if (ai == null || ai.hand == null || ai.hand.Count == 0)
			return;

		int minCost = int.MaxValue;
		for (int i = 0; i < ai.hand.Count; i++)
			if (ai.hand[i] != null)
				minCost = Mathf.Min(minCost, ai.hand[i].energyCost);

		if (minCost == int.MaxValue) return;

		List<CardData> candidates = new();
		for (int i = 0; i < ai.hand.Count; i++)
		{
			var cd = ai.hand[i];
			if (cd != null && cd.energyCost == minCost)
				candidates.Add(cd);
		}
		if (candidates.Count == 0) return;

		CardData chosen = candidates[Random.Range(0, candidates.Count)];

		ai.hand.Remove(chosen);
		gm.discardPile.Add(chosen);

		// Como esto afecta “hand size ongoing” del AI
		gm.RecalculateHandSizeOngoing(false);
		// Y si tienes ongoing por descartes:
		gm.RecalculateDiscardOngoing(true);
		gm.RecalculateDiscardOngoing(false);

		Debug.Log($"[AI DISCARD] {chosen.cardName} (cost {chosen.energyCost})");
	}
}
