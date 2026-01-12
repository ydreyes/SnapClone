using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_MoveHighestPowerCardsHere",
	menuName = "Cards/OnReveal/Move Highest Power Cards Here"
)]
public class OnRevealMoveHighestPowerCardsHereEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone targetZone)
	{
		if (card == null || targetZone == null) return;

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		// índice de la zona destino (para calcular "cercanía")
		int targetIndex = gm.zones.IndexOf(targetZone);
		if (targetIndex < 0) return;

		// 1) Recolectar candidatas: tus cartas en tablero (excluye esta carta y excluye las que ya están en targetZone)
		List<(CardInstance c, Zone fromZone, int fromIndex)> candidates = new();

		foreach (var z in gm.zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;
			foreach (var c in list)
			{
				if (c == null || c.data == null) continue;

				if (c == card) continue;          // no se mueve a sí misma
				if (z == targetZone) continue;    // ya está aquí
				if (c.cantBeMoved) continue;       // bloqueada por efecto ongoing

				int fromIdx = gm.zones.IndexOf(z);
				if (fromIdx < 0) continue;

				candidates.Add((c, z, fromIdx));
			}
		}

		if (candidates.Count == 0) return;

		// 2) Hallar el power máximo actual
		int maxPower = candidates.Max(x => x.c.currentPower);

		// 3) Filtrar solo las que empatan en maxPower
		var top = candidates
			.Where(x => x.c.currentPower == maxPower)
			.ToList();

		if (top.Count == 0) return;

		// 4) Ordenar por cercanía a la zona destino (abs(index diff)).
		//     Si empatan, orden estable por fromIndex (y como extra por nombre) para que sea determinista.
		top.Sort((a, b) =>
		{
			int da = Mathf.Abs(a.fromIndex - targetIndex);
			int db = Mathf.Abs(b.fromIndex - targetIndex);
			if (da != db) return da.CompareTo(db);

			if (a.fromIndex != b.fromIndex) return a.fromIndex.CompareTo(b.fromIndex);

			return string.Compare(a.c.data.cardName, b.c.data.cardName, System.StringComparison.Ordinal);
		});

		// 5) Mover tantas como quepan
		for (int i = 0; i < top.Count; i++)
		{
			if (!targetZone.CanAcceptCard(top[i].c))
				break;

			// MoveCard ya valida destino lleno y aplica Remove/Add + updates
			gm.MoveCard(top[i].c, top[i].fromZone, targetZone);
		}

		// si tienes recalculos globales por mover (adyacentes, etc.), aquí sería buen lugar para llamarlos
		// gm.RecalculateAdjacentLocationBonuses();
		// gm.RecalculateGlobalOngoingBuffs();

		targetZone.UpdatePowerDisplay();
	}
}
