using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/OnActivate/DrawCard")]
public class DrawCardEffect : CardEffectBase
{
	public int amount = 1;
	
	// Efecto On activate: Roba una carta
	public override void ApplyEffect(CardInstance card, Zone zone)
	{
		for (int i = 0; i < amount; i++)
		{
			GameManager.Instance.player.DrawCard();
		}
	}
}

