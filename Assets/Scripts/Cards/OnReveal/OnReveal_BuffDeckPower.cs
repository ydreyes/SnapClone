using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "OnReveal_BuffDeckPower",
    menuName = "Cards/OnReveal/Give Deck +1 Power"
)]
public class OnRevealBuffDeckPowerEffect : CardEffectBase
{
	public int bonusPower = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		bool forPlayer = card.isPlayerCard;
		GameManager gm = GameManager.Instance;

		// Obtener el deck correcto
		List<CardData> deck = forPlayer
			? gm.player.drawPile
			: gm.ai.drawPile;

		if (deck == null || deck.Count == 0)
			return;

		// Aplicar buff permanente a TODAS las cartas del deck
		foreach (var cd in deck)
		{
			if (cd == null) continue;
			cd.permanentPowerBonus += bonusPower;
		}

		Debug.Log(
			$"[ON REVEAL] {(forPlayer ? "Player" : "AI")} deck gana +{bonusPower} Power en todas las cartas."
		);
	}
}