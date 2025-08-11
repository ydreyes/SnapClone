using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Character")]
public class CharacterData : ScriptableObject
{
	public string characterName;
	public Sprite portrait;
	public DeckData starterDeck;
	public Color uiColor = Color.white; // para acentos en UI
}
