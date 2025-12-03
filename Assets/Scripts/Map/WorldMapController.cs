using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WorldMapController : MonoBehaviour
{	
	public Button zone1Btn, zone2Btn, zone3Btn, zoneBossBtn;
	
	void Start()
	{
		var pp = PlayerProgress.Instance;
		
		zone1Btn.interactable = (pp.zonesCompletedOnWorld == 0);
		zone2Btn.interactable = (pp.zonesCompletedOnWorld == 1);
		zone3Btn.interactable = (pp.zonesCompletedOnWorld == 2);
		// Boss: solo visible cuando ya terminaste las 3 zonas
		zoneBossBtn.gameObject.SetActive(pp.zonesCompletedOnWorld >= 3);
		zoneBossBtn.interactable = (pp.zonesCompletedOnWorld >= 3);

		zone1Btn.onClick.RemoveAllListeners();
		zone2Btn.onClick.RemoveAllListeners();
		zone3Btn.onClick.RemoveAllListeners();
		zoneBossBtn.onClick.RemoveAllListeners();

		zone1Btn.onClick.AddListener(() => OpenZone(0));
		zone2Btn.onClick.AddListener(() => OpenZone(1));
		zone3Btn.onClick.AddListener(() => OpenZone(2));
		zoneBossBtn.onClick.AddListener(() => OpenZone(3));	
	}

	void OpenZone(int zoneIndex)
	{
		// Jefe final
		if (zoneIndex == 3)
		{
			PlayerProgress.Instance.currentZoneIndex = 3;
			// Evitar basura de la zona anterior
			PlayerPrefs.SetInt("PendingMark", 0);
			PlayerPrefs.SetInt("LastNodeIndex", -1);

			SceneManager.LoadScene("BossScene");
			return;
		}
		// Zonas normales
		PlayerProgress.Instance.ResetZoneState(zoneIndex);
		// 🔥 RESETEAR DATOS DE BATALLAS ANTERIORES
		PlayerPrefs.SetInt("PendingMark", 0);
		PlayerPrefs.SetInt("LastNodeIndex", -1);

		SceneManager.LoadScene("ZoneScene");
	}
}
