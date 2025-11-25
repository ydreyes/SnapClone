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
		fightButton.onClick.AddListener(() =>
		{
			GameSession.Instance.SetEnemy(bossEnemy);
			SceneManager.LoadScene("GameScene");
		});

		backButton.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("WorldMapScene");
		});
	}

}
