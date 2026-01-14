using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_DiscardCountPower",
	menuName = "Cards/Ongoing/Discard Count +Power"
)]
public class OngoingDiscardCountPowerEffect : CardEffectBase
{
	public int powerPerDiscard = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Se recalcula dinámicamente
	}

	public void Recalculate(CardInstance card)
	{
		if (card == null || card.data == null) return;

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		int discardCount = forPlayer
			? gm.discardPile.Count
			: gm.discardPile.Count; // si luego separas pilas, aquí se ajusta

		int bonus = discardCount * powerPerDiscard;

		// Reset correcto
		card.currentPower = card.data.power + card.permanentPowerBonus + bonus;
		card.UpdatePowerUI();

		var zone = gm.GetZoneForCard(card);
		if (zone != null)
			zone.UpdatePowerDisplay();
	}
}
