using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_CostReductionPerDestroyed",
	menuName = "Cards/Ongoing/Cost Reduction Per Destroyed"
)]
public class OngoingCostReductionPerDestroyedEffect : CardEffectBase
{
	public int costReductionPerCard = 1;
	
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		
	}
}
