using UnityEngine;

[CreateAssetMenu(
	fileName = "OngoingAdjacentLocationsBonus",
	menuName = "Cards/Ongoing/Adjacent Locations Bonus"
)]
public class OngoingAdjacentLocationsBonusEffect : CardEffectBase
{
	public int bonusPower = 3;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// No aplicamos aquí directo para evitar stacking.
		// Solo recalculamos el estado global de bonuses.
		GameManager.Instance.RecalculateAdjacentLocationBonuses();
	}
}
