using UnityEngine;

[CreateAssetMenu(
    fileName = "OnReveal_DiscardYourHand",
    menuName = "Cards/OnReveal/Discard Your Hand"
)]
public class OnRevealDiscardYourHandEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		if (card.isPlayerCard)
			DiscardPlayerHand();
		else
			DiscardAIHand();

		// refresca ongoing dependiente de tamaño de mano (Strong Guy etc.)
		GameManager.Instance.RecalculateHandSizeOngoing(card.isPlayerCard);

		Debug.Log($"[OnReveal] {card.data.cardName}: descartó toda su mano.");
	}

	private void DiscardPlayerHand()
	{
		var gm = GameManager.Instance;
		var p = gm.player;
		if (p == null || p.handArea == null) return;

		// 1) Capturar instancias en mano (no iterar mientras destruyes)
		var list = new System.Collections.Generic.List<CardInstance>();
		for (int i = 0; i < p.handArea.childCount; i++)
		{
			var ci = p.handArea.GetChild(i).GetComponent<CardInstance>();
			if (ci != null) list.Add(ci);
		}

		// 2) Descartar cada instancia usando tu método (mete a discardPile + Destroy GO)
		for (int i = 0; i < list.Count; i++)
		{
			gm.DiscardCard(list[i]);
		}

		// 3) Seguridad: vaciar lista lógica de mano
		p.hand.Clear();
	}

	private void DiscardAIHand()
	{
		var gm = GameManager.Instance;
		var ai = gm.ai;
		if (ai == null) return;

		// pasar todo a discardPile
		for (int i = 0; i < ai.hand.Count; i++)
		{
			gm.discardPile.Add(ai.hand[i]);
		}

		ai.hand.Clear();
	}
}
