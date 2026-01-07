using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Conditional/AllowOneMove")]
public class AllowOneMove : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		card.canMoveOnce = true;
	}
}
