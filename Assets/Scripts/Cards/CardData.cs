using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/New Card")]
public class CardData : ScriptableObject
{
	public CardEffectBase onRevealEffect;
	public CardEffectBase ongoingEffect;
	public CardEffectBase conditionalEffect;
	public CardEffectBase onActivateEffect;
	
	public string cardName;
	public int energyCost;
	public int power;
	public AbilityType abilityType; // Ninguna, OnReveal, Ongoing
	public string description;
	public Sprite artwork;
	public bool hasActivateEffect => onActivateEffect != null;
	public bool startsInOpeningHand = false;

}

public enum AbilityType
{
	None,
	OnReveal,
	Ongoing
}
