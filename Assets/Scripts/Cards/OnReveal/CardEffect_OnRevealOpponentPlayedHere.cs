using UnityEngine;

[CreateAssetMenu(fileName = "OnRevealOpponentPlayedHere", menuName = "Cards/OnReveal/OnRevealOpponentPlayedHere")]
public class CardEffect_OnRevealOpponentPlayedHere : CardEffectBase
{
	public int bonusAmount = 4;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Buscar si el oponente jugó carta en esta zona este turno
		bool opponentPlayedHere = false;

		foreach (var c in zone.cardsPlayedThisTurn)
		{
			if (c != card && c.isPlayerCard != card.isPlayerCard)
			{
				opponentPlayedHere = true;
				break;
			}
		}

		if (!opponentPlayedHere)
		{
			Debug.Log($"[OnReveal] El oponente NO jugó aquí. No se aplica +{bonusAmount}.");
			return;
		}

		// Aplicar buff
		card.currentPower += bonusAmount;

		Debug.Log($"[OnReveal] {card.data.cardName} gana +{bonusAmount} porque el oponente jugó aquí este turno.");

		// Actualizar UI
		card.UpdatePowerUI();
		zone.UpdatePowerDisplay();
	}
}
