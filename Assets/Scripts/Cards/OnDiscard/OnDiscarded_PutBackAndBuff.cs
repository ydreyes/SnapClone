using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnDiscarded_PutBackAndBuff",
	menuName = "Cards/OnDiscarded/Put Back +2 Self +2 One Board Card"
)]
public class OnDiscardedPutBackAndBuffEffect : CardEffectBase
{
	public int bonus = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || card.data == null) return;

		var gm = GameManager.Instance;
		if (gm == null) return;

		bool forPlayer = card.isPlayerCard;

		// 1) "Put this back" -> volver a la mano del dueño
		// Como tu discard actual solo quita de player.hand, aquí lo devolvemos.
		if (forPlayer)
		{
			gm.player.SpawnCardInHand(card.data);
			// (si tienes UI de mano basada en instancias, esto no creará automáticamente el GO.
			//  lo correcto es instanciarlo visualmente, ver paso 4)
		}
		else
		{
			gm.ai.hand.Add(card.data);
		}

		// 2) +2 Power to itself
		// Como el GO se destruirá justo después, debemos guardar ese buff en la DATA
		// usando permanentPowerBonus para que se aplique la próxima vez que se instancie.
		// PERO permanentPowerBonus es de CardInstance, no de CardData.
		// Solución mínima: subir el power base en CardData (si aceptas que sea permanente real).
		// Mejor solución: crear un "bonus persistente" por carta. Como no lo tienes aún,
		// aquí aplico el buff a la carta en mano cuando se re-instancie:
		// -> guardamos el bonus en un diccionario runtime en GameManager (lo agrego abajo).
		gm.RegisterPendingPowerForCardData(card.data, bonus);

		// 3) +2 a UNA de tus cartas en tablero (random)
		var candidates = new List<CardInstance>();

		foreach (var z in gm.zones)
		{
			if (z == null) continue;

			var list = forPlayer ? z.playerCards : z.aiCards;
			for (int i = 0; i < list.Count; i++)
			{
				var c = list[i];
				if (c == null || c.data == null) continue;
				candidates.Add(c);
			}
		}

		if (candidates.Count > 0)
		{
			var target = candidates[Random.Range(0, candidates.Count)];

			target.permanentPowerBonus += bonus;
			target.currentPower += bonus;
			target.UpdatePowerUI();

			var tz = gm.GetZoneForCard(target);
			if (tz != null) tz.UpdatePowerDisplay();

			Debug.Log($"[OnDiscarded] {card.data.cardName}: +{bonus} a {target.data.cardName}");
		}
		else
		{
			Debug.Log($"[OnDiscarded] {card.data.cardName}: no hay cartas en tablero para buffear.");
		}
	}
}
