using UnityEngine;

[CreateAssetMenu(
    fileName = "WhenDiscarded_AddTwoZeroCostCopies",
    menuName = "Cards/OnDiscarded/Add 2 Copies Cost 0"
)]
public class WhenDiscardedAddTwoZeroCostCopiesEffect : CardEffectBase
{
	public int copies = 2;
	public int newCost = 0;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || card.data == null) return;

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		for (int i = 0; i < copies; i++)
		{
			CardData copy = gm.CreateRuntimeCopy(card.data, newCost);
			gm.PutCardBackToHand(copy, forPlayer); // jugador: SpawnCardInHand (UI), IA: a la lista
		}

		if (forPlayer) gm.RecalculateHandSizeOngoing(true);
		else gm.RecalculateHandSizeOngoing(false);

		Debug.Log($"[WhenDiscarded] {card.data.cardName}: agrega {copies} copia(s) de costo {newCost} a la mano.");
	}
}
