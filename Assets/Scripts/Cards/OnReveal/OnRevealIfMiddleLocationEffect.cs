using UnityEngine;

[CreateAssetMenu( fileName = "OnRevealIfMiddleLocation", menuName = "Cards/OnReveal/If Middle Location +Power" )]
public class OnRevealIfMiddleLocationEffect : CardEffectBase
{
	public int bonusPower = 3;
	
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null)
		{
			return;
		}
		
		// Zona central
		Zone middleZone = GameManager.Instance.zones[1];
		
		if (zone != middleZone)
		{
			return;
		}
		
		// apply the bonus power
		card.currentPower += bonusPower;
		card.UpdatePowerUI();
		zone.UpdatePowerDisplay();
		
		Debug.Log(
			$"[OnReveal] {card.data.cardName} está en la zona central y gana +{bonusPower}"
		);
	}
}
