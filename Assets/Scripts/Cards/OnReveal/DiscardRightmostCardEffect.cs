using UnityEngine;

[CreateAssetMenu(
	fileName = "DiscardRightmostCard",
	menuName = "Cards/OnReveal/Discard Rightmost Card"
)]
public class DiscardRightmostCardEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		// Solo afecta a cartas del jugador
		if (!card.isPlayerCard)
			return;

		var gm = GameManager.Instance;
		var player = gm.player;

		// No hay cartas en mano
		if (player.handArea.childCount == 0)
			return;

		// La carta más a la derecha es el último hijo visual
		Transform rightmostTransform =
			player.handArea.GetChild(player.handArea.childCount - 1);

		CardInstance cardToDiscard =
			rightmostTransform.GetComponent<CardInstance>();

		if (cardToDiscard == null)
			return;

		// Seguridad: evitar descartarse a sí misma (caso extremo)
		if (cardToDiscard == card)
			return;

		// Usar el sistema centralizado de descarte
		gm.DiscardCard(cardToDiscard);

		Debug.Log(
			$"[OnReveal] {card.data.cardName} descartó {cardToDiscard.data.cardName}"
		);
	}
}
