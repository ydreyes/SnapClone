using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_Destroy_Other_Cards_Here",
	menuName = "Cards/OnReveal/Destroy Other Cards Here"
)]
public class OnRevealDestroyOtherCardsHereEffect : CardEffectBase
{
	public int powerPerDestroyed = 2;

	public override void ApplyEffect(CardInstance source, Zone zone)
	{
		if (source == null || zone == null) return;

		int destroyedCount = 0;

		// Copia segura de la lista
		List<CardInstance> cardsInZone = new List<CardInstance>(
			source.isPlayerCard ? zone.playerCards : zone.aiCards
		);

		foreach (var card in cardsInZone)
		{
			if (card == null) continue;
			if (card == source) continue;

			// 🔹 Regla de timing
			if (!WasPlayedBefore(card, source))
				continue;

			// Destruir
			GameManager.Instance.DestroyCard(card);
			destroyedCount++;
		}

		// Buff por destrucción
		if (destroyedCount > 0)
		{
			int bonus = destroyedCount * powerPerDestroyed;
			source.permanentPowerBonus += bonus;
			source.currentPower += bonus;
			source.UpdatePowerUI();
		}

		zone.UpdatePowerDisplay();

		Debug.Log($"[ON REVEAL] {source.data.cardName} destruyó {destroyedCount} carta(s) y ganó +{destroyedCount * powerPerDestroyed}");
	}

	private bool WasPlayedBefore(CardInstance target, CardInstance source)
	{
		// Turno anterior SIEMPRE se destruye
		if (target.playedTurn < source.playedTurn)
			return true;

		// Mismo turno comparar orden
		if (target.playedTurn == source.playedTurn)
		{
			var order = GameManager.Instance.playedOrderThisTurn;
			int targetIndex = order.IndexOf(target);
			int sourceIndex = order.IndexOf(source);

			return targetIndex >= 0 && sourceIndex >= 0 && targetIndex < sourceIndex;
		}

		return false;
	}
}
