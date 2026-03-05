using UnityEngine;

[CreateAssetMenu(
	fileName = "EndTurn_GainPowerIfOpponentDidntPlayHere",
	menuName = "Cards/Conditional/Opponent Didnt Play Here"
)]
public class EndTurnOpponentDidntPlayHereEffect : CardEffectBase
{
	public int bonusPower = 2;

	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null || zone == null) return;

		GameManager gm = GameManager.Instance;
		int currentTurn = gm.turnManager.currentTurn;

		// ❗ No aplicar el turno en que se jugó
		if (card.playedTurn == currentTurn)
			return;

		bool opponentPlayedHere = false;

		foreach (var played in gm.playedOrderThisTurn)
		{
			if (played == null) continue;

			// mismo turno
			if (played.playedTurn != currentTurn) continue;

			// verificar zona
			if (gm.GetZoneForCard(played) != zone) continue;

			// verificar oponente
			if (played.isPlayerCard != card.isPlayerCard)
			{
				opponentPlayedHere = true;
				break;
			}
		}

		if (opponentPlayedHere) return;

		card.currentPower += bonusPower;
		card.permanentPowerBonus += bonusPower;

		card.UpdatePowerUI();
		zone.UpdatePowerDisplay();

		Debug.Log($"[END TURN] {card.data.cardName} gana +{bonusPower} porque el oponente no jugó aquí.");
	}
}