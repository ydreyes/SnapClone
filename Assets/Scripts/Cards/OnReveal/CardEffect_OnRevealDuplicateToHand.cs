using UnityEngine;

[CreateAssetMenu(fileName = "OnRevealDuplicateToHand", menuName = "Cards/OnReveal/OnRevealDuplicateToHand")]
public class CardEffect_OnRevealDuplicateToHand : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Solo el jugador debe recibir copia
		if (!card.isPlayerCard)
		{
			Debug.Log("[OnReveal] La IA no recibe copias en mano.");
			return;
		}

		// Obtener PlayerController
		var player = GameManager.Instance.player;

		// 1. Agregar copia lógica a la mano
		player.hand.Add(card.data);

		// 2. Instanciar visualmente en mano
		player.SpawnCardInHand(card.data);

		Debug.Log($"[OnReveal] {card.data.cardName} añadió una copia a la mano.");
	}
}
