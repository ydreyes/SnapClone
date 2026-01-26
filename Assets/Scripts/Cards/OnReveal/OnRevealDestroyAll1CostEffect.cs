using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
	fileName = "OnReveal_DestroyAll1Cost",
	menuName = "Cards/OnReveal/Destroy All 1-Cost"
)]
public class OnRevealDestroyAll1CostEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance source, Zone sourceZone)
	{
		if (source == null) return;

		GameManager gm = GameManager.Instance;

		List<CardInstance> toDestroy = new List<CardInstance>();

		foreach (var zone in gm.zones)
		{
			if (zone == null) continue;

			// Jugador
			foreach (var c in zone.playerCards)
			{
				if (IsValidTarget(c))
					toDestroy.Add(c);
			}

			// IA
			foreach (var c in zone.aiCards)
			{
				if (IsValidTarget(c))
					toDestroy.Add(c);
			}
		}

		// Destruir después (evita modificar listas mientras iteras)
		foreach (var card in toDestroy)
		{
			gm.DestroyCard(card);
		}

		Debug.Log($"[ON REVEAL] {source.data.cardName} destruye todas las cartas de costo 1 reveladas.");
	}

	private bool IsValidTarget(CardInstance card)
	{
		if (card == null || card.data == null) return false;

		// Solo cartas reveladas
		if (!card.isRevealed) return false;

		// Solo costo 1
		if (card.data.energyCost != 1) return false;

		return true;
	}
}
