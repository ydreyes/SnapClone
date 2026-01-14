using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_DiscardOneToDestroyRandomEnemy",
	menuName = "Cards/OnReveal/Discard 1 To Destroy Random Enemy"
)]
public class OnRevealDiscardOneToDestroyRandomEnemyEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		bool forPlayer = card.isPlayerCard;

		// 1) Descarta 1 carta de tu mano (si no hay, no puede pagar el costo)
		bool discarded = forPlayer
			? DiscardRandomFromPlayerHand()
			: DiscardRandomFromAIHand();

		if (!discarded)
		{
			Debug.Log("[OnReveal] No hay cartas en mano para descartar. Efecto no se ejecuta.");
			return;
		}

		// 2) Intentar destruir 1 carta enemiga aleatoria en tablero
		TryDestroyRandomEnemyOnBoard(forPlayer);
	}

	// -------------------------
	// PLAYER HAND DISCARD
	// -------------------------
	private bool DiscardRandomFromPlayerHand()
	{
		var gm = GameManager.Instance;
		var player = gm.player;

		if (player == null || player.hand == null || player.hand.Count == 0)
			return false;

		// Elegir CardData aleatorio de la mano lógica
		CardData chosen = player.hand[Random.Range(0, player.hand.Count)];
		if (chosen == null) return false;

		// Buscar el CardInstance visual correspondiente en handArea
		if (player.handArea == null) return false;

		CardInstance targetInstance = null;
		for (int i = 0; i < player.handArea.childCount; i++)
		{
			var inst = player.handArea.GetChild(i).GetComponent<CardInstance>();
			if (inst != null && inst.data == chosen)
			{
				targetInstance = inst;
				break;
			}
		}

		if (targetInstance == null) return false;

		gm.DiscardCard(targetInstance);
		Debug.Log($"[OnReveal] PLAYER descarta {chosen.cardName}");
		return true;
	}

	// -------------------------
	// AI HAND DISCARD (sin UI)
	// -------------------------
	private bool DiscardRandomFromAIHand()
	{
		var gm = GameManager.Instance;
		var ai = gm.ai;

		if (ai == null || ai.hand == null || ai.hand.Count == 0)
			return false;

		CardData chosen = ai.hand[Random.Range(0, ai.hand.Count)];
		if (chosen == null) return false;

		ai.hand.Remove(chosen);
		gm.discardPile.Add(chosen);

		// refrescos por ongoings que dependen de mano/descartes
		gm.RecalculateHandSizeOngoing(false);

		// Si tú ya tienes un método para “+Power por descartes”, llámalo aquí.
		// Ejemplo (si lo implementaste):
		// gm.RecalculateDiscardOngoing(false);

		Debug.Log($"[OnReveal] AI descarta {chosen.cardName}");
		return true;
	}

	// -------------------------
	// DESTROY RANDOM ENEMY ON BOARD
	// -------------------------
	private void TryDestroyRandomEnemyOnBoard(bool forPlayer)
	{
		var gm = GameManager.Instance;

		// Recolectar todas las cartas enemigas en las 3 zonas
		List<CardInstance> enemies = new();

		for (int i = 0; i < gm.zones.Count; i++)
		{
			var z = gm.zones[i];
			if (z == null) continue;

			var list = forPlayer ? z.aiCards : z.playerCards;
			for (int j = 0; j < list.Count; j++)
			{
				var c = list[j];
				if (c != null) enemies.Add(c);
			}
		}

		if (enemies.Count == 0)
		{
			Debug.Log("[OnReveal] No hay cartas enemigas en el tablero para destruir.");
			return;
		}

		CardInstance target = enemies[Random.Range(0, enemies.Count)];

		// Intentar destruir. Si target.cantBeDestroyed == true,
		// tu GameManager.DestroyCard ya lo bloquea y loguea.
		Debug.Log($"[OnReveal] Intentando destruir enemigo: {target.data.cardName}");
		gm.DestroyCard(target);
	}
}
