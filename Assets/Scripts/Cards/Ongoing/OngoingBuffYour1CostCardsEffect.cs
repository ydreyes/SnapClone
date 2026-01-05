using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_BuffYour1CostCards",
	menuName = "Cards/Ongoing/Buff Your 1-Cost Cards (+1)"
)]
public class OngoingBuffYour1CostCardsEffect : CardEffectBase
{
	public int bonus = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// No buffees directo aquí (evita stacking fantasma).
		GameManager.Instance.RecalculateGlobalOngoingBuffs();
	}
}
