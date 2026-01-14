using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/New Card")]
public class CardData : ScriptableObject
{
	public CardEffectBase onRevealEffect;
	public CardEffectBase ongoingEffect;
	public CardEffectBase conditionalEffect;
	public CardEffectBase onActivateEffect;
	public CardEffectBase onDiscardedEffect;
	public CardEffectBase endGameEffect;
	
	[Header("Inmunidades")]
	public bool cantBeDestroyed;
	public bool cantBeMoved;
	public bool cantHavePowerReduced;
	
	public string cardName;
	public int energyCost;
	public int power;
	public AbilityType abilityType; // Ninguna, OnReveal, Ongoing, End turn, Activate
	public string description;
	public Sprite artwork;
	public bool hasActivateEffect => onActivateEffect != null;
	public bool startsInOpeningHand = false;
	public bool canMoveOnce;
	public bool canMoveEachTurn = false;
	public int permanentPowerBonus = 0;

}

public enum AbilityType
{
	None,
	OnReveal,
	Ongoing,
	EndTurn,
	Activate,
	OnDiscard,
	OnDestroy
}
