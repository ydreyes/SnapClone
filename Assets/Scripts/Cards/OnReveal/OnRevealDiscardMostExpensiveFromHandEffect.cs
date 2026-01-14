using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_DiscardMostExpensive",
	menuName = "Cards/OnReveal/Discard Most Expensive From Hand"
)]
public class OnRevealDiscardMostExpensiveFromHandEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		if (card.isPlayerCard)
			DiscardMostExpensive_Player();
		else
			DiscardMostExpensive_AI();
	}

	private void DiscardMostExpensive_Player()
	{
		var gm = GameManager.Instance;
		var p = gm.player;

		if (p == null || p.hand == null || p.hand.Count == 0) return;
		if (p.handArea == null) return;

		// 1) Buscar CardData con mayor costo
		CardData best = p.hand[0];
		for (int i = 1; i < p.hand.Count; i++)
		{
			var cd = p.hand[i];
			if (cd != null && cd.energyCost > best.energyCost)
				best = cd;
		}

		// 2) Buscar su CardInstance en la UI (handArea)
		CardInstance inst = null;
		for (int i = 0; i < p.handArea.childCount; i++)
		{
			var ci = p.handArea.GetChild(i).GetComponent<CardInstance>();
			if (ci != null && ci.data == best)
			{
				inst = ci;
				break;
			}
		}

		if (inst == null) return;

		gm.DiscardCard(inst);

		Debug.Log($"[OnReveal] Discard most expensive: {best.cardName} (cost {best.energyCost})");
	}

	private void DiscardMostExpensive_AI()
	{
		var gm = GameManager.Instance;
		var ai = gm.ai;

		if (ai == null || ai.hand == null || ai.hand.Count == 0) return;

		// 1) Buscar CardData con mayor costo
		CardData best = ai.hand[0];
		for (int i = 1; i < ai.hand.Count; i++)
		{
			var cd = ai.hand[i];
			if (cd != null && cd.energyCost > best.energyCost)
				best = cd;
		}

		// 2) Descartar lógico (IA no tiene UI de mano)
		ai.hand.Remove(best);
		gm.discardPile.Add(best);

		// por si tienes ongoing que dependa de tamaño de mano
		gm.RecalculateHandSizeOngoing(false);

		Debug.Log($"[OnReveal][AI] Discard most expensive: {best.cardName} (cost {best.energyCost})");
	}
}
