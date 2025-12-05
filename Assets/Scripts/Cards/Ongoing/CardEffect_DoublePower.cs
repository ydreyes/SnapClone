using UnityEngine;

[CreateAssetMenu(fileName = "DoublePowerEffect", menuName = "Cards/Ongoing/DoublePower")]
public class CardEffect_DoublePower : CardEffectBase
{
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		if (card.isPlayerCard)
		{
			zone.doublePlayerPower = true;
			Debug.Log($"[Ongoing] Poder del jugador en {zone.name} es duplicado.");
		}
		else
		{
			zone.doubleAIPower = true;
			Debug.Log($"[Ongoing] Poder de la IA en {zone.name} es duplicado.");
		}

		zone.UpdatePowerDisplay();
	}
}
