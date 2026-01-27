using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
	fileName = "OnReveal_DestroyOthersHere_AddPower",
	menuName = "Cards/OnReveal/Destroy Others Here Add Power"
)]
public class OnRevealDestroyOthersHereAddPowerEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance source, Zone zone)
	{
		if (source == null || zone == null) return;

		GameManager gm = GameManager.Instance;

		int gainedPower = 0;

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

		// Destruir y acumular poder
		foreach (var card in toDestroy)
		{
			gainedPower += card.currentPower;
			gm.DestroyCard(card);
		}

		if (gainedPower > 0)
		{
			source.currentPower += gainedPower;
			source.UpdatePowerUI();
			zone.UpdatePowerDisplay();
		}

		Debug.Log(
			$"[ON REVEAL] {source.data.cardName} destruye {toDestroy.Count} carta(s) y gana +{gainedPower} Power"
		);
	}
}
