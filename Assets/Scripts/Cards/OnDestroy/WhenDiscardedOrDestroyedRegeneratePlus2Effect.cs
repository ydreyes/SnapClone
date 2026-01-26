using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
	fileName = "WhenDiscardedOrDestroyed_RegeneratePlus2",
	menuName = "Cards/OnDestroy/Regenerate +2 Power"
)]
public class WhenDiscardedOrDestroyedRegeneratePlus2Effect : CardEffectBase
{
	public int bonusPower = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || card.data == null) return;

		GameManager gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		// Buscar zonas válidas
		List<Zone> validZones = new List<Zone>();
		foreach (var z in gm.zones)
		{
			if (z == null) continue;
			if (z.CanAcceptCard(card))
				validZones.Add(z);
		}

		if (validZones.Count == 0)
		{
			Debug.Log("[REGENERATE +2] No hay zonas disponibles.");
			return;
		}

		Zone targetZone = validZones[Random.Range(0, validZones.Count)];

		// Instanciar copia runtime
		GameObject prefab = forPlayer ? gm.player.cardPrefab : gm.ai.cardPrefab;
		GameObject go = Object.Instantiate(prefab);

		CardInstance copy = go.GetComponent<CardInstance>();
		copy.data = card.data;
		copy.isPlayerCard = forPlayer;

		// Aplicar bonus permanente
		copy.permanentPowerBonus = card.permanentPowerBonus + bonusPower;
		copy.currentPower = card.currentPower + bonusPower;

		// Copiar estado runtime (movimiento, inmunidades, efectos, etc.)
		gm.CopyRuntimeState(card, copy);

		// Visuales
		var view = go.GetComponent<CardView>();
		if (view != null)
			view.SetUp(copy.data);

		// Agregar a zona SIN PlayCard()
		targetZone.AddCardFromEffect(copy);

		Debug.Log($"[REGENERATE +2] {card.data.cardName} reaparece en {targetZone.name} con +{bonusPower} Power");
	}
}
