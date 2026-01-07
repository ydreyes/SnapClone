using UnityEngine;

[CreateAssetMenu(
	fileName = "Conditional_AfterYouPlayHerePlus1",
	menuName = "Cards/Conditional/After You Play Here +1"
)]
public class ConditionalAfterYouPlayHerePlus1Effect : CardEffectBase
{
	public int bonusPerCard = 1;

	// No hacemos nada aquí porque tú quieres resolverlo al final del turno en Zone.NotifyTurnEnd()
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Intencionalmente vacío
	}
}
