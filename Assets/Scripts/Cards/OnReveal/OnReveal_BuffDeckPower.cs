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

		// Aplicar buff a TODAS las cartas del deck como poder pendiente
		// (jamás se muta el asset CardData; se consume al instanciar la carta).
		foreach (var cd in deck)
		{
			if (cd == null) continue;
			if (forPlayer) gm.AddPendingPower(cd, bonusPower);
			else gm.ai.AddPendingHandPower(cd, bonusPower);
		}

		Debug.Log(
			$"[ON REVEAL] {(forPlayer ? "Player" : "AI")} deck gana +{bonusPower} Power en todas las cartas."
		);
	}
}