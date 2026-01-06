using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_RepeatOtherOnRevealsHere",
	menuName = "Cards/OnReveal/Repeat Other On Reveals Here"
)]
public class OnRevealRepeatOtherOnRevealsHere : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		// Evita loops si por alguna razón esta carta se vuelve a disparar dentro de una repetición
		if (card.isReplayingOnReveal) return;

		bool forPlayer = card.isPlayerCard;

		// Lista de cartas del lado correcto en ESTA zona
		var sideCards = forPlayer ? zone.playerCards : zone.aiCards;

		// 1) Recolectar targets: "other cards here" con onReveal
		List<CardInstance> targets = new List<CardInstance>();
		foreach (var c in sideCards)
		{
			if (c == null || c.data == null) continue;
			if (c == card) continue; // "other"
			if (c.data.onRevealEffect == null) continue;

			// Importante: si la otra carta también es del tipo "Repeat...", NO la re-ejecutamos
			// para evitar cadena infinita de repeticiones.
			if (c.data.onRevealEffect is OnRevealRepeatOtherOnRevealsHere) 
				continue;

			targets.Add(c);
		}

		if (targets.Count == 0)
		{
			Debug.Log($"[OnReveal] {card.data.cardName}: no hay On Reveals para repetir aquí.");
			return;
		}

		Debug.Log($"[OnReveal] {card.data.cardName}: repitiendo {targets.Count} On Reveals en {zone.name}");

		// 2) Ejecutar repetición en el orden en que están en la lista de la zona (estable)
		card.isReplayingOnReveal = true;

		foreach (var t in targets)
		{
			// Si la carta fue destruida o movida durante la repetición, saltar
			if (t == null || t.data == null) continue;

			// Asegurar que todavía está en ESTA zona y del mismo lado
			var currentZone = GameManager.Instance.GetZoneForCard(t);
			if (currentZone != zone) continue;
			if (t.isPlayerCard != forPlayer) continue;

			t.data.onRevealEffect.ApplyEffect(t, zone);

			// Si esa repetición cambió poder, refresca UI
			t.UpdatePowerUI();
		}

		card.isReplayingOnReveal = false;

		zone.UpdatePowerDisplay();
	}
}
