using UnityEngine;

[CreateAssetMenu(fileName = "NextTurnSelfBoost", menuName = "Cards/OnReveal/NextTurnSelfBoost")]
public class CardEffect_NextTurnSelfBoost : CardEffectBase
{
	public int boostAmount = 3;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Registrar efecto en la zona
		zone.RegisterNextTurnBoostForCard(card, boostAmount);

		Debug.Log($"[OnReveal] {card.data.cardName} se prepara para recibir +{boostAmount} el próximo turno si se juega otra carta en esta zona.");
	}
}
