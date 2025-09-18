using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WorldMapController : MonoBehaviour
{	
	public Button zone1Btn, zone2Btn, zone3Btn;
	
	void Start()
	{
		var pp = PlayerProgress.Instance;
		zone1Btn.interactable = true;
		zone2Btn.interactable = (pp.zonesCompletedOnWorld >= 1);
		zone3Btn.interactable = (pp.zonesCompletedOnWorld >= 2);

		zone1Btn.onClick.AddListener(() => OpenZone(0));
		zone2Btn.onClick.AddListener(() => OpenZone(1));
		zone3Btn.onClick.AddListener(() => OpenZone(2));
	}

	void OpenZone(int zoneIndex)
	{
		PlayerProgress.Instance.ResetZoneState(zoneIndex);
		SceneManager.LoadScene("ZoneScene");
	}
}
