using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
	public List<CardData> deck;
	public List<CardData> hand = new List<CardData>();
	public List<CardData> drawPile = new();
	public GameObject cardPrefab;
	public Transform aiZoneArea;
	public bool aiBet = false;

	// Bonos de poder pendientes para cartas de la IA (drawPile/mano).
	// Evita mutar CardData (asset) en runtime; se aplica al instanciar la carta.
	private readonly Dictionary<CardData, int> pendingHandPower = new Dictionary<CardData, int>();

	public void AddPendingHandPower(CardData card, int amount)
	{
		if (card == null || amount == 0) return;
		pendingHandPower.TryGetValue(card, out int current);
		pendingHandPower[card] = current + amount;
	}

	public int GetPendingHandPower(CardData card)
	{
		if (card == null) return 0;
		return pendingHandPower.TryGetValue(card, out int value) ? value : 0;
	}

	public void ShuffleDeck()
	{
		Deck.Shuffle(deck);
	}

	public void DrawCard()
	{
		if (drawPile.Count == 0) return;

		var card = drawPile[0];
		drawPile.RemoveAt(0);
		hand.Add(card);
	}
	
	public void ResetDeckAndHand()
	{
		hand.Clear();
		pendingHandPower.Clear();

		drawPile = new List<CardData>(deck);

		// mezclar drawPile (barajado centralizado en Deck)
		Deck.Shuffle(drawPile);
	}

	public void PlayCardAutomatically()
	{
		// IA juega una carta aleatoria si tiene energía
		if (hand.Count == 0) return;

		var turnEnergy = GameManager.Instance.turnManager.aiEnergy;

		for (int i = 0; i < hand.Count; i++)
		{
			var card = hand[i];
			if (card.energyCost <= turnEnergy)
			{
				// Instanciar carta en zona aleatoria
				GameObject cardGO = Instantiate(cardPrefab);
				CardInstance instance = cardGO.GetComponent<CardInstance>();
				instance.data = card;
				instance.state.basePower = PowerCalculator.ComputeBase(card);
				instance.currentPower = instance.state.currentPower;
				// Bonos de mano/deck acumulados (antes se mutaba el asset CardData)
				int handBonus = GetPendingHandPower(card);
				if (handBonus != 0)
				{
					instance.permanentPowerBonus += handBonus;
					instance.currentPower += handBonus;
				}
				instance.UpdatePowerUI();
				instance.isPlayerCard = false;
				// card can move
				instance.canMoveOnce = card.canMoveOnce;
				instance.canMoveEachTurn = card.canMoveEachTurn;

				instance.GetComponent<CardView>().SetUp(card); // inicializar visuales

				// Elegir zona aleatoria
				int zoneIndex = Random.Range(0, GameManager.Instance.zones.Count);
				Zone selectedZone = GameManager.Instance.zones[zoneIndex];
				//reducir energia cuando juega una carta la IA
				GameManager.Instance.turnManager.aiEnergy -= card.energyCost;
				instance.PlayCard(selectedZone);

				hand.RemoveAt(i);
				break;
			}
		}
	}
	
	public void TryRandomBet(int currentTurn)
	{
		if (aiBet) return;

		// Probabilidad incremental por turno
		float chance = currentTurn * 0.15f; // 15% por turno
		if (Random.value < chance)
		{
			aiBet = true;
			Debug.Log("LA IA HA ACTIVADO UNA APUESTA");
		}
	}
	
}
