using UnityEngine;

[CreateAssetMenu(
	fileName = "Ongoing_DestroyedCardsTotalPower",
	menuName = "Cards/Ongoing/Destroyed Cards Total Power"
)]
public class OngoingDestroyedCardsTotalPowerEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;
		Recalculate(card);
	}
	
	public void Recalculate(CardInstance card)
	{
		if (card == null || card.data == null) return;
		
		GameManager gm = GameManager.Instance;
		
		int totalPower = 0;
		
		foreach (var destroyed in gm.destroyedPile)
		{
			if (destroyed == null) continue;
			
			totalPower += destroyed.power;
			totalPower += destroyed.permanentPowerBonus;
		}
		
		card.currentPower = totalPower;
		card.UpdatePowerUI();
		
		var zone = gm.GetZoneForCard(card);
		if (zone != null)
		{
			zone.UpdatePowerDisplay();
		}
	}
}
