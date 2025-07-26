using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBuffAllCards : ZoneEffect
{
	public int amount = 2;
    
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		effectName = "Zona Energizante";
		description = $"+{amount} de poder a todas las cartas en esta zona al final de cada turno.";
	}
	
	public override void OnTurnEnd(Zone zone)
	{
		foreach(var card in zone.playerCards)
		{
			card.currentPower += amount;
		}
		
		foreach(var card in zone.aiCards)
		{
			card.currentPower += amount;
		}
		
		zone.UpdatePowerDisplay();
	}
}
