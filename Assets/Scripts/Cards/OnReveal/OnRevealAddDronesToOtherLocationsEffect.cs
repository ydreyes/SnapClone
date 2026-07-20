using UnityEngine;

[CreateAssetMenu(
    fileName = "OnReveal_AddDrones_OtherLocations",
    menuName = "Cards/OnReveal/Add 2-Power Drones to Other Locations"
)]
public class OnRevealAddDronesToOtherLocationsEffect : CardEffectBase
{
	[Header("Referencia al CardData del Drone")]
	public CardData droneCardData;

	[Header("Configuración")]
	public int dronePower = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		if (droneCardData == null)
		{
			Debug.LogError("[DRONES] Falta asignar droneCardData.");
			return;
		}

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		foreach (var z in gm.zones)
		{
			if (z == null) continue;
			if (z == zone) continue;

			SpawnDronesFillingSlots(z, forPlayer);
		}
	}

	private void SpawnDronesFillingSlots(Zone targetZone, bool forPlayer)
	{
		var gm = GameManager.Instance;

		int currentCount = forPlayer
			? targetZone.playerCards.Count
			: targetZone.aiCards.Count;

		int spacesLeft = 4 - currentCount;

		if (spacesLeft <= 0) return;

		for (int i = 0; i < spacesLeft; i++)
		{
			SpawnDrone(targetZone, forPlayer);
		}

		targetZone.UpdatePowerDisplay();
	}

	private void SpawnDrone(Zone targetZone, bool forPlayer)
	{
		var gm = GameManager.Instance;

		GameObject prefab = forPlayer
			? gm.player.cardPrefab
			: gm.ai.cardPrefab;

		if (prefab == null)
		{
			Debug.LogError("[DRONES] Falta prefab.");
			return;
		}

		GameObject go = GameObject.Instantiate(prefab);
		CardInstance inst = go.GetComponent<CardInstance>();
		CardView view = go.GetComponent<CardView>();

		if (inst == null || view == null)
		{
			Debug.LogError("[DRONES] Prefab inválido.");
			GameObject.Destroy(go);
			return;
		}

		// Inicialización mínima y segura
		inst.data = droneCardData;
		inst.isPlayerCard = forPlayer;
		inst.currentPower = dronePower;

		inst.permanentPowerBonus = 0;
		inst.effectApplied = false;
		inst.hasMovedOnce = false;
		inst.pendingBoostNextTurn = false;

		// Evita problemas de drag
		inst.Init(forPlayer ? gm.player.handArea : null);

		view.SetUp(droneCardData);
		inst.UpdatePowerUI();

		// IMPORTANTE: usar AddCardFromEffect (no PlayCard)
		targetZone.AddCardFromEffect(inst);
	}
}