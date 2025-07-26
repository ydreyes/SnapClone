using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneWeakenAllCards : ZoneEffect
{
	public int amount = 1;
    
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		effectName = "Zona Debil";
		description = $"-{amount} de poder a todas las cartas en esta zona al final de cada turno.";
	}
	
	public override void OnTurnEnd(Zone zone)
	{	
		// reduce poder al jugador
		foreach (var card in zone.playerCards)
		{
			card.currentPower -= amount;
		}
		
		// reduce poder al IA
		foreach (var card in zone.aiCards)
		{
			card.currentPower -= amount;
		}
		
		zone.UpdatePowerDisplay();
	}
}
