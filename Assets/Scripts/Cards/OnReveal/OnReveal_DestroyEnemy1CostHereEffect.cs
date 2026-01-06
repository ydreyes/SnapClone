using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_DestroyEnemy1CostHere",
	menuName = "Cards/OnReveal/Destroy Enemy 1-Cost Here"
)]
public class OnRevealDestroyEnemy1CostHereEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		bool forPlayer = card.isPlayerCard;

		// Enemigos en esta zona
		var enemies = forPlayer ? zone.aiCards : zone.playerCards;

		// 1) Filtrar candidatos: costo 1
		List<CardInstance> candidates = new List<CardInstance>();
		foreach (var e in enemies)
		{
			if (e == null || e.data == null) continue;
			if (e.data.energyCost != 1) continue;

			// Si ya implementaste inmunidad "can't be destroyed", respétala
			// (ajusta el nombre del flag según tu implementación)
			// if (e.cantBeDestroyed) continue;

			candidates.Add(e);
		}

		if (candidates.Count == 0)
		{
			Debug.Log($"[OnReveal] {card.data.cardName}: no hay 1-Cost enemigos aquí para destruir.");
			return;
		}

		// 2) Elegir 1 al azar
		var target = candidates[Random.Range(0, candidates.Count)];

		Debug.Log($"[OnReveal] {card.data.cardName}: destruye a {target.data.cardName} (1-Cost) en {zone.name}");

		// 3) Destruir usando tu GameManager (maneja quitar de zona/pilas/destroy GO)
		GameManager.Instance.DestroyCard(target);

		// 4) Refrescar UI de la zona
		zone.UpdatePowerDisplay();
	}
}
