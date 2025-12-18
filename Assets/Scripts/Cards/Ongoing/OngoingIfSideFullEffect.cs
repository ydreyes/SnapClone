using UnityEngine;

[CreateAssetMenu(
	fileName = "OngoingIfSideFull", menuName = "Cards/Ongoing/If Side Full +Power"
)]

public class OngoingIfSideFullEffect : CardEffectBase
{
	public int bonusPower = 4;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null)
			return;

		// Determinar el lado correcto
		int cardsOnSide = card.isPlayerCard
			? zone.playerCards.Count
			: zone.aiCards.Count;

		bool sideIsFull = cardsOnSide >= 4;

		// Poder base
		int basePower = card.data.power;

		if (sideIsFull)
		{
			if (card.currentPower != basePower + bonusPower)
			{
				card.currentPower = basePower + bonusPower;
				card.UpdatePowerUI();
				zone.UpdatePowerDisplay();
			}
		}
		else
		{
			if (card.currentPower != basePower)
			{
				card.currentPower = basePower;
				card.UpdatePowerUI();
				zone.UpdatePowerDisplay();
			}
		}
	}
}
