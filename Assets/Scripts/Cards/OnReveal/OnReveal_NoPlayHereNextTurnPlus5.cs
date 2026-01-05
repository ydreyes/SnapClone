using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_NoPlayHereNextTurnPlus5",
	menuName = "Cards/OnReveal/No Play Here Next Turn +5"
)]
public class OnRevealNoPlayHereNextTurnPlus5 : CardEffectBase
{
	public int bonus = 5;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		zone.RegisterNextTurnNoPlayBoostForCard(card, bonus);
		Debug.Log($"[OnReveal] {card.data.cardName}: +{bonus} si NO juega aquí en el turno siguiente.");
	}
}
