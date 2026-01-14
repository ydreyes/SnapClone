using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_Discard2GainMaxEnergy",
	menuName = "Cards/OnReveal/Discard 2 Gain +1 Max Energy"
)]
public class OnRevealDiscard2GainMaxEnergyEffect : CardEffectBase
{
	public int discardCount = 2;
	public int gainMaxEnergy = 1;

	// opcional: además de subir el máximo, subir energía actual de inmediato
	public bool alsoGainCurrentEnergy = true;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		bool forPlayer = card.isPlayerCard;

		bool paid = forPlayer
			? DiscardNFromPlayer(discardCount)
			: DiscardNFromAI(discardCount);

		if (!paid)
		{
			Debug.Log("[OnReveal] No hay suficientes cartas para descartar 2. No se aplica +Max Energy.");
			return;
		}

		var tm = GameManager.Instance.turnManager;

		if (forPlayer)
		{
			tm.playerMaxEnergyBonus += gainMaxEnergy;
			if (alsoGainCurrentEnergy) tm.playerEnergy += gainMaxEnergy;
		}
		else
		{
			tm.aiMaxEnergyBonus += gainMaxEnergy;
			if (alsoGainCurrentEnergy) tm.aiEnergy += gainMaxEnergy;
		}

		GameManager.Instance.UpdateEnergyDisplay();

		Debug.Log($"[OnReveal] {(forPlayer ? "PLAYER" : "AI")} descarta {discardCount} y gana +{gainMaxEnergy} Max Energy.");
	}

	private bool DiscardNFromPlayer(int n)
	{
		var gm = GameManager.Instance;
		var p = gm.player;

		if (p == null || p.hand == null) return false;
		if (p.hand.Count < n) return false;
		if (p.handArea == null) return false;

		for (int k = 0; k < n; k++)
		{
			if (p.hand.Count == 0) return false;

			CardData chosen = p.hand[Random.Range(0, p.hand.Count)];
			if (chosen == null) return false;

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

			if (inst == null) return false;

			gm.DiscardCard(inst); // ya maneja: quitar de hand, agregar a discardPile, Destroy GO + recalcs
		}

		return true;
	}

	private bool DiscardNFromAI(int n)
	{
		var gm = GameManager.Instance;
		var ai = gm.ai;

		if (ai == null || ai.hand == null) return false;
		if (ai.hand.Count < n) return false;

		for (int k = 0; k < n; k++)
		{
			CardData chosen = ai.hand[Random.Range(0, ai.hand.Count)];
			ai.hand.Remove(chosen);
			gm.discardPile.Add(chosen);
		}

		// por si tienes ongoing que depende de tamaño de mano
		gm.RecalculateHandSizeOngoing(false);

		return true;
	}
}
