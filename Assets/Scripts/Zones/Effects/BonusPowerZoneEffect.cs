using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusPowerZoneEffect : ZoneEffect
{
	public int bonusAmount = 1;
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		effectName = "Zona Potenciada";
		description = $"+{bonusAmount} poder a cada carta jugada aquí";
	}

	public override void OnCardPlayed(CardInstance card, Zone zone)
	{
		//card.currentPower += bonusAmount;
	}
}
