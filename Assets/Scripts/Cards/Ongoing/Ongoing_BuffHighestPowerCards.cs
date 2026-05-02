using UnityEngine;

[CreateAssetMenu(
    fileName = "Ongoing_BuffHighestPowerCards",
    menuName = "Cards/Ongoing/Buff Your Highest Power Cards (+3)"
)]
public class OngoingBuffHighestPowerCardsEffect : CardEffectBase
{
	public int bonusPower = 3;

	// No aplicamos aquí directamente.
	// Este efecto se resuelve desde un recalculo global.
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Intencionalmente vacío.
		// El GameManager se encarga del cálculo para evitar stacking.
		GameManager.Instance.RecalculateHighestPowerOngoing(card.isPlayerCard);
	}
}