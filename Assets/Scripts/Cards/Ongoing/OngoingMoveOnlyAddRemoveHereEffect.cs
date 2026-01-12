using UnityEngine;

[CreateAssetMenu(
    fileName = "Ongoing_MoveOnlyAddRemoveHere",
    menuName = "Cards/Ongoing/Move Only Add Remove Here"
)]
public class OngoingMoveOnlyAddRemoveHereEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// No necesita modificar nada directo:
		// La Zona se “bloquea” automáticamente mientras esta carta revelada esté aquí
		// gracias a HasOngoingMoveOnlyRuleActive().
		Debug.Log($"[ONGOING] {card.data.cardName}: Esta ubicación queda bloqueada (solo MOVE).");
	}
}
