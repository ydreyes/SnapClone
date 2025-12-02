using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerProgress : MonoBehaviour
{
	public static PlayerProgress Instance;

	[Header("Progreso global (mapa)")]
	public int zonesCompletedOnWorld = 0;   // cuántas zonas (0..3) del mapa mundial has completado

	[Header("Estado de la zona actual (ZoneScene)")]
	public int currentZoneIndex = 0;        // 0..2
	public bool[] nodesCompleted = new bool[7]; // progreso de los 7 nodos de la zona
	
	[Header("Colección de cartas del jugador")]
	public List<CardData> ownedCards = new List<CardData>();
	public List<int> claimedRewards = new List<int>();

	[Header("Puntuación del jugador")]
	public int lives = 8;                   // puntos de vida iniciales
	public int heroPoints = 0;              // puntos de héroe (100 por zona ganada)
	public bool betActive = false;          // si hay una apuesta activa para el PRÓXIMO combate
	
	[Header("Información del jefe Final")]
	public bool finalBossUnlocked = false;
	public bool finalBossDefeated = false;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// Reinicia el estado interno de una zona al entrar (7 nodos sin completar).
	/// </summary>
	public void ResetZoneState(int zoneIndex)
	{
		currentZoneIndex = zoneIndex;
		nodesCompleted = new bool[7];
	}

	/// <summary>
	/// Intenta activar una apuesta de 4 vidas para el siguiente combate.
	/// Devuelve true si se pudo activar (vidas suficientes), false en caso contrario.
	/// </summary>
	public bool TryPlaceBet()
	{
		// Si no alcanza para apostar, no actives la apuesta
		if (lives < 4) return false;
		betActive = true;
		return true;
	}

	/// <summary>
	/// Cancela manualmente la apuesta (si hiciste un toggle y el jugador se arrepiente).
	/// </summary>
	public void CancelBet()
	{
		betActive = false;
	}

	/// <summary>
	/// Aplica las reglas al terminar un combate:
	/// - Pierde 1 vida por cada zona NO dominada (3 - playerZones).
	/// - Si hay apuesta: +8 vidas si gana, -4 si pierde (y se limpia la apuesta).
	/// - +100 puntos de héroe por cada zona ganada.
	/// </summary>
	public void ApplyMatchOutcome(int playerZones, int aiZones, bool playerWon)
	{
		// 🟣 1) Jefe final: flujo especial
		if (currentZoneIndex == 3)
		{
			finalBossDefeated = playerWon;

			// Reset limpio para evitar reentrada a ZoneScene
			PlayerPrefs.SetInt("PendingMark", 0);
			PlayerPrefs.SetInt("LastNodeIndex", -1);

			betActive = false;
			GameManager.Instance.ai.aiBet = false;

			if (playerWon)
			{
				SceneManager.LoadScene("GameCompletedScene");
			}
			else
			{
				SceneManager.LoadScene("GameOverScene");
			}

			return;
		}

		// 🟢 2) Zonas normales -------------------------------

		// 1) Vidas por zonas no dominadas
		int zonesLost = Mathf.Max(0, 3 - Mathf.Clamp(playerZones, 0, 3));
		lives -= zonesLost;

		// 2) Apuestas
		bool aiBet = GameManager.Instance.ai.aiBet;

		if (betActive || aiBet)
		{
			if (betActive && aiBet)
			{
				lives += playerWon ? 8 : -4;
			}
			else if (betActive)
			{
				lives += playerWon ? 4 : -4;
			}
			else if (aiBet)
			{
				if (playerWon)
					lives += 8;
			}

			betActive = false;
			GameManager.Instance.ai.aiBet = false;
		}

		// 3) Puntos de héroe
		heroPoints += playerZones * 100;

		// 4) Game Over por vidas
		if (lives <= 0)
		{
			SceneManager.LoadScene("GameOverScene");
			return;
		}

		// 5) Volver a ZoneScene para marcar nodo
		PlayerPrefs.SetInt("PendingMark", 1);
		SceneManager.LoadScene("ZoneScene");
	}
	
	public void AddCardToCollection(CardData card)
	{
		if (card == null) 
			return;
			
		if (!ownedCards.Contains(card))
		{
			ownedCards.Add(card);
		}
	}

	public bool IsGameOver()
	{
		return lives <= 0;
	}
}
