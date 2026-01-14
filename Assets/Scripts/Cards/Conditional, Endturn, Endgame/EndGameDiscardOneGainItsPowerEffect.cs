using UnityEngine;

[CreateAssetMenu(
    fileName = "EndGame_DiscardOneGainItsPower",
    menuName = "Cards/Conditional/Discard 1 Card Gain Its Power"
)]
public class EndGameDiscardOneGainItsPowerEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		if (card.isPlayerCard)
			ApplyForPlayer(card);
		else
			ApplyForAI(card);
	}

	private void ApplyForPlayer(CardInstance owner)
	{
		var gm = GameManager.Instance;
		var p = gm.player;

		if (p == null || p.hand == null || p.hand.Count == 0) return;
		if (p.handArea == null) return;

		// Elegir una carta a descartar (random por ahora)
		int idx = Random.Range(0, p.hand.Count);
		CardData chosen = p.hand[idx];

		// Buscar el CardInstance en la UI de mano para usar su currentPower real
		CardInstance inst = null;
		for (int i = 0; i < p.handArea.childCount; i++)
		{
			var ci = p.handArea.GetChild(i).GetComponent<CardInstance>();
			if (ci != null && ci.data == chosen)
			{
				inst = ci;
				break;
			}
		}

		if (inst == null) return;

		int gained = inst.currentPower;

		// Descartar usando tu método (mete a pila + destruye GO + recalcula hand ongoing)
		gm.DiscardCard(inst);

		// Ganar poder en esta carta
		owner.permanentPowerBonus += gained;
		owner.currentPower += gained;
		owner.UpdatePowerUI();

		Debug.Log($"[END GAME] {owner.data.cardName} descarta {chosen.cardName} y gana +{gained}");
	}

	private void ApplyForAI(CardInstance owner)
	{
		var gm = GameManager.Instance;
		var ai = gm.ai;

		if (ai == null || ai.hand == null || ai.hand.Count == 0) return;

		int idx = Random.Range(0, ai.hand.Count);
		CardData chosen = ai.hand[idx];

		// IA no tiene CardInstance en mano, usamos power base (si luego quieres, lo refinamos)
		int gained = chosen.power;

		ai.hand.RemoveAt(idx);
		gm.discardPile.Add(chosen);

		gm.RecalculateHandSizeOngoing(false);

		owner.permanentPowerBonus += gained;
		owner.currentPower += gained;
		owner.UpdatePowerUI();

		Debug.Log($"[END GAME][AI] {owner.data.cardName} descarta {chosen.cardName} y gana +{gained}");
	}
}
