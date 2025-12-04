using UnityEngine;

[CreateAssetMenu(fileName = "OngoingEnemyCount", menuName = "Cards/Ongoing/OngoingEnemyCount")]
public class CardEffect_OngoingEnemyCount : CardEffectBase
{
	public int bonusPerEnemy = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		card.effectApplied = true;
		Recalculate(card, zone);
		Debug.Log($"[Ongoing] {card.data.cardName} obtiene +{bonusPerEnemy} por cada carta enemiga en la zona.");
	}

	public void Recalculate(CardInstance card, Zone zone)
	{
		if (zone == null || card == null) return;

		// Poder base
		card.currentPower = card.data.power;
		// Contar cartas enemigas
		int enemyCount = card.isPlayerCard
			? zone.aiCards.Count
			: zone.playerCards.Count;

		// Aplicar bonus
		card.currentPower += enemyCount * bonusPerEnemy;

		// Actualizar UI
		card.UpdatePowerUI();
		zone.UpdatePowerDisplay();
	}
}
