using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
	public static GameSession Instance;

	[Header("Selection")]
	public CharacterData selectedCharacter;
	public EnemyData selectedEnemy;
	public CharacterData selectedAI;   // opcional: IA fija o aleatoria

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void SelectCharacter(CharacterData c) => selectedCharacter = c;
	public void SelectAI(CharacterData ai) => selectedAI = ai;
	public void SetEnemy(EnemyData e) => selectedEnemy = e;
	
	public void StartGame()
	{		
		selectedAI = null;
		
		// Reinicia el progreso al empezar
		if (PlayerProgress.Instance)
		{
			PlayerProgress.Instance.lives = 8;
			PlayerProgress.Instance.heroPoints = 0;
			PlayerProgress.Instance.zonesCompletedOnWorld = 0;
		}
		
		// cargar el mapa mundial en vez del GameScene
		SceneManager.LoadScene("WorldMapScene");
	}
}
