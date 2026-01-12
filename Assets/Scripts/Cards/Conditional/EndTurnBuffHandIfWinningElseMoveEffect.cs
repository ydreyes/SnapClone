using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EndTurn_BuffHandIfWinningElseMove",
    menuName = "Cards/Conditional/Buff Hand If Winning Else Move"
)]
public class EndTurnBuffHandIfWinningElseMoveEffect : CardEffectBase
{
	public int handBonus = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		bool forPlayer = card.isPlayerCard;

		// ¿Está ganando aquí?
		int myPower = zone.GetTotalPower(forPlayer);
		int oppPower = zone.GetTotalPower(!forPlayer);

		if (myPower > oppPower)
		{
			BuffHand(forPlayer, handBonus);
		}
		else
		{
			TryMoveToAnotherLocation(card, zone);
		}
	}

	private void BuffHand(bool forPlayer, int amount)
	{
		var gm = GameManager.Instance;

		if (forPlayer)
		{
			// Jugador: sí hay GOs en la mano => buff directo a los CardInstance en handArea
			var handArea = gm.player.handArea;
			if (handArea == null) return;

			for (int i = 0; i < handArea.childCount; i++)
			{
				var ci = handArea.GetChild(i).GetComponent<CardInstance>();
				if (ci == null || ci.data == null) continue;

				ci.permanentPowerBonus += amount;
				ci.currentPower += amount;
				ci.UpdatePowerUI();
			}
		}
		else
		{
			// IA: no tiene UI en mano => guardamos el bonus en CardData (simple y funcional)
			// Si NO quieres tocar CardData, dime y lo hacemos con un diccionario en AIController.
			foreach (var cd in gm.ai.hand)
			{
				if (cd == null) continue;
				cd.power += amount;
			}
		}
	}

	private void TryMoveToAnotherLocation(CardInstance card, Zone currentZone)
	{
		// Respeta inmunidad a movimiento
		if (card.cantBeMoved) return;

		var gm = GameManager.Instance;

		// Buscar zonas destino válidas
		List<Zone> candidates = new List<Zone>();
		foreach (var z in gm.zones)
		{
			if (z == null) continue;
			if (z == currentZone) continue;
			if (!z.CanAcceptCard(card)) continue;
			candidates.Add(z);
		}

		if (candidates.Count == 0) return;

		// Elegir una aleatoria (puedes cambiar a "la más cercana" si quieres)
		Zone target = candidates[Random.Range(0, candidates.Count)];

		gm.MoveCard(card, currentZone, target);
	}
}
