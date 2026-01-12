using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_MoveEnemy1or2CostHere",
	menuName = "Cards/OnReveal/Move Enemy 1-2 Cost Here"
)]
public class OnRevealMoveEnemy1or2CostHereEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;     // quien reveló
		bool enemyIsPlayer = !forPlayer;        // enemigo

		// 1) Si la zona destino está llena para el enemigo, no se puede mover nada
		int enemyCountInThisZone = enemyIsPlayer ? zone.playerCards.Count : zone.aiCards.Count;
		if (enemyCountInThisZone >= 4)
		{
			Debug.Log($"[OnReveal] {card.data.cardName}: destino lleno para el enemigo.");
			return;
		}

		// 2) Buscar candidatos (1 o 2-cost) en TODO el tablero (enemigos)
		List<(CardInstance c, Zone fromZone)> candidates = new();

		foreach (var z in gm.zones)
		{
			if (z == null) continue;

			var list = enemyIsPlayer ? z.playerCards : z.aiCards;
			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;

				int cost = c.data.energyCost;
				if (cost != 1 && cost != 2) continue;

				// no mover si está bloqueada
				if (c.cantBeMoved) continue;

				// ya está en esta misma zona => no tiene sentido
				if (z == zone) continue;

				candidates.Add((c, z));
			}
		}

		if (candidates.Count == 0)
		{
			Debug.Log($"[OnReveal] {card.data.cardName}: no hay enemigo 1-2 cost movible.");
			return;
		}

		// 3) Elegir 1 (random simple)
		var chosen = candidates[Random.Range(0, candidates.Count)];
		var target = chosen.c;
		var from = chosen.fromZone;

		// 4) Revalidar espacio por si cambió algo
		int enemyCountNow = enemyIsPlayer ? zone.playerCards.Count : zone.aiCards.Count;
		if (enemyCountNow >= 4)
		{
			Debug.Log($"[OnReveal] {card.data.cardName}: destino se llenó antes de mover.");
			return;
		}

		// 5) Mover usando tu GameManager (mantiene tu lógica y updates)
		gm.MoveCard(target, from, zone);

		Debug.Log($"[OnReveal] {card.data.cardName}: movió {target.data.cardName} (cost {target.data.energyCost}) a {zone.name}");
	}
}
