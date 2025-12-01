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
		zone1Btn.interactable = true;
		zone2Btn.interactable = (pp.zonesCompletedOnWorld >= 1);
		zone3Btn.interactable = (pp.zonesCompletedOnWorld >= 2);
		
		zoneBossBtn.gameObject.SetActive(pp.zonesCompletedOnWorld >=3 ); // cuando se terminan las 3 zonas
		zoneBossBtn.interactable = true;

		zone1Btn.onClick.AddListener(() => OpenZone(0));
		zone2Btn.onClick.AddListener(() => OpenZone(1));
		zone3Btn.onClick.AddListener(() => OpenZone(2));
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
