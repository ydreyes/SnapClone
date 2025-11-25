using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameCompletedController : MonoBehaviour
{
	public Button backButton;

	void Start()
	{
		backButton.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("MainMenu");
		});
	}
}
