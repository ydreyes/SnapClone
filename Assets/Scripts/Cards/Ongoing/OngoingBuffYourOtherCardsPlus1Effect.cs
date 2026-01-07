using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_BuffYourOtherCardsPlus1",
	menuName = "Cards/Ongoing/Buff Your Other Cards +1"
)]
public class OngoingBuffYourOtherCardsPlus1Effect : CardEffectBase
{
	public int bonus = 1;

	// Se aplica desde GameManager.RecalculateGlobalOngoingBuffs()
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// vacio
	}
}
