using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_AddSquirrels_OtherLocations",
	menuName = "Cards/OnReveal/Add Squirrels to Other Locations"
)]
public class OnRevealAddSquirrelsToOtherLocationsEffect : CardEffectBase
{
	[Header("Referencia al CardData de la ardilla")]
	public CardData squirrelCardData;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		if (squirrelCardData == null)
		{
			Debug.LogError("[SQUIRRELS] Falta asignar squirrelCardData en el efecto.");
			return;
		}

		var gm = GameManager.Instance;
		bool isPlayerSide = card.isPlayerCard;

		// Recorre todas las zonas menos donde se jugó la carta
		foreach (var z in gm.zones)
		{
			if (z == null) continue;
			if (z == zone) continue; // no en la misma ubicación

			// Verificar si hay espacio en el lado correspondiente (max 4)
			int countOnSide = isPlayerSide ? z.playerCards.Count : z.aiCards.Count;
			if (countOnSide >= 4) continue;

			SpawnSquirrelInZone(z, isPlayerSide);
			z.UpdatePowerDisplay();
		}
	}

	private void SpawnSquirrelInZone(Zone targetZone, bool forPlayer)
	{
		var gm = GameManager.Instance;

		// Usamos el prefab del jugador o IA solo para instanciar visualmente.
		// Si prefieres, puedes exponer un prefab común en el GameManager.
		GameObject prefab = forPlayer ? gm.player.cardPrefab : gm.ai.cardPrefab;

		if (prefab == null)
		{
			Debug.LogError("[SQUIRRELS] No hay cardPrefab para instanciar ardilla.");
			return;
		}

		GameObject cardGO = GameObject.Instantiate(prefab);
		CardInstance instance = cardGO.GetComponent<CardInstance>();
		CardView view = cardGO.GetComponent<CardView>();

		if (instance == null || view == null)
		{
			Debug.LogError("[SQUIRRELS] El prefab no tiene CardInstance o CardView.");
			GameObject.Destroy(cardGO);
			return;
		}

		// Inicializar instancia
		instance.data = squirrelCardData;
		instance.currentPower = squirrelCardData.power;   // 1
		instance.isPlayerCard = forPlayer;
		instance.hasBeenActivated = false;
		instance.effectApplied = false;
		instance.pendingBoostNextTurn = false;
		instance.hasMovedOnce = false;

		// Para que no "vuelva a la mano" si ocurre un drag accidental
		// (en IA da igual, en jugador también está ok)
		instance.Init(forPlayer ? gm.player.handArea : null);

		// Visual
		view.SetUp(squirrelCardData);
		instance.UpdatePowerUI();

		// Meterla a la zona (esto la parenta al row correcto y actualiza poderes)
		targetZone.AddCard(instance);

		Debug.Log($"[SQUIRRELS] Ardilla agregada en {targetZone.name} para {(forPlayer ? "Jugador" : "IA")}");
	}
}
