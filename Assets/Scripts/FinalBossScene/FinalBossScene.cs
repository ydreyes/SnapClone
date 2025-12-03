using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalBossScene : MonoBehaviour
{
	public Button fightButton;
	public Button backButton;
	public EnemyData bossEnemy;
	
	void Start()
	{
		PlayerProgress.Instance.finalBossUnlocked = true;

		fightButton.onClick.AddListener(() =>
		{
			GameSession.Instance.SetEnemy(bossEnemy);
			PlayerProgress.Instance.currentZoneIndex = 3;
			SceneManager.LoadScene("GameScene");
		});

		backButton.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("WorldMapScene");
		});
	}

}
