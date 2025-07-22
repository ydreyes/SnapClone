using UnityEngine;

public class TurnManager : MonoBehaviour
{
	public int currentTurn = 1;
	public int maxTurns = 2; // cambiar para prueba
	public int playerEnergy = 1;
	public int aiEnergy = 1;

	public void EndTurn()
	{
		currentTurn++;
		
		playerEnergy = Mathf.Min(currentTurn, 6);
		aiEnergy = Mathf.Min(currentTurn, 6);

		if (currentTurn > maxTurns)
		{
			//GameOver();
			GameManager.Instance.EvaluateGame();
			return;
		}
		GameManager.Instance.UpdateEnergyDisplay();
	}

	void GameOver()
	{
		Debug.LogWarning("Fin del juego, Evaluar zonas");
	}
	
	// Modificar
	public void RestartGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}
}
