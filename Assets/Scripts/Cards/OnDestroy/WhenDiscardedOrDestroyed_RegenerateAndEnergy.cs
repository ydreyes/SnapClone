using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
	fileName = "WhenDiscardedOrDestroyed_RegenerateAndEnergy",
	menuName = "Cards/OnDestroy/Regenerate + Energy"
)]
public class WhenDiscardedOrDestroyedRegenerateAndEnergyEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || card.data == null) return;

		GameManager gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		// 1️⃣ Elegir una zona aleatoria válida
		List<Zone> validZones = new List<Zone>();

		foreach (var z in gm.zones)
		{
			if (z == null) continue;
			if (z.CanAcceptCard(card))
				validZones.Add(z);
		}

		if (validZones.Count == 0)
		{
			Debug.Log("[REGENERATE] No hay zonas disponibles.");
			return;
		}

		Zone targetZone = validZones[Random.Range(0, validZones.Count)];

		// 2️⃣ Crear una COPIA runtime de la carta
		GameObject prefab = forPlayer ? gm.player.cardPrefab : gm.ai.cardPrefab;
		GameObject go = Object.Instantiate(prefab);

		CardInstance copy = go.GetComponent<CardInstance>();
		copy.data = card.data;
		copy.isPlayerCard = forPlayer;

		// Mantener PODER TOTAL
		copy.currentPower = card.currentPower;
		copy.permanentPowerBonus = card.permanentPowerBonus;

		// Copiar flags importantes
		gm.CopyRuntimeState(card, copy);

		// Visuales
		var view = go.GetComponent<CardView>();
		if (view != null)
			view.SetUp(copy.data);

		// 3️⃣ Agregar a la zona SIN PlayCard()
		targetZone.AddCardFromEffect(copy);

		Debug.Log($"[REGENERATE] {card.data.cardName} reaparece en {targetZone.name}");

		// 4️⃣ +1 Energía el próximo turno
		gm.AddPendingEnergyNextTurn(forPlayer, 1);
	}
}
