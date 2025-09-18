using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneReturn : MonoBehaviour
{
	public void OnContinue()
	{
		// Si se acabó la partida (vidas <= 0), ve a WorldMap o GameOver
		if (PlayerProgress.Instance.IsGameOver())
		{
			SceneManager.LoadScene("WorldMapScene"); // o "GameOverScene"
			return;
		}

		// Señal para ZoneScene de que debe marcar el último nodo y desbloquear +2
		PlayerPrefs.SetInt("PendingMark", 1);
		SceneManager.LoadScene("ZoneScene");
	}
}
