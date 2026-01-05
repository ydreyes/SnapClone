using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_BuffOtherOngoingHere",
	menuName = "Cards/Ongoing/Buff Other Ongoing Here (+2)"
)]
public class OngoingBuffOtherOngoingHereEffect : CardEffectBase
{
	public int bonus = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// No hacemos stacking directo aquí.
		// Recalculamos el poder de la zona para que sea determinístico.
		if (zone != null)
			zone.RecalculateZonePowers();
	}
}
