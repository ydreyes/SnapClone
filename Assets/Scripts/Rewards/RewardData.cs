using UnityEngine;

public enum RewardType { Card, Credits, Boosters }

[CreateAssetMenu(menuName = "Progression/Reward")]
public class RewardData : ScriptableObject
{
	public RewardType type;

	[Header("Display")]
	public Sprite icon;
	public string rewardName;

	[Header("Value")]
	public int amount; // para créditos o boosters

	[Header("Card Reward Settings")]
	public CardData specificCard;   // si quieres dar una carta concreta
	public bool giveRandomCard = false; // si quieres carta aleatoria

	[Header("Cost to unlock")]
	public int costToUnlock = 4; // 4 u 8 puntos
}
