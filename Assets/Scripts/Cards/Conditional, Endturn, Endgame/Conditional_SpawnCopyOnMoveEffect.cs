using UnityEngine;

[CreateAssetMenu(
    fileName = "Conditional_SpawnCopyOnMove",
    menuName = "Cards/Conditional/When this moves, add a copy to old location"
)]
public class ConditionalSpawnCopyOnMoveEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;
		card.spawnCopyOnMove = true;
	}
}
