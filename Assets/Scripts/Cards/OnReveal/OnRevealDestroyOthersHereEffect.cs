using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
	fileName = "OnReveal_DestroyOthersHere",
	menuName = "Cards/OnReveal/Destroy Others Here"
)]
public class OnRevealDestroyOthersHereEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance source, Zone zone)
	{
		if (source == null || zone == null) return;

		GameManager gm = GameManager.Instance;

		// Índice de esta carta en el orden de revelado
		int sourceIndex = gm.playedOrderThisTurn.IndexOf(source);
		if (sourceIndex < 0) return;

		// Cartas del mismo bando en esta zona
		var list = source.isPlayerCard ? zone.playerCards : zone.aiCards;

		List<CardInstance> toDestroy = new List<CardInstance>();

		foreach (var card in list)
		{
			if (card == null) continue;

			// No destruirse a sí misma
			if (card == source) continue;

			// Debe haberse jugado ANTES que esta carta
			int cardIndex = gm.playedOrderThisTurn.IndexOf(card);
			if (cardIndex == -1 || cardIndex >= sourceIndex) continue;

			// Respeta inmunidad
			if (card.cantBeDestroyed) continue;

			toDestroy.Add(card);
		}

		// Destruir cartas válidas
		foreach (var card in toDestroy)
		{
			gm.DestroyCard(card);
		}

		Debug.Log(
			$"[ON REVEAL] {source.data.cardName} destruye {toDestroy.Count} carta(s) en {zone.name}"
		);
	}
}
