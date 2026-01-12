using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_ReplaceThisLocation",
	menuName = "Cards/OnReveal/Replace This Location"
)]
public class OnRevealReplaceThisLocationEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (zone == null) return;

		GameManager.Instance.ReplaceZoneEffect(zone);

		Debug.Log($"[OnReveal] {card.data.cardName}: reemplazó la ubicación {zone.name}");
	}
}
