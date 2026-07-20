using UnityEngine;

[CreateAssetMenu(
    fileName = "OnReveal_BuffHandPower",
    menuName = "Cards/OnReveal/Give Hand +1 Power"
)]
public class OnRevealBuffHandPowerEffect : CardEffectBase
{
	public int bonusPower = 1;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		GameManager gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		// --- JUGADOR ---
		if (forPlayer)
		{
			var handArea = gm.player.handArea;
			if (handArea == null) return;

			for (int i = 0; i < handArea.childCount; i++)
			{
				var ci = handArea.GetChild(i).GetComponent<CardInstance>();
				if (ci == null || ci.data == null) continue;

				ci.permanentPowerBonus += bonusPower;
				ci.currentPower += bonusPower;
				ci.UpdatePowerUI();
			}

			Debug.Log($"[ON REVEAL] Todas las cartas en la mano del jugador ganan +{bonusPower} Power.");
		}
		// --- IA ---
		else
		{
			// La IA no tiene CardInstance en mano.
			// Solución correcta: aplicar el buff al CardData,
			// ya que esas cartas todavía no existen en el tablero.
			foreach (var cd in gm.ai.hand)
			{
				if (cd == null) continue;
				cd.permanentPowerBonus += bonusPower;
			}

			Debug.Log($"[ON REVEAL][AI] Todas las cartas en la mano de la IA ganan +{bonusPower} Power.");
		}
	}
}