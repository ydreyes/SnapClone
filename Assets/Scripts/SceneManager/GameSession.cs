using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
	public static GameSession Instance;

	[Header("Selection")]
	public CharacterData selectedCharacter;
	public CharacterData selectedAI;   // opcional: IA fija o aleatoria

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void SelectCharacter(CharacterData c) => selectedCharacter = c;
	public void SelectAI(CharacterData ai) => selectedAI = ai;

	public void StartGame()
	{
		// Si no elegiste IA, elige una distinta al jugador
		if (selectedAI == null)
		{
			var all = Resources.LoadAll<CharacterData>("Characters");
			if (all.Length > 0)
			{
				selectedAI = all[Random.Range(0, all.Length)];
				if (selectedCharacter != null && selectedAI == selectedCharacter && all.Length > 1)
					selectedAI = all[(System.Array.IndexOf(all, selectedCharacter) + 1) % all.Length];
			}
		}
		SceneManager.LoadScene("GameScene"); // Escena principal de la partida
	}
}
