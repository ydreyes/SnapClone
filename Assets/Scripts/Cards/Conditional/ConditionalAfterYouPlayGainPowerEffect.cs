using UnityEngine;

[CreateAssetMenu(
	fileName = "Conditional_AfterYouPlayGain1",
	menuName = "Cards/Conditional/After You Play Gain +1"
)]
public class ConditionalAfterYouPlayGainPowerEffect : CardEffectBase
{
	public int amount = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		// Solo marcar; el cálculo ocurre al final del turno en GameManager
		card.gainPowerAfterYouPlay = true;
		card.gainPowerAmount = amount;
	}
}
