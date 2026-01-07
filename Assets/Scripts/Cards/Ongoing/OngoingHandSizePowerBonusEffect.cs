using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_HandSizePowerBonus",
	menuName = "Cards/Ongoing/+6 If Hand <= 1"
)]
public class OngoingHandSizePowerBonusEffect : CardEffectBase
{
	public int bonusPower = 6;
	public int maxHandSize = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		Recalculate(card);
	}

	public void Recalculate(CardInstance card)
	{
		if (card == null) return;

		var gm = GameManager.Instance;
		bool isPlayer = card.isPlayerCard;

		int handSize = isPlayer
			? gm.player.hand.Count
			: gm.ai.hand.Count;

		bool shouldApply = handSize <= maxHandSize;

		// quitar bonus previo
		card.currentPower = card.data.power + card.permanentPowerBonus;

		if (shouldApply)
			card.currentPower += bonusPower;

		card.UpdatePowerUI();

		// actualizar zona
		var zone = gm.GetZoneForCard(card);
		if (zone != null)
			zone.UpdatePowerDisplay();
	}
}
