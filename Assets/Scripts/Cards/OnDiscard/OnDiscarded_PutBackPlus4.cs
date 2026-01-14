using UnityEngine;

[CreateAssetMenu(
    fileName = "WhenDiscarded_PutBackPlus4",
    menuName = "Cards/OnDiscarded/Put Back +4 Power"
)]
public class OnDiscarded_PutBackPlus4 : CardEffectBase
{
	public int bonus = 4;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || card.data == null) return;

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;
		CardData data = card.data;

		// 1) Quitar 1 ocurrencia de esta carta de la pila de descarte (porque "put this back")
		// (busca desde el final para quitar la que se acaba de agregar)
		for (int i = gm.discardPile.Count - 1; i >= 0; i--)
		{
			if (gm.discardPile[i] == data)
			{
				gm.discardPile.RemoveAt(i);
				break;
			}
		}

		// 2) Registrar +4 como poder pendiente PERMANENTE para esta CardData
		gm.AddPendingPower(data, bonus);

		// 3) Volver a la mano del mismo dueño
		gm.PutCardBackToHand(data, forPlayer);

		// 4) Recalcular ongoings que dependan de tamaño de mano (Strong Guy etc.)
		gm.RecalculateHandSizeOngoing(forPlayer);

		Debug.Log($"[WhenDiscarded] {data.cardName}: vuelve a la mano con +{bonus} Power");
	}
}
