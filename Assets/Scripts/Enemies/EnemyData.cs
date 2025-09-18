using UnityEngine;

public enum EnemyDifficulty { Easy, Normal, Hard, Boss }

[CreateAssetMenu(menuName = "Game/Enemy", fileName = "Enemy_")]
public class EnemyData : ScriptableObject
{
	[Header("Info")]
	public string enemyName;
	public Sprite portrait;           // para UI
	public EnemyDifficulty difficulty = EnemyDifficulty.Normal;
	[TextArea] public string blurb;   // descripción corta para preview

	[Header("Gameplay")]
	public DeckData deck;             // mazo del enemigo (12 cartas)
	[Range(0f, 2f)] public float aiThinkDelay = 0.4f; // ejemplo: ritmo IA
	public int mulliganDraw = 0;      // ejemplo: cartas extra al inicio por dificultad

	[Header("UI")]
	public Sprite previewArt;         // opcional: imagen grande para la preview
}
