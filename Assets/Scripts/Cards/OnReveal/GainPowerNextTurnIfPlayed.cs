using UnityEngine;

[CreateAssetMenu(menuName = "Cards/OnReveal/GainPowerIfPlayNextTurn")]
public class GainPowerNextTurnIfPlayed : CardEffectBase
{
	public int bonusPower = 5;
	
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		card.pendingBoostNextTurn = true;
		GameManager.Instance.RegisterDelayedEffect(card, bonusPower);
	}
}
