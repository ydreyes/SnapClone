using UnityEngine;

public class TurnManager : MonoBehaviour
{
	public int currentTurn = 1;
	public int maxTurns = 6; // cambiar para prueba
	public int playerEnergy = 1;
	public int aiEnergy = 1;
	// incremento de energia
	public int playerMaxEnergyBonus = 0;
	public int aiMaxEnergyBonus = 0;

	public void EndTurn()
	{
		currentTurn++;

		playerEnergy = Mathf.Min(currentTurn, 6) + playerMaxEnergyBonus;
		aiEnergy = Mathf.Min(currentTurn, 6) + aiMaxEnergyBonus;

		if (currentTurn > maxTurns)
		{
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
	
	public void ResetToTurn1()
	{
		currentTurn = 1;
		
		playerMaxEnergyBonus = 0;
		aiMaxEnergyBonus = 0;
		
		playerEnergy = 1;
		aiEnergy = 1;
	}

}
