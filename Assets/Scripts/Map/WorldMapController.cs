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
		if (zoneIndex == 3)
		{
			PlayerProgress.Instance.currentZoneIndex = 3; // zona del final boss
			SceneManager.LoadScene("BossScene");
			return;
		}
		
		PlayerProgress.Instance.ResetZoneState(zoneIndex);
		SceneManager.LoadScene("ZoneScene");
	}
}
