using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_GiveTopDeckCardPlus3",
	menuName = "Cards/OnReveal/Give Top Deck Card +3"
)]
public class OnRevealGiveTopDeckCardPlus3Effect : CardEffectBase
{
	public int bonus = 3;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		var gm = GameManager.Instance;

		// Elegir mazo según bando
		var pile = card.isPlayerCard ? gm.player.drawPile : gm.ai.drawPile;

		if (pile == null || pile.Count == 0)
		{
			Debug.Log("[OnReveal] No hay cartas en el mazo (drawPile).");
			return;
		}

		// Top card = primera del drawPile
		var top = pile[0];
		if (top == null) return;

		// Buff permanente a esa carta del mazo
		top.permanentPowerBonus += bonus;

		Debug.Log($"[OnReveal] {card.data.cardName}: {top.cardName} (top deck) gana +{bonus} Power.");
	}
}
