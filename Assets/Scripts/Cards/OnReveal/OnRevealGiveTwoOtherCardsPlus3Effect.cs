using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_GiveTwoOtherCardsPlus3",
	menuName = "Cards/OnReveal/Give 2 Other Cards +3"
)]
public class OnRevealGiveTwoOtherCardsPlus3Effect : CardEffectBase
{
	public int targets = 2;
	public int bonus = 3;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		bool forPlayer = card.isPlayerCard;
		var gm = GameManager.Instance;

		// 1) Recolectar candidatos (todas tus cartas en tablero, excepto esta)
		List<CardInstance> candidates = new List<CardInstance>();

		foreach (var z in gm.zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;
			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;
				if (c == card) continue; // "exeptuar esta carta"
				candidates.Add(c);
			}
		}

		if (candidates.Count == 0)
		{
			Debug.Log($"[OnReveal] {card.data.cardName}: no hay otras cartas para buffear.");
			return;
		}

		// 2) Elegir hasta 2 sin repetir (shuffle simple)
		int pickCount = Mathf.Min(targets, candidates.Count);
		Shuffle(candidates);

		for (int i = 0; i < pickCount; i++)
		{
			var target = candidates[i];
			target.permanentPowerBonus += bonus;
			target.currentPower += bonus;
			target.UpdatePowerUI();

			// actualizar zona del target
			var z = gm.GetZoneForCard(target);
			if (z != null) z.UpdatePowerDisplay();

			Debug.Log($"[OnReveal] {card.data.cardName} -> {target.data.cardName} gana +{bonus}");
		}
	}

	private void Shuffle(List<CardInstance> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int r = Random.Range(i, list.Count);
			var tmp = list[i];
			list[i] = list[r];
			list[r] = tmp;
		}
	}
}
