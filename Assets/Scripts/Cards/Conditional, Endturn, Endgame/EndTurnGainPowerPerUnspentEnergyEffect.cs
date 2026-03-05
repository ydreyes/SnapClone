using UnityEngine;

[CreateAssetMenu(
	fileName = "EndTurn_GainPowerPerUnspentEnergy",
	menuName = "Cards/Conditional/Gain Power Per Unspent Energy"
)]
public class EndTurnGainPowerPerUnspentEnergyEffect : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card == null) return;

		GameManager gm = GameManager.Instance;

		int unspentEnergy;

		if (card.isPlayerCard)
			unspentEnergy = gm.turnManager.playerEnergy;
		else
			unspentEnergy = gm.turnManager.aiEnergy;

		if (unspentEnergy <= 0) return;

		card.currentPower += unspentEnergy;
		card.permanentPowerBonus += unspentEnergy;

		card.UpdatePowerUI();

		if (zone != null)
			zone.UpdatePowerDisplay();

		Debug.Log($"[END TURN] {card.data.cardName} gana +{unspentEnergy} Power por energía no usada.");
	}
}
