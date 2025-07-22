using UnityEngine;

public abstract class CardEffectBase : ScriptableObject, ICardEffect
{
	public abstract void ApplyEffect(CardInstance card, Zone zone);
}
