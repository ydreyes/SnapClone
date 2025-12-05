using UnityEngine;

[CreateAssetMenu(fileName = "OnRevealMiddleBoost", menuName = "Cards/OnReveal/OnRevealMiddleBoost")]
public class CardEffect_OnRevealMiddleBoost : CardEffectBase
{
	public int boostAmount = 3;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// 1) Determinar índice de esta zona usando GameManager
		int zoneIndex = GameManager.Instance.zones.IndexOf(zone);

		// 2) Si no es la zona del medio, no pasa nada
		if (zoneIndex != 1)
		{
			Debug.Log($"[OnReveal] {card.data.cardName} NO está en la zona del medio. No se aplica el buff.");
			return;
		}

		// 3) Aplicar el buff
		card.currentPower += boostAmount;

		Debug.Log($"[OnReveal] {card.data.cardName} está en la zona del medio → gana +{boostAmount} poder.");

		// 4) Actualizar UI
		card.UpdatePowerUI();
		zone.UpdatePowerDisplay();
	}
}
