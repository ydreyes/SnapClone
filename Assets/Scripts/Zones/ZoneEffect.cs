using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Scripts/Zones/ZoneEffect.cs
public abstract class ZoneEffect : MonoBehaviour
{
	public string effectName;
	public string description;

	// Se ejecuta cuando una carta es colocada en esta zona
	public virtual void OnCardPlayed(CardInstance card, Zone zone) {}

	// Se ejecuta cada turno si está activa la zona
	public virtual void OnTurnStart(Zone zone) {}

	// Otros hooks posibles
	public virtual void OnTurnEnd(Zone zone) {}
}
