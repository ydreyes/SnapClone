using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Ongoing/PowerPerCardInZone")]
public class PowerPerCardInZone : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		int totalCards = zone.playerCards.Count + zone.aiCards.Count;
		card.currentPower += totalCards;
	}
}