using UnityEngine;

[CreateAssetMenu(
	fileName = "Conditional_GainPowerWhenCardMovesHere",
	menuName = "Cards/Conditional/When a card moves here, this gains +2"
)]
public class ConditionalGainPowerWhenCardMovesHereEffect : CardEffectBase
{
	public int amount = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		card.gainsPowerWhenCardMovesHere = true;
		card.gainsPowerWhenCardMovesHereAmount = amount;
	}
}
