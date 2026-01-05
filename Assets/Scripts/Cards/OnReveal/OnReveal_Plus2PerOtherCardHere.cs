using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_Plus2PerOtherCardHere",
	menuName = "Cards/OnReveal/+2 per Other Card Here"
)]
public class OnRevealPlus2PerOtherCardHere : CardEffectBase
{
	public int bonusPerCard = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		// Lista de cartas en esta zona según el lado
		var cardsHere = card.isPlayerCard ? zone.playerCards : zone.aiCards;

		// Contar OTRAS cartas (excluyéndose)
		int otherCardsCount = 0;

		foreach (var c in cardsHere)
		{
			if (c != null && c != card)
				otherCardsCount++;
		}

		if (otherCardsCount <= 0)
			return;

		int totalBonus = otherCardsCount * bonusPerCard;

		// Aplicar bonus permanente
		card.permanentPowerBonus += totalBonus;
		card.currentPower += totalBonus;

		card.UpdatePowerUI();
		zone.UpdatePowerDisplay();

		Debug.Log(
			$"[OnReveal] {card.data.cardName} gana +{totalBonus} " +
			$"({bonusPerCard} x {otherCardsCount} cartas en esta zona)"
		);
	}
}
