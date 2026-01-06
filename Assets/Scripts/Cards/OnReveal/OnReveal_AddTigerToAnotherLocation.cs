using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "OnReveal_AddTigerToAnotherLocation",
	menuName = "Cards/OnReveal/Add 9-Power Tiger to Another Location"
)]
public class OnRevealAddTigerToAnotherLocation : CardEffectBase
{
	public CardData tigerCard; // Asignar en inspector (CardData Tiger)
	public int tigerPowerOverride = 9; // por si quieres forzarlo

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;
		if (tigerCard == null)
		{
			Debug.LogWarning("[OnReveal] Tiger cardData no asignado en el efecto.");
			return;
		}

		var gm = GameManager.Instance;
		bool forPlayer = card.isPlayerCard;

		// 1) Buscar "otras" zonas (excluir donde se jugó la carta)
		List<Zone> candidates = new List<Zone>();
		foreach (var z in gm.zones)
		{
			if (z == null) continue;
			if (z == zone) continue; // "another location"
			if (z.CanAcceptCard(new DummyCardInstance(forPlayer))) // ver abajo
				candidates.Add(z);
		}

		// Si no hay espacio en otras zonas, no hace nada
		if (candidates.Count == 0)
		{
			Debug.Log("[OnReveal] No hay otra zona disponible para poner el Tiger.");
			return;
		}

		// 2) Elegir una zona aleatoria
		Zone targetZone = candidates[Random.Range(0, candidates.Count)];

		// 3) Spawnear el Tiger para el bando correcto y añadirlo a la zona
		SpawnTokenToZone(tigerCard, targetZone, forPlayer, tigerPowerOverride);

		Debug.Log($"[OnReveal] {card.data.cardName} agrega TIGER en {targetZone.name}");
	}

	// Helper: spawnear token y meterlo a la zona
	private void SpawnTokenToZone(CardData data, Zone targetZone, bool forPlayer, int powerOverride)
	{
		var gm = GameManager.Instance;

		GameObject cardGO = Instantiate(gm.player.cardPrefab); 
		// ^ si prefieres un prefab específico para tokens, cámbialo aquí

		CardInstance inst = cardGO.GetComponent<CardInstance>();
		inst.data = data;
		inst.isPlayerCard = forPlayer;
		inst.currentPower = powerOverride;

		// init visual (para que no quede null handParent)
		// Si es del jugador, lo “inicializas” con handArea aunque no vaya a la mano.
		// Solo evita nulls cuando uses ReturnToHand/drag.
		if (forPlayer)
			inst.Init(gm.player.handArea);

		inst.GetComponent<CardView>().SetUp(data);
		inst.UpdatePowerUI();

		// Meter a la zona
		targetZone.AddCard(inst);
	}
	
	// Pequeño dummy para reutilizar CanAcceptCard sin cambiar tu firma.
	// Solo provee isPlayerCard.
	private class DummyCardInstance : CardInstance
	{
		public DummyCardInstance(bool isPlayer) { isPlayerCard = isPlayer; }
	}
}
