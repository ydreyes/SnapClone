using UnityEngine;

[CreateAssetMenu(
	fileName = "WhenDestroyed_ReturnToHandDoublePower",
	menuName = "Cards/OnDestroy/Return To Hand Double Power"
)]
public class WhenDestroyedReturnToHandDoublePowerEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || card.data == null) return;

		GameManager gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		// Poder TOTAL actual
		int totalPower = card.currentPower;

		// Bonus para duplicar
		int bonus = totalPower;

		// Crear copia runtime del CardData
		CardData runtimeCopy = gm.CreateRuntimeCopy(card.data, card.data.energyCost);

		// Guardar bonus para cuando vuelva a la mano
		gm.AddPendingPower(runtimeCopy, bonus);

		// Devolver a la mano
		gm.PutCardBackToHand(runtimeCopy, forPlayer);

		Debug.Log($"[ON DESTROY] {card.data.cardName} vuelve a la mano con poder duplicado (+{bonus})");
	}
}
