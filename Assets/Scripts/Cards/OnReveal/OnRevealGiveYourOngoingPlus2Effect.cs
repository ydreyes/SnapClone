using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_GiveYourOngoingPlus2",
	menuName = "Cards/OnReveal/Give Your Ongoing +2"
)]
public class OnRevealGiveYourOngoingPlus2Effect : CardEffectBase
{
	public int bonus = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		bool forPlayer = card.isPlayerCard;
		var gm = GameManager.Instance;

		foreach (var z in gm.zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;

			foreach (var target in list)
			{
				if (target == null || target.data == null) continue;

				// Solo cartas Ongoing
				if (target.data.ongoingEffect == null) continue;

				// ✅ buff permanente
				target.permanentPowerBonus += bonus;
				target.currentPower += bonus;

				target.UpdatePowerUI();
			}

			z.UpdatePowerDisplay();
		}

		Debug.Log($"[OnReveal] {card.data.cardName} da +{bonus} a tus cartas Ongoing.");
	}
}
